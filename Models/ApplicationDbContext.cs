using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<RsvpModel> Rsvps { get; set; }
    public DbSet<TableAssignmentModel> TableAssignments { get; set; } = default!;
    public DbSet<GuestbookEntry> GuestbookEntries { get; set; }
}
