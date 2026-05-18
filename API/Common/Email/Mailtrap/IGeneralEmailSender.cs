namespace API.Common.Email.Mailtrap;

public interface IGeneralEmailSender
{
    Task SendEmailAsync(string toEmail, string? toName, string subject, string message);
}