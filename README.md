# CoconutDal2
This is the new home of CoconutDal, as seen on codeplex (https://coconutdal.codeplex.com/)

Version 2 will add support for popular RDBMSs such as MySql and Oracle.

---

Coconut Dal is a lightweight data access layer, for use in projects where Entity Framework cannot be used or Microsoft's Enterprise Library Data Block is unsuitable. Anyone who is handwriting ADO.NET should use a library instead and Coconut Dal might be the answer. In short:

Handwritten ADO.NET code < Coconut Dal < Enterprise Library Data Block < Entity Framework

If you are using an existing legacy database that does not map easily to entities, you can simplify your data access code by using Coconut Dal to run ad hoc queries and stored procedures.  

Coconut Dal:

 - manages connections
 - catches and stores exceptions
 - protects against sql injection
 - supports both stored procedures and parametrized SQL
 - supports Sql Server and Sql Server Compact (good for unit testing)

Write:
```
ICoconutDal dal = new SqlServerCoconutDal();
dal.CatchDbExceptions = true;
int result = dal.GetSingleValue<int>("MyStoredProc", foo, "bar");
// do something with dal.LastError if not null 
```
Instead of:
```
int result;
try
{
    using (SqlConnection conn = new SqlConnection("connectionstring"))
    {
        conn.Open();
        using (SqlCommand cmd = new SqlCommand("MyStoredProc", conn))
        {
            cmd.Parameters.Add(new SqlParameter("@p1", foo ));
            cmd.Parameters.Add(new SqlParameter("@p2", "bar" ));
            result = int.Parse(cmd.ExecuteScalar());
        }
    }
}
catch (SqlException sqex)
{
     // do something
}
catch (Exception ex)
{
    throw;
}
```
