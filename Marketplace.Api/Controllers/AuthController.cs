using Marketplace.Api.Services.Auth;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public Task<AuthResponse> Register(RegisterRequest request) => auth.RegisterAsync(request);

    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request) => auth.LoginAsync(request);
}
