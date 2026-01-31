using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddParentServiceOption
{
    private readonly IServiceOptionService _service;

    public AddParentServiceOption(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("AddParentServiceOption")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<AddParentOptionRequest>(body);

        if (payload == null || payload.OptionId <= 0 || payload.ParentId <= 0)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid payload");
            return bad;
        }

        var response = _service.AddParentServiceOptionAsync(payload).Result switch
        {
            HttpStatusCode.OK => req.CreateResponse(HttpStatusCode.OK),
            HttpStatusCode.Conflict => req.CreateResponse(HttpStatusCode.Conflict),
            _ => req.CreateResponse(HttpStatusCode.InternalServerError),
        };

        await response.WriteStringAsync(
            response.StatusCode == HttpStatusCode.OK
                ? "Parent option linked successfully"
                : "Parent option already exists"
        );

        return response;
    }
}
