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
    public class PersonRepositoryTests : TestBase
    {
        private AareonContext _context;
        private PersonRepository _repo;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _context = ContextHelper.CreateInMemoryContext();
            ContextHelper.SeedContext(_context);
            _repo = new PersonRepository(_context);
        }

        [Test]
        public async Task GetBySurname_NoPeopleWithSurname_ReturnEmptySet()
        {
            var result = await _repo.GetBySurname("").FirstOrDefaultAsync();
            
            result.Should().BeNull();
        }

        [Test]
        public async Task GetBySurname_PersonHasSurname_ReturnsPersonInCollection()
        {
            var person = await _repo.GetAll().Where(x => 
                !string.IsNullOrWhiteSpace(x.Surname)).FirstAsync();
            
            var result = await _repo.GetBySurname(person.Surname).ToListAsync();

            result.Should().Contain(person);
        }
    }
}