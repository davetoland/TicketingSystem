using System.Linq;
using Aareon.Data.Entities;

namespace Aareon.Data.Interfaces
{
    public interface INoteRepository : IRepository<Note>
    {
        IQueryable<Note> GetByTicket(int ticketId);
    }
}