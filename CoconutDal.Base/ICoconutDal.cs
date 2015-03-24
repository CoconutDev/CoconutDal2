namespace CoconutDal.Base
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Common interface for all Coconut Dal implementations.
    /// </summary>
    public interface ICoconutDal
    {
        /// <summary>
        /// Specify the connection string
        /// </summary>
        string Connection { get; set; }
        /// <summary>
        /// Gets the last caught exception. Only DbException and other data exceptions should be caught.
        /// </summary>
        Exception LastError { get; }

        /// <summary>
        /// If true, data exceptions (including type DbException) will be trapped by the dal and stored in LastError.
        /// </summary>
        bool CatchDbExceptions {get;set;}

        /// <summary>
        /// Execute a database command that returns no results
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>A boolean matchingElement indicating whether the command was successful</returns>
        bool ExecuteNonQuery(string command, params object[] parameters);

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>An object representing a single datum</returns>
        object GetSingleValue(string command, params object[] parameters);

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="isNewIdentityValue">Indicates whether to select a newly created identity value. Dal will attempt to modify the query to select the new id (if neccesary)</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>
        /// An object representing a single datum
        /// </returns>
        object GetSingleValue(string command, bool isNewIdentityValue, params object[] parameters);

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional colums or rows are ignored.
        /// </summary>
        /// <typeparam name="TExpectedType">The expected type of the result</typeparam>
        /// <param name="command">The query</param>
        /// <param name="isNewIdentityValue">Indicates whether to select a newly created identity value. Dal will attempt to modify the query to select the new id (if neccesary)</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>An object representing a single datum</returns>
        TExpectedType GetSingleValue<TExpectedType>(string command, bool isNewIdentityValue, params object[] parameters);

        /// <summary>
        /// Retrieves the first column of the first row returned by a query. Additional colums or rows are ignored.
        /// </summary>
        /// <typeparam name="TExpectedType">The expected type of the result</typeparam>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>An object representing a single datum</returns>
        TExpectedType GetSingleValue<TExpectedType>(string command, params object[] parameters);


        /// <summary>
        /// Retrieve a single row from the database
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>A dictionary representing a row of data</returns>
        Dictionary<string, object> GetDataRow(string command, params object[] parameters);

        /// <summary>
        /// Retrieves a table of data
        /// </summary>
        /// <param name="command">The query</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>A DataTable</returns>
        DataTable GetDataTable(string command, params object[] parameters);

        /// <summary>
        /// Retrieve a single column of data from the database. The data can be matched to query parameters in the normal way,
        /// or can be matched to a list of identitiy values.
        /// </summary>
        /// <param name="command">A query to retrieve a column of data. Include the identity column name as the first item in the select list, if those values are needed for indexing.</param>
        /// <param name="identityColumnName">Name of the identity column.</param>
        /// <param name="identityValues">The identity values.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A collection that represents a column of data. The supplied identity values are used for indexing, if these were included in the query. If not, rows are numbered starting at 1.</returns>
        /// <remarks>
        /// Example
        /// Assuming you wish to retrieve a column of data that matches any of these ints: 7,8 or 9 and you want those ints to be used to index the data:
        /// <code>
        /// GetDataColumn("SELECT itemId, itemName FROM contentTable WHERE type='@type'", "itemId", 
        ///         new int[] { 7,8,9 },
        ///         new object[]{ new SqlParameter{ ParameterName ="@type", Value = "fruit"}});
        /// </code>
        /// The call is translated to the following sql:
        /// <code>
        /// SELECT itemId, itemName FROM contentTable WHERE type='@type' AND itemID in (7,8,9)
        /// </code>
        /// and these data are returned:
        /// <list type="table">
        /// <listheader>
        /// <term>index</term>
        /// <description>itemName</description>
        /// </listheader>
        /// <item><term>7</term>
        /// <description>Apples</description></item>
        /// <item><term>8</term>
        /// <description>Oranges</description></item>
        /// <item><term>9</term>
        /// <description>Bananas</description></item>
        /// </list>
        /// If the indices are not required, remove itemId from the SELECT statement, and the following data are returned:
        /// <list type="table">
        /// <listheader>
        /// <term>index</term>
        /// <description>itemName</description>
        /// </listheader>
        /// <item><term>1</term>
        /// <description>Apples</description></item>
        /// <item><term>2</term>
        /// <description>Oranges</description></item>
        /// <item><term>3</term>
        /// <description>Bananas</description></item>
        /// </list>
        ///         
        /// </remarks>
        Dictionary<int, object> GetDataColumn(string command, string identityColumnName,
            int[] identityValues, params object[] parameters);

        /// <summary>
        /// Retrieve a single column of data from the database.
        /// </summary>
        /// <param name="command">A query to retrieve a column of data with indices. To use a data column for indices, include the  column name as the first item in the select list.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A collection that represents a column of data. Database ids will be used for indexing if a suitable column name was included in the query. If not, rows are indexed starting at 1.
        /// </returns>
        Dictionary<int, object> GetDataColumn(string command, params object[] parameters);

        /// <summary>
        /// When true, specifies that the current command is a text query, not a stored procedure. Reset to false after each command.
        /// </summary>
        bool IsTextQuery { get; set; }

        /// <summary>
        /// When true, specifies that all commands are text queries, not a stored procedure.
        /// </summary>
        bool IsAlwaysTextQuery { get; set; }

    }
}
