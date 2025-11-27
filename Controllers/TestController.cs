using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketPlace.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    [HttpGet]
    public ActionResult<string?> Test()
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        return id;
    }
}