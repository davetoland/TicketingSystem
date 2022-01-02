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
    [ApiController]
    [Route("people")]
    public class PersonController : Controller<PersonDto>
    {
        private readonly IPersonService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<PersonController> _logger;

        public PersonController(IPersonService service, IMapper mapper, ILogger<PersonController> logger)
            : base(service, logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }
        
        [HttpPost]
        [Route("new")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(PersonModel newPerson)
        {
            _logger.LogTrace(AuditLogHelper.AuditCreate(CtlrName, newPerson.ToJson(), UserName));
            var dto = _mapper.Map<PersonDto>(newPerson);
            var created = await _service.Create(dto);
            return Created(BuildUri($"people/{created.Id}"), created);
        }

        [HttpPut]
        [Route("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, PersonModel updatePerson)
        {
            _logger.LogTrace(AuditLogHelper.AuditUpdate(CtlrName, id, updatePerson.ToJson(), UserName));
            var dto = _mapper.Map<PersonDto>(updatePerson);
            dto.Id = id;
            var updated = await _service.Update(dto);
            if (!updated)
                return NotFound();
            
            return Ok();
        }
    }
}