using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ResLink.Api.Auth;
using ResLink.Api.Contracts;
using ResLink.Api.Data;
using ResLink.Api.Domain;
using ResLink.Api.Services;

namespace ResLink.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/posts")]
public class FeedController(AppDbContext db, CurrentUser current, PointsService points) : ControllerBase
{
    // Any signed-in role can read posts so Security and Maintenance see announcements.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostDto>>> List()
    {
        var posts = await db.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Votes)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(posts.Select(DtoMapper.ToPost).ToList());
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<ActionResult<PostDto>> Create(CreatePostRequest request)
    {
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = user.Id,
            ResidenceId = user.ResidenceId ?? Guid.Empty,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Kind = string.IsNullOrWhiteSpace(request.Kind) ? (request.IsPoll ? "Poll" : "Post") : request.Kind.Trim(),
            IsPoll = request.IsPoll || string.Equals(request.Kind, "Poll", StringComparison.OrdinalIgnoreCase),
            PollOptionsJson = request.IsPoll
                ? JsonSerializer.Serialize(request.PollOptions ?? [])
                : null
        };
        db.Posts.Add(post);
        await points.AwardAsync(user, 5);
        await db.Entry(post).Reference(p => p.Author).LoadAsync();
        return Ok(DtoMapper.ToPost(post));
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<ActionResult<CommentDto>> Comment(Guid id, CreateCommentRequest request)
    {
        if (!await db.Posts.AnyAsync(p => p.Id == id))
        {
            return NotFound();
        }

        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = id,
            AuthorId = user.Id,
            Body = request.Body.Trim()
        };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return Ok(new CommentDto(comment.Id, user.FullName, comment.Body, comment.CreatedAt));
    }

    [HttpPost("{id:guid}/vote")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<IActionResult> Vote(Guid id, VoteRequest request)
    {
        var post = await db.Posts.Include(p => p.Votes).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null || !post.IsPoll)
        {
            return NotFound();
        }

        var existing = post.Votes.FirstOrDefault(v => v.UserId == current.UserId);
        if (existing is null)
        {
            db.PollVotes.Add(new PollVote
            {
                Id = Guid.NewGuid(),
                PostId = id,
                UserId = current.UserId,
                OptionIndex = request.OptionIndex
            });
        }
        else
        {
            existing.OptionIndex = request.OptionIndex;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    // Admin can moderate any post; students can remove only their own.
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.Student},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            return NotFound();
        }

        if (current.Role != Roles.Admin && post.AuthorId != current.UserId)
        {
            return Forbid();
        }

        db.Posts.Remove(post);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
