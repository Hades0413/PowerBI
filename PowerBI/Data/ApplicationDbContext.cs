using Microsoft.EntityFrameworkCore;
using PowerBI.Models;

namespace PowerBI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RotacionObrero> RotacionObreros { get; set; }
}