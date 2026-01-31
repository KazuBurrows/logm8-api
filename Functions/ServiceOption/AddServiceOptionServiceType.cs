using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddServiceOptionServiceType
{
    private readonly IServiceOptionService _service;

    public AddServiceOptionServiceType(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("AddServiceOptionServiceType")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<AddServiceTypeRequest>(body);

        if (payload == null || string.IsNullOrEmpty(payload.OptionId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid payload");
            return bad;
        }

        var response = _service.AddServiceOptionServiceTypeAsync(payload).Result switch
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
