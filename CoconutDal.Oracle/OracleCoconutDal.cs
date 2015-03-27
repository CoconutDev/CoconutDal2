using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Globalization;
using CoconutDal.Base;

namespace CoconutDal.Oracle
{
    /// <summary>
    /// A Coconut Dal implementation that works with Sql Server and Sql Server Compact
    /// </summary>
    public class OracleCoconutDal : ICoconutDal
    {
        /// <summary>
        /// Specify the connection string
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// Gets the last caught exception. Only OracleExceptions are caught.
        /// </summary>
        public Exception LastError { get; private set; }

        /// <summary>
        /// If true, exceptions of type DbException and SqlCeException will be trapped by the dal and stored in LastError.
        /// </summary>
        public bool CatchDbExceptions { get; set; }

        /// <summary>
        /// Indicates that the command is a text query, rather than a stored procedure.
        /// Note that the Dal will reject text queries that appear to be open to injection attacks.
        /// </summary>
        public bool IsTextQuery { get; set; }

        /// <summary>
        /// When true, specifies that all commands are text queries, not a stored procedure.
        /// </summary>
        public bool IsAlwaysTextQuery { get; set; }

        /// <summary>
        /// Creates a new instance of OracleCoconutDal, using the supplied connection string
        /// </summary>
        /// <param name="connectionString">A sql server connection string</param>
        public OracleCoconutDal(string connectionString)
        {
            this.Connection = connectionString;
        }

        private DbConnection GetDbConnection()
        {
            return new OracleConnection(this.Connection);
        }

        private DbCommand GetDbCommand(string command, DbConnection conn)
        {
            return new OracleCommand(command, (OracleConnection)conn);
        }

        private void CheckConnection()
        {
            if (string.IsNullOrEmpty(this.Connection))
            {
                throw new ArgumentException("Connection String not specified.");
            }
        }

        private void InitializeCommand(DbCommand command, object[] parameters)
        {
            if (command.CommandText.Contains("'") || command.CommandText.Contains("--") || command.CommandText.Contains("/*") || command.CommandText.Contains("*/"))
            {
                throw new ArgumentException("Command is not safe. Text query cannot contain raw string input or comments.");
            }

            if (command.CommandText.Contains(";shutdown"))
            {
                throw new InvalidOperationException("Someone is trying to shutdown Sql Server.");
            }

            if (!this.IsTextQuery && !this.IsAlwaysTextQuery)
            {
                command.CommandType = CommandType.StoredProcedure;
            }

            AddParameters(command, parameters);
            this.IsTextQuery = false; // reset
        }

        private static void AddParameters(DbCommand command, object[] parameters)
        {
            if (parameters.Length < 1)
            {
                return;
            }

            // 3 valid cases: SP + Params, SP + Values, Text + Params
            if (parameters.Count(p => p is DbParameter) == parameters.Length)
            {
                // for SP + Params & Text + Params
                command.Parameters.AddRange(parameters);
            }
            else if (command.CommandType == CommandType.StoredProcedure)
            {
                var cmd = command as OracleCommand;
                if (cmd != null)
                {
                    OracleCommandBuilder.DeriveParameters(cmd);
                }

                int index = 0;
                foreach (DbParameter parameter in command.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.Input ||
                         parameter.Direction == ParameterDirection.InputOutput)
                    {
                        parameter.Value = parameters[index++];
                    }
                }
            }
            else
            {
                // Text + Values is not valid
                throw new ArgumentException("Could not add query parameters. For text queries, each parameter must be of type SqlParameter.");
            }
        }

        /// <summary>
        /// Execute a database command that returns no results
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>A boolean matchingElement indicating whether the command was successful</returns>
        public bool ExecuteNonQuery(string command, params object[] parameters)
        {
            this.LastError = null;
            this.CheckConnection();
            try
            {
                using (var conn = this.GetDbConnection())
                {
                    conn.ConnectionString = this.Connection;
                    conn.Open();
                    using (var cmd = this.GetDbCommand(command, conn))
                    {
                        this.InitializeCommand(cmd, parameters);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception sqex)
            {
                if (sqex is OracleException)
                {
                    this.LastError = sqex;
                }
                else
                {
                    throw;
                }

                if (!this.CatchDbExceptions)
                {
                    throw;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>An object representing a single datum</returns>
        public object GetSingleValue(string command, params object[] parameters)
        {
            return GetSingleValue(command, false, parameters);
        }

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="isNewIdentityValue">Indicates whether to select a newly created identity value. Dal will attempt to modify the query to select the new id (if neccesary)</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>
        /// An object representing a single datum
        /// </returns>
        public object GetSingleValue(string command, bool isNewIdentityValue, params object[] parameters)
        {
            this.CheckConnection();
            try
            {
                using (var conn = this.GetDbConnection())
                {
                    conn.Open();
                    using (var cmd = this.GetDbCommand(command, conn))
                    {
                        this.InitializeCommand(cmd, parameters);

                        // if user has indicated they want the newly created identity AND the query does not already contain a call to SCOPE_IDENTITY()...
                        if (isNewIdentityValue && !command.ToUpper().Contains("SCOPE_IDENTITY()"))
                        {
                                throw new NotImplementedException("Oracle select identity not implemented.");
                        }

                        var result = cmd.ExecuteScalar();
                        return result is DBNull ? null : result;
                    }
                }
            }
            catch (Exception sqex)
            {
                if (sqex is OracleException)
                    LastError = sqex;
                else
                    throw;

                if (!CatchDbExceptions)
                    throw;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional colums or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="isNewIdentityValue">indicate whether to select a newly created identity value. Dal will attempt to modify the query to select the new id (if neccesary.)</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>An object representing a single datum.</returns>
        public TExpectedType GetSingleValue<TExpectedType>(string command, bool isNewIdentityValue, params object[] parameters)
        {
            object o = GetSingleValue(command, isNewIdentityValue, parameters);

            if (o == null) return default(TExpectedType);

            try
            {
                // try to handle the fact that casting SCOPE_IDENTITY() and @@IDENTITY to int is a narrowing conversion
                if (isNewIdentityValue)
                {
                    throw new NotImplementedException("Oracle select identity not implemented.");

                    Type t = typeof(TExpectedType);
                    if (t == typeof(int))
                    {
                        o = Convert.ToInt32((decimal)o);
                    }
                    else if (t == typeof(Int16))
                    {
                        o = Convert.ToInt16((decimal)o);
                    }
                    else if (t == typeof(Int64))
                    {
                        o = Convert.ToInt64((decimal)o);
                    }
                }
            }
            catch (OverflowException) { }

            try
            {
                return (TExpectedType)o;

            }
            catch (InvalidCastException) { }

            return default(TExpectedType);
        }

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional colums or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>An object representing a single datum</returns>
        public TExpectedType GetSingleValue<TExpectedType>(string command, params object[] parameters)
        {
            return GetSingleValue<TExpectedType>(command, false, parameters);
        }

        public Dictionary<string, object> GetDataRow(string command, params object[] parameters)
        {
            Dictionary<string, object> returnRow = new Dictionary<string, object>();
            // use internal GetReader method to fill a data row
            using (IDataReader reader = GetReader(command, parameters))
            {
                if (reader != null && reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object result = reader[i];
                        returnRow.Add(reader.GetName(i),
                            result is DBNull ? null : result);
                    }
                }
            }

            return returnRow;
        }

        protected IDataReader GetReader(string command, params object[] parameters)
        {
            try
            {
                LastError = null;
                CheckConnection();
                DbConnection conn = GetDbConnection();
                conn.Open();
                DbCommand cmd = GetDbCommand(command, conn);
                InitializeCommand(cmd, parameters);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception sqex)
            {
                if (sqex is OracleException)
                    LastError = sqex;
                else
                    throw;

                if (!CatchDbExceptions)
                    throw;
            }
            return null;
        }

        public DataTable GetDataTable(string command, params object[] parameters)
        {
            DataTable table = new DataTable();
            // use internal GetReader method to fill a data table
            using (IDataReader reader = GetReader(command, parameters))
            {
                if (reader != null)
                {
                    table.Load(reader);
                    table.Locale = CultureInfo.InvariantCulture;
                }
            }

            return table;
        }

        public Dictionary<int, object> GetDataColumn(string command, params object[] parameters)
        {
            return GetDataColumn(command, null, null, parameters);
        }

        public Dictionary<int, object> GetDataColumn(string command, string identityColumnName, int[] identityValues, params object[] parameters)
        {
            var returnColumn = new Dictionary<int, object>();

            // TODO: extract this re-usable portion in DalLibrary
            if (!string.IsNullOrEmpty(identityColumnName) && identityValues != null)
            {
                // build the SQL "WHERE IN (...)" clause
                if (command != null)
                {
                    StringBuilder matchCommand = new StringBuilder(command);
                    matchCommand.Append((command.ToUpperInvariant().Contains(" WHERE ") ?
                            " AND " : " WHERE ") + identityColumnName + " IN ( ");
                    foreach (int matchValue in identityValues)
                    {
                        matchCommand.AppendFormat("{0},", matchValue);
                    }
                    matchCommand.Append(" )");
                    command = matchCommand.ToString().Replace(", )", " )");
                }
            }

            int count = 1;
            using (IDataReader reader = GetReader(command, parameters))
            {
                while (reader != null && reader.Read())
                {
                    try
                    {
                        object result;
                        if (reader.FieldCount > 1)
                        {
                            result = reader[1];
                            returnColumn.Add(reader.GetInt32(0), result is DBNull ? null : result);
                        }
                        else
                        {
                            result = reader[0];
                            returnColumn.Add(count++, result is DBNull ? null : result);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is InvalidCastException || ex is IndexOutOfRangeException)
                        {
                            throw new ArgumentException("Data and/or query are not compatible with GetDataColumn method.", ex);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return returnColumn;
        }
    }
}

