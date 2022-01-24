using System.Threading.Tasks;
using Aareon.Business.DTO;
using Aareon.Business.Exceptions;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aareon.Business.Services
{
    public class TicketService : Service<Ticket, TicketDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<TicketService> _logger;
        private readonly IMapper _mapper;

        public TicketService(IUnitOfWork uow, ILogger<TicketService> logger, IMapper mapper)
            : base(uow, mapper, logger)
        {
            _uow = uow;
            _logger = logger;
            _mapper = mapper;
        }
        
        public override async Task<TicketDto> Create(TicketDto dto)
        {
            var person = await _uow.PersonRepo.GetById(dto.Owner.Id).SingleOrDefaultAsync(); 
            if (person == null)
                throw new InvalidPersonException(dto.Owner.Id);
            
            var mapped = _mapper.Map<Ticket>(dto);
            mapped.Owner = person;
            var ticket = await _uow.TicketRepo.Add(mapped);
            _logger.LogInformation(AuditCreated(ticket.ToJson()));
            await _uow.Commit();
            
            // Map the generated entity back to a DTO rather than just
            // returning the passed in one to incorporate data set at the repo level
            // i.e. Db ID, and any date or timestamps, etc
            return _mapper.Map<TicketDto>(ticket);
        }

        public override async Task<bool> Update(TicketDto dto)
        {
            var ticket = await _uow.TicketRepo.GetById(dto.Id).SingleOrDefaultAsync();
            if (ticket == null)
            {
                _logger.LogWarning(AuditNonExist("update", dto.Id));
                return false;
            }
            
            var updated = _mapper.Map(dto, ticket);
            updated.Notes = ticket.Notes;
            if (updated.PersonId != ticket.Owner.Id)
                updated.Owner = await _uow.PersonRepo.GetById(updated.PersonId).SingleOrDefaultAsync();
            await _uow.TicketRepo.Update(updated);
            await _uow.Commit();
            _logger.LogInformation(AuditUpdated(ticket.ToJson(), updated.ToJson()));
            return true;
        }
    }
}