using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aareon.Business.Interfaces
{
    public interface IService<TDto>
    {
        Task<TDto> Create(TDto dto);
        Task<IEnumerable<TDto>> GetAll();
        Task<TDto> GetById(int id);
        Task<bool> Update(TDto dto);
        Task<bool> Delete(int id);
    }
}