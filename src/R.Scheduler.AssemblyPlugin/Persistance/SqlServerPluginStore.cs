using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Persistance
{
    public class SqlServerPluginStore : IPluginStore
    {
        private readonly string _connectionString;

        public SqlServerPluginStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Plugin GetRegisteredPlugin(string pluginName)
        {
            Plugin retval = null;

            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id plugin_name, assembly_path, status FROM rsched_plugins WHERE plugin_name = @name;";
            var command = new SqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar));
                command.Parameters[0].Value = pluginName;

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    retval = new Plugin { Name = pluginName };

                    while (reader.Read())
                    {
                        retval.AssemblyPath = (string)reader["assembly_path"];
                        retval.Status = (string)reader["status"];
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return retval;
        }

        public IList<Plugin> GetRegisteredPlugins()
        {
            IList<Plugin> retval = new List<Plugin>();

            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id, plugin_name, assembly_path, status FROM rsched_plugins";
            var command = new SqlCommand(sql, conn);

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval.Add(new Plugin
                        {
                            Id = (Guid)reader["id"],
                            Name = (string)reader["plugin_name"],
                            AssemblyPath = (string)reader["assembly_path"],
                            Status = (string)reader["status"]
                        });
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return retval;
        }

        public void RegisterPlugin(Plugin plugin)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sqlInsert = @"INSERT INTO rsched_plugins(id, plugin_name, assembly_path, status) VALUES (@id, @name, @assemblyPath, @status);";
            var sqlUpdate = @"UPDATE rsched_plugins SET assembly_path='@assemblyPath', status='@status' WHERE plugin_name='@name'";
            var command = new SqlCommand(sqlUpdate, conn);

            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.AddWithValue("@id", Guid.NewGuid());
                        command.Parameters.AddWithValue("@name", plugin.Name);
                        command.Parameters.AddWithValue("@assemblyPath", plugin.AssemblyPath);
                        command.Parameters.AddWithValue("@status", "registered");

                        command.Transaction = transaction;

                        int rowsAffected = command.ExecuteNonQuery();

                        if (0 == rowsAffected)
                        {
                            command.CommandText = sqlInsert;
                            command.ExecuteScalar();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public int RemovePlugin(string pluginName)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"DELETE FROM rsched_plugins WHERE plugin_name=@name";
            var command = new SqlCommand(sql, conn);
            command.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar));
            command.Parameters[0].Value = pluginName;

            try
            {
                return command.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public int RemoveAllPlugins()
        {
            throw new NotImplementedException();
        }

        public PluginDetails GetRegisteredPluginDetails(string pluginName)
        {
            throw new NotImplementedException();
        }

    }
}
