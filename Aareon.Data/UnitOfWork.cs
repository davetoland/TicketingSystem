using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using Aareon.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aareon.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AareonContext _context;
        public IRepository<Ticket> TicketRepo { get; }
        public IPersonRepository PersonRepo { get; }

        public UnitOfWork(AareonContext context, 
            IRepository<Ticket> ticketRepo, IPersonRepository personRepo)
        {
            _context = context;
            TicketRepo = ticketRepo;
            PersonRepo = personRepo;
        }

        public IRepository<T> GetRepository<T>() where T : DbEntity, new()
        {
            return new Repository<T>(_context);
        }

        public async Task Commit()
        {
            #if DEBUG
            _context.ChangeTracker.DetectChanges();  // for debugging
            var createdEntries = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
            Debug.WriteLine($"Created entries: {createdEntries.Count}");
            foreach (var entry in createdEntries)
            {
                var id = entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity);
                foreach (var prop in entry.CurrentValues.Properties)
                {
                    Debug.WriteLine($"Entity ({id}): {entry.Metadata.Name}");
                    Debug.WriteLine($"Property: {prop.Name}");
                    Debug.WriteLine($"Value: {entry.CurrentValues[prop]}");
                }
            }
            
            var modifiedEntries = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
            Debug.WriteLine($"Modified entries: {createdEntries.Count}");
            foreach (var entry in modifiedEntries)
            {
                var id = entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity);
                foreach (var prop in entry.CurrentValues.Properties)
                {
                    if (entry.CurrentValues[prop]?.ToString() == entry.OriginalValues[prop]?.ToString())
                        continue;

                    Debug.WriteLine($"Entity ({id}): {entry.Metadata.Name}");
                    Debug.WriteLine($"Property: {prop.Name}");
                    Debug.WriteLine($"Current: {entry.CurrentValues[prop]}");
                    Debug.WriteLine($"Original: {entry.OriginalValues[prop]}");
                }
            }
            #endif

            await _context.SaveChangesAsync();
        }
    }
}