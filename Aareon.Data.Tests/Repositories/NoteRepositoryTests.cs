using System;
using System.Linq;
using System.Threading.Tasks;
using Aareon.Business.Tests;
using Aareon.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Aareon.Data.Tests.Repositories
{
    [TestFixture]
    public class NoteRepositoryTests : TestBase
    {
        private AareonContext _context;
        private NoteRepository _repo;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _context = ContextHelper.CreateInMemoryContext();
            ContextHelper.SeedContext(_context);
            _repo = new NoteRepository(_context);
        }

        [Test]
        public async Task GetByTicket_TicketIdDoesntExists_ReturnEmptySet()
        {
            var result = await _repo.GetByTicket(-1).FirstOrDefaultAsync();
            
            result.Should().BeNull();
        }

        [Test]
        public async Task GetByTicket_TicketIdExists_ReturnsNotes()
        {
            var withTicket = await _repo.GetAll().Where(x => x.TicketId != null).FirstAsync();
            if (withTicket.TicketId == null)
                throw new Exception("TicketId cannot be null");
            var ticketId = (int)withTicket.TicketId;
            
            var result = await _repo.GetByTicket(ticketId).ToListAsync();

            result.Count.Should().BeGreaterThan(0);
            result.ForEach(x => x.TicketId.Should().Be(ticketId));
        }
    }
}