using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Email;

public class EmailService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public EmailService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _apiKey =
            configuration["BrevoSettings:ApiKey"]!;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent)
    {
        _httpClient.DefaultRequestHeaders.Clear();

        _httpClient.DefaultRequestHeaders.Add(
            "api-key",
            _apiKey);

        var payload = new
        {
            sender = new
            {
                name = "SkillToRole LMS",
                email = "saiveertech@gmail.com"
            },
            to = new[]
            {
                new
                {
                    email = toEmail,
                    name = toName
                }
            },
            subject,
            htmlContent
        };

        var content =
            new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

        var response =
            await _httpClient.PostAsync(
                "https://api.brevo.com/v3/smtp/email",
                content);
            var responseBody = await response.Content.ReadAsStringAsync();

Console.WriteLine("STATUS : " + response.StatusCode);
Console.WriteLine("RESPONSE : " + responseBody);
Console.WriteLine("API KEY : " + _apiKey);

response.EnsureSuccessStatusCode();

    }
}