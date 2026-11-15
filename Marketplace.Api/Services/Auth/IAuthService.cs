using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
