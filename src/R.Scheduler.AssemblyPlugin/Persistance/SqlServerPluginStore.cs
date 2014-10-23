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

            var sql = @"SELECT id, plugin_name, assembly_path FROM rsched_plugins WHERE plugin_name = @name;";

            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var command = new SqlCommand(sql, conn);
            command.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar));
            command.Parameters[0].Value = pluginName;

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval = new Plugin
                        {
                            Id = (Guid)reader["id"],
                            Name = (string)reader["plugin_name"],
                            AssemblyPath = (string)reader["assembly_path"],
                        };
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return retval;
        }

        public Plugin GetRegisteredPlugin(Guid id)
        {
            Plugin retval = null;

            var sql = @"SELECT [id], [plugin_name], [assembly_path] FROM rsched_plugins WHERE [id] = @id;";

            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@id", id);

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval = new Plugin
                        {
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["plugin_name"],
                            AssemblyPath = (string)reader["assembly_path"],
                        };
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

            var sql = @"SELECT id, plugin_name, assembly_path FROM rsched_plugins";
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
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["plugin_name"],
                            AssemblyPath = (string)reader["assembly_path"],
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

            var sqlInsert = @"INSERT INTO rsched_plugins(id, plugin_name, assembly_path) VALUES (@id, @name, @assemblyPath);";
            var sqlUpdate = @"UPDATE rsched_plugins SET assembly_path='@assemblyPath' WHERE plugin_name='@name'";
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

        public void UpdatePluginName(Guid id, string name)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sqlUpdate = @"UPDATE rsched_plugins SET plugin_name=@name WHERE id=@id";
            var command = new SqlCommand(sqlUpdate, conn);

            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);

                        command.Transaction = transaction;

                        command.ExecuteNonQuery();

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
