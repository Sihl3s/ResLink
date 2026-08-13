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
// Panic and noise can be withdrawn or reopened so a demo alert is not stuck Open.
public class SafetyController(
    AppDbContext db,
    CurrentUser current,
    PointsService points,
    NotificationService notifications) : ControllerBase
{
    [HttpGet("api/noise")]
    [Authorize(Roles = $"{Roles.Security},{Roles.Admin}")]
    public async Task<ActionResult<IReadOnlyList<NoiseComplaintDto>>> Noise()
    {
        var items = await db.NoiseComplaints.OrderByDescending(n => n.CreatedAt).ToListAsync();
        var reporterIds = items.Where(i => i.ReporterId.HasValue).Select(i => i.ReporterId!.Value).Distinct().ToList();
        var names = await db.Users.Where(u => reporterIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName);
        return Ok(items.Select(n => new NoiseComplaintDto(
            n.Id,
            n.IsAnonymous ? "Anonymous" : names.GetValueOrDefault(n.ReporterId ?? Guid.Empty, "Student"),
            n.Location,
            n.Description,
            n.IsAnonymous,
            n.Status,
            n.CreatedAt)).ToList());
    }

    [HttpPost("api/noise")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> CreateNoise(CreateNoiseRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        db.NoiseComplaints.Add(new NoiseComplaint
        {
            Id = Guid.NewGuid(),
            ReporterId = user.Id,
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            IsAnonymous = request.IsAnonymous,
            Status = "Open"
        });
        await points.AwardAsync(user, 8);
        await notifications.NotifyRoleAsync(user.ResidenceId ?? Guid.Empty, Roles.Security, "Noise complaint",
            $"New complaint at {request.Location}.", "Noise");
        return NoContent();
    }

    [HttpPut("api/noise/{id:guid}/status")]
    [Authorize(Roles = $"{Roles.Security},{Roles.Admin}")]
    public async Task<IActionResult> UpdateNoise(Guid id, UpdateStatusRequest request)
    {
        var item = await db.NoiseComplaints.FirstOrDefaultAsync(n => n.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = request.Status;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/noise/mine")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<IReadOnlyList<NoiseComplaintDto>>> MyNoise()
    {
        var items = await db.NoiseComplaints
            .Where(n => n.ReporterId == current.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return Ok(items.Select(n => new NoiseComplaintDto(
            n.Id, n.IsAnonymous ? "Anonymous" : "You", n.Location, n.Description, n.IsAnonymous, n.Status, n.CreatedAt)).ToList());
    }

    // Student withdraws a complaint they just filed; security can still reopen it.
    [HttpPut("api/noise/{id:guid}/withdraw")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> WithdrawNoise(Guid id)
    {
        var item = await db.NoiseComplaints.FirstOrDefaultAsync(n => n.Id == id && n.ReporterId == current.UserId);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = "Withdrawn";
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/emergencies")]
    [Authorize(Roles = $"{Roles.Security},{Roles.Admin}")]
    public async Task<ActionResult<IReadOnlyList<EmergencyAlertDto>>> Emergencies()
    {
        var items = await db.EmergencyAlerts.Include(e => e.Reporter).OrderByDescending(e => e.CreatedAt).ToListAsync();
        return Ok(items.Select(e => new EmergencyAlertDto(
            e.Id, e.Reporter?.FullName ?? "Student", e.Location, e.Message, e.Status, e.CreatedAt)).ToList());
    }

    [HttpPost("api/emergencies")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Panic(CreateEmergencyRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var location = string.IsNullOrWhiteSpace(request.Location)
            ? $"{user.Room ?? "Unknown room"}, {user.Residence?.Name ?? "Residence"}"
            : request.Location.Trim();

        await db.Entry(user).Reference(u => u.Residence).LoadAsync();
        db.EmergencyAlerts.Add(new EmergencyAlert
        {
            Id = Guid.NewGuid(),
            ReporterId = user.Id,
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            Location = location,
            Message = string.IsNullOrWhiteSpace(request.Message) ? "Panic button activated" : request.Message.Trim(),
            Status = "Open"
        });
        await db.SaveChangesAsync();
        await notifications.NotifyRoleAsync(user.ResidenceId ?? Guid.Empty, Roles.Security, "Panic alert",
            $"Open panic alert from {location}.", "Emergency");
        return NoContent();
    }

    [HttpPut("api/emergencies/{id:guid}/status")]
    [Authorize(Roles = $"{Roles.Security},{Roles.Admin}")]
    public async Task<IActionResult> UpdateEmergency(Guid id, UpdateStatusRequest request)
    {
        var item = await db.EmergencyAlerts.FirstOrDefaultAsync(e => e.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = request.Status;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/emergencies/mine")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<IReadOnlyList<EmergencyAlertDto>>> MyEmergencies()
    {
        var items = await db.EmergencyAlerts.Include(e => e.Reporter)
            .Where(e => e.ReporterId == current.UserId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
        return Ok(items.Select(e => new EmergencyAlertDto(
            e.Id, e.Reporter?.FullName ?? "You", e.Location, e.Message, e.Status, e.CreatedAt)).ToList());
    }

    // Marks a panic as a false alarm so security is not left chasing a live alert.
    [HttpPut("api/emergencies/{id:guid}/withdraw")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> WithdrawEmergency(Guid id)
    {
        var item = await db.EmergencyAlerts.FirstOrDefaultAsync(e => e.Id == id && e.ReporterId == current.UserId);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = "Withdrawn";
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
