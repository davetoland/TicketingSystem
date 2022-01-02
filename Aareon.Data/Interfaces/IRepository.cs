using System.Linq;
using System.Threading.Tasks;

namespace Aareon.Data.Interfaces
{
    public interface IRepository<T>
    {
        Task<T> Add(T item);
        Task<bool> Exists(int id);
        IQueryable<T> GetAll();
        IQueryable<T> GetById(int id);
        Task Update(T item);
        Task Delete(int id);
    }
}