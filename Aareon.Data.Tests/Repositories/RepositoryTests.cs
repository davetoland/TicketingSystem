using System.Linq;
using System.Threading.Tasks;
using Aareon.Business.Tests;
using Aareon.Data.Entities;
using Aareon.Data.Exceptions;
using Aareon.Data.Repositories;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace Aareon.Data.Tests.Repositories
{
    [TestFixture]
    public class RepositoryTests : TestBase
    {
        private AareonContext _context;
        private Repository<Person> _repo;
        private Person _newPerson;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _context = ContextHelper.CreateInMemoryContext();
            ContextHelper.SeedContext(_context);
            _repo = new Repository<Person>(_context);
            _newPerson = Fixture.Build<Person>()
                .With(x => x.Id, 0)
                .Without(x => x.Tickets)
                .Without(x => x.Notes)
                .Create();
        }

        [Test]
        public void Add_IdAlreadyExists_ThrowsException()
        {
            _newPerson.Id = _repo.GetAll().First().Id; 

            _repo.Invoking(x => x.Add(_newPerson))
                .Should().ThrowAsync<RepositoryException>()
                .WithMessage($"Person with Id {_newPerson.Id} already exists");
        }

        [Test]
        public async Task Add_IdDoesntExist_ItemAdded()
        {
            var initialId = _newPerson.Id;
            
            var person = await _repo.Add(_newPerson);
            await _context.SaveChangesAsync();

            person.Id.Should().NotBe(initialId);
            var exists = await _repo.Exists(person.Id);
            exists.Should().BeTrue();
        }
        
        [Test]
        public void Update_NonExistentItem_ThrowsException()
        {
            _repo.Invoking(x => x.Update(_newPerson))
                .Should().ThrowAsync<RepositoryException>()
                .WithMessage($"Person with Id {_newPerson.Id} does not exist");
        }

        [Test]
        public async Task Update_ExistingItem_UpdatesItem()
        {
            var existing = _repo.GetAll().First();
            existing.Forename = Fixture.Create<string>();
            
            await _repo.Update(existing);
            await _context.SaveChangesAsync();
            
            var itemFromRepo = _repo.GetById(existing.Id).First();
            itemFromRepo.Forename.Should().Be(existing.Forename);
        }

        [Test]
        public void Delete_NonExistentItem_ThrowsException()
        {
            _repo.Invoking(async x => await x.Delete(_newPerson.Id))
                .Should().ThrowAsync<RepositoryException>()
                .WithMessage($"Person with Id {_newPerson.Id} does not exist");
        }

        [Test]
        public async Task Delete_ExistingItem_DeletesItem()
        {
            var existing = _repo.GetAll().First();
            
            await _repo.Delete(existing.Id);
            await _context.SaveChangesAsync();

            var itemFromRepo = _repo.GetById(existing.Id).FirstOrDefault();
            itemFromRepo.Should().BeNull();
        }
    }
}