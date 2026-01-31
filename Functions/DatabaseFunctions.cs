using System.Data;
using System.Text.Json.Nodes;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class SqlFunctions
    {
        /// <summary>
        /// Generates a unique token, stores it in the database along with the provided LogId,
        /// and returns the generated token. If an error occurs, returns "fail".
        /// </summary>
        /// <param name="logId">The unique identifier for the log entry, used to associate the token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a string
        /// representing the generated token if successful, or "fail" in case of an error.
        /// </returns>
        /// <exception cref="SqlException">
        /// Thrown when an error occurs while executing the SQL query.
        /// </exception>
        /// <remarks>
        /// This method generates a new GUID as the token, inserts the token along with the LogId
        /// into the "onelifes" table in the database, and handles any database-related errors.
        /// </remarks>
        public static async Task<string> GenerateOneLifeUrlAsync(
            string logId,
            int mode,
            string? userId
        )
        {
            var token = Guid.NewGuid().ToString();

            // Get the connection string from environment variables
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
                        // Add parameters to prevent SQL injection
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

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                }

                return token;
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Database error: {ex.Message}");
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
                        // Add parameters to prevent SQL injection
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
            catch (Exception ex)
            {
                // Ideally log error here
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
                        // Add parameters to prevent SQL injection
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
            catch (Exception ex)
            {
                // Log and handle the exception
                // _logger.LogError($"Error fetching data: {ex.Message}");
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
                        // Add the TokenKey parameter to prevent SQL injection
                        command.Parameters.Add(
                            new SqlParameter("@TokenKey", SqlDbType.VarChar) { Value = tokenKey }
                        );

                        // Execute the update query and check if any rows were affected
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        return rowsAffected > 0; // If rowsAffected > 0, the update was successful
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and handle error
                // _logger.LogError($"Error updating IsConsumed for TokenKey {tokenKey}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously inserts a new record into the "rsas" table in the database.
        /// The record includes an access token, private key, public key, and a TTL (time-to-live) value.
        /// Returns true if the operation is successful, otherwise false.
        /// </summary>
        /// <param name="accessToken">The access token to associate with the record.</param>
        /// <param name="privateKey">The private key to store in the database.</param>
        /// <param name="publicKey">The public key to store in the database.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a boolean
        /// indicating whether the insertion was successful (true) or failed (false).
        /// </returns>
        /// <exception cref="SqlException">
        /// Thrown when an error occurs while executing the SQL command, such as a connection failure or constraint violation.
        /// </exception>
        /// <remarks>
        /// This method inserts a new row into the "rsas" table with the specified values for the access token, private key,
        /// and public key. The TTL value is set to DBNull to indicate no expiration. The method handles any database errors
        /// by logging them and returning false.
        /// </remarks>
        public static async Task<bool> InsertRSAsAsync(
            string accessToken,
            string privateKey,
            string publicKey
        )
        {
            // Get the connection string from environment variables
            string connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string insertQuery =
                @"
            INSERT INTO [dbo].[rsas] (AccessToken, PrivateKey, PublicKey, TTL) 
            VALUES (@AccessToken, @PrivateKey, @PublicKey, @TTL);";

            DateTime ttlValue = DateTime.Now; // Current time
            DateTime newTtlValue = ttlValue.AddMinutes(45); // Add 45 minutes

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Add parameters to prevent SQL injection
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

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch
            {
                // _logger.LogError($"Database error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the private key associated with the provided access token from the database.
        /// Returns the private key if found, otherwise returns "fail".
        /// </summary>
        /// <param name="accessToken">The access token used to identify the record in the database.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a string containing the private key if successful,
        /// or "fail" if the record is not found or an error occurs.
        /// </returns>
        /// <exception cref="SqlException">
        /// Thrown when an error occurs while executing the SQL query, such as a connection failure or incorrect query.
        /// </exception>
        /// <remarks>
        /// This method queries the "rsas" table in the database using the provided access token to fetch the associated private key.
        /// If the access token is found, the corresponding private key is returned. If no record matches, the method returns "fail".
        /// Database errors are logged and also result in a "fail" return value.
        /// </remarks>
        public static async Task<string> GetPrivateKey(string accessToken)
        {
            string connectionString =
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
                        // Add parameter to prevent SQL injection
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
                                // Read the data from the result
                                var record = new
                                {
                                    Id = reader["Id"],
                                    AccessToken = reader["AccessToken"],
                                    PrivateKey = reader["PrivateKey"],
                                    PublicKey = reader["PublicKey"],
                                    TTL = reader["TTL"],
                                };

                                return (string)record.PrivateKey;
                            }
                            else
                            {
                                return "fail";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Database error: {ex.Message}");
                return "fail";
            }
        }

        /// <summary>
        /// Asynchronously truncates the "rsas" table in the database.
        /// Returns true if the operation is successful, otherwise false.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a boolean
        /// indicating whether the truncation was successful (true) or failed (false).
        /// </returns>
        /// <exception cref="SqlException">
        /// Thrown when an error occurs while executing the SQL command, such as a connection failure or permission issue.
        /// </exception>
        /// <remarks>
        /// This method uses the TRUNCATE command to remove all rows from the "rsas" table without logging individual row deletions.
        /// It handles database errors by returning false if the operation fails.
        /// </remarks>
        public static async Task<bool> TruncateTableAsync()
        {
            // Get the connection string from environment variables
            string connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string query = $"TRUNCATE TABLE rsas;";

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
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Used for testing
        /// </summary>
        /// <returns></returns>
        public static async Task InsertTags()
        {
            string connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            // SQL Insert Query with multiple rows
            string insertQuery =
                @"
                INSERT INTO [dbo].[tags] 
                (
                    [Make],
                    [Model],
                    [Year],
                    [Vehicle],
                    [Style],
                    [Engine],
                    [Fuel],
                    [Transmission],
                    [Color],
                    [VinNumber],
                    [LicencePlate]
                )
                VALUES
                (@Make1, @Model1, @Year1, @Vehicle1, @Style1, @Engine1, @Fuel1, @Transmission1, @Color1, @VinNumber1, @LicencePlate1),
                (@Make2, @Model2, @Year2, @Vehicle2, @Style2, @Engine2, @Fuel2, @Transmission2, @Color2, @VinNumber2, @LicencePlate2),
                (@Make3, @Model3, @Year3, @Vehicle3, @Style3, @Engine3, @Fuel3, @Transmission3, @Color3, @VinNumber3, @LicencePlate3);";

            // Values for multiple rows
            var tags = new[]
            {
                new
                {
                    Make = "Harley-Davidson",
                    Model = "Sportster S",
                    Year = 2023,
                    Vehicle = "Motorcycle",
                    Style = "Cruiser",
                    Engine = 1252,
                    Fuel = "Petrol",
                    Transmission = "Manual",
                    Color = "Black",
                    VinNumber = "1HD1ZES1XNB016123",
                    LicencePlate = "MOTO123",
                },
                new
                {
                    Make = "Yamaha",
                    Model = "MT-07",
                    Year = 2022,
                    Vehicle = "Motorcycle",
                    Style = "Naked",
                    Engine = 689,
                    Fuel = "Petrol",
                    Transmission = "Manual",
                    Color = "Blue",
                    VinNumber = "JYARN39E1NA012345",
                    LicencePlate = "BIKE456",
                },
                new
                {
                    Make = "Kawasaki",
                    Model = "Ninja 650",
                    Year = 2021,
                    Vehicle = "Motorcycle",
                    Style = "Sport",
                    Engine = 649,
                    Fuel = "Petrol",
                    Transmission = "Manual",
                    Color = "Green",
                    VinNumber = "JKAEXKD1NMA123456",
                    LicencePlate = "RIDE789",
                },
            };

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        // Add parameters for the first row
                        command.Parameters.AddWithValue("@Make1", tags[0].Make);
                        command.Parameters.AddWithValue("@Model1", tags[0].Model);
                        command.Parameters.AddWithValue("@Year1", tags[0].Year);
                        command.Parameters.AddWithValue("@Vehicle1", tags[0].Vehicle);
                        command.Parameters.AddWithValue("@Style1", tags[0].Style);
                        command.Parameters.AddWithValue("@Engine1", tags[0].Engine);
                        command.Parameters.AddWithValue("@Fuel1", tags[0].Fuel);
                        command.Parameters.AddWithValue("@Transmission1", tags[0].Transmission);
                        command.Parameters.AddWithValue("@Color1", tags[0].Color);
                        command.Parameters.AddWithValue(
                            "@VinNumber1",
                            tags[0].VinNumber ?? (object)DBNull.Value
                        );
                        command.Parameters.AddWithValue(
                            "@LicencePlate1",
                            tags[0].LicencePlate ?? (object)DBNull.Value
                        );

                        // Add parameters for the second row
                        command.Parameters.AddWithValue("@Make2", tags[1].Make);
                        command.Parameters.AddWithValue("@Model2", tags[1].Model);
                        command.Parameters.AddWithValue("@Year2", tags[1].Year);
                        command.Parameters.AddWithValue("@Vehicle2", tags[1].Vehicle);
                        command.Parameters.AddWithValue("@Style2", tags[1].Style);
                        command.Parameters.AddWithValue("@Engine2", tags[1].Engine);
                        command.Parameters.AddWithValue("@Fuel2", tags[1].Fuel);
                        command.Parameters.AddWithValue("@Transmission2", tags[1].Transmission);
                        command.Parameters.AddWithValue("@Color2", tags[1].Color);
                        command.Parameters.AddWithValue(
                            "@VinNumber2",
                            tags[1].VinNumber ?? (object)DBNull.Value
                        );
                        command.Parameters.AddWithValue(
                            "@LicencePlate2",
                            tags[1].LicencePlate ?? (object)DBNull.Value
                        );

                        // Add parameters for the third row
                        command.Parameters.AddWithValue("@Make3", tags[2].Make);
                        command.Parameters.AddWithValue("@Model3", tags[2].Model);
                        command.Parameters.AddWithValue("@Year3", tags[2].Year);
                        command.Parameters.AddWithValue("@Vehicle3", tags[2].Vehicle);
                        command.Parameters.AddWithValue("@Style3", tags[2].Style);
                        command.Parameters.AddWithValue("@Engine3", tags[2].Engine);
                        command.Parameters.AddWithValue("@Fuel3", tags[2].Fuel);
                        command.Parameters.AddWithValue("@Transmission3", tags[2].Transmission);
                        command.Parameters.AddWithValue("@Color3", tags[2].Color);
                        command.Parameters.AddWithValue(
                            "@VinNumber3",
                            tags[2].VinNumber ?? (object)DBNull.Value
                        );
                        command.Parameters.AddWithValue(
                            "@LicencePlate3",
                            tags[2].LicencePlate ?? (object)DBNull.Value
                        );

                        // Execute the query
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"{rowsAffected} row(s) inserted.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<ServiceHierarchy> GetHierarchy()
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");
            ;

            using var conn = new SqlConnection(connString);
            await conn.OpenAsync();

            // =======================
            // 🚗 Motorbike hierarchy
            // =======================
            var motorbikeSql =
                @"
                    WITH MotorbikeOptions AS (
                        SELECT so.Id, so.Name, so.Description, CAST(NULL AS INT) AS ParentId
                        FROM ServiceOption so
                        JOIN VehicleTypeServiceOption vtso ON vtso.ServiceOptionId = so.Id
                        JOIN VehicleType vt ON vt.Id = vtso.VehicleTypeId
                        WHERE vt.Name = 'Motorbike'

                        UNION ALL

                        SELECT child.Id, child.Name, child.Description, rel.ParentId
                        FROM ServiceOptionRelation rel
                        JOIN ServiceOption child ON child.Id = rel.ChildId
                        JOIN MotorbikeOptions m ON m.Id = rel.ParentId
                    )
                    SELECT mo.Id,
                        mo.Name,
                        mo.Description,
                        mo.ParentId,
                        STRING_AGG(st.Name, ',') AS ServiceTypes
                    FROM MotorbikeOptions mo
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = mo.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    GROUP BY mo.Id, mo.Name, mo.Description, mo.ParentId;
                    ";

            var motorbikeFlat = await QueryFlatOptions(conn, motorbikeSql);

            // =======================
            // 🧾 Ownership
            // =======================
            var ownershipSql =
                @"
                    SELECT so.Id,
                        so.Name,
                        so.Description,
                        CAST(NULL AS INT) AS ParentId,
                        STRING_AGG(st.Name, ',') AS ServiceTypes
                    FROM ServiceOption so
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = so.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    WHERE so.Name = 'Ownership'
                    GROUP BY so.Id, so.Name, so.Description;
                    ";

            var ownershipFlat = await QueryFlatOptions(conn, ownershipSql);

            return new ServiceHierarchy
            {
                MotorbikeOptions = BuildHierarchy(motorbikeFlat),
                OwnershipOptions = BuildHierarchy(ownershipFlat),
            };
        }

        // =======================
        // Helpers
        // =======================

        private static async Task<List<FlatServiceOption>> QueryFlatOptions(
            SqlConnection conn,
            string sql
        )
        {
            var results = new List<FlatServiceOption>();

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(
                    new FlatServiceOption
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ParentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        ServiceTypes = reader.IsDBNull(4)
                            ? new List<string>()
                            : reader.GetString(4).Split(',').ToList(),
                    }
                );
            }

            return results;
        }

        private static List<ServiceOption> BuildHierarchy(List<FlatServiceOption> flatList)
        {
            var dict = flatList
                .GroupBy(x => x.Id)
                .Select(g => new ServiceOption
                {
                    Id = g.Key,
                    Name = g.First().Name,
                    Description = g.First().Description,
                    ServiceTypes = g.SelectMany(x => x.ServiceTypes).Distinct().ToList(),
                })
                .ToDictionary(x => x.Id);

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && dict.ContainsKey(item.ParentId.Value))
                {
                    dict[item.ParentId.Value].Children.Add(dict[item.Id]);
                }
            }

            return dict
                .Values.Where(x => !flatList.Any(f => f.Id == x.Id && f.ParentId.HasValue))
                .ToList();
        }

        public static async Task<bool> AddServiceTypeById(string optionId, string serviceTypeId)
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");
            ;

            const string sql =
                @"
                    INSERT INTO dbo.ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
                    SELECT @OptionId, @ServiceTypeId
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM dbo.ServiceOptionServiceType
                        WHERE ServiceOptionId = @OptionId
                        AND ServiceTypeId = @ServiceTypeId
                    );
                ";

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@OptionId", SqlDbType.Int).Value = optionId;
            cmd.Parameters.Add("@ServiceTypeId", SqlDbType.Int).Value = serviceTypeId;

            await conn.OpenAsync();

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            // true = inserted, false = already existed
            return rowsAffected > 0;
        }

        public static async Task<bool> AddParentOptionById(int optionId, int parentId)
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");
            ;
            const string sql =
                @"
                INSERT INTO dbo.ServiceOptionRelation (ParentId, ChildId)
                SELECT @ParentId, @ChildId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.ServiceOptionRelation
                    WHERE ParentId = @ParentId
                    AND ChildId = @ChildId
                );
            ";

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(sql, conn);

            // FIX: correct variables
            cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId;
            cmd.Parameters.Add("@ChildId", SqlDbType.Int).Value = optionId;

            await conn.OpenAsync();

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public static async Task<bool> CreateNewServiceOption(
            string name,
            string description,
            int categoryId
        )
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");
            ;

            const string sql =
                @"
                DECLARE @ServiceOptionId INT;

                -- Insert ServiceOption if it does not exist
                IF NOT EXISTS (
                    SELECT 1 FROM dbo.ServiceOption WHERE Name = @Name
                )
                BEGIN
                    INSERT INTO dbo.ServiceOption (Name, Description, CategoryId)
                    VALUES (@Name, @Description, @CategoryId);

                    SET @ServiceOptionId = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @ServiceOptionId = Id
                    FROM dbo.ServiceOption
                    WHERE Name = @Name;
                END

                -- Always link to Motorbike (VehicleTypeId = 3)
                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.VehicleTypeServiceOption
                    WHERE VehicleTypeId = 3
                    AND ServiceOptionId = @ServiceOptionId
                )
                BEGIN
                    INSERT INTO dbo.VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
                    VALUES (3, @ServiceOptionId);
                END
            ";

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = name.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(
                description
            )
                ? DBNull.Value
                : description;
            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value =
                categoryId > 0 ? categoryId : DBNull.Value;

            await conn.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            // If we got here, operation succeeded
            return true;
        }
    }

    public class CosmosFunctions
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDb_Endpoint");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDb_PrimaryKey");
        private static readonly string DatabaseId = "LogmateCosmosDB";
        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        public static async Task<Record> InsertRecord(Record record)
        {
            string ContainerId = "records";

            try
            {
                var _container = cosmosClient.GetContainer(DatabaseId, ContainerId);
                ItemResponse<Record> response = await _container.CreateItemAsync(
                    record,
                    new PartitionKey(record.TagId)
                );
                return response.Resource; // Returning the inserted record
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null; // Or throw exception based on your error handling strategy
            }
        }

        public static async Task<List<Record>> GetRecordsByTagId(string tagId)
        {
            string ContainerId = "records";
            // Create a new Cosmos DB client
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @TagId ORDER BY c.ServicedDate DESC"
            ).WithParameter("@TagId", tagId);

            var resultSetIterator = container.GetItemQueryIterator<Record>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(tagId) }
            );

            var results = new List<Record>();
            while (resultSetIterator.HasMoreResults)
            {
                var response = await resultSetIterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public static async Task<Record?> GetRecordById(string id)
        {
            try
            {
                string ContainerId = "records";
                // Create a new Cosmos DB client
                var container = cosmosClient.GetContainer(DatabaseId, ContainerId);
                // Query by id
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter(
                    "@id",
                    id
                );

                using FeedIterator<Record> iterator = container.GetItemQueryIterator<Record>(query);

                if (iterator.HasMoreResults)
                {
                    FeedResponse<Record> response = await iterator.ReadNextAsync();
                    return response.Resource.FirstOrDefault();
                }

                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Graceful "not found"
                return null;
            }
        }

        public static async Task<bool> InsertTag(string hash_id)
        {
            Tag data = new Tag
            {
                id = Guid.NewGuid().ToString(),
                TagId = hash_id,
                IsConfigured = false,
            };

            string ContainerId = "tags";
            // Create a new Cosmos DB client
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            try
            {
                // Insert the item into the container
                var response = await container.CreateItemAsync(data, new PartitionKey(data.TagId));
                return true;
            }
            catch (CosmosException ex)
            {
                // _logger.LogError($"Failed to insert item. Status code: {ex.StatusCode}, Message: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateTag(Tag tag)
        {
            string ContainerId = "tags";
            // Create a new Cosmos DB client
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            try
            {
                // Fix TagId format. '+' lost when in url and is decoded to ' '.
                string tagId = tag.TagId.Replace(" ", "+");
                tag.TagId = tagId;

                // Create a query to retrieve the item using TagId as the partition key
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.TagId = @tagId"
                ).WithParameter("@tagId", tagId);

                var iterator = container.GetItemQueryIterator<Tag>(queryDefinition);

                // Execute the query
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        // Modify the fields you want to update
                        item.Make = tag.Make;
                        item.Model = tag.Model;
                        item.Year = tag.Year;
                        item.Vehicle = tag.Vehicle;
                        item.Style = tag.Style;
                        item.Engine = tag.Engine;
                        item.Fuel = tag.Fuel;
                        item.Transmission = tag.Transmission;
                        item.Color = tag.Color;
                        item.VinNumber = tag.VinNumber;
                        item.LicencePlate = tag.LicencePlate;
                        item.IsConfigured = true;

                        // Replace the item in Cosmos DB
                        await container.ReplaceItemAsync(
                            item,
                            item.id,
                            new PartitionKey(item.TagId)
                        );

                        return true;
                    }
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // _logger.LogError($"Item with TagId '{tag.TagId}' not found.");
            }
            catch (CosmosException ex)
            {
                // _logger.LogError($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // _logger.LogError($"500");
            }

            return false;
        }

        public static async Task<Record> PatchRecord(string id, Record updates)
        {
            string containerName = "records";
            var container = cosmosClient.GetContainer(DatabaseId, containerName);

            var operations = new List<PatchOperation>();

            if (updates.ServicedDate != null)
                operations.Add(PatchOperation.Set("/ServicedDate", updates.ServicedDate));

            if (updates.MechanicName != null)
                operations.Add(PatchOperation.Set("/MechanicName", updates.MechanicName));

            if (updates.Odometer != null)
                operations.Add(PatchOperation.Set("/Odometer", updates.Odometer));

            if (updates.ServiceCategory != null)
                operations.Add(PatchOperation.Set("/ServiceCategory", updates.ServiceCategory));

            if (updates.ServiceType != null)
                operations.Add(PatchOperation.Set("/ServiceType", updates.ServiceType));

            if (updates.ServiceOption != null)
                operations.Add(PatchOperation.Set("/ServiceOption", updates.ServiceOption));

            if (updates.Comment != null)
                operations.Add(PatchOperation.Set("/Comment", updates.Comment));

            if (updates.FileUrls != null)
                operations.Add(PatchOperation.Set("/FileUrls", updates.FileUrls));

            var response = await container.PatchItemAsync<Record>(
                id,
                new PartitionKey(updates.TagId),
                operations
            );

            return response.Resource;
        }

        public static async Task<int> ReplaceAllRecordsForTagAsync(
            string oldTagId,
            string newTagId,
            ILogger logger
        )
        {
            var ContainerId = "records";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @oldTagId"
            ).WithParameter("@oldTagId", oldTagId);

            using var iterator = container.GetItemQueryIterator<Record>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(oldTagId),
                }
            );

            int migratedCount = 0;

            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();

                foreach (var record in page)
                {
                    var oldId = record.id;

                    // ---- mutate record for new partition ----
                    record.id = Guid.NewGuid().ToString();
                    record.TagId = newTagId;

                    // Optional audit fields
                    record.Comment += $" (Migrated from TagId {oldTagId} at {DateTime.UtcNow})";

                    // ---- create new record in new partition ----
                    await container.CreateItemAsync(record, new PartitionKey(newTagId));

                    // ---- delete old record ----
                    await container.DeleteItemAsync<Record>(oldId, new PartitionKey(oldTagId));

                    migratedCount++;
                }
            }

            logger.LogInformation(
                "Migrated {Count} records from TagId {Old} → {New}",
                migratedCount,
                oldTagId,
                newTagId
            );

            return migratedCount;
        }

        public static async Task<int> GetNextTagId()
        {
            string ContainerId = "tag_counter";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            // SQL query to retrieve items where the TagId matches
            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c");

            var queryIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                foreach (var item in await queryIterator.ReadNextAsync())
                {
                    // Assuming you want the first item
                    return item.next_id;
                }
            }

            // No items found for the specified TagId.
            return -1;
        }

        public static async Task<bool> UpdateNextTagId()
        {
            string ContainerId = "tag_counter";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            // SQL query to retrieve items where the TagId matches
            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c");

            var queryIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                foreach (var item in await queryIterator.ReadNextAsync())
                {
                    string id = item.id; // Ensure this is the correct property for the item's ID
                    string partitionKeyValue = item.p_key;
                    item.next_id = item.next_id + 1;

                    // Step 3: Replace the item in the container
                    await container.ReplaceItemAsync(item, id, new PartitionKey(partitionKeyValue));

                    return true;
                }
            }

            return false;
        }

        public static async Task<int> IsTagConfigured(string id)
        {
            string containerId = "tags";
            var container = cosmosClient.GetContainer(DatabaseId, containerId);

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @tagId"
            ).WithParameter("@tagId", id);

            var queryIterator = container.GetItemQueryIterator<Tag>(queryDefinition);

            while (queryIterator.HasMoreResults)
            {
                foreach (var item in await queryIterator.ReadNextAsync())
                {
                    return item.IsConfigured ? 1 : 0;
                }
            }

            // No matching tag found
            return -1;
        }

        /// <summary>
        /// Asynchronously retrieves tag information from the "tags" table by the provided ID.
        /// Returns the tag information as a JSON string if the record is found, otherwise returns null.
        /// </summary>
        /// <param name="id">The ID used to identify the record in the "tags" table.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a string containing the tag information in JSON format if the record is found,
        /// or null if the record is not found or an error occurs.
        /// </returns>
        /// <exception cref="SqlException">
        /// Thrown when an error occurs while executing the SQL query, such as a connection failure or incorrect query.
        /// </exception>
        /// <remarks>
        /// This method queries the "tags" table in the database using the provided ID. If a matching record is found, the tag information is returned in JSON format.
        /// If no record is found, a warning is logged, and null is returned. In case of any database error, an error is logged, and null is returned.
        /// </remarks>
        public static async Task<Tag> GetTagInfo(string tagId)
        {
            Tag defaultTag = new Tag
            {
                Make = "",
                Model = "",
                Year = 0,
                Vehicle = "",
                Style = "",
                Engine = 0,
                Fuel = [],
                Transmission = "",
                Color = "",
                VinNumber = "",
                LicencePlate = "",
            };

            string ContainerId = "tags";
            // Create a new Cosmos DB client
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            try
            {
                var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.TagId = @TagId"
                ).WithParameter("@TagId", tagId);

                using var resultSetIterator = container.GetItemQueryIterator<Tag>(
                    query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(tagId),
                    }
                );

                if (resultSetIterator.HasMoreResults)
                {
                    var response = await resultSetIterator.ReadNextAsync();
                    var tag = response.FirstOrDefault();

                    if (tag != null)
                    {
                        return tag;
                    }
                }

                return defaultTag;
            }
            catch (CosmosException ex)
            {
                // _logger.LogError(ex, "Error retrieving item from Cosmos DB.");
                return defaultTag;
            }
        }
    }

    public class BlobFunctions
    {
        private static string _blobConnectionString = Environment.GetEnvironmentVariable("BlobStorage_ConnectionString");

        public static async Task<string> UploadBlob(
        Stream content,
        string fileName,
        string contentType)
    {
        try
        {
            const string containerName = "receipts";

            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(
                content,
                new BlobHttpHeaders
                {
                    ContentType = contentType ?? GetContentType(fileName)
                }
            );

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file to Blob Storage: {ex}");
            throw; // let the function handle the failure properly
        }
    }

        // Helper method to get the correct content type based on file extension
        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".docx" =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream", // Default for unknown file types
            };
        }
    }
}
