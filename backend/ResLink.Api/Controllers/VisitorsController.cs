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
[Route("api/visitors")]
public class VisitorsController(AppDbContext db, CurrentUser current) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Security},{Roles.Admin}")]
    public async Task<ActionResult<IReadOnlyList<VisitorDto>>> List()
    {
        var items = await db.Visitors.Include(v => v.Host).OrderByDescending(v => v.CreatedAt).ToListAsync();
        return Ok(items.Select(v => new VisitorDto(
            v.Id, v.VisitorName, v.Host?.FullName ?? "Resident", v.HostRoom, v.Purpose, v.Status, v.CreatedAt)).ToList());
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Student},{Roles.Security},{Roles.Admin}")]
    public async Task<ActionResult<VisitorDto>> Create(CreateVisitorRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var visitor = new Visitor
        {
            Id = Guid.NewGuid(),
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            HostId = user.Id,
            VisitorName = request.VisitorName.Trim(),
            HostRoom = string.IsNullOrWhiteSpace(request.HostRoom) ? user.Room ?? "" : request.HostRoom.Trim(),
            Purpose = request.Purpose.Trim(),
            Status = "OnSite"
        };
        db.Visitors.Add(visitor);
        await db.SaveChangesAsync();
        return Ok(new VisitorDto(visitor.Id, visitor.VisitorName, user.FullName, visitor.HostRoom, visitor.Purpose, visitor.Status, visitor.CreatedAt));
    }

    // Check-out sets Departed; security can also put a visitor back to OnSite if needed (Microsoft, 2025e).
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = $"{Roles.Security},{Roles.Admin}")]
    public async Task<IActionResult> Update(Guid id, UpdateStatusRequest request)
    {
        var visitor = await db.Visitors.FirstOrDefaultAsync(v => v.Id == id);
        if (visitor is null) return NotFound();
        visitor.Status = request.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
