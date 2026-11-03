namespace Marketplace.Contracts;

public record RegisterRequest(string Login, string Password, string RepeatPassword);

public record LoginRequest(string Login, string Password);

public record AuthResponse(string Token, string Login, string Role);
