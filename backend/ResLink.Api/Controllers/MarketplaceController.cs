using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResLink.Api.Auth;
using ResLink.Api.Contracts;
using ResLink.Api.Data;
using ResLink.Api.Domain;

namespace ResLink.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/marketplace")]
public class MarketplaceController(AppDbContext db, CurrentUser current) : ControllerBase
{
    // Sold / unsold are reversible so a listing is never stuck after a demo tap.
    [HttpGet]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<ActionResult<IReadOnlyList<MarketplaceItemDto>>> List()
    {
        var items = await db.MarketplaceItems.Include(i => i.Seller).OrderByDescending(i => i.CreatedAt).ToListAsync();
        return Ok(items.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<MarketplaceItemDto>> Create(CreateMarketplaceItemRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var item = new MarketplaceItem
        {
            Id = Guid.NewGuid(),
            SellerId = user.Id,
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Condition = string.IsNullOrWhiteSpace(request.Condition) ? "Good" : request.Condition.Trim(),
            Price = request.Price
        };
        db.MarketplaceItems.Add(item);
        await db.SaveChangesAsync();
        item.Seller = user;
        return Ok(ToDto(item));
    }

    [HttpPut("{id:guid}/sold")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<IActionResult> MarkSold(Guid id)
    {
        var item = await db.MarketplaceItems.FirstOrDefaultAsync(i => i.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (current.Role != Roles.Admin && item.SellerId != current.UserId)
        {
            return Forbid();
        }

        item.IsSold = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Lets a seller or admin put a listing back on the market after Mark sold.
    [HttpPut("{id:guid}/unsold")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<IActionResult> MarkAvailable(Guid id)
    {
        var item = await db.MarketplaceItems.FirstOrDefaultAsync(i => i.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (current.Role != Roles.Admin && item.SellerId != current.UserId)
        {
            return Forbid();
        }

        item.IsSold = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await db.MarketplaceItems.FirstOrDefaultAsync(i => i.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (current.Role != Roles.Admin && item.SellerId != current.UserId)
        {
            return Forbid();
        }

        db.MarketplaceItems.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static MarketplaceItemDto ToDto(MarketplaceItem item) =>
        new(item.Id, item.Seller?.FullName ?? "Resident", item.SellerId, item.Title, item.Description,
            item.Category, item.Condition, item.Seller?.Room, item.Price, item.IsSold, item.CreatedAt);
}
