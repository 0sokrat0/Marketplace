using Marketplace.Api.Services.Auth;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request) =>
        (await auth.RegisterAsync(request)).ToActionResult();

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) =>
        (await auth.LoginAsync(request)).ToActionResult();
}
