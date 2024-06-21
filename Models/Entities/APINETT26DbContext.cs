using Microsoft.EntityFrameworkCore;

namespace APINET_T26.Models.Entities
{
    public class APINETT26DbContext : DbContext
    {
        public APINETT26DbContext()
        {
        }
        public DbSet<Product> Products { get; set; }
        public APINETT26DbContext(DbContextOptions<APINETT26DbContext> options) : base(options)
        {
        }
    }
}
