using System.Collections.Generic;
using System.Threading.Tasks;
using Aareon.Business.DTO;
using Aareon.Business.Exceptions;
using Aareon.Business.Interfaces;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aareon.Business.Services
{
    public class NoteService : Service<Note, NoteDto>, INoteService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<NoteService> _logger;
        private readonly IMapper _mapper;

        public NoteService(IUnitOfWork uow, ILogger<NoteService> logger, IMapper mapper)
            : base(uow, mapper, logger)
        {
            _uow = uow;
            _logger = logger;
            _mapper = mapper;
        }
        
        public override async Task<NoteDto> Create(NoteDto dto)
        {
            var person = await _uow.PersonRepo.GetById(dto.Owner.Id).SingleOrDefaultAsync(); 
            if (person == null)
                throw new InvalidPersonException(dto.Owner.Id);

            if (dto.TicketId == null)
                throw new InvalidTicketException(null);
            
            var ticket = await _uow.TicketRepo.GetById((int)dto.TicketId).SingleOrDefaultAsync(); 
            if (ticket == null)
                throw new InvalidTicketException(dto.TicketId);
            
            var mapped = _mapper.Map<Note>(dto);
            mapped.Owner = person;
            mapped.Ticket = ticket;
            var note = await _uow.NoteRepo.Add(mapped);
            await _uow.Commit();
            _logger.LogInformation(AuditCreated(note.ToJson()));
            
            // Map the generated entity back to a DTO rather than just
            // returning the passed in one to incorporate data set at the repo level
            // i.e. Db ID, and any date or timestamps, etc
            return _mapper.Map<NoteDto>(note);
        }

        public async Task<IEnumerable<NoteDto>> GetByTicket(int ticketId)
        {
            return await _uow.NoteRepo.GetByTicket(ticketId)
                .ProjectTo<NoteDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
        
        public override async Task<bool> Update(NoteDto dto)
        {
            var note = await _uow.NoteRepo.GetById(dto.Id).SingleOrDefaultAsync();
            if (note == null)
            {
                _logger.LogWarning(AuditNonExist("update", dto.Id));
                return false;
            }

            var updated = _mapper.Map(dto, note);
            if (updated.PersonId != note.Owner.Id)
                updated.Owner = await _uow.PersonRepo.GetById(updated.PersonId).SingleOrDefaultAsync();
            if (updated.TicketId != null && updated.TicketId != note.Ticket.Id)
                updated.Ticket = await _uow.TicketRepo.GetById((int)updated.TicketId).SingleOrDefaultAsync();
            
            await _uow.NoteRepo.Update(updated);
            await _uow.Commit();
            
            _logger.LogInformation(AuditUpdated(note.ToJson(), updated.ToJson()));
            return true;
        }

        public async Task<bool> Remove(int id)
        {
            var note = await GetById(id);
            if (note == null) 
                return false;
            
            _logger.LogInformation($"NoteService: removing Note from Ticket: {note.TicketId}");
            note.TicketId = null;
            return await Update(note);
        }
    }
}