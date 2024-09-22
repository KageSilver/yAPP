using Microsoft.AspNetCore.Mvc;

namespace yAppLambda.Controllers;

[ApiController]
[Route("api/")]
public class MainController:ControllerBase
{
    public MainController()
    {
        
    }
    
    // GET: api/my
    [HttpGet("test")]
    public IActionResult GetTest()
    {
        return Ok(new { message = "Hello from the GET method!" });
    }
    
}