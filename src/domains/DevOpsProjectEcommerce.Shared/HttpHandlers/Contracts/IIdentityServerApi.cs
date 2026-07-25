using DevOpsProjectEcommerce.Shared.InOut.Requests;
using Refit;

namespace DevOpsProjectEcommerce.Shared.HttpHandlers.Contracts
{
    public interface IIdentityServerApi
    {
        [Post("/api/auth")]
        Task<string> AuthAsync(AuthRequest userRequest);
    }
}
