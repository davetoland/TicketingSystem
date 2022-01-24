using System.Linq;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;

namespace Aareon.Data.Repositories
{
    public class PersonRepository : Repository<Person>, IPersonRepository
    {
        public PersonRepository(AareonContext db) : base(db) { }
        
        public IQueryable<Person> GetBySurname(string surname)
        {
            return Context.Persons.Where(x => x.Surname == surname);
        }
    }
}