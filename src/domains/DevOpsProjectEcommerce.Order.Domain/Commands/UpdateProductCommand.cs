using MediatR;
using DevOpsProjectEcommerce.Shared.InOut.Responses;

namespace DevOpsProjectEcommerce.Order.Domain.Commands
{
    public sealed class UpdateOrderCommand: IRequest<OrderResponse>
    {
        public int Id { get; init; }
        public int ProductId { get; init; }
        public int Quantity { get; init; } 
        public string BoughtBy { get; set; }

        public UpdateOrderCommand WithBoughBy(string boughtBy)
        {
            BoughtBy = boughtBy;
            return this;
        }
    }
}
