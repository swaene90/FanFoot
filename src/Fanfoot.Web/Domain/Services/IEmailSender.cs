namespace Fanfoot.Domain.Services;

public interface IEmailSender
{
    Task SendPasswordResetAsync(string email, string resetUrl, CancellationToken cancellationToken = default);
}
