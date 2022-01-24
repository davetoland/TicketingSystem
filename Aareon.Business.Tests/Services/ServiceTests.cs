using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aareon.Api.Tests.Utilities;
using Aareon.Business.Services;
using Aareon.Data.Entities;
using Aareon.Data.Interfaces;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Aareon.Business.Tests.Services
{
    [TestFixture]
    public class ServiceTests : TestBase
    {
        private Mock<IUnitOfWork> _uow;
        private IMapper _mapper;
        private Mock<ILogger> _logger;
        private Mock<IRepository<TestEntity>> _repo;
        private TestEntityService _sut;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _uow = new Mock<IUnitOfWork>();
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TestEntity, TestDto>();
                cfg.AddProfile<BusinessMappings>();
            }));
            _logger = new Mock<ILogger>();
            _repo = new Mock<IRepository<TestEntity>>();
            _sut = new TestEntityService(_uow.Object, _mapper, _logger.Object);
            _uow.Setup(x => x.GetRepository<TestEntity>()).Returns(_repo.Object);
        }
        
        [Test]
        public async Task GetAll_NoItems_ReturnsEmptySet()
        {
            _repo.Setup(x => x.GetAll()).Returns(Enumerable.Empty<TestEntity>().ToQueryable);

            var result = await _sut.GetAll();

            result.Should().BeEmpty();
        }
        
        [Test]
        public async Task GetAll_HasItems_ReturnsProjectedSet()
        {
            var entities = Fixture.CreateMany<TestEntity>().ToList();
            _repo.Setup(x => x.GetAll()).Returns(entities.ToQueryable);

            var result = (await _sut.GetAll()).ToList();
            
            result.Should().HaveCount(entities.Count);
            result.Should().BeEquivalentTo(_mapper.Map<IEnumerable<TestDto>>(entities));
        }
        
        [Test]
        public async Task GetById_NoItem_ReturnsNull()
        {
            _repo.Setup(x => x.GetById(It.IsAny<int>())).Returns(Enumerable.Empty<TestEntity>().ToQueryable());

            var result = await _sut.GetById(Fixture.Create<int>());

            result.Should().BeNull();
        }
        
        [Test]
        public async Task GetById_HasItem_ReturnsProjectedItem()
        {
            var entity = Fixture.Create<TestEntity>();
            _repo.Setup(x => x.GetById(It.IsAny<int>())).Returns(entity.ToQueryable);

            var id = Fixture.Create<int>();
            var result = await _sut.GetById(id);
            
            result.Should().BeEquivalentTo(_mapper.Map<TestDto>(entity));
        }
        
        [Test]
        public async Task Delete_ItemDoesntExist_ReturnsFalse()
        {
            _repo.Setup(x => x.Exists(It.IsAny<int>())).ReturnsAsync(false);
            var id = Fixture.Create<int>();
            
            var result = await _sut.Delete(id);

            result.Should().BeFalse();
            _logger.VerifyLogExact(LogLevel.Warning, Service<TestEntity, TestDto>.AuditNonExist("delete", id));
        }
        
        [Test]
        public async Task Delete_ItemExists_CommitsDeleteReturnsTrue()
        {
            _repo.Setup(x => x.Exists(It.IsAny<int>())).ReturnsAsync(true);
            var id = Fixture.Create<int>();
            
            var result = await _sut.Delete(id);

            result.Should().BeTrue();
            _repo.Verify(x => x.Delete(id), Times.Once);
            _uow.Verify(x => x.Commit(), Times.Once);
            _logger.VerifyLogExact(LogLevel.Information, $"TestEntityService: TestEntity with Id: {id} deleted.");
        }
    }
    
    // Create a default implementation of the (abstract) system under test that we can test against
    public class TestEntityService : Service<TestEntity, TestDto>
    {
        public TestEntityService(IUnitOfWork uow, IMapper mapper, ILogger logger)
            : base(uow, mapper, logger) { }

        public override Task<TestDto> Create(TestDto dto) =>
            throw new System.NotImplementedException();

        public override Task<bool> Update(TestDto dto) =>
            throw new System.NotImplementedException();
    }

    public class TestEntity : DbEntity { }

    public class TestDto
    {
        public int Id { get; set; }
    }
}