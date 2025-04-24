using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers
{
  [ApiController]
  [Route("api/example")]
  public class ExampleController : ControllerBase
  {
    [HttpGet]
    [Route("something")]
    public IActionResult GetSomething()
    {
      return Ok("ExampleController is working!");
    }
  }
}