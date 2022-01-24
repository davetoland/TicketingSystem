using System.Linq;
using System.Threading.Tasks;
using Aareon.Api.Tests.Controllers;
using Aareon.Api.Tests.Utilities;
using Aareon.Business.DTO;
using Aareon.Business.Exceptions;
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
    public class NoteServiceTests : TestBase
    {
        private Mock<IUnitOfWork> _uow;
        private Mapper _mapper;
        private Mock<ILogger<NoteService>> _logger;
        private Mock<INoteRepository> _noteRepo;
        private Mock<IPersonRepository> _personRepo;
        private Mock<IRepository<Ticket>> _ticketRepo;
        private NoteService _sut;

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
            Fixture = new Fixture();
            _logger = new Mock<ILogger<NoteService>>();
            _noteRepo = new Mock<INoteRepository>();
            _personRepo = new Mock<IPersonRepository>();
            _ticketRepo = new Mock<IRepository<Ticket>>();
            _sut = new NoteService(_uow.Object, _logger.Object, _mapper);
            _uow.Setup(x => x.NoteRepo).Returns(_noteRepo.Object);
            _uow.Setup(x => x.PersonRepo).Returns(_personRepo.Object);
            _uow.Setup(x => x.TicketRepo).Returns(_ticketRepo.Object);
        }
        
        [Test]
        public void Create_PersonDoesntExist_ThrowsInvalidPersonException()
        {
            _personRepo.Setup(x => x.GetById(It.IsAny<int>())).Returns(Enumerable.Empty<Person>().ToQueryable());
            var dto = Fixture.Create<NoteDto>();

            _sut.Invoking(x => x.Create(dto))
                .Should().ThrowAsync<InvalidPersonException>()
                .Where(x => x.PersonId == dto.Owner.Id);
        }
        
        [Test]
        public void Create_TicketIdIsNull_ThrowsInvalidTicketException()
        {
            // AutoFixture doesn't properly deal with lazy loading
            // properties (virtual props) that end up in circular references
            // and so we need to manually help it out. In a real scenario
            // you would write a customisation for this to stay DRY...
            var dto = Fixture.Build<NoteDto>()
                .With(x => x.TicketId, (int?)null)
                .Create();
            var person = Fixture.Build<Person>()
                .Without(x => x.Tickets)
                .Without(x => x.Notes)
                .Create();
            _personRepo.Setup(x => x.GetById(It.IsAny<int>())).Returns(person.ToQueryable());
            
            _sut.Invoking(x => x.Create(dto))
                .Should().ThrowAsync<InvalidTicketException>()
                .Where(x => x.TicketId == null);
        }
        
        [Test]
        public void Create_TicketDoesntExist_ThrowsInvalidTicketException()
        {
            var person = Fixture.Build<Person>()
                .Without(x => x.Tickets)
                .Without(x => x.Notes)
                .Create();
            
            _personRepo.Setup(x => x.GetById(It.IsAny<int>())).Returns(person.ToQueryable());
            _ticketRepo.Setup(x => x.GetById(It.IsAny<int>())).Returns(Enumerable.Empty<Ticket>().ToQueryable());
            var dto = Fixture.Create<NoteDto>();

            _sut.Invoking(x => x.Create(dto))
                .Should().ThrowAsync<InvalidTicketException>()
                .Where(x => x.TicketId == dto.TicketId);
        }
        
        [Test]
        public async Task Create_PersonAndTicketExist_NoteAdded()
        {
            var person = Fixture.Build<Person>()
                .Without(x => x.Tickets)
                .Without(x => x.Notes)
                .Create();
            var ticket = Fixture.Build<Ticket>()
                .Without(x => x.Owner)
                .Without(x => x.Notes)
                .Create();
            person.Tickets = new[] {ticket};
            ticket.Owner = person;
            
            _personRepo.Setup(x => x.GetById(It.IsAny<int>())).Returns(person.ToQueryable());
            _ticketRepo.Setup(x => x.GetById(It.IsAny<int>())).Returns(ticket.ToQueryable());
            var dto = Fixture.Create<NoteDto>();

            await _sut.Create(dto);

            _uow.Verify(x => x.Commit(), Times.Once);
            _noteRepo.Verify(x => x.Add(It.IsAny<Note>()), Times.Once);
            _logger.VerifyLogStartsWith(LogLevel.Information, "NoteService: Note created: ");
        }

        // [Test]
        // public void GetByTicket_NoNotesForTicket_ReturnsEmptySet()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void GetByTicket_NotesExistForTicket_ReturnsNotes()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void Update_NotesDoesntExist_ReturnsFalse()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void Update_PersonIdChanges_UpdatesOwnerReturnsTrue()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void Update_TicketIdChanges_UpdatesTicketReturnsTrue()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void Update_ContentChanges_UpdatesContentReturnsTrue()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void Remove_NoteDoesntExist_ReturnsFalse()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [Test]
        // public void Remove_NoteExists_DetachesNoteFromTicket()
        // {
        //     throw new NotImplementedException();
        // }
    }
}