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
public class CommunityController(
    AppDbContext db,
    CurrentUser current,
    PointsService points) : ControllerBase
{
    // Join/leave and redeem/cancel keep study groups and rewards reversible in a demo (Microsoft, 2025a).
    [HttpGet("api/study-groups")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<ActionResult<IReadOnlyList<StudyGroupDto>>> Groups()
    {
        var groups = await db.StudyGroups.Include(g => g.Members).OrderByDescending(g => g.CreatedAt).ToListAsync();
        return Ok(groups.Select(g => new StudyGroupDto(
            g.Id, g.Name, g.Topic, g.CourseCode, g.Description, g.Schedule, g.Location,
            g.Members.Count, g.MaxMembers,
            g.Members.Any(m => m.UserId == current.UserId), g.CreatedAt)).ToList());
    }

    [HttpPost("api/study-groups")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<ActionResult<StudyGroupDto>> CreateGroup(CreateStudyGroupRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var group = new StudyGroup
        {
            Id = Guid.NewGuid(),
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            CreatedById = user.Id,
            Name = request.Name.Trim(),
            Topic = request.Topic.Trim(),
            CourseCode = request.CourseCode?.Trim() ?? "",
            Description = request.Description.Trim(),
            Schedule = request.Schedule?.Trim() ?? "",
            Location = request.Location?.Trim() ?? "",
            MaxMembers = request.MaxMembers ?? 12
        };
        db.StudyGroups.Add(group);
        db.StudyGroupMembers.Add(new StudyGroupMember { Id = Guid.NewGuid(), StudyGroupId = group.Id, UserId = user.Id });
        await points.AwardAsync(user, 5);
        return Ok(new StudyGroupDto(group.Id, group.Name, group.Topic, group.CourseCode, group.Description,
            group.Schedule, group.Location, 1, group.MaxMembers, true, group.CreatedAt));
    }

    [HttpPost("api/study-groups/{id:guid}/join")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Join(Guid id)
    {
        if (!await db.StudyGroups.AnyAsync(g => g.Id == id))
        {
            return NotFound();
        }

        if (!await db.StudyGroupMembers.AnyAsync(m => m.StudyGroupId == id && m.UserId == current.UserId))
        {
            db.StudyGroupMembers.Add(new StudyGroupMember { Id = Guid.NewGuid(), StudyGroupId = id, UserId = current.UserId });
            var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
            await points.AwardAsync(user, 5);
        }

        return NoContent();
    }

    [HttpDelete("api/study-groups/{id:guid}/leave")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Leave(Guid id)
    {
        var member = await db.StudyGroupMembers.FirstOrDefaultAsync(m => m.StudyGroupId == id && m.UserId == current.UserId);
        if (member is not null)
        {
            db.StudyGroupMembers.Remove(member);
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpGet("api/rewards")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<ActionResult<IReadOnlyList<RewardDto>>> Rewards()
    {
        var items = await db.Rewards.OrderBy(r => r.PointsCost)
            .Select(r => new RewardDto(r.Id, r.Name, r.Description, r.Category, r.PointsCost, r.Stock))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("api/rewards/redemptions")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<IReadOnlyList<RedemptionDto>>> Redemptions()
    {
        var items = await db.Redemptions.Include(r => r.Reward)
            .Where(r => r.UserId == current.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RedemptionDto(r.Id, r.RewardId, r.Reward!.Name, r.Reward.PointsCost, r.CreatedAt))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("api/rewards/{id:guid}/redeem")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Redeem(Guid id)
    {
        var reward = await db.Rewards.FirstOrDefaultAsync(r => r.Id == id);
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        if (reward is null)
        {
            return NotFound();
        }

        if (user.Points < reward.PointsCost)
        {
            return BadRequest("Not enough points to redeem this reward.");
        }

        user.Points -= reward.PointsCost;
        if (reward.Stock > 0) reward.Stock -= 1;
        db.Redemptions.Add(new Redemption { Id = Guid.NewGuid(), UserId = user.Id, RewardId = reward.Id });
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Refunds points and restocks the catalogue so a redeem can be undone (Microsoft, 2025a).
    [HttpPost("api/rewards/redemptions/{id:guid}/cancel")]
    [Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> CancelRedemption(Guid id)
    {
        var redemption = await db.Redemptions.Include(r => r.Reward)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == current.UserId);
        if (redemption is null)
        {
            return NotFound();
        }

        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        user.Points += redemption.Reward?.PointsCost ?? 0;
        if (redemption.Reward is not null)
        {
            redemption.Reward.Stock += 1;
        }

        db.Redemptions.Remove(redemption);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("api/rewards")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RewardDto>> CreateReward(CreateRewardRequest request)
    {
        var reward = new Reward
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Food" : request.Category.Trim(),
            PointsCost = request.PointsCost,
            Stock = request.Stock ?? 20
        };
        db.Rewards.Add(reward);
        await db.SaveChangesAsync();
        return Ok(new RewardDto(reward.Id, reward.Name, reward.Description, reward.Category, reward.PointsCost, reward.Stock));
    }
}
