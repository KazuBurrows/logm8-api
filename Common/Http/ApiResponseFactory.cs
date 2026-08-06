using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace LogMate.Common.Http;

public static class ApiResponseFactory
{
    public static ApiResponse Build(
        HttpStatusCode status,
        string title,
        string detail,
        Dictionary<string, object>? extensions = null)
        => new ApiResponse
        {
            Title = title,
            Status = (int)status,
            Detail = detail,
            Extensions = extensions
        };

    private static async Task<HttpResponseData> Create(
        HttpRequestData req,
        HttpStatusCode status,
        string title,
        string detail,
        Dictionary<string, object>? extensions = null)
    {
        var response = req.CreateResponse(status);
        await response.WriteAsJsonAsync(Build(status, title, detail, extensions));
        return response;
    }

    public static Task<HttpResponseData> Ok(
        HttpRequestData req,
        string message)
        => Create(req, HttpStatusCode.OK, "Success", message);

    public static Task<HttpResponseData> Ok(
        HttpRequestData req,
        string message,
        Dictionary<string, object> extensions)
        => Create(req, HttpStatusCode.OK, "Success", message, extensions);

    public static Task<HttpResponseData> UnsupportedMediaType(
        HttpRequestData req,
        string message)
        => Create(req, HttpStatusCode.UnsupportedMediaType, "Unsupported Media Type", message);

    public static Task<HttpResponseData> NotFound(
        HttpRequestData req,
        string message)
        => Create(req, HttpStatusCode.NotFound, "Not Found", message);

    public static Task<HttpResponseData> BadRequest(
        HttpRequestData req,
        string message)
        => Create(req, HttpStatusCode.BadRequest, "Bad Request", message);

    public static Task<HttpResponseData> Forbidden(
        HttpRequestData req,
        string message)
        => Create(req, HttpStatusCode.Forbidden, "Forbidden", message);

    public static Task<HttpResponseData> ServerError(
        HttpRequestData req,
        string message = "An unexpected error occurred.")
        => Create(req, HttpStatusCode.InternalServerError, "Server Error", message);

    public static Task<HttpResponseData> Success(
        HttpRequestData req,
        string entity,
        int id,
        ActionType action)
    {
        string actionStr = action.ToString().ToLower();

        var statusCode = action switch
        {
            ActionType.Created => HttpStatusCode.Created,
            ActionType.Deleted => HttpStatusCode.OK,  // ← was NoContent
            _ => HttpStatusCode.OK
        };

        return Create(
            req,
            statusCode,
            $"Successfully {char.ToUpper(actionStr[0]) + actionStr[1..]}",
            $"{entity} with id {id} was successfully {actionStr}.",
            new Dictionary<string, object> { { "id", id } }
        );
    }

    public static Task<HttpResponseData> Success<T>(
        HttpRequestData req,
        string entity,
        T item,
        ActionType action)
    {
        string actionStr = action.ToString().ToLower();

        var statusCode = action switch
        {
            ActionType.Created => HttpStatusCode.Created,
            ActionType.Deleted => HttpStatusCode.OK,  // ← was NoContent
            _ => HttpStatusCode.OK
        };

        return Create(
            req,
            statusCode,
            $"Successfully {char.ToUpper(actionStr[0]) + actionStr[1..]}",
            $"{entity} was successfully {actionStr}.",
            new Dictionary<string, object> { { "item", item! } }
        );
    }

    public static Task<HttpResponseData> Conflict(
        HttpRequestData req,
        string entity,
        List<int> ids)
    {
        var extensions = new Dictionary<string, object>
        {
            { "ids", ids }
        };

        return Create(
            req,
            HttpStatusCode.Conflict,
            "Conflict",
            $"{entity} with ids {string.Join(", ", ids)} had a conflict.",
            extensions
        );
    }
}
