using DevOpsProjectEcommerce.Shared.InOut.Responses;
using Refit;

namespace DevOpsProjectEcommerce.Shared.HttpHandlers.Contracts;

public interface IOrderApi
{
    [Get("/api/request/{id}")]
    [Headers("Authorization: Bearer")]
    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(int id);
}
