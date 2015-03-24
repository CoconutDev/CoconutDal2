namespace CoconutDal
{
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;

    /// <summary>
    ///     Represents a parameter to a DbCommand for Sql Server and Sql Server Compact Edition databases. Optionally
    ///     includes the parameter's mapping to System.Data.DataSet columns. This class cannot be inherited.
    /// </summary>    
    public sealed class SqlDalParameter : DbParameter
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDalParameter"/> class.
        /// </summary>
        public SqlDalParameter() 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the SqlDalParameter class
        ///     that uses the parameter name and a value of the new SqlDalParameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public SqlDalParameter(string name, object value)
        {
            this.parameter = new SqlParameter(name, value);        
        }
        /// <summary>
        /// Initializes a new instance of the SqlDalParameter class
        ///     that uses the parameter name and the data type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        public SqlDalParameter(string name, SqlDbType dataType) 
        {
            this.parameter = new SqlParameter(name, dataType);
        }

        /// <summary>
        /// Initializes a new instance of the SqlDalParameter class
        ///     that uses the parameter name, the System.Data.SqlDbType, and the size.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="size"></param>
        public SqlDalParameter(string name, SqlDbType dataType, int size) 
        {

            this.parameter = new SqlParameter(name, dataType, size);
        }

        /// <summary>
        /// Initializes a new instance of the SqlDalParameter class
        ///     that uses the parameter name, the System.Data.SqlDbType, the size, and the
        ///     source column name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="size"></param>
        /// <param name="sourceColumn"></param>
        public SqlDalParameter(string name, SqlDbType dataType, int size, string sourceColumn) 
        {
            this.parameter = new SqlParameter(name, dataType, size, sourceColumn);
        }

        /// <summary>
        /// Initializes a new instance of the System.Data.SqlClient.SqlParameter class
        ///     that uses the parameter name, the type of the parameter, the size of the
        ///     parameter, the precision of the parameter,
        ///     the scale of the parameter, the source column, a System.Data.DataRowVersion
        ///     to use, and the value of the parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="isNullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="value"></param>
        public SqlDalParameter(string parameterName, SqlDbType dbType, int size, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
        {
            this.parameter = new SqlParameter(parameterName, dbType, size, ParameterDirection.Input, isNullable, precision, scale, sourceColumn, sourceVersion, value);
        }

        /// <summary>
        /// Initializes a new instance of the System.Data.SqlClient.SqlParameter class
        ///     that uses the parameter name, the type of the parameter, the size of the
        ///     parameter, a System.Data.ParameterDirection, the precision of the parameter,
        ///     the scale of the parameter, the source column, a System.Data.DataRowVersion
        ///     to use, and the value of the parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <param name="isNullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="value"></param>
        public SqlDalParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
        {
            this.parameter = new SqlParameter(parameterName, dbType, size, direction, isNullable, precision, scale, sourceColumn, sourceVersion, value);
        }

        /// <summary>
        ///    Initializes a new instance of the System.Data.SqlClient.SqlParameter class
        ///     that uses the parameter name, the type of the parameter, the length of the
        ///     parameter the direction, the precision, the scale, the name of the source
        ///     column, one of the System.Data.DataRowVersion values, a Boolean for source
        ///     column mapping, the value of the SqlParameter, the name of the database where
        ///     the schema collection for this XML instance is located, the owning relational
        ///     schema where the schema collection for this XML instance is located, and
        ///     the name of the schema collection for this parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="sourceColumnNullMapping"></param>
        /// <param name="value"></param>
        /// <param name="xmlSchemaCollectionDatabase"></param>
        /// <param name="xmlSchemaCollectionOwningSchema"></param>
        /// <param name="xmlSchemaCollectionName"></param>
        public SqlDalParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value, string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema, string xmlSchemaCollectionName)
        {
            this.parameter = new SqlParameter(parameterName, dbType, size, direction, precision, scale, sourceColumn, sourceVersion, 
                sourceColumnNullMapping,value, xmlSchemaCollectionDatabase, xmlSchemaCollectionOwningSchema, xmlSchemaCollectionName);       
        }

        private SqlParameter parameter;


        /// <summary>
        /// Converts the SqlDalParameter to a Sql Server-specific parameter (SqlParameter).
        /// </summary>
        /// <returns></returns>
        public SqlParameter ToSqlParameter()
        {
            return this.parameter;
        }

        /// <summary>
        /// Converts the SqlDalParameter to a Sql Server Compact-specific parameter (SqlCeParameter).
        /// </summary>
        /// <returns></returns>
        public SqlCeParameter ToSqlCeParameter()
        {
            return new SqlCeParameter(this.parameter.ParameterName, this.parameter.SqlDbType, this.parameter.Size, this.parameter.Direction, this.parameter.IsNullable, this.parameter.Precision, this.parameter.Scale, this.parameter.SourceColumn, this.parameter.SourceVersion, this.parameter.Value);
        }

        /// <summary>
        /// Gets or sets the System.Data.SqlDbType of the parameter.
        /// </summary>
        public override DbType DbType
        {
            get
            {
                return this.parameter.DbType;
            }
            set
            {
                this.parameter.DbType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter is input-only,
        ///     output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        public override ParameterDirection Direction
        {
            get
            {
                return this.parameter.Direction;
            }
            set
            {
                this.parameter.Direction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter accepts null values.
        /// </summary>
        public override bool IsNullable
        {
            get
            {
                return this.parameter.IsNullable;
            }
            set
            {
                this.parameter.IsNullable = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the SqlDalParameter.
        /// </summary>
        public override string ParameterName
        {
            get
            {
                return this.parameter.ParameterName;
            }
            set
            {
                this.parameter.ParameterName = value;
            }
        }

        /// <summary>
        /// Resets the type associated with this System.Data.SqlClient.SqlParameter.
        /// </summary>
        public override void ResetDbType()
        {
            this.parameter.ResetDbType();
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        public override int Size
        {
            get
            {
                return this.parameter.Size;
            }
            set
            {
                this.parameter.Size = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the source column mapped to the System.Data.DataSet
        ///     and used for loading or returning the System.Data.SqlClient.SqlParameter.Value
        /// </summary>
        public override string SourceColumn
        {
            get
            {
                return this.parameter.SourceColumn;
            }
            set
            {
                this.parameter.SourceColumn = value;
            }
        }
        /// <summary>
        /// Gets or sets a value which indicates whether the source column is nullable.
        ///     This allows System.Data.SqlClient.SqlCommandBuilder to correctly generate
        ///     Update statements for nullable columns.
        /// </summary>
        public override bool SourceColumnNullMapping
        {
            get
            {
                return this.parameter.SourceColumnNullMapping;
            }
            set
            {
                this.parameter.SourceColumnNullMapping = value;
            }
        }

        /// <summary>
        /// Gets or sets the System.Data.DataRowVersion to use when you load System.Data.SqlClient.SqlParameter.Value
        /// </summary>
        public override DataRowVersion SourceVersion
        {
            get
            {
                return this.parameter.SourceVersion;
            }
            set
            {
                this.parameter.SourceVersion = value;
            }
        }
        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        public override object Value
        {
            get
            {
                return this.parameter.Value;
            }
            set
            {
                this.parameter.Value = value;
            }
        }
    }
}
