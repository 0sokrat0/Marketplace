using Marketplace.Api.Entities;

namespace Marketplace.Api.Services.Security;

public interface IJwtService
{
    string CreateToken(User user);
}
