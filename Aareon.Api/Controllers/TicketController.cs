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
    [Route("tickets")]
    public class TicketController : Controller<TicketDto>
    {
        private readonly IService<TicketDto> _service;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketController> _logger;

        public TicketController(IService<TicketDto> service, IMapper mapper, ILogger<TicketController> logger)
            : base(service, logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        // We're returning IActionResult here, which means we also need to provide
        // ProducesResponseType annotations to describe the possible outcomes. We could
        // just mark the method as returning the type (public async Task<TicketDto> for example) but
        // by using IActionResult we can return proper REST responses i.e. Ok, Ok(result)
        // which is especially relevant in scenarios where we want to return Problem, NoContent, BadRequest etc.
        // If nothing else it makes life easier for whoever is implementing this API...
        [HttpPost]
        [Route("new")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(TicketModel newTicket)
        {
            // Why use a TicketModel here and not just have an instance of the DTO passed in?
            // A separate model allows us to specify annotations to validate the incoming data,
            // it also allows us to omit irrelevant/confusing props..
            // i.e. we don't need an ID passed in, we'll generate that, having it as a prop will leave
            // the client unsure of whether they need to pass that in - which they don't
            _logger.LogTrace(AuditLogHelper.AuditCreate(CtlrName, newTicket.ToJson(), UserName));
            var dto = _mapper.Map<TicketDto>(newTicket);
            dto.Owner.Id = User.GetUserId();
            var ticket = await _service.Create(dto);
            return Created(BuildUri($"tickets/{ticket.Id}"), ticket);
        }

        [HttpPut]
        [Route("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, TicketModel updateTicket)
        {
            _logger.LogTrace(AuditLogHelper.AuditUpdate(CtlrName, id, updateTicket.ToJson(), UserName));
            var dto = _mapper.Map<TicketDto>(updateTicket);
            dto.Id = id;
            var updated = await _service.Update(dto);
            if (!updated)
                return NotFound();
            
            // It's fine to just return Ok here, if there were any problems above an exception
            // would have been thrown by the service which would be caught and handled by the middleware
            return Ok();
        }
    }
}