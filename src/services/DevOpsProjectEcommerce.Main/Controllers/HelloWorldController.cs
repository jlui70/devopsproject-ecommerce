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
                message = "Hello World v-bluegreen-20260903161254 from Main Service",
                version = "v-bluegreen-20260903161254",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
