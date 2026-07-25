using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.Main.Domain.Mappings;
using DevOpsProjectEcommerce.Main.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Main.Controllers
{
    [Authorize]
    [Route("api/stock")]
    [ApiController]
    public sealed class StockController : ControllerBase
    {
        private readonly IStockReadRepository _stockReadRepository;
        public StockController(IStockReadRepository stockReadRepository)
        {
            _stockReadRepository = stockReadRepository ?? throw new ArgumentNullException(nameof(stockReadRepository));
        }
        
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            var stocks = await _stockReadRepository.GetAll()
                .Include(stock=> stock.Product)
                .ToListAsync(cancellationToken);
            
            if (!stocks.Any())
                return NoContent();

            return Ok(stocks.MapToResponse());
        }
    }
}
