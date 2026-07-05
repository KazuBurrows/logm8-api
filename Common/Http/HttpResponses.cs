using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace LogMate.Common.Http;

public static class HttpResponses
{
    public static async Task<HttpResponseData> OkAsync(HttpRequestData req, object? body = null)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        if (body is not null)
        {
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteAsJsonAsync(body);
        }
        return response;
    }

    public static HttpResponseData BadRequest(HttpRequestData req, string message = "Bad Request")
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.WriteString(message);
        return response;
    }

    public static HttpResponseData NotFound(HttpRequestData req, string message = "Not Found")
    {
        var response = req.CreateResponse(HttpStatusCode.NotFound);
        response.WriteString(message);
        return response;
    }

    public static HttpResponseData InternalServerError(HttpRequestData req, string message = "Internal Server Error")
    {
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);
        response.WriteString(message);
        return response;
    }

    public static HttpResponseData StatusResponse(HttpRequestData req, HttpStatusCode statusCode)
    {
        return req.CreateResponse(statusCode);
    }
}
