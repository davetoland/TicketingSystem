using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aareon.Api.Controllers;
using Aareon.Api.Models;
using Aareon.Api.Tests.Utilities;
using Aareon.Api.Utilities;
using Aareon.Business;
using Aareon.Business.DTO;
using Aareon.Business.Interfaces;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Aareon.Api.Tests.Controllers
{
    [TestFixture]
    public class TicketControllerTests : TestBase
    {
        private TicketController _sut;
        private Mock<IService<TicketDto>> _service;
        private Mock<INoteService> _noteSvc;
        private IMapper _mapper;
        private Mock<ILogger<TicketController>> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IService<TicketDto>>();
            _noteSvc = new Mock<INoteService>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<ApiMappings>()));
            _logger = new Mock<ILogger<TicketController>>();

            _sut = new TicketController(_service.Object, _noteSvc.Object, _mapper, _logger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = FakeHttpContext }
            };
        }

        [Test]
        public async Task Create_CreatesOk_ReturnsCreated()
        {
            var input = Fixture.Create<TicketDto>();
            _service.Setup(x => x.Create(It.IsAny<TicketDto>())).ReturnsAsync(input);
            var dto = Fixture.Create<TicketModel>();
            
            var result = await _sut.Create(dto);
            
            result.Should().BeOfType<CreatedResult>();
            var created = (CreatedResult) result; 
            created.Location.Should().Be($"{ContextScheme}://{ContextHost}/tickets/{input.Id}");
            created.Value.Should().Be(input);
            var log = AuditLogHelper.AuditCreate("TicketController", dto.ToJson(), "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }

        [Test]
        public async Task GetNotes_NoNotes_ReturnsNoContent()
        {
            _noteSvc.Setup(x => x.GetByTicket(It.IsAny<int>())).ReturnsAsync(Enumerable.Empty<NoteDto>());
            
            var result = await _sut.GetNotes(Fixture.Create<int>());

            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task GetNotes_NotesExist_ReturnsOkWithContent()
        {
            var notes = Fixture.CreateMany<NoteDto>().ToList();
            _noteSvc.Setup(x => x.GetByTicket(It.IsAny<int>())).ReturnsAsync(notes);
            
            var result = await _sut.GetNotes(Fixture.Create<int>());

            result.Should().BeOfType<OkObjectResult>();
            var resultNotes = result.GetFromActionResult<IEnumerable<NoteDto>>();
            resultNotes.Should().BeEquivalentTo(notes);
        }

        [Test]
        public async Task Update_UpdateFailed_ReturnsNotFound()
        {
            _service.Setup(x => x.Update(It.IsAny<TicketDto>())).ReturnsAsync(false);

            var result = await _sut.Update(Fixture.Create<int>(), Fixture.Create<TicketModel>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Update_UpdateSucceeds_ReturnsOk()
        {
            _service.Setup(x => x.Update(It.IsAny<TicketDto>())).ReturnsAsync(true);
            var id = Fixture.Create<int>();
            var model = Fixture.Create<TicketModel>();
            
            var result = await _sut.Update(id, model);
            
            result.Should().BeOfType<OkResult>();
            var log = AuditLogHelper.AuditUpdate("TicketController", id, model.ToJson(), "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }
    }
}