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
    public class PersonControllerTests : TestBase
    {
        private PersonController _sut;
        private Mock<IPersonService> _service;
        private IMapper _mapper;
        private Mock<ILogger<PersonController>> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IPersonService>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<ApiMappings>()));
            _logger = new Mock<ILogger<PersonController>>();

            _sut = new PersonController(_service.Object, _mapper, _logger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = FakeHttpContext }
            };
        }

        [Test]
        public async Task Create_CreatesOk_ReturnsCreated()
        {
            var input = Fixture.Create<PersonDto>();
            _service.Setup(x => x.Create(It.IsAny<PersonDto>())).ReturnsAsync(input);
            var dto = Fixture.Create<PersonModel>();
            
            var result = await _sut.Create(dto);
            
            result.Should().BeOfType<CreatedResult>();
            var created = (CreatedResult) result; 
            created.Location.Should().Be($"{ContextScheme}://{ContextHost}/people/{input.Id}");
            created.Value.Should().Be(input);
            var log = AuditLogHelper.AuditCreate("PersonController", dto.ToJson(), "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }

        [Test]
        public async Task Update_UpdateFailed_ReturnsNotFound()
        {
            _service.Setup(x => x.Update(It.IsAny<PersonDto>())).ReturnsAsync(false);

            var result = await _sut.Update(Fixture.Create<int>(), Fixture.Create<PersonModel>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Update_UpdateSucceeds_ReturnsOk()
        {
            _service.Setup(x => x.Update(It.IsAny<PersonDto>())).ReturnsAsync(true);
            var id = Fixture.Create<int>();
            var model = Fixture.Create<PersonModel>();
            
            var result = await _sut.Update(id, model);
            
            result.Should().BeOfType<OkResult>();
            var log = AuditLogHelper.AuditUpdate("PersonController", id, model.ToJson(), "TestUser");
            _logger.VerifyLogExact(LogLevel.Trace, log);
        }
    }
}