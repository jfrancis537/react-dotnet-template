using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetOracle.Controllers
{
  [Route("api/auth")]
  [ApiController]
  [Authorize]
  public class AuthController : ControllerBase
  {
    [HttpGet]
    [Route("login")]
    public async Task<IActionResult> Login()
    {
      var result = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
      if (result.Succeeded)
      {
        return Redirect("/");
      }
      return Redirect("/");
    }

    [HttpGet]
    [Route("logout")]
    [Authorize]
    public IActionResult Logout()
    {
      return SignOut(new AuthenticationProperties
      {
        RedirectUri = "/"
      }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [Route("username")]
    [Authorize]
    public IActionResult GetUsername()
    {
      if (User.Identity != null)
      {
        return Ok(User.Identity.Name);
      }
      return Unauthorized();
    }

    [HttpGet]
    [Route("**")]
    public IActionResult Nothing()
    {
      return NotFound();
    }

  }
}