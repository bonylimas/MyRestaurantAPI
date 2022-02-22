
using Microsoft.EntityFrameworkCore;

namespace MyRestaurantAPI
{
    public class ApplicationDBContext: DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        public DbSet<Reservation> Reservations { get; set; }
    }
}
