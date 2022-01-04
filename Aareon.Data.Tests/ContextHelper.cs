using System;
using Aareon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aareon.Data.Tests
{
    public class ContextHelper
    {
        internal static AareonContext CreateInMemoryContext()
        {
            var context = new AareonContext(new DbContextOptionsBuilder<AareonContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .LogTo(Console.WriteLine)
                .Options);
            
            context.Database.EnsureCreated();
            return context;
        }
        
        internal static void SeedContext(AareonContext context)
        {
            context.Persons.AddRange(
                new Person {Id = 1, Forename = "John", Surname = "Doe", IsAdmin = true },
                new Person {Id = 2, Forename = "Tom", Surname = "Smith", IsAdmin = false});

            context.Tickets.AddRange(
                new Ticket { Id = 1, Content = "First ticket content", PersonId = 2 },
                new Ticket { Id = 2, Content = "Second ticket content", PersonId = 2 });
            
            context.Notes.AddRange(
                new Note { Id = 1, Content = "1st note content", PersonId = 2, TicketId = 1 },
                new Note { Id = 2, Content = "2nd note content", PersonId = 2, TicketId = 1 });
            
            context.SaveChanges();
        }
    }
}