namespace CoconutDal
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using CoconutDal.Base;
    using CoconutDal.Configuration;

    /// <summary>
    /// A Coconut Dal implementation that works with Sql Server and Sql Server Compact
    /// </summary>
    public class SqlServerCoconutDal : ICoconutDal
    {
        /// <summary>
        /// Specify the connection string
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// Gets the last caught exception. Only DbException and SqlCeException are caught.
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

        private SqlVariant _sqlDatabaseType;

        /// <summary>
        /// Creates a new instance of SqlServerCoconutDal, using the supplied connection string
        /// </summary>
        /// <param name="doNotUseAppConfig">Indicates that app config should be ignored</param>
        /// <param name="connectionString">A sql server connection string</param>
        /// <param name="sqlDatabaseType">The type of sql server instance</param>
        public SqlServerCoconutDal(ConfigurationBehaviour doNotUseAppConfig, string connectionString, SqlVariant sqlDatabaseType)
        {
            this.Connection = connectionString;
            this._sqlDatabaseType = sqlDatabaseType;
        }

        /// <summary>
        /// Creates a new instance of SqlServerCoconutDal, using the supplied connection string
        /// </summary>
        /// <param name="doNotUseAppConfig">Indicates that app config should be ignored</param>
        /// <param name="connectionString">A sql server connection string</param>
        public SqlServerCoconutDal(ConfigurationBehaviour doNotUseAppConfig, string connectionString)
        {
            this.Connection = connectionString;
            this._sqlDatabaseType = SqlVariant.SqlServer;
        }

        /// <summary>
        /// Creates a new instance of SqlServerCoconutDal, using the named connection from the CoconutDalConfigurationSection in app.config 
        /// </summary>
        /// <param name="connectionName">The name of a connection in CoconutDalConfigurationSection.Connections</param>
        public SqlServerCoconutDal(string connectionName)
        {
            this.Connection = CoconutDalConfigurationSection.GetEnvironmentConfig(connectionName).ConnectionString;
            this._sqlDatabaseType = SqlVariant.SqlServer;
        }

        /// <summary>
        /// Creates a new instance of SqlServerCoconutDal, using the active connection from the CoconutDalConfigurationSection in app.config
        /// </summary>
        public SqlServerCoconutDal()
        {
            this.Connection = CoconutDalConfigurationSection.GetEnvironmentConfig().ConnectionString;
 			this._sqlDatabaseType = SqlVariant.SqlServer;
        }

        /// <summary>
        /// Creates a new instance of SqlServerCoconutDal, using the named connection from the CoconutDalConfigurationSection in app.config 
        /// </summary>
        /// <param name="connectionName">The name of a connection in CoconutDalConfigurationSection.Connections</param>
        /// <param name="sqlDatabaseType">The type of sql server instance</param>
        public SqlServerCoconutDal(string connectionName, SqlVariant sqlDatabaseType)
        {
            this.Connection = CoconutDalConfigurationSection.GetEnvironmentConfig(connectionName).ConnectionString;            
            this._sqlDatabaseType = sqlDatabaseType;
        }
        /// <summary>
        /// Creates a new instance of SqlServerCoconutDal, using the active connection from the CoconutDalConfigurationSection in app.config
        /// </summary>
        /// <param name="sqlDatabaseType">The type of sql server instance</param>
        public SqlServerCoconutDal(SqlVariant sqlDatabaseType)
        {
            this.Connection = CoconutDalConfigurationSection.GetEnvironmentConfig().ConnectionString;
            this._sqlDatabaseType = sqlDatabaseType;
        }

        /// <summary>
        /// Execute a database command that returns no results
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>
        /// A boolean matchingElement indicating whether the command was successful
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")] // parameter command is validated
        public bool ExecuteNonQuery(string command, params object[] parameters)
        {
            this.LastError = null;
            this.CheckConnection();
            try
            {
                using (DbConnection conn = this.GetDbConnection())
                {
                    conn.ConnectionString = this.Connection;
                    conn.Open();
                    using (DbCommand cmd = this.GetDbCommand(command, conn))
                    {
                        this.InitializeCommand(cmd, parameters);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception sqex)
            {
                if(sqex is SqlCeException || sqex is DbException)
                    this.LastError = sqex;
                else
                    throw;

                if (!this.CatchDbExceptions)
                    throw;
                
            }
            return false;
        }

        /// <summary>
        /// Retrieve a single matchingElement from the database
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>
        /// An object representing a single datum
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")] // parameter command is validated
        public object GetSingleValue(string command, params object[] parameters)
        {
            return this.GetSingleValue(command, false, parameters);
        }

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="isNewIdentityValue">indicate whether to select a newly created identity value. Dal will attempt to modify the query to select the new id (if neccesary.)</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>
        /// An object representing a single datum
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")] // parameter command is validated
        public object GetSingleValue(string command, bool isNewIdentityValue, params object[] parameters)
        {
            this.CheckConnection();
            try
            {
                using (DbConnection conn = this.GetDbConnection())
                {
                    conn.Open();
                    using (DbCommand cmd = this.GetDbCommand(command, conn))
                    {
                        this.InitializeCommand(cmd, parameters);
                        // if user has indicated they want the newly created identity AND the query does not already contain a call to SCOPE_IDENTITY()...
                        if (isNewIdentityValue && !command.ToUpper().Contains("SCOPE_IDENTITY()"))
                        {
                            if (this._sqlDatabaseType != SqlVariant.SqlServerCompact)
                            {
                                //... add a call to select the new id (not supported by Sql Server CE)
                                cmd.CommandText += "; SELECT SCOPE_IDENTITY()";
                            }
                            else
                            {
                                //... for SqlServer CE we run the original query, then select @@identity as the next operation
                                cmd.ExecuteNonQuery();
                                cmd.CommandText = "SELECT @@IDENTITY";
                                cmd.CommandType = CommandType.Text;
                            }
                        }
                        object result = cmd.ExecuteScalar();
                        return result is DBNull ? null : result;
                    }
                }
            }
            catch (Exception sqex)
            {
                if (sqex is SqlCeException || sqex is DbException)
                    this.LastError = sqex;
                else
                    throw;

                if (!this.CatchDbExceptions)
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
            object o = this.GetSingleValue(command, isNewIdentityValue, parameters);

            try
            {
                // try to handle the fact that casting SCOPE_IDENTITY() and @@IDENTITY to int is a narrowing conversion
                if (isNewIdentityValue)
                {
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
            return this.GetSingleValue<TExpectedType>(command, false, parameters);
        }


        /// <summary>
        /// Retrieve a single row from the database. Additional rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>
        /// A dictionary representing a row of data
        /// </returns>
        public Dictionary<string, object> GetDataRow(string command, params object[] parameters)
        {
            Dictionary<string, object> returnRow = new Dictionary<string, object>();
            // use internal GetReader method to fill a data row
            using (IDataReader reader = this.GetReader(command, parameters))
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


        /// <summary>
        /// Retrieve a single column of data from the database.
        /// </summary>
        /// <param name="command">A query to retrieve a column of data with indices. To use a data column for indices, include the  column name as the first item in the select list.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A collection that represents a column of data. Database ids will be used for indexing if a suitable column name was included in the query. If not, rows are indexed starting at 1.
        /// </returns>
        public Dictionary<int, object> GetDataColumn(string command, params object[] parameters)
        {
            return this.GetDataColumn(command, null, null, parameters);
        }


        /// <summary>
        /// Retrieve a single column of data from the database. The data can be matched to query parameters in the normal way,
        /// or can be matched to a list of identitiy values.
        /// </summary>
        /// <param name="command">A query to retrieve a column of data. Include the identity column name as the first item in the select list, if those values are needed for indexing.</param>
        /// <param name="identityColumnName">Name of the identity column.</param>
        /// <param name="identityValues">The identity values.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A collection that represents a column of data. The supplied identity values are used for indexing, if these were included in the query. If not, rows are numbered starting at 1.
        /// </returns>        
        /// <remarks>
        /// For examples of usage see CoconutDal.ICoconutDal.GetDataColumn() 
        /// </remarks>
        public Dictionary<int, object> GetDataColumn(string command, string identityColumnName,
            int[] identityValues, params object[] parameters)
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
            using (IDataReader reader = this.GetReader(command, parameters))
            {
                while (reader != null && reader.Read())
                {
                    try
                    {
                        object result;
                        if (reader.FieldCount > 1)
                        {
                            result = reader[1];
                            returnColumn.Add(reader.GetInt32(0), result is DBNull ? null : result );
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

        /// <summary>
        /// Retrieves a DataTable that can be Disposed when finished with.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>A DataTable object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")] // most experts agree that DataTables do not need to be disposed
        public DataTable GetDataTable(string command, params object[] parameters)
        {
            DataTable table = new DataTable();
            // use internal GetReader method to fill a data table
            using (IDataReader reader = this.GetReader(command, parameters))
            {
                if (reader != null)
                {
                    table.Load(reader);
                    table.Locale = CultureInfo.InvariantCulture;
                }
            }

            return table;
        }

        /// <summary>
        /// This method returns an IDataReader that MUST BE CLOSED when finished with.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        // To return an IDataReader risks connection leaks, so this method is restricted to derived classes.
        // As long as the caller closes the returned reader, the underlying connection is also closed. 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")] // parameter command is validated
        protected IDataReader GetReader(string command, params object[] parameters)
        {
            try
            {
                this.LastError = null;
                this.CheckConnection();
                DbConnection conn = this.GetDbConnection();
                conn.Open();
                DbCommand cmd = this.GetDbCommand(command, conn);
                this.InitializeCommand(cmd, parameters);
                return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception sqex)
            {
                if (sqex is SqlCeException || sqex is DbException)
                    this.LastError = sqex;
                else
                    throw;

                if (!this.CatchDbExceptions)
                    throw;
            }
            return null;
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

            if (this._sqlDatabaseType == SqlVariant.SqlServerCompact && !(this.IsTextQuery | this.IsAlwaysTextQuery))
            {
                throw new ArgumentException("Sql Server Compact only supports Text Queries."); 
            }

            if (command.CommandText.Contains("'") || command.CommandText.Contains("--") || command.CommandText.Contains("/*") || command.CommandText.Contains("*/"))
            {
                throw new ArgumentException("Command is not safe. Text query cannot contain raw string input or comments.");
            }

            if (command.CommandText.Contains(";shutdown"))
            {
                throw new InvalidOperationException("Someone is trying to shutdown Sql Server.");
            }
            
            if (!this.IsTextQuery && !this.IsAlwaysTextQuery)
                command.CommandType = CommandType.StoredProcedure;
            AddParameters(command, parameters, this._sqlDatabaseType);
            this.IsTextQuery = false; // reset
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")] // parameter command is validated elsewhere and this method is private
        private DbConnection GetDbConnection()
        {
            if (this._sqlDatabaseType == SqlVariant.SqlServer)
                return new SqlConnection(this.Connection);
            else
                return new SqlCeConnection(this.Connection);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")] // parameter command is validated elsewhere and this method is private
        private DbCommand GetDbCommand(string command, DbConnection conn)
        {
            if (this._sqlDatabaseType == SqlVariant.SqlServer)
                return new SqlCommand(command, (SqlConnection)conn);
            else
                return new SqlCeCommand(command, (SqlCeConnection)conn);
        }

        private static DbParameter[] ConvertParameters(object[] input, SqlVariant databaseType)
        {
            DbParameter[] output = new DbParameter[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                SqlDalParameter p = (SqlDalParameter)input[i];

                if (databaseType == SqlVariant.SqlServerCompact)
                {
                    output[i] = p.ToSqlCeParameter();
                }
                else
                {
                    output[i] = p.ToSqlParameter();
                }
            }
            return output;
        }

        private static void AddParameters(DbCommand command, object[] parameters, SqlVariant databaseType)
        {

            if (parameters.Length < 1)
            {
                return;
            }
            // 3 valid cases: SP + Params, SP + Values, Text + Params
            if (parameters.Where(p => p is SqlDalParameter).Count() == parameters.Length)
            {
                //for SP + Params & Text + Params
                command.Parameters.AddRange(ConvertParameters(parameters, databaseType));
            }
            else if (command.CommandType == CommandType.StoredProcedure && databaseType!= SqlVariant.SqlServerCompact)
            {
                SqlCommand cmd = command as SqlCommand;
                if (cmd != null)
                {
                    SqlCommandBuilder.DeriveParameters(cmd);
                }
                
                int index = 0;
                foreach (SqlParameter parameter in command.Parameters)
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
                throw new ArgumentException("Could not add query parameters. For text queries, each parameter must be of type SqlDalParameter.");
            }
        }
    }
}
