using MediatR;

namespace DevOpsProjectEcommerce.Order.Domain.Commands
{
    public sealed class DeleteOrderCommand: IRequest
    {
        public DeleteOrderCommand(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
