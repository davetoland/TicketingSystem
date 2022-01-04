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
    public class NoteControllerTests : TestBase
    {
        private NoteController _sut;
        private Mock<INoteService> _service;
        private IMapper _mapper;
        private Mock<ILogger<NoteController>> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<INoteService>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<ApiMappings>()));
            _logger = new Mock<ILogger<NoteController>>();

            _sut = new NoteController(_service.Object, _mapper, _logger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = FakeHttpContext }
            };
        }

        [Test]
        public async Task Create_CreatesOk_ReturnsCreated()
        {
            var input = Fixture.Create<NoteDto>();
            _service.Setup(x => x.Create(It.IsAny<NoteDto>())).ReturnsAsync(input);
            var dto = Fixture.Create<NoteModel>();
            
            var result = await _sut.Create(dto);
            
            result.Should().BeOfType<CreatedResult>();
            var created = (CreatedResult) result; 
            created.Location.Should().Be($"{ContextScheme}://{ContextHost}/notes/{input.Id}");
            created.Value.Should().Be(input);
            var log = AuditLogHelper.AuditCreate("NoteController", dto.ToJson(), "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }

        [Test]
        public async Task Update_UpdateFailed_ReturnsNotFound()
        {
            _service.Setup(x => x.Update(It.IsAny<NoteDto>())).ReturnsAsync(false);

            var result = await _sut.Update(Fixture.Create<int>(), Fixture.Create<NoteModel>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Update_UpdateSucceeds_ReturnsOk()
        {
            _service.Setup(x => x.Update(It.IsAny<NoteDto>())).ReturnsAsync(true);
            var id = Fixture.Create<int>();
            var model = Fixture.Create<NoteModel>();
            
            var result = await _sut.Update(id, model);
            
            result.Should().BeOfType<OkResult>();
            var log = AuditLogHelper.AuditUpdate("NoteController", id, model.ToJson(), "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }

        [Test]
        public async Task Remove_RemoveFailed_ReturnsNotFound()
        {
            _service.Setup(x => x.Remove(It.IsAny<int>())).ReturnsAsync(false);

            var result = await _sut.Remove(Fixture.Create<int>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Remove_RemoveSucceeds_ReturnsOk()
        {
            _service.Setup(x => x.Remove(It.IsAny<int>())).ReturnsAsync(true);
            var id = Fixture.Create<int>();
            
            var result = await _sut.Remove(id);
            
            result.Should().BeOfType<OkResult>();
            _logger.VerifyLogExact(LogLevel.Trace, $"NoteController: Remove called with id: {id} by User: TestUser");
        }

        [Test]
        public async Task Delete_DeleteFailed_ReturnsNotFound()
        {
            _service.Setup(x => x.Delete(It.IsAny<int>())).ReturnsAsync(false);

            var result = await _sut.Delete(Fixture.Create<int>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Delete_DeleteSucceeds_ReturnsOk()
        {
            _service.Setup(x => x.Delete(It.IsAny<int>())).ReturnsAsync(true);
            var id = Fixture.Create<int>();
            
            var result = await _sut.Delete(id);
            
            result.Should().BeOfType<OkResult>();
            var log = AuditLogHelper.AuditDelete("NoteController", id,  "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }
    }
}