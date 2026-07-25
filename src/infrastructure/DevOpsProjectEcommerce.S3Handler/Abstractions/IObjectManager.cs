using System.Net;
using DevOpsProjectEcommerce.S3Handler.Models;

namespace DevOpsProjectEcommerce.S3Handler.Abstractions;

public interface IObjectManager
{
    Task<string> GetPreSignedUrlAsync(string objectKey, CancellationToken cancellationToken);
    Task<string> PutObjectAsync(ObjectRegister @object, CancellationToken cancellationToken);
    Task<HttpStatusCode> DeleteObjectAsync(string objectKey, CancellationToken cancellationToken);
}
