using MediatR;
using DevOpsProjectEcommerce.Shared.InOut.Responses;

namespace DevOpsProjectEcommerce.Main.Domain.Commands
{
    public sealed class RegisterProductCommand: IRequest<ProductResponse>
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }
}
