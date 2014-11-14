using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    public class PostgreStore : ICustomJobStore
    {
        private readonly string _connectionString;

        public PostgreStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Get registered CustomJob by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public ICustomJob GetRegisteredJob(string name, string jobType)
        {
            ICustomJob retval = null;

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id, name, params, jobType FROM RSCHED_CUSTOM_JOBS WHERE name = :name;";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
                command.Parameters[0].Value = name;

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval = new CustomJob
                        {
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["name"],
                            Params = (string)reader["params"],
                            JobType = (string)reader["jobType"]
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
        /// Get registered CustomJob by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICustomJob GetRegisteredJob(Guid id)
        {
            ICustomJob retval = null;

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id, name, params, jobType FROM RSCHED_CUSTOM_JOBS WHERE id = :id;";
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
                        retval = new CustomJob
                        {
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["name"],
                            Params = (string)reader["params"],
                            JobType = (string)reader["jobType"]
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
        /// Get all registered CustomJobs
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public IList<ICustomJob> GetRegisteredJobs(string jobType)
        {
            IList<ICustomJob> retval = new List<ICustomJob>();

            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT id, name, params, jobType FROM RSCHED_CUSTOM_JOBS WHERE jobType = :jobType;";
            var command = new NpgsqlCommand(sql, conn);

            try
            {
                command.Parameters.Add(new NpgsqlParameter("jobType", NpgsqlDbType.Varchar));
                command.Parameters[0].Value = jobType;

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval.Add(new CustomJob
                        {
                            Id = Guid.Parse(reader["id"].ToString()),
                            Name = (string)reader["name"],
                            Params = (string)reader["params"],
                            JobType = (string)reader["jobType"]
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
        /// Register new CustomJob, or update existing one.
        /// </summary>
        /// <param name="job"></param>
        public void RegisterJob(ICustomJob job)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sqlInsert = @"INSERT INTO RSCHED_CUSTOM_JOBS(id, name, params, jobType) VALUES (:id, :name, :params, :jobType);";
            var sqlUpdate = @"UPDATE RSCHED_CUSTOM_JOBS SET params=:params WHERE name=:name";
            var command = new NpgsqlCommand(sqlUpdate, conn);

            try
            {
                using (var pgTransaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("params", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobType", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = Guid.NewGuid();
                        command.Parameters[1].Value = job.Name;
                        command.Parameters[2].Value = job.Params;
                        command.Parameters[3].Value = job.JobType;

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
                conn.Close();
            }
        }

        /// <summary>
        /// Update CustomJob name. (Params cannot be updated once registered)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public void UpdateName(Guid id, string name)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sqlUpdate = @"UPDATE RSCHED_CUSTOM_JOBS SET name=:name WHERE id=:id";
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
        /// Remove registered CustomJob from db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Remove(Guid id)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"DELETE FROM RSCHED_CUSTOM_JOBS WHERE id=:id";
            var command = new NpgsqlCommand(sql, conn);
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
            command.Parameters[0].Value = id;

            try
            {
                return command.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Remove registered CustomJobs of type <paramref name="jobType"/> from db
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public int RemoveAll(string jobType)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var sql = @"DELETE FROM RSCHED_CUSTOM_JOBS WHERE jobType=:jobType";
            var command = new NpgsqlCommand(sql, conn);
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Varchar));
            command.Parameters[0].Value = jobType;

            try
            {
                return command.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
