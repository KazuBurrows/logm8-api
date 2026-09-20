using System.Data;
using System.Net;
using System.Text.Json.Nodes;
using LogMate.Application.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public partial class SqlFunctions
    {
        public static async Task<(string LogId, int? Mode, string? UserId)> EnsureOneLifeTokenNotExpiredAsync(
            string tokenKey,
            ILogger? logger = null
        )
        {
            string str_log = await IsOneLifeUrlExpired(tokenKey, logger);
            var log = JsonNode.Parse(str_log)?.AsObject();

            string? logId = log?["LogId"]?.GetValue<string>();
            DateTime? ttl = log?["TTL"]?.GetValue<DateTime?>();

            if (string.IsNullOrEmpty(logId) || logId == "Not Found" || !ttl.HasValue || ttl.Value < DateTime.UtcNow)
            {
                logger?.LogWarning("Invalid or expired OneLife token {TokenKey}", tokenKey);
                throw new ApiException(HttpStatusCode.NotFound, "Invalid or expired token.");
            }

            int? mode = log?["Mode"]?.GetValue<int?>();
            string? userId = log?["UserId"]?.GetValue<string?>();
            return (logId, mode, userId);
        }

        public static async Task<string> GenerateOneLifeUrlAsync(
            string logId,
            int mode,
            string? userId
        )
        {
            var token = Guid.NewGuid().ToString();
            var ttl = DateTime.UtcNow.AddMinutes(30);

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
                            new SqlParameter("@TTL", SqlDbType.DateTime) { Value = ttl }
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

        public static async Task<string> IsOneLifeUrlExpired(string tokenKey, ILogger? logger = null)
        {
            var log = new JsonObject
            {
                ["LogId"] = "Not Found",
                ["TTL"] = null,
                ["Mode"] = 1,
                ["UserId"] = null,
            };

            string query =
                "SELECT TOP 1 [LogId], [TTL], [Mode], [UserId] FROM [dbo].[onelifes] WHERE [TokenKey] = @TokenKey";

            var stopwatch = logger != null ? System.Diagnostics.Stopwatch.StartNew() : null;

            try
            {
                using (
                    SqlConnection connection = new SqlConnection(
                        Environment.GetEnvironmentVariable("SqlConnectionString")
                    )
                )
                {
                    await connection.OpenAsync();
                    logger?.LogInformation(
                        "IsOneLifeUrlExpired SQL connection opened for {TokenKey} in {Elapsed}ms",
                        tokenKey, stopwatch?.ElapsedMilliseconds
                    );

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TokenKey", tokenKey);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string recordLogId = reader.GetString(reader.GetOrdinal("LogId"));
                                DateTime? recordTtl = reader.IsDBNull(reader.GetOrdinal("TTL"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("TTL"));
                                int? recordMode = reader.IsDBNull(reader.GetOrdinal("Mode"))
                                    ? null
                                    : reader.GetInt32(reader.GetOrdinal("Mode"));
                                string? recordUserId = reader.IsDBNull(reader.GetOrdinal("UserId"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("UserId"));

                                var record = new JsonObject
                                {
                                    ["LogId"] = recordLogId,
                                    ["TTL"] = recordTtl,
                                    ["Mode"] = recordMode,
                                    ["UserId"] = recordUserId,
                                };

                                logger?.LogInformation(
                                    "IsOneLifeUrlExpired SQL query for {TokenKey} completed in {Elapsed}ms total",
                                    tokenKey, stopwatch?.ElapsedMilliseconds
                                );

                                return record.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(
                    ex,
                    "IsOneLifeUrlExpired failed for token {TokenKey} after {Elapsed}ms",
                    tokenKey, stopwatch?.ElapsedMilliseconds
                );

                var errorLog = new JsonObject
                {
                    ["LogId"] = "Not Found",
                    ["TTL"] = null,
                    ["Mode"] = null,
                    ["UserId"] = null,
                };
                return errorLog.ToString();
            }

            logger?.LogInformation(
                "IsOneLifeUrlExpired SQL query for {TokenKey} found no record in {Elapsed}ms",
                tokenKey, stopwatch?.ElapsedMilliseconds
            );
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

        public static async Task CleanOneLifeUrls()
        {
            string deleteQuery =
            @"
            DELETE FROM [dbo].[onelifes]
            WHERE [TTL] < DATEADD(MINUTE, -720, GETUTCDATE()) AND IsConsumed = 1;
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

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exception if needed
                throw;
            }
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
