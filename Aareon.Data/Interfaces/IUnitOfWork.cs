using System.Threading.Tasks;
using Aareon.Data.Entities;

namespace Aareon.Data.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Ticket> TicketRepo { get; }
        INoteRepository NoteRepo { get; }
        IPersonRepository PersonRepo { get; }
        IRepository<T> GetRepository<T>() where T : DbEntity, new();
        Task Commit();
    }
}