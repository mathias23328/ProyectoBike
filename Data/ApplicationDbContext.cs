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