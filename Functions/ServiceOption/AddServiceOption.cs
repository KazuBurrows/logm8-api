using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddServiceOption
{
    private readonly IServiceOptionService _service;

    public AddServiceOption(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("AddServiceOption")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<AddServiceOptionRequest>(body);

        if (payload.Name.IsNullOrEmpty())
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid payload");
            return bad;
        }

        var response = _service.AddServiceOptionAsync(payload).Result switch
        {
            HttpStatusCode.OK => req.CreateResponse(HttpStatusCode.OK),
            HttpStatusCode.Conflict => req.CreateResponse(HttpStatusCode.Conflict),
            _ => req.CreateResponse(HttpStatusCode.InternalServerError),
        };

        await response.WriteStringAsync(
            response.StatusCode == HttpStatusCode.OK
                ? "Service option created successfully"
                : "Service option name already exists"
        );

        return response;
    }
}
