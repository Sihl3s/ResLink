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
[Route("api/events")]
public class EventsController(
    AppDbContext db,
    CurrentUser current,
    PointsService points,
    NotificationService notifications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> List()
    {
        var events = await db.Events.Include(e => e.Rsvps).OrderBy(e => e.StartsAt).ToListAsync();
        return Ok(events.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EventDto>> Create(CreateEventRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var item = new ResidenceEvent
        {
            Id = Guid.NewGuid(),
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            CreatedById = user.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Social" : request.Category.Trim(),
            StartsAt = request.StartsAt,
            CheckInCode = $"EVT-{Random.Shared.Next(1000, 9999)}"
        };
        db.Events.Add(item);
        await db.SaveChangesAsync();
        return Ok(ToDto(item));
    }

    [HttpPost("{id:guid}/rsvp")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Rsvp(Guid id)
    {
        var ev = await db.Events.Include(e => e.Rsvps).FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.Rsvps.Any(r => r.UserId == current.UserId))
        {
            return NoContent();
        }

        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        db.Rsvps.Add(new EventRsvp { Id = Guid.NewGuid(), EventId = id, UserId = user.Id });
        await points.AwardAsync(user, 10);
        await notifications.NotifyRoleAsync(user.ResidenceId ?? Guid.Empty, Roles.Admin, "New RSVP",
            $"{user.FullName} RSVP'd to {ev.Title}.", "Event");
        return NoContent();
    }

    [HttpDelete("{id:guid}/rsvp")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var rsvp = await db.Rsvps.FirstOrDefaultAsync(r => r.EventId == id && r.UserId == current.UserId);
        if (rsvp is not null)
        {
            db.Rsvps.Remove(rsvp);
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    private EventDto ToDto(ResidenceEvent ev) =>
        new(ev.Id, ev.Title, ev.Description, ev.Location, ev.Category, ev.IsFeatured, ev.StartsAt, ev.CheckInCode,
            ev.Rsvps.Count, ev.Rsvps.Any(r => r.UserId == current.UserId));
}
