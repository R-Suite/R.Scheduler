using System.Data;
using System.Data.SQLite;
using Moq;
using Quartz;
using Quartz.Impl;
using R.Scheduler.Sql;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class SqlJobTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();
        private const string ConnectionString = "Data Source=MyDatabase.sqlite;Version=3;";

        public SqlJobTests()
        {
            SQLiteConnection.CreateFile("MyDatabase.sqlite");
        }

        [Fact]
        public void TestSqlStatementInsertsNewRowIntoExistingTable()
        {
            using (var dbConnection = new SQLiteConnection(ConnectionString))
            {
                // Arrange
                var pluginRunner = new SqlJob();
                dbConnection.Open();
                const string sql = "create table names (name varchar(20))";
                var command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();

                IJobDetail jobDetail = new JobDetailImpl("TestJob1", typeof(IJob));
                jobDetail.JobDataMap.Add("connectionString", ConnectionString);
                jobDetail.JobDataMap.Add("providerAssemblyName", @"System.Data.SQLite");
                jobDetail.JobDataMap.Add("connectionClass", @"System.Data.SQLite.SQLiteConnection");
                jobDetail.JobDataMap.Add("commandClass", @"System.Data.SQLite.SQLiteCommand");
                jobDetail.JobDataMap.Add("dataAdapterClass", @"System.Data.SQLite.SQLiteDataAdapter");
                jobDetail.JobDataMap.Add("nonQueryCommand", @"insert into names (name) values ('Me')");
                _mockJobExecutionContext.SetupGet(p => p.MergedJobDataMap).Returns(jobDetail.JobDataMap);
                _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

                // Act
                pluginRunner.Execute(_mockJobExecutionContext.Object);

                // Assert
                const string resultSql = "select * from names";
                var resultDa = new SQLiteDataAdapter(resultSql, dbConnection);
                var ds = new DataSet();
                resultDa.Fill(ds, "Results");

                Assert.Equal(1, ds.Tables.Count);
                Assert.Equal("Me", ds.Tables[0].Rows[0][0]);
            }
        }
    }
}
