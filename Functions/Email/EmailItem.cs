using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using static System.Net.WebUtility;

public class EmailItem
{
    private readonly IEmailService _email;
    private readonly ILogger<EmailItem> _logger;

    public EmailItem(IEmailService email, ILogger<EmailItem> logger)
    {
        _email = email;
        _logger = logger;
    }

    [Function("EmailItem")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var body = await req.ReadFromJsonAsync<EmailItemRequest>();

        if (body == null || string.IsNullOrEmpty(body.FromEmail))
            throw new BadRequestException("Missing required fields: fromEmail, fromName, message.");

        var safeName = HtmlEncode(body.FromName);
        var safeEmail = HtmlEncode(body.FromEmail);
        var safeMessage = HtmlEncode(body.Message);

        var subject = $"New enquiry from {safeName}";

        var htmlBody = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #333;">New Website Enquiry</h2>
                <table style="width: 100%; border-collapse: collapse;">
                    <tr>
                        <td style="padding: 8px; font-weight: bold; color: #555;">Name</td>
                        <td style="padding: 8px;">{safeName}</td>
                    </tr>
                    <tr style="background-color: #f9f9f9;">
                        <td style="padding: 8px; font-weight: bold; color: #555;">Email</td>
                        <td style="padding: 8px;"><a href="mailto:{safeEmail}">{safeEmail}</a></td>
                    </tr>
                    <tr>
                        <td style="padding: 8px; font-weight: bold; color: #555; vertical-align: top;">Message</td>
                        <td style="padding: 8px;">{safeMessage}</td>
                    </tr>
                </table>
                <hr style="margin-top: 24px; border: none; border-top: 1px solid #eee;" />
                <p style="font-size: 12px; color: #aaa;">Sent via logm8 website enquiry form</p>
            </div>
            """;

        await _email.SendAsync(
            toEmail: "joel@logm8.com",
            toName: "Joel",
            subject: subject,
            htmlBody: htmlBody
        );

        return await ApiResponseFactory.Ok(req, "Email sent successfully.");
    }
}

public class EmailItemRequest
{
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";
    public string Message { get; set; } = "";
}