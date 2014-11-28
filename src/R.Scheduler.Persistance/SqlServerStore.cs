using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    public class SqlServerStore : IPersistanceStore
    {
        private readonly string _connectionString;

        public SqlServerStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertAuditLog(AuditLog log)
        {
            throw new NotImplementedException();
        }
    }
}
