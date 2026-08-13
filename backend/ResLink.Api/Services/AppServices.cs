using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ResLink.Api.Contracts;
using ResLink.Api.Data;
using ResLink.Api.Domain;

namespace ResLink.Api.Services;

public class NotificationService(AppDbContext db)
{
    public async Task NotifyAsync(Guid userId, string title, string body, string type)
    {
        db.Notifications.Add(new AppNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Body = body,
            Type = type
        });
        await db.SaveChangesAsync();
    }

    public async Task NotifyRoleAsync(Guid residenceId, string role, string title, string body, string type)
    {
        var userIds = await db.Users
            .Where(u => u.ResidenceId == residenceId && u.Role == role && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var userId in userIds)
        {
            db.Notifications.Add(new AppNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Body = body,
                Type = type
            });
        }

        await db.SaveChangesAsync();
    }
}

public class PointsService(AppDbContext db)
{
    public async Task AwardAsync(AppUser user, int points)
    {
        user.Points += points;
        await db.SaveChangesAsync();
    }
}

public static class DtoMapper
{
    public static AuthResponse ToAuth(AppUser user, string token) =>
        new(token, user.Id, user.FullName, user.Email, user.Role, user.Points, user.ResidenceId,
            user.Residence?.Name, user.Room, user.StudentNumber, user.StaffId);

    public static UserSummaryDto ToUser(AppUser user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.Points, user.IsActive, user.Room,
            user.StudentNumber, user.StaffId, user.ResidenceId, user.Residence?.Name);

    public static PostDto ToPost(Post post)
    {
        var options = new List<PollOptionDto>();
        if (post.IsPoll && !string.IsNullOrWhiteSpace(post.PollOptionsJson))
        {
            var labels = JsonSerializer.Deserialize<List<string>>(post.PollOptionsJson) ?? [];
            options = labels
                .Select((label, index) => new PollOptionDto(index, label, post.Votes.Count(v => v.OptionIndex == index)))
                .ToList();
        }

        return new PostDto(
            post.Id,
            post.AuthorId,
            post.Author?.FullName ?? "Resident",
            post.Author?.Role ?? "Student",
            post.Title,
            post.Body,
            string.IsNullOrWhiteSpace(post.Kind) ? (post.IsPoll ? "Poll" : "Post") : post.Kind,
            post.IsPoll,
            post.LikeCount,
            options,
            post.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto(c.Id, c.Author?.FullName ?? "Resident", c.Body, c.CreatedAt))
                .ToList(),
            post.CreatedAt);
    }
}
