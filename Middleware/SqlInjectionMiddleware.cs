using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace LogMate.Middleware;

public class SqlInjectionMiddleware : IFunctionsWorkerMiddleware
{
    // Targets attack patterns rather than individual keywords to minimise false positives.
    // Parameterised queries remain the primary defence; this is an additional layer.
    private static readonly Regex SqlPattern = new(
        @"(
              --                                          # SQL line comment
            | \/\*                                        # Block comment start
            | \bUNION\s+(?:ALL\s+)?SELECT\b              # UNION-based injection
            | ;\s*(?:DROP|ALTER|TRUNCATE|CREATE|EXEC(?:UTE)?)\b   # Stacked DDL/exec
            | \bEXEC\s*\(                                 # Dynamic EXEC()
            | xp_\w+                                     # Extended stored procs (xp_cmdshell etc.)
            | \bWAITFOR\s+DELAY\b                        # Time-based blind injection (SQL Server)
            | \bSLEEP\s*\(                               # Time-based blind injection (MySQL)
            | \bBENCHMARK\s*\(                           # Time-based blind injection (MySQL)
            | 0x[0-9a-fA-F]{4,}                         # Hex-encoded payloads
            | '\s*(?:OR|AND)\s+.+=                       # Boolean injection  e.g. ' OR 1=1
        )",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace
    );

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var req = await context.GetHttpRequestDataAsync();

        if (req != null)
        {
            if (IsSuspicious(Uri.UnescapeDataString(req.Url.Query)))
            {
                await RejectAsync(context, req);
                return;
            }

            if (await BodyIsSuspiciousAsync(req))
            {
                await RejectAsync(context, req);
                return;
            }
        }

        await next(context);
    }

    private static async Task<bool> BodyIsSuspiciousAsync(HttpRequestData req)
    {
        // Skip multipart — boundary parsing is handled inside each function and
        // reading it here would corrupt the stream for the actual handler.
        req.Headers.TryGetValues("Content-Type", out var contentTypeValues);
        var contentType = contentTypeValues?.FirstOrDefault() ?? string.Empty;

        if (contentType.Contains("multipart", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!req.Body.CanSeek || req.Body.Length == 0)
            return false;

        using var reader = new StreamReader(req.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        req.Body.Position = 0;

        return IsSuspicious(body);
    }

    private static bool IsSuspicious(string input) =>
        !string.IsNullOrEmpty(input) && SqlPattern.IsMatch(input);

    private static async Task RejectAsync(FunctionContext context, HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(new { error = "Request contains invalid input." });
        context.GetInvocationResult().Value = response;
    }
}
