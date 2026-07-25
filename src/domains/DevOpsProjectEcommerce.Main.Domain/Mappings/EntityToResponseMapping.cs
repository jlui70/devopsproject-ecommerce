using DevOpsProjectEcommerce.Shared.InOut.Responses;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Main.Domain.Mappings
{
    public static class EntityToResponseMapping
    {
        public static StockResponse MapToResponse(this StockEntity stock)
        {
            return new StockResponse(stock.Id, stock.Product.MapToResponse(), stock.Quantity);
        }

        public static ProductResponse MapToResponse(this ProductEntity product)
        {
            return new ProductResponse(product.Id, product.Name, product.Price);
        }

        public static IEnumerable<ProductResponse> MapToResponse(this IEnumerable<ProductEntity> products)
        {
            return products.Select(product => product.MapToResponse());
        }
        
        public static IEnumerable<StockResponse> MapToResponse(this IEnumerable<StockEntity> stocks)
        {
            return stocks.Select(stock => stock.MapToResponse());
        }
    }
}
