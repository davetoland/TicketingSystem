using Aareon.Business.DTO;
using Aareon.Data.Entities;
using AutoMapper;

namespace Aareon.Business
{
    public class BusinessMappings : Profile
    {
        public BusinessMappings()
        {
            CreateMap<Ticket, TicketDto>();
            CreateMap<Person, PersonDto>()
                .ForMember(dest => dest.FullName, o => o.MapFrom(src => $"{src.Forename} {src.Surname}") );

            CreateMap<PersonDto, Person>();
            CreateMap<TicketDto, Ticket>()
                .ForMember(dest => dest.Owner, opts => opts.Ignore())
                .ForMember(dest => dest.PersonId, opts => opts.MapFrom(src => src.Owner.Id));
        }
    }
}