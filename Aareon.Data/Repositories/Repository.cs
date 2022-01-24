using System.Linq;
using System.Threading.Tasks;
using Aareon.Data.Entities;
using Aareon.Data.Exceptions;
using Aareon.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aareon.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : DbEntity, new()
    {
        protected readonly AareonContext Context;

        public Repository(AareonContext db)
        {
            Context = db;
        }

        public virtual async Task<T> Add(T item)
        {
            var exists = await Exists(item.Id);
            if (exists)
                throw new RepositoryException($"{typeof(T).Name} with Id {item.Id} already exists");

            var entry = await Context.AddAsync(item);
            return entry.Entity;
        }

        public virtual async Task<bool> Exists(int id)
        {
            return await Context.Set<T>().AsNoTracking().AnyAsync(x => x.Id == id);
        }

        // Rather than using Find, and loading the entire object into memory,
        // do a Where call and return an IQueryable, therefore giving the user the
        // chance the project/select before calling Single/First to execute the query 
        public virtual IQueryable<T> GetById(int id)
        {
            return Context.Set<T>().Where(x => x.Id == id);
        }

        public virtual IQueryable<T> GetAll()
        {
            return Context.Set<T>().AsQueryable();
        }

        // Theory: you shouldn't expose any type of dynamic 'Query' method...
        // Services should not be responsible for queries, that's the 
        // responsibility of a Repository - to abstract database type queries away
        // from the service layer. 
        // If you find yourself in a position where you need to query/filter/sort db entities
        // (i.e. operations that you'd normally do in sql) in the business logic layer,
        // then you should probably consider creating a Repository for that entity type and putting that
        // logic in there instead. It helps to reduce bloat and duplication in the business logic layer, 
        // where services can easily become laden with data logic.
        //
        // public virtual async Task<IEnumerable<T>> Query(Expression<Func<T, bool>> expr = null)
        // {
        //     var query = expr != null ? Context.Set<T>().Where(expr) : Context.Set<T>();
        //     return await query.ToListAsync();
        // }

        public virtual async Task Update(T item)
        {
            var exists = await Exists(item.Id);
            if (!exists)
                throw new RepositoryException($"{typeof(T).Name} with Id {item.Id} does not exist");

            Context.Entry(item).State = EntityState.Modified;
        }

        public virtual async Task Delete(int id)
        {
            var existing = await Context.FindAsync<T>(id);
            if (existing == null)
                throw new RepositoryException($"{typeof(T).Name} with Id {id} does not exist");

            Context.Remove(existing);
        }
    }
}