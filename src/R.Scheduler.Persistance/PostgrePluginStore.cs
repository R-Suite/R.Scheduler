using System;
using System.Collections.Generic;
using Npgsql;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.Persistance
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

            var sql = @"SELECT plugin_name, assembly_path, status FROM rsched_plugins WHERE plugin_name = :name;";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar));
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

        /// <summary>
        /// Get all registered plugins
        /// </summary>
        /// <returns></returns>
        public IList<Plugin> GetRegisteredPlugins()
        {
            IList<Plugin> retval = new List<Plugin>();

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT plugin_name, assembly_path, status FROM rsched_plugins";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval.Add(new Plugin()
                        {
                            Name = (string) reader["plugin_name"],
                            AssemblyPath = (string) reader["assembly_path"],
                            Status = (string) reader["status"]
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

            var sqlInsert = @"INSERT INTO rsched_plugins(plugin_name, assembly_path, status) VALUES (:name, :assemblyPath, :status);";
            var sqlUpdate = @"UPDATE rsched_plugins SET assembly_path=:assemblyPath, status=:status WHERE plugin_name=:name";
            var command = new NpgsqlCommand(sqlUpdate, conn);

            try
            {
                using (var pgTransaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("assemblyPath", NpgsqlTypes.NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlTypes.NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = plugin.Name;
                        command.Parameters[1].Value = plugin.AssemblyPath;
                        command.Parameters[2].Value = "registered";

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

        public PluginDetails GetRegisteredPluginDetails(string pluginName)
        {
            throw new NotImplementedException();
        }
    }
}
