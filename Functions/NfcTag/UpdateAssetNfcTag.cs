using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class UpdateAssetNfcTagAsync
{
    private readonly INfcTagService _service;

    public UpdateAssetNfcTagAsync(INfcTagService service)
    {
        _service = service;
    }

    [Function("UpdateAssetNfcTagAsync")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<UpdateAssetNfcTagRequest>(body);

            if (request == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(
                    new { success = false, message = "Invalid request body" }
                );
                return bad;
            }

            var success = await _service.UpdateAssetNfcTagAsync(request);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(
                new
                {
                    success,
                    message = success
                        ? "Tag updated successfully"
                        : "Tag not found or update failed",
                }
            );

            return response;
        }
        catch (Exception ex)
        {
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { success = false, message = ex.Message });

            return response;
        }
    }
}
