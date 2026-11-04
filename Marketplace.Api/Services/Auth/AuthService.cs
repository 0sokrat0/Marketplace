using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Security;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Auth;

public class AuthService(MarketplaceDbContext db, IPasswordHasher hasher, IJwtService jwt) : IAuthService
{
    private const int MinLoginLength = 3;
    private const int MaxLoginLength = 32;
    private const int MinPasswordLength = 6;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var login = NormalizeLogin(request.Login);
        if (request.Password.Length < MinPasswordLength)
        {
            throw AppException.BadRequest($"Пароль должен быть не короче {MinPasswordLength} символов");
        }

        if (request.Password != request.RepeatPassword)
        {
            throw AppException.BadRequest("Пароли не совпадают");
        }

        var existing = await FindUser(login);
        if (existing is not null)
        {
            return SignIn(existing, request.Password);
        }

        var role = await db.Roles.SingleAsync(r => r.Name == Roles.User);
        var user = new User { Login = login, PasswordHash = hasher.Hash(request.Password), Role = role };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await FindUser(request.Login.Trim());
        if (user is null)
        {
            throw AppException.Unauthorized("Неверный логин или пароль");
        }

        return SignIn(user, request.Password);
    }

    private Task<User?> FindUser(string login) =>
        db.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Login == login);

    private AuthResponse SignIn(User user, string password)
    {
        if (!hasher.Verify(password, user.PasswordHash))
        {
            throw AppException.Unauthorized("Неверный логин или пароль");
        }

        return ToResponse(user);
    }

    private AuthResponse ToResponse(User user) => new(jwt.CreateToken(user), user.Login, user.Role.Name);

    private static string NormalizeLogin(string login)
    {
        var trimmed = login.Trim();
        if (trimmed.Length is < MinLoginLength or > MaxLoginLength || !trimmed.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            throw AppException.BadRequest(
                $"Логин от {MinLoginLength} до {MaxLoginLength} символов: буквы, цифры и подчеркивание");
        }

        return trimmed;
    }
}
