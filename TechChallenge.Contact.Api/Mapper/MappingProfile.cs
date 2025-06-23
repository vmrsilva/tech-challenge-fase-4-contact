using AutoMapper;
using TechChallange.Contact.Api.Controllers.Contact.Dto;
using TechChallange.Domain.Contact.Entity;

namespace TechChallange.Contact.Api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ContactCreateDto, ContactEntity>();
            CreateMap<ContactUpdateDto, ContactEntity>();
            CreateMap<ContactEntity, ContactResponseDto>();
        }
    }
}
