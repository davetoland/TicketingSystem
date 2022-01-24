using Aareon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aareon.Data
{
    public class AareonContext : DbContext
    {
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        
        public AareonContext(DbContextOptions<AareonContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasMany(e => e.Tickets)
                    .WithOne(e => e.Owner);
            });
            
            modelBuilder.Entity<Ticket>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasOne(e => e.Owner)
                    .WithMany(e => e.Tickets);
            });
            
            modelBuilder.Entity<Note>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasOne(e => e.Ticket)
                    .WithMany(e => e.Notes);
            });
        }
    }
}