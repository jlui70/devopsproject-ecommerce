using DevOpsProjectEcommerce.Shared.Enums;

namespace DevOpsProjectEcommerce.Shared.InOut.Responses;

public record OrderResponse
(
    int Id, 
    ProductResponse Product, 
    int Quantity, 
    string BoughtBy,
    OrderStatus StatusId,
    string Status
);

