using Microsoft.EntityFrameworkCore;
using ResLink.Api.Domain;

namespace ResLink.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();
    public DbSet<ResidenceEvent> Events => Set<ResidenceEvent>();
    public DbSet<EventRsvp> Rsvps => Set<EventRsvp>();
    public DbSet<MarketplaceItem> MarketplaceItems => Set<MarketplaceItem>();
    public DbSet<MaintenanceTicket> MaintenanceTickets => Set<MaintenanceTicket>();
    public DbSet<NoiseComplaint> NoiseComplaints => Set<NoiseComplaint>();
    public DbSet<EmergencyAlert> EmergencyAlerts => Set<EmergencyAlert>();
    public DbSet<StudyGroup> StudyGroups => Set<StudyGroup>();
    public DbSet<StudyGroupMember> StudyGroupMembers => Set<StudyGroupMember>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<Redemption> Redemptions => Set<Redemption>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();
    public DbSet<Visitor> Visitors => Set<Visitor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Residence)
            .WithMany()
            .HasForeignKey(u => u.ResidenceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Votes)
            .WithOne()
            .HasForeignKey(v => v.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PollVote>()
            .HasIndex(v => new { v.PostId, v.UserId })
            .IsUnique();

        modelBuilder.Entity<ResidenceEvent>()
            .HasMany(e => e.Rsvps)
            .WithOne(r => r.Event)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventRsvp>()
            .HasIndex(r => new { r.EventId, r.UserId })
            .IsUnique();

        modelBuilder.Entity<StudyGroup>()
            .HasMany(g => g.Members)
            .WithOne(m => m.StudyGroup)
            .HasForeignKey(m => m.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudyGroupMember>()
            .HasIndex(m => new { m.StudyGroupId, m.UserId })
            .IsUnique();

        modelBuilder.Entity<MarketplaceItem>()
            .Property(i => i.Price)
            .HasPrecision(10, 2);
    }
}
