using System.Text;
using System.Text.Json;
using API.Shared.Common.Email.Mailtrap.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetBlocks.Models.Environment;

namespace API.Shared.Common.Email.Mailtrap;

public class MailtrapEmailSender(
    IOptions<EmailConnection> connectionOptions,
    ILogger<MailtrapEmailSender> logger) : IGeneralEmailSender
{
    private readonly ILogger _logger = logger;
    private readonly EmailConnection _connection = connectionOptions.Value;
    
    public async Task SendEmailAsync(string toEmail, string? toName, string subject, string message)
    {
        if (string.IsNullOrEmpty(_connection.Token))
        {
            throw new Exception("Null EmailAuthKey");
        }

        var email = new EmailWithHtml
        {
            From = new Address
            {
                Email = _connection.FromAddress
            },
            To = new List<Address>
            {
                new Address
                {
                    Email = toEmail
                },
            },
            Subject = subject,
            Html = message
        };


        await Execute(email);
    }

    private async Task Execute(EmailWithHtml email)
    {
        var endpoint = new StringBuilder();
        endpoint.Append($"https://{_connection.Host}/api");
        if (string.IsNullOrEmpty(_connection.TestInbox))
        {
            endpoint.Append("/send");
        }
        else
        {
            endpoint.Append($"/send/{_connection.TestInbox}");
        }

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint.ToString());
        request.Headers.Add("Authorization", $"Bearer {_connection.Token}");
        
        var jsonContent = JsonSerializer.Serialize(email);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine(responseContent);

        _logger.LogInformation("Email to {EmailAddress} sent!", email.To.First().Email);
    }
}