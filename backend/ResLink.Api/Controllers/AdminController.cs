using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResLink.Api.Auth;
using ResLink.Api.Contracts;
using ResLink.Api.Data;
using ResLink.Api.Domain;
using ResLink.Api.Services;

namespace ResLink.Api.Controllers;

[ApiController]
[Authorize]
public class AdminController(AppDbContext db, CurrentUser current) : ControllerBase
{
    [HttpGet("api/dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard()
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var highlights = current.Role switch
        {
            Roles.Student => new[]
            {
                "Post in the community feed and earn points.",
                "RSVP to residence events from the Events tab.",
                "Use Panic if you need security right now."
            },
            Roles.Admin => new[]
            {
                "Create events and review RSVP counts.",
                "Moderate feed posts and marketplace listings.",
                "Deactivate accounts from the users list."
            },
            Roles.Security => new[]
            {
                "Acknowledge open panic alerts first.",
                "Noise complaints include optional anonymous reports.",
                "Mark incidents resolved after you attend."
            },
            Roles.Maintenance => new[]
            {
                "Pick up Open tickets and move them to In Progress.",
                "Add resolution notes before marking Resolved.",
                "Students see status updates in notifications."
            },
            _ => Array.Empty<string>()
        };

        AnalyticsDto? analytics = null;
        if (current.Role is Roles.Admin or Roles.Security or Roles.Maintenance)
        {
            analytics = await BuildAnalytics();
        }

        return Ok(new DashboardDto(user.Role, user.FullName, user.Points, highlights, analytics));
    }

    [HttpGet("api/admin/analytics")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AnalyticsDto>> Analytics() => Ok(await BuildAnalytics());

    [HttpGet("api/admin/users")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> Users()
    {
        var users = await db.Users.Include(u => u.Residence).OrderBy(u => u.FullName).ToListAsync();
        return Ok(users.Select(DtoMapper.ToUser).ToList());
    }

    [HttpPut("api/admin/users/{id:guid}/active")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SetActive(Guid id, UpdateActiveRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/notifications")]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Notifications()
    {
        var items = await db.Notifications
            .Where(n => n.UserId == current.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Body, n.Type, n.IsRead, n.CreatedAt))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPut("api/notifications/{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id)
    {
        var item = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == current.UserId);
        if (item is null)
        {
            return NotFound();
        }

        item.IsRead = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/profile")]
    public async Task<ActionResult<UserSummaryDto>> Profile()
    {
        var user = await db.Users.Include(u => u.Residence).FirstAsync(u => u.Id == current.UserId);
        return Ok(DtoMapper.ToUser(user));
    }

    private async Task<AnalyticsDto> BuildAnalytics()
    {
        return new AnalyticsDto(
            await db.Users.CountAsync(),
            await db.Users.CountAsync(u => u.Role == Roles.Student),
            await db.MaintenanceTickets.CountAsync(t => t.Status != TicketStatuses.Resolved),
            await db.NoiseComplaints.CountAsync(n => n.Status == "Open"),
            await db.EmergencyAlerts.CountAsync(e => e.Status == "Open"),
            await db.Rsvps.CountAsync(),
            await db.Users.SumAsync(u => u.Points),
            await db.MarketplaceItems.CountAsync(i => !i.IsSold));
    }
}
