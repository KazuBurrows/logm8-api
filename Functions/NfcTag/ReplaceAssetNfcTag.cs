using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class ReplaceAssetNfcTag
{
    private readonly INfcTagService _service;

    public ReplaceAssetNfcTag(INfcTagService service)
    {
        _service = service;
    }

    [Function("ReplaceAssetNfcTag")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<ReplaceNfcTagRequest>(body);
        if (
            payload == null
            || string.IsNullOrEmpty(payload.OldTagId)
            || string.IsNullOrEmpty(payload.NewTagId)
        )
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid request payload");
            return bad;
        }

        var response = _service.ReplaceAssetNfcTagAsync(payload).Result switch
        {
            HttpStatusCode.OK => req.CreateResponse(HttpStatusCode.OK),
            HttpStatusCode.Conflict => req.CreateResponse(HttpStatusCode.Conflict),
            _ => req.CreateResponse(HttpStatusCode.InternalServerError),
        };

        await response.WriteStringAsync(
            response.StatusCode == HttpStatusCode.OK
                ? "Service type linked successfully"
                : "Service type already exists"
        );

        return response;
    }
}
