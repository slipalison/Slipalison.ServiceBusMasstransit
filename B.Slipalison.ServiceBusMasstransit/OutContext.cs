using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace B.Slipalison.ServiceBusMasstransit
{
    public class OutContext : DbContext
    {
        public OutContext(DbContextOptions<OutContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}