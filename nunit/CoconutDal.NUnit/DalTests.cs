using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlServerCe;
using System.Data.SqlClient;
using System.Transactions;
using CoconutDal;
using CoconutDal.Configuration;
using System.Data.Common;

namespace CoconutDal.NUnit
{
    using CoconutDal.Base;

    [TestFixture]
    public class DalTests
    {
        #region setupteardown

        private static string _sqlCompactConnectionString =
            "Data Source = 'Test.sdf'; LCID=1033; Password=p667158767dae4320849c31eefad13a06;";

        static ICoconutDal dal = new SqlServerCoconutDal(SqlVariant.SqlServerCompact);

        static string guaranteedResultStringHolmes = "SELECT LastName FROM Person WHERE Id = 1";

        bool supportsTransactions = false;

        [TestFixtureSetUp]
        public void SetUp()
        {
            RipItUp();
            SqlCeEngine engine = new SqlCeEngine(_sqlCompactConnectionString);
            engine.CreateDatabase();

            SqlCeConnection conn = new SqlCeConnection(_sqlCompactConnectionString);
            conn.Open();
            string query = "CREATE TABLE Person(Id int, LastName nvarchar(255),FirstName nvarchar(255))";

            SqlCeCommand cmd = new SqlCeCommand(query, conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.ExecuteNonQuery();

            query = "INSERT INTO Person(Id, LastName, FirstName) VALUES(1, 'Holmes', 'Sherlock')";
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            query = "INSERT INTO Person(Id, LastName, FirstName) VALUES(2, 'Watson', 'Dr')";
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            query = "INSERT INTO Person(Id, LastName, FirstName) VALUES(3, 'Holmes', 'Mycroft')";
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            query = "INSERT INTO Person(Id, LastName, FirstName) VALUES(4, 'Moriarty', 'James')";
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();

            query = "CREATE TABLE Cases(Id int IDENTITY, CaseName nvarchar(255))";
            cmd.CommandText = query; ;
            cmd.ExecuteNonQuery();

            conn.Close();

        }

        [TestFixtureTearDown]
        public void RipItUp()
        {
            try
            {
                System.IO.File.Delete("Test.sdf");
            }
            catch { }
        }

        #endregion

        [Test]
        public void TestGetSingleValue()
        {            
            dal.IsTextQuery = true;
            object r = dal.GetSingleValue(guaranteedResultStringHolmes);
            Assert.AreEqual(r.ToString(),"Holmes");
        }

        [Test]
        public void TestGetSingleValueNumeric()
        {
            dal.IsTextQuery = true;
            object r = dal.GetSingleValue("SELECT 1");
            Assert.AreEqual(r.ToString(), "1");
        }

        [Test]
        public void TestGetSingleValueGeneric()
        {
            dal.IsTextQuery = true;
            string r = dal.GetSingleValue<string>(guaranteedResultStringHolmes);
            Assert.AreEqual(r, "Holmes");
        }

        [Test]
        public void TestGetSingleValueGenericInvalidCast()
        {
            dal.IsTextQuery = true;
            StringBuilder r = dal.GetSingleValue<StringBuilder>(guaranteedResultStringHolmes);
            Assert.IsNull(r);
        }

        [Test]
        public void TestGetSingleValueGenericWithDateTime()
        {
            dal.IsTextQuery = true;
            DateTime r = dal.GetSingleValue<DateTime>("SELECT getdate()");
            Assert.AreNotEqual(r, DateTime.MinValue);
        }

        [Test]
        public void TestGetSingleValueGenericWithInt()
        {
            dal.IsTextQuery = true;
            int r = dal.GetSingleValue<int>("SELECT 1");
            Assert.AreEqual(r, 1);
        }

        [Test]
        public void TestGetColumnOfDataSimple()
        {
            dal.IsTextQuery = true;
            SqlDalParameter[] para = new[] { new SqlDalParameter("@LastName", "Holmes") };
            var result = dal.GetDataColumn("SELECT FirstName FROM Person WHERE LastName = @LastName", para);
            Assert.IsTrue(result.Count == 2);
        }

        [Test]
        public void TestGetColumnOfDataUsingIdentityValues()
        {
            dal.IsTextQuery = true;
            SqlDalParameter[] para = new[] { new SqlDalParameter("@LastName", "Holmes") };
            var result = dal.GetDataColumn("SELECT Id, FirstName FROM Person WHERE LastName = @LastName", 
                "Id", new[] { 1, 3 }, para);
            Assert.IsTrue(result.Count == 2);
        }

        [Test]
        public void TestGetRowOfData()
        {
            dal.IsTextQuery = true;
            SqlDalParameter[] para = new[] { new SqlDalParameter("@LastName", "Holmes") };
            var result = dal.GetDataRow("SELECT Id, FirstName, LastName FROM Person WHERE LastName = @LastName", para);
            Assert.IsTrue(result.Count == 3);
        }

        [Test]
        public void TestExecuteNonQuery()
        {
            dal.IsTextQuery = true;
            SqlDalParameter[] para = new[] { 
                new SqlDalParameter("@LastName", "Watson"), 
                new SqlDalParameter("@NewFirstName", "John")
            };
            var result = dal.ExecuteNonQuery("Update Person SET FirstName = @NewFirstName WHERE LastName = @LastName", para);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestGetDataTable()
        {
            dal.IsTextQuery = true;
            var result = dal.GetDataTable("SELECT Id, FirstName, LastName FROM Person");
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count > 1);
        }


        [Test]
        public void TestDalRejectsVarChar()
        {
            dal.IsTextQuery = true;
            Assert.Throws(typeof(ArgumentException), 
                ()=> dal.GetSingleValue("SELECT LastName FROM Person WHERE FirstName = 'Sherlock'"));
        }

        [Test]
        public void TestDalRejectsTextQueryAndParamsArray()
        {
            dal.IsTextQuery = true;
            Assert.Throws(typeof(ArgumentException),
                () => dal.GetSingleValue("SELECT LastName FROM Person WHERE FirstName = @FirstName", "Sherlock"),
                "Could not add query parameters. For text queries, each parameter must be of type SqlParameter.");
        }


        [Test]
        public void TestDalAcceptsParameter()
        {
           dal.IsTextQuery = true;
           SqlDalParameter[] para = new[] { new SqlDalParameter("@FirstName", "Sherlock") };
           object r = dal.GetSingleValue("SELECT LastName FROM Person WHERE FirstName = @FirstName", para);
           Assert.AreEqual(r.ToString(), "Holmes");
        }

        [Test]
        public void TestDalRejectsShutdown()
        {
            dal.IsTextQuery = true;
            Assert.Throws(typeof(InvalidOperationException),
                () => dal.GetSingleValue(guaranteedResultStringHolmes + ";shutdown"));
        }


        [Test]
        public void TestDelete()
        {
            dal.IsAlwaysTextQuery = true;
            object r = dal.GetSingleValue("SELECT LastName FROM Person WHERE Id = 2");
            Assert.IsNotNull(r);

            dal.ExecuteNonQuery("DELETE FROM Person WHERE id = 2");

            r = dal.GetSingleValue("SELECT LastName FROM Person WHERE Id = 2");
            Assert.IsNull(r);
        }

        [Test]
        public void TestTransactionScopeRollback()
        {
            if (!supportsTransactions)
            {
                Assert.Inconclusive("Transactions are not supported by this database.");
                return;
            }

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    dal.IsTextQuery = true;
                    dal.ExecuteNonQuery("DELETE FROM Person WHERE id = 1");
                    throw new Exception("Noooooo!");
                }
            }
            catch { }

            dal.IsTextQuery = true;
            object r = dal.GetSingleValue(guaranteedResultStringHolmes);
            Assert.AreEqual(r.ToString(), "Holmes");
        }

        [Test]
        public void TestTransactionScopeCommit()
        {
            if (!supportsTransactions)
            {
                Assert.Inconclusive("Transactions are not supported by this database.");
                return;
            }

            dal.IsAlwaysTextQuery = true;

            using (TransactionScope transaction = new TransactionScope())
            {
                SqlDalParameter[] para = new[] 
                { 
                    new SqlDalParameter("@FirstName", "Irene"),
                    new SqlDalParameter("@LastName", "Adler")
                };
                dal.ExecuteNonQuery("INSERT INTO Person(Id, FirstName, LastName) VALUES(999, @FirstName, @LastName)", para);
                transaction.Complete();
            }

            object r = dal.GetSingleValue("SELECT LastName FROM Person WHERE id = 999");
            Assert.AreEqual(r.ToString(), "Adler");

            dal.IsAlwaysTextQuery = false;
        }


        [Test]
        public void TestDualWieldDalsInTransactionScope()
        {
            if (!supportsTransactions)
            {
                Assert.Inconclusive("Transactions are not supported by this database.");
                return;
            }

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    dal.IsTextQuery = true;
                    dal.ExecuteNonQuery("DELETE FROM Person WHERE id = 1");

                    ICoconutDal doubleDal = new SqlServerCoconutDal(SqlVariant.SqlServerCompact);
                    doubleDal.IsTextQuery = true;
                    doubleDal.ExecuteNonQuery("DELETE FROM Person WHERE id = 4");

                    throw new Exception("Noooooo!");
                }
            }
            catch { }

            dal.IsTextQuery = true;
            object r = dal.GetSingleValue(guaranteedResultStringHolmes);
            Assert.AreEqual(r.ToString(), "Holmes");
            dal.IsTextQuery = true;
            r = dal.GetSingleValue("SELECT LastName FROM Person WHERE id = 4");
            Assert.AreEqual(r.ToString(), "Moriarty");

        }

        [Test]
        public void TestIsTextQueryResets()
        {
            dal.IsTextQuery = true;
            object r = dal.GetSingleValue(guaranteedResultStringHolmes);
            Assert.IsFalse(dal.IsTextQuery);
        }

        [Test]
        public void TestIsAlwaysTextQueryDoesnotReset()
        {
            dal.IsAlwaysTextQuery = true;
            object r = dal.GetSingleValue(guaranteedResultStringHolmes);
            r = dal.GetSingleValue(guaranteedResultStringHolmes);
            Assert.IsNotNull(r);
            Assert.IsTrue(dal.IsAlwaysTextQuery);
            dal.IsAlwaysTextQuery = false;
        }

        [Test]
        public void TestIsTextQueryAndIsAlwaysTextQueryCoexistPeacefully()
        {
            dal.IsAlwaysTextQuery = true;
            dal.IsTextQuery = true;
            object r = dal.GetSingleValue(guaranteedResultStringHolmes); // both true
            dal.IsTextQuery = false;
            r = dal.GetSingleValue(guaranteedResultStringHolmes); // one true, one false
            Assert.IsNotNull(r);

            dal.IsAlwaysTextQuery = false;
            dal.IsTextQuery = true; 
            r = dal.GetSingleValue(guaranteedResultStringHolmes); // one false, one true
            Assert.IsNotNull(r);

            dal.IsAlwaysTextQuery = false;
            dal.IsTextQuery = false;
            Assert.Throws(typeof(ArgumentException),
                () => dal.GetSingleValue(guaranteedResultStringHolmes)); // both false
            
        }

        [Test]
        public void TestDalWithNoConnectionString()
        {
            ICoconutDal newDal = new SqlServerCoconutDal(ConfigurationBehaviour.DoNotUseAppConfig, "");
            newDal.IsTextQuery = true;
            Assert.Throws(typeof(ArgumentException),
                () => newDal.GetSingleValue(guaranteedResultStringHolmes), 
                "Connection String not specified.");
        }

        [Test]
        public void TestDbExceptionNotCaughtInLastError()
        {
            dal.IsTextQuery = true;
            dal.CatchDbExceptions = false;
            string badQuery = "SELECT nonsense FROM rubbish";
            Assert.Throws(typeof(SqlCeException),() => dal.GetSingleValue(badQuery));
            Assert.IsNotNull(dal.LastError);
        }

        [Test]
        public void TestDbExceptionCaughtInLastErrorWhenCatchDbExceptionsEnabled()
        {
            dal.IsTextQuery = true;
            dal.CatchDbExceptions = true;
            string badQuery = "SELECT nonsense FROM rubbish";
            Assert.DoesNotThrow(() => dal.GetSingleValue(badQuery));
            Assert.IsNotNull(dal.LastError);
        }


        [Test]
        public void TestIdentityFeature()
        {

            dal.IsTextQuery = true;
            object newid = dal.GetSingleValue("INSERT INTO Cases (CaseName) VALUES (@Case)", true,
                new SqlDalParameter("@Case", "A Study in Scarlet"));

            Assert.NotNull(newid);
            int n = int.Parse(newid.ToString());
            Assert.Greater(n, 0);

        }

        [Test]
        public void TestIdentityFeatureGenericInt16()
        {
            dal.IsTextQuery = true;
            Int16 newid = dal.GetSingleValue<Int16>("INSERT INTO Cases (CaseName) VALUES (@Case)", true,
            new SqlDalParameter("@Case", "The Sign of the Four"));
            Assert.Greater(newid, 0);
        }

        [Test]
        public void TestIdentityFeatureGenericInt32()
        {
            dal.IsTextQuery = true;
            Int32 newid = dal.GetSingleValue<Int32>("INSERT INTO Cases (CaseName) VALUES (@Case)", true,
            new SqlDalParameter("@Case", "The Hound of the Baskervilles"));
            Assert.Greater(newid, 0);
        }

        [Test]
        public void TestIdentityFeatureGenericInt64()
        {
            dal.IsTextQuery = true;
            Int64 newid = dal.GetSingleValue<Int64>("INSERT INTO Cases (CaseName) VALUES (@Case)", true,
            new SqlDalParameter("@Case", "The Valley of Fear"));
            Assert.Greater(newid, 0);
        }



    }
}