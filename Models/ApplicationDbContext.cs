using Microsoft.EntityFrameworkCore;

namespace HotelLuxuryWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Cầu nối đến bảng Bookings trong SQL
        public DbSet<Booking> Bookings { get; set; }
    }
}