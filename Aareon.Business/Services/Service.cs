using System.Collections.Generic;
using System.Threading.Tasks;
using Aareon.Business.Interfaces;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aareon.Business.Services
{
    public abstract class Service<T, TDto> : IService<TDto>
        where T : DbEntity, new()
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        
        private static string SvcName => $"{typeof(T).Name}Service";

        public static string AuditCreated (string json) => 
            $"{SvcName}: {typeof(T).Name} created: {json}";
        public static string AuditUpdated (string pre, string post) => 
            $"{SvcName}: {typeof(T).Name} updated. From: {pre}. To: {post}";
        public static string AuditNonExist (string operation, int id) => 
            $"{SvcName}: Cannot {operation} {typeof(T).Name}: {id}, as no record with that Id exists";  

        protected Service(IUnitOfWork uow, IMapper mapper, ILogger logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public abstract Task<TDto> Create(TDto dto);
        public abstract Task<bool> Update(TDto dto);
        
        public virtual async Task<IEnumerable<TDto>> GetAll()
        {
            return await _uow.GetRepository<T>().GetAll()
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public virtual async Task<TDto> GetById(int id)
        {
            return await _uow.GetRepository<T>().GetById(id)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public virtual async Task<bool> Delete(int id)
        {
            var repo = _uow.GetRepository<T>();
            var exists = await repo.Exists(id);
            if (!exists)
            {
                _logger.LogWarning(AuditNonExist("delete", id));
                return false;
            }
            
            await repo.Delete(id);
            await _uow.Commit();
            _logger.LogInformation($"{SvcName}: {typeof(T).Name} with Id: {id} deleted.");
            return true;
        }
    }
}