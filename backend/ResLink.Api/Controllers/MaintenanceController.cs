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
[Route("api/maintenance")]
// Students cancel their own open ticket; staff can move status forward or back to Open (Microsoft, 2025e).
public class MaintenanceController(
    AppDbContext db,
    CurrentUser current,
    PointsService points,
    NotificationService notifications) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin},{Roles.Maintenance}")]
    public async Task<ActionResult<IReadOnlyList<MaintenanceTicketDto>>> List()
    {
        var query = db.MaintenanceTickets.Include(t => t.Reporter).AsQueryable();
        if (current.Role == Roles.Student)
        {
            query = query.Where(t => t.ReporterId == current.UserId);
        }

        var items = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(items.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<MaintenanceTicketDto>> Create(CreateMaintenanceRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var ticket = new MaintenanceTicket
        {
            Id = Guid.NewGuid(),
            ReporterId = user.Id,
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
            PhotoUrl = request.PhotoUrl,
            Status = TicketStatuses.Open
        };
        db.MaintenanceTickets.Add(ticket);
        await points.AwardAsync(user, 8);
        await notifications.NotifyRoleAsync(user.ResidenceId ?? Guid.Empty, Roles.Maintenance, "New ticket",
            $"{ticket.Title} reported in {ticket.Location}.", "Maintenance");
        ticket.Reporter = user;
        return Ok(ToDto(ticket));
    }

    // Staff can reopen a ticket; students can only cancel their own still-open request.
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = $"{Roles.Maintenance},{Roles.Admin},{Roles.Student}")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTicketStatusRequest request)
    {
        var ticket = await db.MaintenanceTickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        if (current.Role == Roles.Student)
        {
            if (ticket.ReporterId != current.UserId || ticket.Status != TicketStatuses.Open ||
                request.Status != TicketStatuses.Cancelled)
            {
                return Forbid();
            }
        }

        ticket.Status = request.Status;
        ticket.ResolutionNotes = request.ResolutionNotes;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (current.Role == Roles.Maintenance)
        {
            ticket.AssignedToId = current.UserId;
        }

        await db.SaveChangesAsync();
        await notifications.NotifyAsync(ticket.ReporterId, "Ticket update",
            $"{ticket.Title} is now {ticket.Status}.", "Maintenance");
        return NoContent();
    }

    private static MaintenanceTicketDto ToDto(MaintenanceTicket ticket) =>
        new(ticket.Id, ticket.Reporter?.FullName ?? "Student", ticket.Title, ticket.Description, ticket.Location,
            ticket.Category, ticket.Priority, ticket.PhotoUrl, ticket.Status, ticket.ResolutionNotes, ticket.CreatedAt, ticket.UpdatedAt);
}
