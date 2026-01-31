using System.Data;
using System.Net; // <-- for HttpStatusCode
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using iText.Html2pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public class LogMateFunction
    {
        public enum UserMode
        {
            Service = 0,
            Guest = 1,
        }

        private readonly ILogger<LogMateFunction> _logger;

        private readonly string BaseLogm8Url = "https://logm8.com";

        public LogMateFunction(ILogger<LogMateFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the HTTP GET request for the negotiation process, generating RSA keys,
        /// inserting records into the database, and returning an access token and public key.
        /// </summary>
        /// <param name="req">The HTTP request that triggers the function.</param>
        /// <returns>
        /// An IActionResult containing a JSON object with the generated access token and public key,
        /// or an internal server error if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following actions:
        /// 1. Logs request details such as connection ID and remote IP address.
        /// 2. Generates a new RSA key pair (512-bit key size).
        /// 3. Generates an access token.
        /// 4. Truncates any existing records in the database.
        /// 5. Inserts the generated keys and access token into the database.
        /// 6. Returns a JSON response with the generated access token and public key,
        ///    or an internal server error response if the insert operation fails.
        /// </remarks>
        [Function("Negotiate")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            var rsa = System.Security.Cryptography.RSA.Create();
            rsa.KeySize = 2048;

            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);
            string accessToken = GenerateAccessToken();

            bool insertRSAsStatus = await SqlFunctions.InsertRSAsAsync(
                accessToken,
                privateKey,
                publicKey
            );
            if (insertRSAsStatus)
            {
                JsonObject resJson = new JsonObject
                {
                    ["accessToken"] = accessToken,
                    ["publicKey"] = publicKey,
                };
                string resString = resJson.ToJsonString();
                return new OkObjectResult(resString);
            }
            ;

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Handles the HTTP GET request to generate a OneLife URL based on the provided request string.
        /// </summary>
        /// <param name="req">The HTTP request containing the query parameter 'reqString' in JSON format.</param>
        /// <returns>
        /// An IActionResult containing the generated OneLife URL as a string if successful,
        /// or an appropriate error response if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method first retrieves the 'reqString' query parameter from the incoming HTTP request. It then parses the
        /// string to extract the 'accessToken' and 'eId'. The method retrieves the private key using the access token,
        /// decrypts the 'eId' using the RSA private key, and generates a OneLife URL. Finally, the generated URL is returned in the response.
        /// </remarks>
        [Function("OneLifeUrl")]
        public async Task<IActionResult> OneLifeUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];

            // Reverse: Convert JSON string back to JsonObject
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            // Access properties from reversed JsonObject
            string accessToken = reversedJson["accessToken"]?.ToString();
            string eId = reversedJson["eId"]?.ToString();

            // Decrypting TagId.
            string privateKey = await SqlFunctions.GetPrivateKey(accessToken);
            var rsa = System.Security.Cryptography.RSA.Create();
            string id = DecryptText(rsa, eId, privateKey);

            // Check if Tag has been configured.
            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 1) // If Tag is found and configured.
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(
                    id,
                    (int)UserMode.Service,
                    null
                );
                return new OkObjectResult($"{BaseLogm8Url}/log?token={tokenUrl}");
            }
            else if (isConfigured == -1) // If Tag is not found.
            {
                return new NotFoundResult();
            }

            // If Tag is not found and not configured.
            return new OkObjectResult($"{BaseLogm8Url}/tag/create?token={id}"); // NOTE: Must change 'id' to hash or some encryption.
        }

        [Function("OneLifeUrlNoEncrypt")]
        public async Task<IActionResult> OneLifeUrlNoEncrypt(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];

            // Reverse: Convert JSON string back to JsonObject
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            // Access properties from reversed JsonObject
            string accessToken = reversedJson["accessToken"]?.ToString();
            string eId = reversedJson["eId"]?.ToString();

            string returnStr = "";
            _logger.LogInformation("eId: " + eId);
            // string id = "+1kZyxo9o8SBK8ZY0fjPFDB4+iuKtsCzOfjB9QEaUiY=";
            string id = eId.Replace(" ", "+");
            // Check if Tag has been configured.
            // int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 1) // If Tag is found and configured.
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(
                    id,
                    (int)UserMode.Service,
                    null
                );
                returnStr = $"{BaseLogm8Url}/log?token=" + tokenUrl;
                return new OkObjectResult(returnStr);
            }
            else if (isConfigured == -1) // If Tag is not found.
            {
                // returnStr = "Id not found";
                // return new OkObjectResult(returnStr);
                return new NotFoundResult();
            }

            // If Tag is not found and not configured.
            returnStr = $"{BaseLogm8Url}/tag/create?token=" + id;
            return new OkObjectResult(returnStr); // NOTE: Must change 'id' to hash or some encryption.
        }

        [Function("OneLifeUrlOneStep")]
        public async Task<IActionResult> OneLifeUrlOneStep(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];

            // Reverse: Convert JSON string back to JsonObject
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            // Access properties from reversed JsonObject
            string eId = reversedJson["eId"]?.ToString();
            string userId = reversedJson["userId"]?.ToString();

            var result = DecryptString(eId);

            string returnStr = "";
            _logger.LogInformation("eId: " + result.decryptedText);
            // string id = "+1kZyxo9o8SBK8ZY0fjPFDB4+iuKtsCzOfjB9QEaUiY=";
            string id = result.decryptedText.Replace(" ", "+");
            // Check if Tag has been configured.
            // int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 1) // If Tag is found and configured.
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(
                    id,
                    (int)UserMode.Service,
                    userId
                );
                returnStr = $"{BaseLogm8Url}/log?token=" + tokenUrl;
                return new OkObjectResult(returnStr);
            }
            else if (isConfigured == -1) // If Tag is not found.
            {
                // returnStr = "Id not found";
                // return new OkObjectResult(returnStr);
                return new NotFoundResult();
            }

            // If Tag is not found and not configured.
            returnStr = $"{BaseLogm8Url}/tag/create?token=" + id;
            return new OkObjectResult(returnStr); // NOTE: Must change 'id' to hash or some encryption.
        }

        [Function("OneLifeUrlGuestMode")]
        public async Task<IActionResult> OneLifeUrlGuestMode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];

            // Reverse: Convert JSON string back to JsonObject
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            // Access properties from reversed JsonObject
            string eId = reversedJson["eId"]?.ToString();

            var result = DecryptString(eId);

            string returnStr = "";
            _logger.LogInformation("eId: " + result.decryptedText);
            // string id = "+1kZyxo9o8SBK8ZY0fjPFDB4+iuKtsCzOfjB9QEaUiY=";
            string id = result.decryptedText.Replace(" ", "+");

            string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(
                id,
                (int)UserMode.Guest,
                null
            );
            returnStr = $"{BaseLogm8Url}/log?token=" + tokenUrl;
            return new OkObjectResult(returnStr);
        }

        [Function("OneLifeUrlOneStepNoDecrypt")]
        public async Task<IActionResult> OneLifeUrlOneStepNoDecrypt(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];

            // Reverse: Convert JSON string back to JsonObject
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            // Access properties from reversed JsonObject
            string result = reversedJson["eId"]?.ToString();

            string returnStr = "";
            _logger.LogInformation("eId: " + result);
            // string id = "+1kZyxo9o8SBK8ZY0fjPFDB4+iuKtsCzOfjB9QEaUiY=";
            string id = result.Replace(" ", "+");
            _logger.LogInformation("id: " + id);
            // Check if Tag has been configured.
            // int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            _logger.LogInformation("isConfigured: " + isConfigured);
            if (isConfigured == 1) // If Tag is found and configured.
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(
                    id,
                    (int)UserMode.Service,
                    null
                );
                // returnStr = $"{BaseLogm8Url}/log?token=" + tokenUrl;
                return new OkObjectResult(
                    new { success = true, url = $"{BaseLogm8Url}/log?token={tokenUrl}" }
                );
            }
            else if (isConfigured == -1) // If Tag is not found.
            {
                // returnStr = "Id not found";
                // return new OkObjectResult(returnStr);
                return new NotFoundResult();
            }

            // If Tag is not found and not configured.
            // returnStr = $"{BaseLogm8Url}/tag/create?token=" + id;
            return new OkObjectResult(
                new { success = false, url = $"{BaseLogm8Url}/tag/create?token={id}" }
            ); // NOTE: Must change 'id' to hash or some encryption.
        }

        private readonly string key = Environment.GetEnvironmentVariable("AES_KEY"); // 32-char AES-256 key

        public (string version, string decryptedText) DecryptString(string payload)
        {
            var parts = payload.Split(':');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid payload format");

            string version = parts[0];
            string encryptedData = parts[1];
            string ivBase64 = parts[2];

            byte[] keyBytes = Encoding.UTF8.GetBytes(this.key);
            byte[] ivBytes = Convert.FromBase64String(ivBase64);
            byte[] cipherBytes = Convert.FromBase64String(encryptedData);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(
                        cipherBytes,
                        0,
                        cipherBytes.Length
                    );
                    string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                    return (version, decryptedText);
                }
            }
        }

        /// <summary>
        /// Handles the HTTP GET request to activate the OneLife URL by retrieving associated tag information.
        /// </summary>
        /// <param name="req">The HTTP request containing the query parameter 'token' used to retrieve the LogId.</param>
        /// <returns>
        /// An IActionResult containing the tag information as a JSON string if successful,
        /// or an appropriate error response if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method first retrieves the token from the query string of the incoming HTTP request. It then uses the token to fetch
        /// the LogId from the database. After obtaining the LogId, it retrieves the associated tag information and returns it in JSON format.
        /// If any step fails, an error message is returned instead.
        /// </remarks>
        [Function("ConsumeOneLifeUrl")]
        public async Task<IActionResult> ConsumeOneLifeUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string tokenKey = req.Query["token"];

            await SqlFunctions.CommitOneLifeUrlComsumed(tokenKey);

            return new OkObjectResult("");
        }

        // [Function("GetLogData")]
        // public async Task<IActionResult> GetLogData(
        //     [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        // )
        // {
        //     string tokenKey = req.Query["token"];
        //     // string str_log = await SqlFunctions.IsOneLifeUrlConsumed(tokenKey);
        //     string str_log = await SqlFunctions.IsOneLifeUrlExpired(tokenKey);
        //     _logger.LogInformation("str_log: {str_log}", str_log);

        //     var log = JsonNode.Parse(str_log).AsObject();
        //     string logId = (string)log["LogId"];

        //     _logger.LogInformation("log: {log}", log);
        //     DateTime? ttl = log["TTL"]?.GetValue<DateTime?>();
        //     _logger.LogInformation("ttl: {TTL}", ttl);

        //     if (logId == "Not Found" || !ttl.HasValue || ttl.Value < DateTime.UtcNow)
        //     {
        //         return new NotFoundResult();
        //     }

        //     Tag local_tag = await CosmosFunctions.GetTagInfo(logId);
        //     List<Record> list_records = await CosmosFunctions.GetRecordsByTagId(logId);
        //     int? view_mode = log["Mode"]?.GetValue<int>();

        //     // await SqlFunctions.CommitOneLifeUrlComsumed(tokenKey);

        //     return new OkObjectResult(
        //         new
        //         {
        //             tag = local_tag,
        //             records = list_records,
        //             mode = view_mode,
        //         }
        //     );
        // }

        [Function("GetLogDataForGarage")]
        public async Task<IActionResult> GetLogDataForGarage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];

            // Reverse: Convert JSON string back to JsonObject
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            // Access properties from reversed JsonObject
            string eId = reversedJson["eId"]?.ToString();

            var result = DecryptString(eId);

            string returnStr = "";
            _logger.LogInformation("eId: " + result.decryptedText);
            // string id = "+1kZyxo9o8SBK8ZY0fjPFDB4+iuKtsCzOfjB9QEaUiY=";
            string id = result.decryptedText.Replace(" ", "+");

            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 0) // If Tag is found and configured.
            {
                return new OkObjectResult(new { tag = "Not Found" });
            }

            Tag local_tag = await CosmosFunctions.GetTagInfo(id);
            local_tag.TagId = "";
            return new OkObjectResult(new { tag = local_tag });
        }

        [Function("AcquireNewTag")]
        public async Task<IActionResult> AcquireNewTag(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            int id = await CosmosFunctions.GetNextTagId();
            if (id == -1)
                return new NotFoundResult();
            string str_id = id.ToString();

            _logger.LogInformation("str_id: " + str_id);

            string pepper = "FalconsFC>CTFCTurtles";
            string hash_id = HashWithPepper(str_id, pepper);

            bool isInserted = await CosmosFunctions.InsertTag(hash_id);
            bool isUpdated = await CosmosFunctions.UpdateNextTagId();
            if (isUpdated == false)
                return new NotFoundResult();
            if (isInserted)
                return new OkObjectResult($"{hash_id}");

            // if (isInserted) return new OkObjectResult($"{str_id}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        // Compute a SHA-256 hash with pepper
        private static string HashWithPepper(string id, string pepper)
        {
            // Combine input, salt, and pepper
            string combinedInput = id + pepper;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        [Function("SubmitTag")]
        public async Task<IActionResult> SubmitTag(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string str_tag = req.Query["tag"];
            _logger.LogInformation("str_tag: " + str_tag);

            Tag tag = JsonConvert.DeserializeObject<Tag>(str_tag);

            // JObject tag = JObject.Parse(str_tag);
            _logger.LogInformation("tag: " + tag.TagId);

            bool status = await CosmosFunctions.UpdateTag(tag);
            _logger.LogInformation("status: " + status);

            // if (status) return new OkObjectResult($"Successfull Tag Insert");

            return new OkObjectResult(
                new
                {
                    success = status,
                    message = status ? "Successful Tag Insert" : "Failed to insert tag",
                }
            );
            // return new NotFoundResult();
        }

        [Function("UpdateTag")]
        public async Task<IActionResult> UpdateTag(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string str_tag = req.Query["tag"];
            _logger.LogInformation("str_tag: " + str_tag);

            Tag tag = JsonConvert.DeserializeObject<Tag>(str_tag);

            // JObject tag = JObject.Parse(str_tag);
            _logger.LogInformation("tag: " + tag.TagId);
            string token = tag.TagId;
            string str_log = await SqlFunctions.IsOneLifeUrlConsumed(token);
            var log = JsonNode.Parse(str_log).AsObject();
            tag.TagId = (string)log["LogId"];

            _logger.LogInformation("tag: " + tag.TagId);

            bool status = await CosmosFunctions.UpdateTag(tag);
            _logger.LogInformation("status: " + status);

            // if (status) return new OkObjectResult($"Successfull Tag Insert");

            return new OkObjectResult(
                new
                {
                    success = status,
                    message = status ? "Successful Tag Insert" : "Failed to insert tag",
                }
            );
            // return new NotFoundResult();
        }

        // [Function("SubmitRecord")]
        // public async Task<IActionResult> SubmitRecord(
        //     [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req
        // )
        // {
        //     // Retrieve form fields
        //     var formData = await req.ReadFormAsync();
        //     string token = formData["Token"];
        //     string tagId = formData["TagId"];
        //     string enteredDate = formData["EnteredDate"];
        //     string servicedDate = formData["ServicedDate"];
        //     string mechanicName = formData["MechanicName"];
        //     string odometer = formData["Odometer"];
        //     string serviceCategory = formData["serviceCategory"];
        //     string serviceOption = formData["serviceOption"];
        //     string serviceType = formData["ServiceType"];
        //     string comment = formData["Comment"];

        //     List<IFormFile> files = new List<IFormFile>();
        //     foreach (var file in formData.Files)
        //     {
        //         var fileName = file.FileName;
        //         var contentType = file.ContentType;

        //         // Optional: Read stream now or store file reference
        //         using var stream = file.OpenReadStream();

        //         files.Add(file); // Add to list
        //     }

        //     List<string> fileUrls = new();
        //     foreach (var file in files)
        //     {
        //         _logger.LogInformation("My file: " + file.FileName);
        //         string fileUrl = await BlobFunctions.UploadBlob(file);
        //         fileUrls.Add(fileUrl);
        //     }

        //     Record record = new Record
        //     {
        //         Token = token,
        //         TagId = tagId,
        //         EnteredDate = enteredDate,
        //         ServicedDate = servicedDate,
        //         MechanicName = mechanicName,
        //         Odometer = odometer,
        //         ServiceCategory = serviceCategory,
        //         ServiceType = serviceType,
        //         ServiceOption = serviceOption,
        //         Comment = comment,
        //         FileUrls = fileUrls,
        //     };

        //     string str_log = await SqlFunctions.IsOneLifeUrlConsumed(record.Token);
        //     var log = JsonNode.Parse(str_log).AsObject();
        //     record.TagId = (string)log["LogId"];
        //     record.id = Guid.NewGuid().ToString();

        //     return new OkObjectResult(await CosmosFunctions.InsertRecord(record));

        //     // if (status) return new OkObjectResult($"Successfull Record Insert");
        //     // return new NotFoundResult();
        // }

        // [Function("UpdateRecord")]
        // public async Task<IActionResult> UpdateRecord(
        //     [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req
        // )
        // {
        //     try
        //     {
        //         // Read form fields
        //         var formData = await req.ReadFormAsync();

        //         string id = formData["Id"]; // Required for updates
        //         if (string.IsNullOrEmpty(id))
        //             return new BadRequestObjectResult("Record Id is required for updating.");

        //         string token = formData["Token"];
        //         string tagId = formData["TagId"];
        //         string enteredDate = formData["EnteredDate"];
        //         string servicedDate = formData["ServicedDate"];
        //         string mechanicName = formData["MechanicName"];
        //         string odometer = formData["Odometer"];
        //         string serviceCategory = formData["serviceCategory"];
        //         string serviceOption = formData["serviceOption"];
        //         string serviceType = formData["ServiceType"];
        //         string comment = formData["Comment"];

        //         // --- Process Files (optional) ---
        //         List<IFormFile> newFiles = new();
        //         foreach (var file in formData.Files)
        //         {
        //             using var stream = file.OpenReadStream();
        //             newFiles.Add(file);
        //         }

        //         List<string> newFileUrls = new();
        //         foreach (var file in newFiles)
        //         {
        //             _logger.LogInformation("Uploading updated file: " + file.FileName);
        //             string fileUrl = await BlobFunctions.UploadBlob(file);
        //             newFileUrls.Add(fileUrl);
        //         }

        //         // --- Fetch existing record from Cosmos ---
        //         var existingRecord = await CosmosFunctions.GetRecordById(id);
        //         if (existingRecord is null)
        //             return new NotFoundObjectResult($"Record with id '{id}' not found.");
        //         _logger.LogInformation("id: {id}", id);
        //         _logger.LogInformation("tagId: {tagId}", existingRecord.TagId);
        //         // --- Merge updated values ---
        //         // existingRecord.Token = token ?? existingRecord.Token;
        //         // existingRecord.TagId = tagId ?? existingRecord.TagId;
        //         existingRecord.EnteredDate = enteredDate ?? existingRecord.EnteredDate;
        //         existingRecord.ServicedDate = servicedDate ?? existingRecord.ServicedDate;
        //         existingRecord.MechanicName = mechanicName ?? existingRecord.MechanicName;
        //         existingRecord.Odometer = odometer ?? existingRecord.Odometer;
        //         existingRecord.ServiceCategory = serviceCategory ?? existingRecord.ServiceCategory;
        //         existingRecord.ServiceType = serviceType ?? existingRecord.ServiceType;
        //         existingRecord.ServiceOption = serviceOption ?? existingRecord.ServiceOption;
        //         existingRecord.Comment = comment ?? existingRecord.Comment;

        //         // Append new files to existing list if any
        //         if (newFileUrls.Count > 0)
        //         {
        //             existingRecord.FileUrls ??= new List<string>();
        //             existingRecord.FileUrls.AddRange(newFileUrls);
        //         }

        //         // --- Write back to Cosmos ---
        //         var updated = await CosmosFunctions.PatchRecord(id, existingRecord);

        //         return new OkObjectResult(updated);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error updating record");
        //         return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //     }
        // }

        
        // [Function("RequestRecords")]
        // public async Task<IActionResult> RequestRecords(
        //     [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        // )
        // {
        //     string token = req.Query["token"];
        //     string str_log = await SqlFunctions.IsOneLifeUrlConsumed(token);
        //     var log = JsonNode.Parse(str_log).AsObject();
        //     string logId = (string)log["LogId"];

        //     List<Record> records = await CosmosFunctions.GetRecordsByTagId(logId);

        //     string jsonString = JsonConvert.SerializeObject(records, Formatting.Indented);
        //     return new OkObjectResult(jsonString);
        // }

        private string GenerateAccessToken()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Decrypts an encrypted text using RSA encryption and the provided private key.
        /// </summary>
        /// <param name="rsa">The RSA instance used for decryption.</param>
        /// <param name="encryptedText">The encrypted text (in Base64 format) that needs to be decrypted.</param>
        /// <param name="privateKey">The RSA private key used for decryption (in XML format).</param>
        /// <returns>
        /// A string representing the decrypted text in UTF-8 encoding.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the input parameters (rsa, encryptedText, privateKey) is null or empty.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if the encrypted text is not a valid Base64 string.
        /// </exception>
        /// <exception cref="CryptographicException">
        /// Thrown if an error occurs during the decryption process (e.g., invalid private key or mismatched encryption padding).
        /// </exception>
        /// <remarks>
        /// This method initializes the provided RSA instance with the private key in XML format,
        /// then decrypts the provided encrypted text using RSA decryption with OAEP padding (SHA1).
        /// The decrypted byte array is then converted to a UTF-8 string and returned.
        /// </remarks>
        private static string DecryptText(RSA rsa, string encryptedText, string privateKey)
        {
            rsa.FromXmlString(privateKey);
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }

        private string EncryptText(RSA rsa, string id, string publicKey)
        {
            rsa.FromXmlString(publicKey);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(id);
            byte[] encryptedBytes = rsa.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA256);
            string encryptedString = Convert.ToBase64String(encryptedBytes);

            return encryptedString;
        }

        [Function("runSqlTest")]
        public async Task<HttpResponseData> RunSqlTest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("runSqlTest");
            var dbPath =
                @"C:\Users\ikdev\Desktop\IKDevelopers\Azure_Workspace\LogMate\Data\ServiceOptions.db";
            var sqlPath =
                @"C:\Users\ikdev\Desktop\IKDevelopers\Azure_Workspace\LogMate\Data\ServiceOptions.sql";

            var connectionString = $"Data Source={dbPath};";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Execute the SQL script
            var sql = await File.ReadAllTextAsync(sqlPath);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            logger.LogInformation($"Running SQL:\n{sql}");

            cmd.ExecuteNonQuery();

            // Build a response
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Database initialized successfully!");
            return response;
        }

        [Function("ReplaceNfcTag")]
        public async Task<HttpResponseData> ReplaceNfcTag(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("ReplaceNfcTag");

            ReplaceTagRequest? request;

            try
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();
                request = JsonConvert.DeserializeObject<ReplaceTagRequest>(body);
            }
            catch
            {
                return await CreateResponseAsync(
                    req,
                    HttpStatusCode.BadRequest,
                    "Invalid JSON payload"
                );
            }

            if (
                request == null
                || string.IsNullOrWhiteSpace(request.OldTagId)
                || string.IsNullOrWhiteSpace(request.NewTagId)
            )
            {
                return await CreateResponseAsync(
                    req,
                    HttpStatusCode.BadRequest,
                    "OldTagId and NewTagId are required"
                );
            }

            try
            {
                var count = await CosmosFunctions.ReplaceAllRecordsForTagAsync(
                    request.OldTagId,
                    request.NewTagId,
                    logger
                );

                return await CreateResponseAsync(
                    req,
                    HttpStatusCode.OK,
                    $"Successfully migrated {count} records."
                );
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex, "Cosmos DB failure");

                return await CreateResponseAsync(
                    req,
                    HttpStatusCode.InternalServerError,
                    "Database operation failed"
                );
            }
        }

        private static async Task<HttpResponseData> CreateResponseAsync(
            HttpRequestData req,
            HttpStatusCode status,
            string message
        )
        {
            var response = req.CreateResponse(status);
            await response.WriteStringAsync(message);
            return response;
        }

        public class ServiceOptionResult
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<ServiceOptionResult> Children { get; set; } =
                new List<ServiceOptionResult>();
            public List<string> ServiceTypes { get; set; } = new List<string>();
        }

        public class FlatServiceOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int? ParentId { get; set; }
            public List<string> ServiceTypes { get; set; } = new List<string>();
        }

        private static readonly SemaphoreSlim _downloadLock = new SemaphoreSlim(1, 1);
        private const string BlobContainerName = "assets";
        private const string BlobFileName = "ServiceOptions.db";
        private static string? _dbPath;
        private static bool _dbReady = false;

        [Function("GetMotorbikeOptions")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("GetMotorbikeOptions");

            const string blobUrl =
                "https://bloblogmate.blob.core.windows.net/assets/ServiceOptions.db";
            var tempDir = Path.GetTempPath();
            var localDbPath = Path.Combine(tempDir, "ServiceOptions.db");

            try
            {
                // ✅ Download once per runtime
                await _downloadLock.WaitAsync();
                try
                {
                    if (!File.Exists(localDbPath))
                    {
                        logger.LogInformation("Downloading ServiceOptions.db from blob...");
                        using var http = new HttpClient();
                        using var stream = await http.GetStreamAsync(blobUrl);
                        using var fs = File.Create(localDbPath);
                        await stream.CopyToAsync(fs);
                        logger.LogInformation("Database downloaded successfully.");
                    }
                }
                finally
                {
                    _downloadLock.Release();
                }

                // ✅ Load file DB into an in-memory copy
                using var source = new SqliteConnection(
                    $"Data Source={localDbPath};Mode=ReadOnly;"
                );
                await source.OpenAsync();

                using var memory = new SqliteConnection("Data Source=:memory:;Mode=ReadWrite;");
                await memory.OpenAsync();
                source.BackupDatabase(memory);

                logger.LogInformation("Database loaded into memory.");

                // ✅ Now safely query the in-memory DB (no file lock risk)
                var checkCmd = memory.CreateCommand();
                checkCmd.CommandText =
                    "SELECT name FROM sqlite_master WHERE type='table' AND name='ServiceOption';";
                var tableName = await checkCmd.ExecuteScalarAsync();
                if (tableName == null)
                    throw new Exception("Table 'ServiceOption' not found in SQLite database.");

                // =======================
                // 🚗 1. Motorbike hierarchy query
                // =======================
                var sql =
                    @"
                    WITH RECURSIVE MotorbikeOptions(Id, Name, Description, ParentId) AS (
                        SELECT so.Id, so.Name, so.Description, NULL AS ParentId
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
                    SELECT mo.Id, mo.Name, mo.Description, mo.ParentId,
                        GROUP_CONCAT(st.Name) AS ServiceTypes
                    FROM MotorbikeOptions mo
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = mo.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    GROUP BY mo.Id, mo.Name, mo.Description, mo.ParentId;
                    ";

                var flatResults = new List<FlatServiceOption>();
                using (var cmd = memory.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        flatResults.Add(
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
                }

                // =======================
                // 🧾 2. Ownership hierarchy query
                // =======================
                var ownershipSql =
                    @"
                    SELECT so.Id, so.Name, so.Description, NULL AS ParentId,
                        GROUP_CONCAT(st.Name) AS ServiceTypes
                    FROM ServiceOption so
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = so.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    WHERE so.Name = 'Ownership'
                    GROUP BY so.Id, so.Name, so.Description;
                    ";

                var ownershipResults = new List<FlatServiceOption>();

                using (var cmd = memory.CreateCommand())
                {
                    cmd.CommandText = ownershipSql;

                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var result = new FlatServiceOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ParentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ServiceTypes = reader.IsDBNull(4)
                                ? new List<string>()
                                : reader.GetString(4).Split(',').ToList(),
                        };

                        ownershipResults.Add(result);
                    }
                }

                // =======================
                // 🏗️ Build hierarchies
                // =======================
                List<ServiceOptionResult> BuildHierarchy(List<FlatServiceOption> flatList)
                {
                    var dict = flatList
                        .GroupBy(x => x.Id)
                        .Select(g => new ServiceOptionResult
                        {
                            Id = g.Key,
                            Name = g.First().Name,
                            Description = g.First().Description,
                            ServiceTypes = g.SelectMany(f => f.ServiceTypes).Distinct().ToList(),
                        })
                        .ToDictionary(x => x.Id);

                    foreach (var node in flatList)
                    {
                        if (node.ParentId.HasValue && dict.ContainsKey(node.ParentId.Value))
                            dict[node.ParentId.Value].Children.Add(dict[node.Id]);
                    }

                    return dict
                        .Values.Where(x => !flatList.Any(r => r.Id == x.Id && r.ParentId.HasValue))
                        .ToList();
                }

                var motorbikeHierarchy = BuildHierarchy(flatResults);
                var ownershipHierarchy = BuildHierarchy(ownershipResults);

                // =======================
                // 📦 Combine results
                // =======================
                var combined = new
                {
                    MotorbikeOptions = motorbikeHierarchy,
                    OwnershipOptions = ownershipHierarchy,
                };

                // =======================
                // 📤 Return JSON response
                // =======================
                var json = JsonConvert.SerializeObject(combined, Formatting.Indented);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(json);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Server error: {ex.Message}");
                return response;
            }
        }

        [Function("UpdateServiceOptionsDb")]
        public async Task<HttpResponseData> UpdateServiceOptionsDb(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("UpdateServiceOptionsDb");

            const string blobUrl =
                "https://bloblogmate.blob.core.windows.net/assets/ServiceOptions.db";
            var tempDir = Path.GetTempPath();
            var localDbPath = Path.Combine(tempDir, "ServiceOptions.db");

            try
            {
                await _downloadLock.WaitAsync();
                try
                {
                    logger.LogInformation("Forcing update of ServiceOptions.db from blob...");

                    using var http = new HttpClient();
                    using var stream = await http.GetStreamAsync(blobUrl);
                    using var fs = File.Create(localDbPath); // Overwrites existing file
                    await stream.CopyToAsync(fs);

                    logger.LogInformation("Database file updated successfully.");
                }
                finally
                {
                    _downloadLock.Release();
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("ServiceOptions.db has been updated.");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error updating DB: {ex}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Failed to update DB: {ex.Message}");
                return response;
            }
        }

        public class ReplaceTagRequest
        {
            public string OldTagId { get; set; } = default!;
            public string NewTagId { get; set; } = default!;
        }

        [Function("GetServiceRecordHierarchy")]
        public async Task<HttpResponseData> GetServiceRecordHierarchy(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("GetServiceRecordHierarchy");

            try
            {
                var result = await SqlFunctions.GetHierarchy();

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(
                    JsonConvert.SerializeObject(result, Formatting.Indented)
                );

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get service record hierarchy");

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Failed to get service record hierarchy");
                return response;
            }
        }

        public class AddServiceTypeRequest
        {
            public string OptionId { get; set; }
            public string ServiceTypeId { get; set; }
        }

        [Function("AddServiceType")]
        public async Task<HttpResponseData> AddServiceType(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            // Read request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            var payload = JsonConvert.DeserializeObject<AddServiceTypeRequest>(body);

            if (payload == null || string.IsNullOrEmpty(payload.OptionId))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload");
                return bad;
            }

            var inserted = await SqlFunctions.AddServiceTypeById(payload.OptionId, payload.ServiceTypeId);

            var response = req.CreateResponse(
                inserted ? HttpStatusCode.OK : HttpStatusCode.Conflict
            );

            await response.WriteStringAsync(
                inserted
                    ? "Service type linked successfully"
                    : "Service type already exists"
            );

            return response;

        }


        public class AddParentOptionRequest
        {
            public int OptionId { get; set; }
            public int ParentId { get; set; }
        }

        [Function("AddParentOption")]
        public async Task<HttpResponseData> AddParentOption(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            var payload = JsonConvert.DeserializeObject<AddParentOptionRequest>(body);

            // FIX: validate ints properly
            if (payload == null || payload.OptionId <= 0 || payload.ParentId <= 0)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload");
                return bad;
            }

            var inserted = await SqlFunctions.AddParentOptionById(
                payload.OptionId,
                payload.ParentId
            );

            var response = req.CreateResponse(
                inserted ? HttpStatusCode.OK : HttpStatusCode.Conflict
            );

            await response.WriteStringAsync(
                inserted
                    ? "Parent option linked successfully"
                    : "Parent option already exists"
            );

            return response;
        }



        public class CreateServiceOptionRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int CategoryId { get; set; }
        }

[Function("CreateServiceOption")]
        public async Task<HttpResponseData> CreateServiceOption(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            var payload = JsonConvert.DeserializeObject<CreateServiceOptionRequest>(body);

            // Validation
            if (payload == null ||
                string.IsNullOrWhiteSpace(payload.Name))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload");
                return bad;
            }

            var inserted = await SqlFunctions.CreateNewServiceOption(
                payload.Name.Trim(),
                payload.Description,
                payload.CategoryId
            );

            var response = req.CreateResponse(
                inserted ? HttpStatusCode.OK : HttpStatusCode.Conflict
            );

            await response.WriteStringAsync(
                inserted
                    ? "Service option created successfully"
                    : "Service option name already exists"
            );

            return response;


    }
    }
}
