using Aareon.Api.Models;
using Aareon.Business.DTO;
using AutoMapper;

namespace Aareon.Api
{
    public class ApiMappings : Profile
    {
        public ApiMappings()
        {
            CreateMap<PersonModel, PersonDto>()
                .ForMember(dest => dest.FullName, o => o.MapFrom(src => $"{src.Forename} {src.Surname}"));
            CreateMap<TicketModel, TicketDto>()
                .ForMember(dest => dest.Owner, o => o.MapFrom(src => new PersonDto()));
            CreateMap<NoteModel, NoteDto>()
                .ForMember(dest => dest.Owner, o => o.MapFrom(src => new PersonDto()));
        }
    }
}