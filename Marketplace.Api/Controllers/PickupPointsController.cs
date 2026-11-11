using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/pickup-points")]
public class PickupPointsController(MarketplaceDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<PickupPointDto>> Get() =>
        db.PickupPoints
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PickupPointDto(p.Id, p.Name, p.Address))
            .ToListAsync();
}
