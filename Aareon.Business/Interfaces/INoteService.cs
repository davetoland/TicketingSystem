using System.Collections.Generic;
using System.Threading.Tasks;
using Aareon.Business.DTO;

namespace Aareon.Business.Interfaces
{
    public interface INoteService : IService<NoteDto>
    {
        Task<IEnumerable<NoteDto>> GetByTicket(int ticketId);
        Task<bool> Remove(int id);
    }
}