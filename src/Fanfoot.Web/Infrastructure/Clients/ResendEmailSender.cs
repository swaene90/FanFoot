using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fanfoot.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace Fanfoot.Infrastructure.Clients;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public ResendEmailSender(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
    }

    public async Task SendPasswordResetAsync(string email, string resetUrl, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Resend:ApiKey"];
        var from = _configuration["Resend:FromEmail"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Resend:ApiKey and Resend:FromEmail must be configured.");

        var link = WebUtility.HtmlEncode(resetUrl);
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from,
                to = new[] { email },
                subject = "Reset your FanFoot password",
                html = $"<p>We received a request to reset your FanFoot password.</p><p><a href=\"{link}\">Reset password</a></p><p>This link expires in one hour. If you did not request it, you can safely ignore this email.</p>"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
