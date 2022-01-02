using System.Linq;
using Aareon.Data.Entities;

namespace Aareon.Data.Interfaces
{
    public interface IPersonRepository : IRepository<Person>
    {
        IQueryable<Person> GetBySurname(string surname);
    }
}