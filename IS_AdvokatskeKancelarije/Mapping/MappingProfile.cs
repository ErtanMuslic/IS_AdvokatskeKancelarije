using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name));
        }
    }
}
