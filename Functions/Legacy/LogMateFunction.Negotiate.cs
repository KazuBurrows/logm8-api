using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Company.Function
{
    public partial class LogMateFunction
    {
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
                return new OkObjectResult(resJson.ToJsonString());
            }

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
