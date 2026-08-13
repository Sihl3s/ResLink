using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResLink.Api.Domain;

namespace ResLink.Api.Data;

public static class DbSeeder
{
    // Fixed IDs keep demo logins and sample tickets stable across local and Azure resets.
    public static readonly Guid ResidenceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid SecurityId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid MaintenanceId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid LenaId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid SiphoId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid PriyaId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid NomsaId = Guid.Parse("99999999-9999-9999-9999-999999999991");
    public static readonly Guid JamesId = Guid.Parse("99999999-9999-9999-9999-999999999992");

    public static async Task SeedAsync(AppDbContext db)
    {
        // Seed only on an empty database so a live Azure instance keeps lecturer-created accounts.
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var hasher = new PasswordHasher<AppUser>();
        var residence = new Residence
        {
            Id = ResidenceId,
            Name = "Campus Flow Residence",
            Campus = "Main Campus",
            Address = "14 Residence Walk, University Avenue"
        };

        var student = CreateUser(hasher, StudentId, "Sihle Simelane", "student@reslink.app", "Student123!", Roles.Student, residence.Id, "A-214", "STU2026001", null, 0);
        var admin = CreateUser(hasher, AdminId, "Lebo Mokoena", "admin@reslink.app", "Admin123!", Roles.Admin, residence.Id, null, null, "ADM-001", 40);
        var security = CreateUser(hasher, SecurityId, "Thabo Nkosi", "security@reslink.app", "Security123!", Roles.Security, residence.Id, null, null, "SEC-014", 15);
        var maintenance = CreateUser(hasher, MaintenanceId, "Aisha Patel", "maintenance@reslink.app", "Maintenance123!", Roles.Maintenance, residence.Id, null, null, "MNT-008", 20);
        var lena = CreateUser(hasher, LenaId, "Lena Dube", "lena@reslink.app", "Student123!", Roles.Student, residence.Id, "C-214", "STU2026002", null, 35);
        var sipho = CreateUser(hasher, SiphoId, "Sipho Nkosi", "sipho@reslink.app", "Student123!", Roles.Student, residence.Id, "B-108", "STU2026003", null, 20);
        var priya = CreateUser(hasher, PriyaId, "Priya Singh", "priya@reslink.app", "Student123!", Roles.Student, residence.Id, "312", "STU2026004", null, 15);
        var nomsa = CreateUser(hasher, NomsaId, "Nomsa Khumalo", "nomsa@reslink.app", "Student123!", Roles.Student, residence.Id, "D-101", "STU2026005", null, 45);
        var james = CreateUser(hasher, JamesId, "James van Wyk", "james@reslink.app", "Student123!", Roles.Student, residence.Id, "A-110", "STU2026006", null, 10);

        db.Residences.Add(residence);
        db.Users.AddRange(student, admin, security, maintenance, lena, sipho, priya, nomsa, james);

        // Community feed plus admin announcements shown on every role dashboard.
        db.Posts.AddRange(
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                AuthorId = lena.Id,
                ResidenceId = residence.Id,
                Title = "Just moved in",
                Body = "Hey everyone! Just moved into Block C, Room 214. Looking forward to meeting you all. Any coffee spot recommendations nearby?",
                Kind = "Post",
                LikeCount = 12,
                CreatedAt = new DateTime(2026, 5, 21, 7, 45, 0, DateTimeKind.Utc)
            },
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                AuthorId = sipho.Id,
                ResidenceId = residence.Id,
                Title = "ECON201 study group",
                Body = "Starting a study group for ECON201 - Macro exam is in 3 weeks. Looking for 3-4 people who want to work through past papers together.",
                Kind = "Discussion",
                LikeCount = 7,
                CreatedAt = new DateTime(2026, 5, 21, 9, 10, 0, DateTimeKind.Utc)
            },
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                AuthorId = admin.Id,
                ResidenceId = residence.Id,
                Title = "Gym hours",
                Body = "Reminder: the gym is now open until 22:00 on weekdays. Please bring your residence card for access.",
                Kind = "Announcement",
                LikeCount = 18,
                CreatedAt = new DateTime(2026, 5, 21, 11, 0, 0, DateTimeKind.Utc)
            },
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                AuthorId = student.Id,
                ResidenceId = residence.Id,
                Title = "Braai night or movie night?",
                Body = "Help us pick this Friday's residence event.",
                Kind = "Poll",
                IsPoll = true,
                PollOptionsJson = JsonSerializer.Serialize(new[] { "Residence braai", "Movie marathon", "Both!" }),
                LikeCount = 9,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                AuthorId = admin.Id,
                ResidenceId = residence.Id,
                Title = "Quiet hours",
                Body = "Quiet hours are 22:00–07:00 Sunday to Thursday, and 23:00–08:00 on Friday and Saturday. Security will follow up on complaints.",
                Kind = "Announcement",
                LikeCount = 21,
                CreatedAt = new DateTime(2026, 8, 4, 8, 0, 0, DateTimeKind.Utc)
            },
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6"),
                AuthorId = admin.Id,
                ResidenceId = residence.Id,
                Title = "Weekend laundry",
                Body = "The laundry room stays open until 21:00 on Saturdays and Sundays this semester. Please clear machines promptly.",
                Kind = "Announcement",
                LikeCount = 14,
                CreatedAt = new DateTime(2026, 8, 8, 9, 30, 0, DateTimeKind.Utc)
            },
            new Post
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7"),
                AuthorId = nomsa.Id,
                ResidenceId = residence.Id,
                Title = "Lost student card",
                Body = "If anyone found a blue lanyard near the courtyard yesterday evening, please message me. Name on the card is Nomsa.",
                Kind = "Post",
                LikeCount = 4,
                CreatedAt = DateTime.UtcNow.AddHours(-18)
            });

        db.Events.AddRange(
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Movie Night Under the Stars",
                Description = "Blankets, popcorn and a rooftop screening. Bring a jacket.",
                Location = "Rooftop Deck",
                Category = "Social",
                IsFeatured = true,
                StartsAt = new DateTime(2026, 6, 6, 17, 30, 0, DateTimeKind.Utc),
                CheckInCode = "MOVIE-0606"
            },
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Welcome Braai & Mixer",
                Description = "Meet your floor, grab a plate, and earn reward points for checking in.",
                Location = "Common Courtyard",
                Category = "Social",
                StartsAt = new DateTime(2026, 5, 24, 14, 0, 0, DateTimeKind.Utc),
                CheckInCode = "BRAAI-7741"
            },
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Residence 5-a-Side Football",
                Description = "Friendly match between blocks. All skill levels welcome.",
                Location = "Sports Field",
                Category = "Sports",
                StartsAt = new DateTime(2026, 5, 31, 7, 0, 0, DateTimeKind.Utc),
                CheckInCode = "FOOT-0531"
            },
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Exam Prep Study Hall",
                Description = "Quiet hall, extra tables, and coffee from 18:00 to 21:00.",
                Location = "Common Room B",
                Category = "Academic",
                StartsAt = new DateTime(2026, 6, 10, 16, 0, 0, DateTimeKind.Utc),
                CheckInCode = "STUDY-2208"
            },
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Sunrise Yoga",
                Description = "Gentle session on the courtyard lawn. Mats provided for the first 20 residents.",
                Location = "Common Courtyard",
                Category = "Sports",
                StartsAt = new DateTime(2026, 8, 16, 5, 30, 0, DateTimeKind.Utc),
                CheckInCode = "YOGA-0816"
            },
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb6"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Heritage Evening",
                Description = "Food, music and stories from home. Bring a dish if you can.",
                Location = "Dining Hall",
                Category = "Cultural",
                IsFeatured = true,
                StartsAt = new DateTime(2026, 8, 22, 16, 0, 0, DateTimeKind.Utc),
                CheckInCode = "CULT-0822"
            },
            new ResidenceEvent
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb7"),
                ResidenceId = residence.Id,
                CreatedById = admin.Id,
                Title = "Block A floor meeting",
                Description = "Short house-committee update on Wi-Fi, visitors and exam quiet weeks.",
                Location = "Block A lounge",
                Category = "Meeting",
                StartsAt = new DateTime(2026, 8, 18, 16, 0, 0, DateTimeKind.Utc),
                CheckInCode = "MEET-0818"
            });

        db.MarketplaceItems.AddRange(
            new MarketplaceItem
            {
                SellerId = priya.Id,
                ResidenceId = residence.Id,
                Title = "Mini Desk Fan",
                Description = "Quiet USB fan, barely used. Perfect for late study sessions.",
                Category = "Electronics",
                Condition = "Like New",
                Price = 150
            },
            new MarketplaceItem
            {
                SellerId = lena.Id,
                ResidenceId = residence.Id,
                Title = "First-year Accounting textbook",
                Description = "Light highlighting, includes tutorial pack.",
                Category = "Textbooks",
                Condition = "Good",
                Price = 250
            },
            new MarketplaceItem
            {
                SellerId = sipho.Id,
                ResidenceId = residence.Id,
                Title = "Desk lamp",
                Description = "Warm LED lamp. Selling because I upgraded.",
                Category = "Furniture",
                Condition = "Fair",
                Price = 120
            },
            new MarketplaceItem
            {
                SellerId = student.Id,
                ResidenceId = residence.Id,
                Title = "Rice cooker",
                Description = "Leaving it behind for the next resident. Collect from A-214.",
                Category = "Other",
                Condition = "Good",
                Price = 0
            },
            new MarketplaceItem
            {
                SellerId = nomsa.Id,
                ResidenceId = residence.Id,
                Title = "City bicycle",
                Description = "Single-speed campus bike with a lock. Tyres pumped last week.",
                Category = "Other",
                Condition = "Good",
                Price = 800
            },
            new MarketplaceItem
            {
                SellerId = james.Id,
                ResidenceId = residence.Id,
                Title = "Microwave",
                Description = "700W microwave. Collect from A-110 this weekend.",
                Category = "Electronics",
                Condition = "Fair",
                Price = 350
            },
            new MarketplaceItem
            {
                SellerId = priya.Id,
                ResidenceId = residence.Id,
                Title = "Denim jacket",
                Description = "Size M, barely worn. Sold pending collection.",
                Category = "Clothing",
                Condition = "Like New",
                Price = 180,
                IsSold = true
            });

        db.MaintenanceTickets.AddRange(
            new MaintenanceTicket
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1"),
                ReporterId = lena.Id,
                ResidenceId = residence.Id,
                Title = "Shower draining slowly",
                Description = "Water pools around the drain after a short shower. Started this week.",
                Location = "Room C-214",
                Category = "Plumbing",
                Priority = "Medium",
                Status = TicketStatuses.Open
            },
            new MaintenanceTicket
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2"),
                ReporterId = student.Id,
                ResidenceId = residence.Id,
                Title = "Flickering corridor light",
                Description = "Level 2 east wing light flickers after 20:00.",
                Location = "Level 2 East",
                Category = "Electrical",
                Priority = "High",
                Status = TicketStatuses.InProgress,
                AssignedToId = maintenance.Id,
                ResolutionNotes = "Electrician booked for tomorrow morning."
            },
            new MaintenanceTicket
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3"),
                ReporterId = james.Id,
                ResidenceId = residence.Id,
                Title = "Broken cupboard latch",
                Description = "The wardrobe door in A-110 will not stay closed.",
                Location = "Room A-110",
                Category = "Furniture",
                Priority = "Low",
                Status = TicketStatuses.Resolved,
                AssignedToId = maintenance.Id,
                ResolutionNotes = "Latch replaced on 10 August."
            },
            new MaintenanceTicket
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc4"),
                ReporterId = nomsa.Id,
                ResidenceId = residence.Id,
                Title = "Kitchen tap dripping",
                Description = "Shared kitchen tap on D-level drips overnight.",
                Location = "Block D kitchen",
                Category = "Plumbing",
                Priority = "Medium",
                Status = TicketStatuses.Open
            });

        db.StudyGroups.AddRange(
            new StudyGroup
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff1"),
                ResidenceId = residence.Id,
                CreatedById = sipho.Id,
                Name = "Stats & Probability Squad",
                Topic = "Statistics",
                CourseCode = "STA201",
                Description = "Working through past papers and tough problem sets together. All levels welcome!",
                Schedule = "Tue & Thu 15:00-17:00",
                Location = "Library Room 2",
                MaxMembers = 8
            },
            new StudyGroup
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff2"),
                ResidenceId = residence.Id,
                CreatedById = student.Id,
                Name = "Code & Coffee",
                Topic = "Computer Science",
                CourseCode = "CS101",
                Description = "CS students helping each other debug, prep for interviews and work on side projects.",
                Schedule = "Mon, Wed & Fri 10:00-12:00",
                Location = "Computer Lab B",
                MaxMembers = 12
            },
            new StudyGroup
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff3"),
                ResidenceId = residence.Id,
                CreatedById = priya.Id,
                Name = "Law Notes Exchange",
                Topic = "Law",
                CourseCode = "LAW301",
                Description = "Share notes, case summaries and exam prep resources. Weekly discussion of recent cases.",
                Schedule = "Wednesday 18:00-20:00",
                Location = "Common Room",
                MaxMembers = 5
            });

        db.Rewards.AddRange(
            new Reward { Name = "Free Coffee Voucher", Description = "One free coffee at the residence cafe.", Category = "Food", PointsCost = 50, Stock = 20 },
            new Reward { Name = "Campus cafe voucher", Description = "R30 off at the residence cafe.", Category = "Food", PointsCost = 80, Stock = 15 },
            new Reward { Name = "Laundry credit", Description = "One free washer cycle.", Category = "Services", PointsCost = 60, Stock = 12 },
            new Reward { Name = "ResLink tote bag", Description = "Canvas tote with the ResLink mark.", Category = "Merchandise", PointsCost = 90, Stock = 8 },
            new Reward { Name = "Wi-Fi boost weekend", Description = "Priority bandwidth for the weekend.", Category = "Experiences", PointsCost = 100, Stock = 5 });

        db.Comments.AddRange(
            new Comment
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd1"),
                PostId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                AuthorId = sipho.Id,
                Body = "Welcome! The campus cafe does a decent flat white."
            },
            new Comment
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd2"),
                PostId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                AuthorId = priya.Id,
                Body = "Also try the cart outside Block B before 09:00."
            },
            new Comment
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd3"),
                PostId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                AuthorId = student.Id,
                Body = "I am in. Library room 2 on Thursday?"
            },
            new Comment
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd4"),
                PostId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7"),
                AuthorId = lena.Id,
                Body = "I will keep an eye out near the gym."
            });

        db.StudyGroupMembers.AddRange(
            new StudyGroupMember { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1"), StudyGroupId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff1"), UserId = sipho.Id },
            new StudyGroupMember { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2"), StudyGroupId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff1"), UserId = student.Id },
            new StudyGroupMember { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3"), StudyGroupId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff2"), UserId = student.Id },
            new StudyGroupMember { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4"), StudyGroupId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff2"), UserId = lena.Id },
            new StudyGroupMember { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee5"), StudyGroupId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffff3"), UserId = priya.Id });

        db.Visitors.Add(new Visitor
        {
            Id = Guid.Parse("12121212-1212-1212-1212-121212121212"),
            ResidenceId = residence.Id,
            HostId = student.Id,
            VisitorName = "Kabelo Mthembu",
            HostRoom = "A-214",
            Purpose = "Study session",
            Status = "OnSite"
        });

        db.NoiseComplaints.Add(new NoiseComplaint
        {
            ReporterId = student.Id,
            ResidenceId = residence.Id,
            Location = "B-wing, 3rd floor",
            Description = "Loud music after quiet hours.",
            IsAnonymous = true,
            Status = "Open"
        });

        db.EmergencyAlerts.Add(new EmergencyAlert
        {
            ReporterId = student.Id,
            ResidenceId = residence.Id,
            Location = "Room A-214, Campus Flow Residence",
            Message = "Demo panic alert — student needs assistance.",
            Status = "Open"
        });

        db.Notifications.AddRange(
            new AppNotification { UserId = student.Id, Title = "Welcome to ResLink", Body = "Explore events, connect with neighbours, and report issues in one place.", Type = "Info" },
            new AppNotification { UserId = security.Id, Title = "Panic alert", Body = "Open panic alert from Room A-214.", Type = "Emergency" },
            new AppNotification { UserId = maintenance.Id, Title = "New ticket", Body = "Shower draining slowly reported in C-214.", Type = "Maintenance" },
            new AppNotification { UserId = admin.Id, Title = "Community", Body = "Lena Dube posted in the feed.", Type = "Feed" });

        await db.SaveChangesAsync();
    }

    private static AppUser CreateUser(
        PasswordHasher<AppUser> hasher,
        Guid id,
        string name,
        string email,
        string password,
        string role,
        Guid residenceId,
        string? room,
        string? studentNumber,
        string? staffId,
        int points)
    {
        var user = new AppUser
        {
            Id = id,
            FullName = name,
            Email = email,
            Role = role,
            ResidenceId = residenceId,
            Room = room,
            StudentNumber = studentNumber,
            StaffId = staffId,
            Points = points,
            IsActive = true
        };
        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }
}
