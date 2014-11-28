using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    public class PostgreStore : IPersistanceStore
    {
        private readonly string _connectionString;

        public PostgreStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertAuditLog(AuditLog log)
        {
            throw new NotImplementedException();
        }
    }
}
