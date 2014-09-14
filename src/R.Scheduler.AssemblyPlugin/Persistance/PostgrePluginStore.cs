using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Persistance
{
    /// <summary>
    /// Postgre implementation of IPluginStore
    /// </summary>
    public class PostgrePluginStore : IPluginStore, IUseSchedulerConnectionString
    {
        private string _connectionString;

        //public PostgrePluginStore(string connectionString)
        //{
        //    _connectionString = connectionString;
        //}

        public void SetConnectionString(string connectionString)
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

            var sql = @"SELECT id plugin_name, assembly_path, status FROM rsched_plugins WHERE plugin_name = :name;";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
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

            var sql = @"SELECT id, plugin_name, assembly_path, status FROM rsched_plugins";
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

            var sqlInsert = @"INSERT INTO rsched_plugins(id, plugin_name, assembly_path, status) VALUES (:id, :name, :assemblyPath, :status);";
            var sqlUpdate = @"UPDATE rsched_plugins SET assembly_path=:assemblyPath, status=:status WHERE plugin_name=:name";
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
                        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = Guid.NewGuid();
                        command.Parameters[1].Value = plugin.Name;
                        command.Parameters[2].Value = plugin.AssemblyPath;
                        command.Parameters[3].Value = "registered";

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
