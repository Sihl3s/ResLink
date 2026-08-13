namespace ResLink.Api.Domain;

public class Residence
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Campus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class AppUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Student;
    public string? StudentNumber { get; set; }
    public string? StaffId { get; set; }
    public string? Room { get; set; }
    public Guid? ResidenceId { get; set; }
    public Residence? Residence { get; set; }
    public int Points { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public AppUser? Author { get; set; }
    public Guid ResidenceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Kind { get; set; } = "Post";
    public bool IsPoll { get; set; }
    public string? PollOptionsJson { get; set; }
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Comment> Comments { get; set; } = [];
    public List<PollVote> Votes { get; set; } = [];
}

public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    public Guid AuthorId { get; set; }
    public AppUser? Author { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PollVote
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public int OptionIndex { get; set; }
}

public class ResidenceEvent
{
    public Guid Id { get; set; }
    public Guid ResidenceId { get; set; }
    public Guid CreatedById { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = "Social";
    public bool IsFeatured { get; set; }
    public DateTime StartsAt { get; set; }
    public string CheckInCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<EventRsvp> Rsvps { get; set; } = [];
}

public class EventRsvp
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public ResidenceEvent? Event { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MarketplaceItem
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public AppUser? Seller { get; set; }
    public Guid ResidenceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Condition { get; set; } = "Good";
    public decimal Price { get; set; }
    public bool IsSold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MaintenanceTicket
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public AppUser? Reporter { get; set; }
    public Guid ResidenceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Priority { get; set; } = "Medium";
    public string? PhotoUrl { get; set; }
    public string Status { get; set; } = TicketStatuses.Open;
    public string? ResolutionNotes { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class TicketStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Resolved = "Resolved";
    public const string Cancelled = "Cancelled";
}

public class NoiseComplaint
{
    public Guid Id { get; set; }
    public Guid? ReporterId { get; set; }
    public Guid ResidenceId { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class EmergencyAlert
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public AppUser? Reporter { get; set; }
    public Guid ResidenceId { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Message { get; set; } = "Panic button activated";
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class StudyGroup
{
    public Guid Id { get; set; }
    public Guid ResidenceId { get; set; }
    public Guid CreatedById { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int MaxMembers { get; set; } = 12;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<StudyGroupMember> Members { get; set; } = [];
}

public class StudyGroupMember
{
    public Guid Id { get; set; }
    public Guid StudyGroupId { get; set; }
    public StudyGroup? StudyGroup { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
}

public class Reward
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Food";
    public int PointsCost { get; set; }
    public int Stock { get; set; } = 20;
}

public class Visitor
{
    public Guid Id { get; set; }
    public Guid ResidenceId { get; set; }
    public Guid HostId { get; set; }
    public AppUser? Host { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string HostRoom { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = "OnSite";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Redemption
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid RewardId { get; set; }
    public Reward? Reward { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
