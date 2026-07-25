using DevOpsProjectEcommerce.Order.Domain.Commands;
using DevOpsProjectEcommerce.Shared.Enums;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Order.Domain.Mappings
{
    public static class ProductCommandToEntityMapping
    {
        public static OrderEntity MapToEntity(this CreateOrderCommand order)
        {
            return new OrderEntity
            (
                productId: order.ProductId,
                quantity: order.Quantity,
                boughtBy:order.BoughtBy,
                statusId: OrderStatus.Confirmado
            );
        }
        
        public static OrderEntity MapToEntity(this UpdateOrderCommand order)
        {
            return new OrderEntity
            (
                productId: order.ProductId,
                quantity: order.Quantity,
                boughtBy:order.BoughtBy,
                statusId: OrderStatus.Pendente
            );
        }
    }
}
