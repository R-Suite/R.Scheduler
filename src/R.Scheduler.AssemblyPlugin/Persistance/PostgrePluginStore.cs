using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Persistance
{
    /// <summary>
    /// Postgre implementation of IPluginStore
    /// </summary>
    public class PostgrePluginStore : IPluginStore
    {
        private readonly string _connectionString;

        public PostgrePluginStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Get registered plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public Plugin GetRegisteredPlugin(string pluginName)
        {
            Plugin retval = null;

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id plugin_name, assembly_path FROM rsched_plugins WHERE plugin_name = :name;";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
                command.Parameters[0].Value = pluginName;

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval = new Plugin
                        {
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["plugin_name"],
                            AssemblyPath = (string)reader["assembly_path"]
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

        /// <summary>
        /// Get registered plugin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Plugin GetRegisteredPlugin(Guid id)
        {
            Plugin retval = null;

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id plugin_name, assembly_path FROM rsched_plugins WHERE id = :id;";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                command.Parameters[0].Value = id;

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval = new Plugin
                        {
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["plugin_name"],
                            AssemblyPath = (string)reader["assembly_path"]
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
        /// <summary>
        /// Get all registered plugins
        /// </summary>
        /// <returns></returns>
        public IList<Plugin> GetRegisteredPlugins()
        {
            IList<Plugin> retval = new List<Plugin>();

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id, plugin_name, assembly_path FROM rsched_plugins";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval.Add(new Plugin
                        {
                            Id = (Guid) reader["id"] ,
                            Name = (string) reader["plugin_name"],
                            AssemblyPath = (string) reader["assembly_path"]
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

        /// <summary>
        /// Register new plugin, or update existing one.
        /// </summary>
        /// <param name="plugin"></param>
        public void RegisterPlugin(Plugin plugin)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sqlInsert = @"INSERT INTO rsched_plugins(id, plugin_name, assembly_path) VALUES (:id, :name, :assemblyPath);";
            var sqlUpdate = @"UPDATE rsched_plugins SET assembly_path=:assemblyPath WHERE plugin_name=:name";
            var command = new NpgsqlCommand(sqlUpdate, conn);

            try
            {
                using (var pgTransaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("assemblyPath", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = Guid.NewGuid();
                        command.Parameters[1].Value = plugin.Name;
                        command.Parameters[2].Value = plugin.AssemblyPath;

                        int rowsAffected = command.ExecuteNonQuery();

                        if (0 == rowsAffected)
                        {
                            command.CommandText = sqlInsert;
                            command.ExecuteScalar();
                        }

                        pgTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        pgTransaction.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
              conn.Close() ;
            }
        }

        /// <summary>
        /// Update plugin name. (Assembly path cannot be updated once registered)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public void UpdatePluginName(Guid id, string name)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sqlUpdate = @"UPDATE rsched_plugins SET plugin_name=:name WHERE id=:id";
            var command = new NpgsqlCommand(sqlUpdate, conn);

            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = id;
                        command.Parameters[1].Value = name;

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

        /// <summary>
        /// Removes registered plugin from db
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public int RemovePlugin(string pluginName)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"DELETE FROM rsched_plugins WHERE plugin_name=:name";
            var command = new NpgsqlCommand(sql, conn);
            command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
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
