using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using DevOpsProjectEcommerce.Main.Domain.Models;
using DevOpsProjectEcommerce.Main.Domain.Repositories.Contexts;

namespace DevOpsProjectEcommerce.Main.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private const string ReportSql = @"
            SELECT
                p.""Id""          AS product_id,
                p.""Name""        AS product_name,
                COUNT(o.""Id"")::int                               AS total_orders,
                COALESCE(SUM(o.""Quantity""), 0)::int              AS total_ordered,
                COALESCE(SUM(o.""Quantity"" * p.""Price""), 0)     AS total_sold
            FROM ""Product"" p
            LEFT JOIN ""Order"" o ON o.""ProductId"" = p.""Id"" AND o.""StatusId"" = 1
            GROUP BY p.""Id"", p.""Name""
            ORDER BY p.""Id""";

        [HttpGet("order")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken,
            [FromServices] IConfiguration configuration,
            [FromServices] ProductContext productContext)
        {
            // Try MongoDB first (populated by report-job Lambda in AWS)
            try
            {
                var settings = MongoClientSettings.FromUrl(new MongoUrl(configuration.GetConnectionString("Mongo")));
                settings.SslSettings = new SslSettings
                {
                    ServerCertificateValidationCallback = (_, __, ___, ____) => true
                };
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);

                var client = new MongoClient(settings);
                var database = client.GetDatabase(configuration.GetValue<string>("Mongo:Database"));
                var collection = database.GetCollection<ReportEntity>(configuration.GetValue<string>("Mongo:Report_Collection"));
                var mongoReport = await collection.Find(_ => true).ToListAsync(cancellationToken);

                if (mongoReport.Any(r => r.TotalSold > 0))
                    return Ok(mongoReport);
            }
            catch
            {
                // MongoDB unavailable or empty — fall through to PostgreSQL
            }

            // Real-time calculation from PostgreSQL via ADO.NET
            var result = new List<ReportEntity>();
            var conn = productContext.Database.GetDbConnection();
            try
            {
                await productContext.Database.OpenConnectionAsync(cancellationToken);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = ReportSql;
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(new ReportEntity
                    {
                        ProductId    = reader.GetInt32(0),
                        ProductName  = reader.GetString(1),
                        TotalOrders  = reader.GetInt32(2),
                        TotalOrdered = reader.GetInt32(3),
                        TotalSold    = reader.GetDecimal(4)
                    });
                }
            }
            finally
            {
                await productContext.Database.CloseConnectionAsync();
            }

            if (!result.Any())
                return NoContent();

            return Ok(result);
        }
    }
}
