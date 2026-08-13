namespace ResLink.Api.Contracts;

public record StudentRegisterRequest(
    string FullName,
    string Email,
    string Password,
    string StudentNumber,
    Guid ResidenceId,
    string Room);

public record StaffRegisterRequest(
    string FullName,
    string Email,
    string Password,
    string StaffId,
    string Role,
    string AccessCode,
    Guid ResidenceId);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    int Points,
    Guid? ResidenceId,
    string? ResidenceName,
    string? Room,
    string? StudentNumber,
    string? StaffId);

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    int Points,
    bool IsActive,
    string? Room,
    string? StudentNumber,
    string? StaffId,
    Guid? ResidenceId,
    string? ResidenceName);

public record ResidenceDto(Guid Id, string Name, string Campus, string Address);

public record CommentDto(Guid Id, string AuthorName, string Body, DateTime CreatedAt);

public record PollOptionDto(int Index, string Label, int Votes);

public record PostDto(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string AuthorRole,
    string Title,
    string Body,
    string Kind,
    bool IsPoll,
    int LikeCount,
    IReadOnlyList<PollOptionDto> PollOptions,
    IReadOnlyList<CommentDto> Comments,
    DateTime CreatedAt);

public record CreatePostRequest(string Title, string Body, string? Kind, bool IsPoll, IReadOnlyList<string>? PollOptions);
public record CreateCommentRequest(string Body);
public record VoteRequest(int OptionIndex);

public record EventDto(
    Guid Id,
    string Title,
    string Description,
    string Location,
    string Category,
    bool IsFeatured,
    DateTime StartsAt,
    string CheckInCode,
    int RsvpCount,
    bool HasRsvp);

public record CreateEventRequest(string Title, string Description, string Location, DateTime StartsAt, string? Category);

public record MarketplaceItemDto(
    Guid Id,
    string SellerName,
    Guid SellerId,
    string Title,
    string Description,
    string Category,
    string Condition,
    string? SellerRoom,
    decimal Price,
    bool IsSold,
    DateTime CreatedAt);

public record CreateMarketplaceItemRequest(string Title, string Description, string Category, decimal Price, string? Condition);

public record MaintenanceTicketDto(
    Guid Id,
    string ReporterName,
    string Title,
    string Description,
    string Location,
    string Category,
    string Priority,
    string? PhotoUrl,
    string Status,
    string? ResolutionNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateMaintenanceRequest(string Title, string Description, string Location, string? PhotoUrl, string? Category, string? Priority);
public record UpdateTicketStatusRequest(string Status, string? ResolutionNotes);

public record NoiseComplaintDto(
    Guid Id,
    string ReporterName,
    string Location,
    string Description,
    bool IsAnonymous,
    string Status,
    DateTime CreatedAt);

public record CreateNoiseRequest(string Location, string Description, bool IsAnonymous);
public record UpdateStatusRequest(string Status);

public record EmergencyAlertDto(
    Guid Id,
    string ReporterName,
    string Location,
    string Message,
    string Status,
    DateTime CreatedAt);

public record CreateEmergencyRequest(string? Location, string? Message);

public record StudyGroupDto(
    Guid Id,
    string Name,
    string Topic,
    string CourseCode,
    string Description,
    string Schedule,
    string Location,
    int MemberCount,
    int MaxMembers,
    bool IsMember,
    DateTime CreatedAt);

public record CreateStudyGroupRequest(string Name, string Topic, string Description, string? CourseCode, string? Schedule, string? Location, int? MaxMembers);

public record RewardDto(Guid Id, string Name, string Description, string Category, int PointsCost, int Stock);

public record CreateRewardRequest(string Name, string Description, string Category, int PointsCost, int? Stock);

public record VisitorDto(Guid Id, string VisitorName, string HostName, string HostRoom, string Purpose, string Status, DateTime CreatedAt);
public record CreateVisitorRequest(string VisitorName, string HostRoom, string Purpose);
public record RedemptionDto(Guid Id, Guid RewardId, string RewardName, int PointsCost, DateTime CreatedAt);

public record NotificationDto(Guid Id, string Title, string Body, string Type, bool IsRead, DateTime CreatedAt);

public record AnalyticsDto(
    int TotalUsers,
    int Students,
    int OpenTickets,
    int OpenComplaints,
    int OpenEmergencies,
    int TotalRsvps,
    int PointsIssued,
    int MarketplaceListings);

public record DashboardDto(
    string Role,
    string FullName,
    int Points,
    IReadOnlyList<string> Highlights,
    AnalyticsDto? Analytics);

public record UpdateActiveRequest(bool IsActive);
