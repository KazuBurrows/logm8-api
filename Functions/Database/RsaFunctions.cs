using System.Data;
using Microsoft.Data.SqlClient;

namespace Company.Function
{
    public partial class SqlFunctions
    {
        public static async Task<bool> InsertRSAsAsync(
            string accessToken,
            string privateKey,
            string publicKey
        )
        {
            string? connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string insertQuery =
                @"
            INSERT INTO [dbo].[rsas] (AccessToken, PrivateKey, PublicKey, TTL)
            VALUES (@AccessToken, @PrivateKey, @PublicKey, @TTL);";

            DateTime newTtlValue = DateTime.Now.AddMinutes(45);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@AccessToken", SqlDbType.VarChar)
                            {
                                Value = accessToken,
                            }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@PrivateKey", SqlDbType.VarChar)
                            {
                                Value = privateKey,
                            }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@PublicKey", SqlDbType.VarChar) { Value = publicKey }
                        );
                        command.Parameters.Add(
                            new SqlParameter("@TTL", SqlDbType.DateTime2) { Value = newTtlValue }
                        );

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> GetPrivateKey(string accessToken)
        {
            string? connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string query =
                "SELECT [Id], [AccessToken], [PrivateKey], [PublicKey], [TTL] FROM [dbo].[rsas] WHERE [AccessToken] = @AccessToken";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@AccessToken", SqlDbType.VarChar)
                            {
                                Value = accessToken,
                            }
                        );

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return (string)reader["PrivateKey"];
                            }
                            else
                            {
                                return "fail";
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "fail";
            }
        }

        public static async Task<bool> TruncateTableAsync()
        {
            string? connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string query = "TRUNCATE TABLE rsas;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
