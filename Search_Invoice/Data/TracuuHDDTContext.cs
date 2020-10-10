using System.Data.Entity;
using Search_Invoice.Data.Domain;

namespace Search_Invoice.Data
{
    public class TracuuHDDTContext : DbContext
    {
        public TracuuHDDTContext()
          : base("TracuuHDDTConnectionString")
        {
            this.Configuration.ProxyCreationEnabled = false;
        }
        public DbSet<inv_admin> Inv_admin { get; set; }
        public DbSet<inv_user> inv_users { get; set; }
        public DbSet<inv_customer_banned> inv_customer_banneds { get; set; }
    }
}