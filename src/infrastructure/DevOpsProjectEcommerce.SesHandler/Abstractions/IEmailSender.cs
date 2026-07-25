using DevOpsProjectEcommerce.SesHandler.Models;

namespace DevOpsProjectEcommerce.SesHandler.Abstractions;

public interface IEmailSender
{
    Task SendAsync
    (
        EmailParams @params,
        CancellationToken cancellationToken
    );
}
