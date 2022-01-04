using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aareon.Api.Controllers;
using Aareon.Api.Tests.Utilities;
using Aareon.Business.Interfaces;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Aareon.Api.Tests.Controllers
{
    public class ControllerTests : TestBase
    {
        private TestController _sut;
        private Mock<IService<TestDto>> _service;
        private Mock<ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IService<TestDto>>();
            _logger = new Mock<ILogger>();
            _sut = new TestController(_service.Object, _logger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = FakeHttpContext }
            };
        }

        [Test]
        public async Task GetAll_NoResult_ReturnsNoContent()
        {
            _service.Setup(x => x.GetAll()).ReturnsAsync(Enumerable.Empty<TestDto>());
            
            var result = await _sut.GetAll();
            
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task GetAll_ItemsResult_ReturnsOkWithContent()
        {
            const int count = 5;
            _service.Setup(x => x.GetAll()).ReturnsAsync(Enumerable.Repeat(new TestDto(), count));
            
            var result = await _sut.GetAll();
            
            result.Should().BeOfType<OkObjectResult>();
            var dto = result.GetFromActionResult<IEnumerable<TestDto>>();
            dto.Count().Should().Be(count);
            _logger.VerifyLogExact(LogLevel.Trace, "TestController: GetAll called by User: TestUser");
        }

        [Test]
        public async Task GetById_NullResult_ReturnsNotFound()
        {
            _service.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync((TestDto)null);
            
            var result = await _sut.GetById(Fixture.Create<int>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetById_ItemResult_ReturnsOkWithContent()
        {
            var input = Fixture.Create<TestDto>();
            _service.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(input);

            var id = Fixture.Create<int>();
            var result = await _sut.GetById(id);
            
            result.Should().BeOfType<OkObjectResult>();
            var dto = result.GetFromActionResult<TestDto>();
            dto.Id.Should().Be(input.Id);
            _logger.VerifyLogExact(LogLevel.Trace, 
                $"TestController: GetById called with id: {id} by User: TestUser");
        }

        [Test]
        public async Task Delete_FalseResult_ReturnsNotFound()
        {
            _service.Setup(x => x.Delete(It.IsAny<int>())).ReturnsAsync(false);
            
            var result = await _sut.Delete(Fixture.Create<int>());
            
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Delete_TrueResult_ReturnsOk()
        {
            var id = Fixture.Create<int>();
            _service.Setup(x => x.Delete(It.IsAny<int>())).ReturnsAsync(true);
            
            var result = await _sut.Delete(id);
            
            result.Should().BeOfType<OkResult>();
            _logger.VerifyLogExact(LogLevel.Trace, $"TestController: Delete called with id: {id} by User: TestUser");
        }
    }
    
    public class TestController : Controller<TestDto>
    {
        public TestController(IService<TestDto> service, ILogger logger) : 
            base(service, logger) { }
    }

    public class TestDto
    {
        public int Id { get; set; }
    }
}