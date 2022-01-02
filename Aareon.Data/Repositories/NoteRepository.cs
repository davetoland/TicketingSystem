using System.Linq;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;

namespace Aareon.Data.Repositories
{
    public class NoteRepository : Repository<Note>, INoteRepository
    {
        public NoteRepository(AareonContext db) : base(db) { }
        
        public IQueryable<Note> GetByTicket(int ticketId)
        {
            return Context.Notes.Where(x => x.TicketId == ticketId);
        }
    }
}