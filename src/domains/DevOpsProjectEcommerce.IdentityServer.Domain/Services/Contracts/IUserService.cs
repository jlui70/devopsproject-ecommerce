using DevOpsProjectEcommerce.Shared.InOut.Requests;

namespace DevOpsProjectEcommerce.IdentityServer.Domain.Services.Contracts
{
    public interface IUserService
    {
        Task<bool> CheckPasswordAsync(AuthRequest userRequest);
    }
}
