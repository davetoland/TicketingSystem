using System.Threading.Tasks;
using Aareon.Business.DTO;

namespace Aareon.Business.Interfaces
{
    public interface IPersonService : IService<PersonDto>
    {
        Task<PersonDto> GetBySurname(string surname);
    }
}