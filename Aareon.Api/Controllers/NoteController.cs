using System.Threading.Tasks;
using Aareon.Api.Models;
using Aareon.Api.Utilities;
using Aareon.Business;
using Aareon.Business.DTO;
using Aareon.Business.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aareon.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("notes")]
    public class NoteController : Controller<NoteDto>
    {
        private readonly INoteService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<NoteController> _logger;
        
        public NoteController(INoteService service, IMapper mapper, ILogger<NoteController> logger)
            : base(service, logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }
        
        [HttpPost]
        [Route("new")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(NoteModel newNote)
        {
            _logger.LogTrace(AuditLogHelper.AuditCreate(CtlrName, newNote.ToJson(), UserName));
            var dto = _mapper.Map<NoteDto>(newNote);
            dto.Owner.Id = User.GetUserId();
            var note = await _service.Create(dto);
            return Created(BuildUri($"notes/{note.Id}"), note);
        }

        [HttpPut]
        [Route("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, NoteModel updateNote)
        {
            _logger.LogTrace(AuditLogHelper.AuditUpdate(CtlrName, id, updateNote.ToJson(), UserName));
            var dto = _mapper.Map<NoteDto>(updateNote);
            dto.Id = id;
            var updated = await _service.Update(dto);
            if (!updated)
                return NotFound();
            
            return Ok();
        }

        [HttpPut]
        [Route("remove/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(int id)
        {
            _logger.LogTrace("NoteController: Remove called with id: {@Id} by User: {@User}", id, UserName);
            var removed = await _service.Remove(id);
            if (!removed)
                return NotFound();

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override async Task<IActionResult> Delete(int id)
        {
            _logger.LogTrace(AuditLogHelper.AuditDelete(CtlrName, id, UserName));
            var deleted = await _service.Delete(id);
            if (!deleted)
                return NotFound();
            
            return Ok();
        }
    }
}