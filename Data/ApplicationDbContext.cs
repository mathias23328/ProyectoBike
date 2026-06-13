using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace grupomathias.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BikeRouteLog> BikeRouteLogs => Set<BikeRouteLog>();
    public DbSet<UserMembership> UserMemberships => Set<UserMembership>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserMembership>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(um => um.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserMembership>()
            .HasIndex(um => um.PreferenceId)
            .IsUnique();
    }
}

public class BikeRouteLog
{
    public int Id { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public int DistanceKm { get; set; }

    public string RiderType { get; set; } = string.Empty;

    public int SafetyScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public class UserMembership
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public decimal MonthlyPrice { get; set; }

    public string PreferenceId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Status { get; set; } = "pending";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }
}