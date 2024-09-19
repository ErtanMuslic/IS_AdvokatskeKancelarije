using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            //User
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name));

            //Client
            CreateMap<Client, ClientDTO>();
            CreateMap<ClientDTO, Client>();

            //Document
            CreateMap<Document, DocumentDTO>();
            CreateMap<DocumentDTO, Document>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
