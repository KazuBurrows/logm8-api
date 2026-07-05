using System.Data;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;

namespace Company.Function
{
    public partial class SqlFunctions
    {
        public static async Task<string> GenerateOneLifeUrlAsync(
            string logId,
            int mode,
            string? userId
        )
        {
            var token = Guid.NewGuid().ToString();

            string connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string insertQuery =
                @"
            INSERT INTO [dbo].[onelifes] (LogId, TokenKey, TTL, Mode, UserId)
            VALUES (@LogId, @TokenKey, @TTL, @Mode, @UserId);";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@LogId", SqlDbType.VarChar) { Value = logId }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@TokenKey", SqlDbType.VarChar) { Value = token }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@TTL", SqlDbType.DateTime)
                            {
                                Value = DateTime.UtcNow.AddMinutes(30),
                            }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@Mode", SqlDbType.Int) { Value = mode }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@UserId", SqlDbType.VarChar)
                            {
                                Value = (object?)userId ?? DBNull.Value,
                            }
                        );

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return token;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> IsOneLifeUrlExpired(string tokenKey)
        {
            var log = new JsonObject
            {
                ["LogId"] = "Not Found",
                ["TTL"] = null,
                ["Mode"] = 1,
            };

            string query =
                "SELECT TOP 1 [LogId], [TTL], [Mode] FROM [dbo].[onelifes] WHERE [TokenKey] = @TokenKey";

            try
            {
                using (
                    SqlConnection connection = new SqlConnection(
                        Environment.GetEnvironmentVariable("SqlConnectionString")
                    )
                )
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TokenKey", tokenKey);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var record = new JsonObject
                                {
                                    ["LogId"] = reader.GetString(reader.GetOrdinal("LogId")),
                                    ["TTL"] = reader.IsDBNull(reader.GetOrdinal("TTL"))
                                        ? null
                                        : reader.GetDateTime(reader.GetOrdinal("TTL")),
                                    ["Mode"] = reader.IsDBNull(reader.GetOrdinal("Mode"))
                                        ? null
                                        : reader.GetInt32(reader.GetOrdinal("Mode")),
                                };

                                return record.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                var errorLog = new JsonObject
                {
                    ["LogId"] = "Not Found",
                    ["TTL"] = null,
                    ["Mode"] = null,
                };
                return errorLog.ToString();
            }

            return log.ToString();
        }

        public static async Task<string> IsOneLifeUrlConsumed(string tokenKey)
        {
            var log = new JsonObject { ["LogId"] = "Not Found", ["IsConsumed"] = 0 };
            string query =
                "SELECT TOP 1 [LogId], [IsConsumed] FROM [dbo].[onelifes] WHERE [TokenKey] = @TokenKey";

            try
            {
                using (
                    SqlConnection connection = new SqlConnection(
                        Environment.GetEnvironmentVariable("SqlConnectionString")
                    )
                )
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TokenKey", tokenKey);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new JsonObject
                                {
                                    ["LogId"] = reader.GetString(reader.GetOrdinal("LogId")),
                                    ["IsConsumed"] = reader.GetByte(
                                        reader.GetOrdinal("IsConsumed")
                                    ),
                                };

                                return record.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "Not Found";
            }

            return log.ToString();
        }

        public static async Task<bool> CommitOneLifeUrlComsumed(string tokenKey)
        {
            string updateQuery =
                @"
                UPDATE [dbo].[onelifes]
                SET [IsConsumed] = 1
                WHERE [TokenKey] = @TokenKey;
            ";

            try
            {
                using (
                    SqlConnection connection = new SqlConnection(
                        Environment.GetEnvironmentVariable("SqlConnectionString")
                    )
                )
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@TokenKey", SqlDbType.VarChar) { Value = tokenKey }
                        );

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
