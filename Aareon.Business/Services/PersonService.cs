using System.Threading.Tasks;
using Aareon.Business.DTO;
using Aareon.Business.Interfaces;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Aareon.Business.Services
{
    public class PersonService : Service<Person, PersonDto>, IPersonService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PersonService> _logger;
        private readonly IMapper _mapper;

        public PersonService(IUnitOfWork uow, ILogger<PersonService> logger, IMapper mapper)
            : base(uow, mapper, logger)
        {
            _uow = uow;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<PersonDto> Create(PersonDto dto)
        {
            var mapped = _mapper.Map<Person>(dto);
            var person = await _uow.PersonRepo.Add(mapped);
            await _uow.Commit();
            _logger.LogInformation(AuditCreated(person.ToJson()));
            
            // Map the generated entity back to a DTO rather than just
            // returning the passed in one to incorporate data set at the repo level
            // i.e. Db ID, and any date or timestamps, etc
            return _mapper.Map<PersonDto>(person);
        }

        public async Task<PersonDto> GetBySurname(string surname)
        {
            return await _uow.PersonRepo.GetBySurname(surname)
                .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public override async Task<bool> Update(PersonDto dto)
        {
            var person = await _uow.PersonRepo.GetById(dto.Id).SingleOrDefaultAsync();
            if (person == null)
            {
                _logger.LogWarning(AuditNonExist("update", dto.Id));
                return false;
            }

            var before = JsonConvert.SerializeObject(person);
            var updated = _mapper.Map(dto, person);
            await _uow.PersonRepo.Update(updated);
            await _uow.Commit();
            var after = JsonConvert.SerializeObject(person);
            
            _logger.LogInformation(AuditUpdated(before, after));
            return true;
        }
    }
}