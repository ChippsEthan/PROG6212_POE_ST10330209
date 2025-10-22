using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PROG6212ST10330209.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
    }
}