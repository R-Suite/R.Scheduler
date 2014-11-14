using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    public class SqlServerStore : ICustomJobStore
    {
        private readonly string _connectionString;

        public SqlServerStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ICustomJob GetRegisteredJob(string name, string jobType)
        {
            ICustomJob retval = null;

            var sql = @"SELECT id, name, params, jobType FROM RSCHED_CUSTOM_JOBS WHERE name = @name AND jobType = @jobType;";

            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var command = new SqlCommand(sql, conn);
            command.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar));
            command.Parameters.Add(new SqlParameter("jobType", SqlDbType.VarChar));
            command.Parameters[0].Value = name;
            command.Parameters[1].Value = jobType;

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval = new CustomJob
                        {
                            Id = (Guid)reader["id"],
                            Name = (string)reader["pname"],
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

        public ICustomJob GetRegisteredJob(Guid id)
        {
            ICustomJob retval = null;

            var sql = @"SELECT [id], [name], [params], [jobType] FROM RSCHED_CUSTOM_JOBS WHERE [id] = @id;";

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
                        retval = new CustomJob
                        {
                            Id = (Guid)reader["id"],
                            Name = (string)reader["pname"],
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

        public IList<ICustomJob> GetRegisteredJobs(string jobType)
        {
            IList<ICustomJob> retval = new List<ICustomJob>();

            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT [id], [name], [params], [jobType] FROM RSCHED_CUSTOM_JOBS WHERE [jobType] = @jobType;";
            var command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@jobType", jobType);

            try
            {
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        retval.Add(new CustomJob
                        {
                            Id = (Guid)reader["id"],
                            Name = (string)reader["pname"],
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

        public void RegisterJob(ICustomJob job)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sqlInsert = @"INSERT INTO RSCHED_CUSTOM_JOBS(id, name, params, jobType) VALUES (@id, @name, @params, @jobType);";
            var sqlUpdate = @"UPDATE RSCHED_CUSTOM_JOBS SET params='@params' WHERE name='@name' AND jobType='@jobType'";
            var command = new SqlCommand(sqlUpdate, conn);

            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        command.Parameters.AddWithValue("@id", Guid.NewGuid());
                        command.Parameters.AddWithValue("@name", job.Name);
                        command.Parameters.AddWithValue("@params", job.Params);
                        command.Parameters.AddWithValue("@jobType", job.JobType);

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

        public void UpdateName(Guid id, string name)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sqlUpdate = @"UPDATE RSCHED_CUSTOM_JOBS SET name=@name WHERE id=@id";
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

        public int Remove(Guid id)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"DELETE FROM RSCHED_CUSTOM_JOBS WHERE id=@id";
            var command = new SqlCommand(sql, conn);
            command.Parameters.Add(new SqlParameter("id", SqlDbType.VarChar));
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

        public int RemoveAll(string jobType)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"DELETE FROM RSCHED_CUSTOM_JOBS WHERE jobType=@jobType";
            var command = new SqlCommand(sql, conn);
            command.Parameters.Add(new SqlParameter("jobType", SqlDbType.VarChar));
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
