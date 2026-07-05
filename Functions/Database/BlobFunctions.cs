using LogMate.Domain.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Company.Function
{
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
                throw;
            }
        }

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
                _ => "application/octet-stream",
            };
        }
    }
}
