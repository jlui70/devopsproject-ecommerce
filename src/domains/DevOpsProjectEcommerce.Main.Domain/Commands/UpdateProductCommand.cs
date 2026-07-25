using MediatR;
using DevOpsProjectEcommerce.Shared.InOut.Responses;

namespace DevOpsProjectEcommerce.Main.Domain.Commands
{
    public sealed class UpdateProductCommand: IRequest<ProductResponse>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }
}
