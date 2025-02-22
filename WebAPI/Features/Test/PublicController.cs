using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Features.Test
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        public TestController()
        {

        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Text = "Success!" });
        }        
    }
}
