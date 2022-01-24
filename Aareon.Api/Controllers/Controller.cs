using System;
using System.Linq;
using System.Threading.Tasks;
using Aareon.Api.Utilities;
using Aareon.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aareon.Api.Controllers
{
    public abstract class Controller<TDto> : ControllerBase
    {
        private readonly IService<TDto> _service;
        private readonly ILogger _logger;
        protected string UserName => User.Identity?.Name ?? "Unknown";
        protected static string CtlrName => $"{typeof(TDto).Name.Replace("Dto", "")}Controller";
        protected Uri BuildUri (string path) => new ($"{Request.Scheme}://{Request.Host.Host}/{path}");
        

        protected Controller(IService<TDto> service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Route("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public virtual async Task<IActionResult> GetAll()
        {
            _logger.LogTrace("{@CtlrName}: GetAll called by User: {@User}", CtlrName, UserName);
            var items = await _service.GetAll();
            if (!items.Any())
                return NoContent();

            return Ok(items);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            _logger.LogTrace("{@CtlrName}: GetById called with id: {@Id} by User: {@User}", CtlrName, id, UserName);
            var item = await _service.GetById(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            _logger.LogTrace(AuditLogHelper.AuditDelete(CtlrName, id, UserName));
            var deleted = await _service.Delete(id);
            if (!deleted)
                return NotFound();
            
            return Ok();
        }
    }
}