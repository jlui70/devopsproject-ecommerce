using Microsoft.AspNetCore.Mvc;

namespace DevOpsProjectEcommerce.Main.Controllers
{
    [ApiController]
    [Route("api/hello")]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Hello World v-demo-20260903144612 from Main Service",
                version = "v-demo-20260903144612",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
