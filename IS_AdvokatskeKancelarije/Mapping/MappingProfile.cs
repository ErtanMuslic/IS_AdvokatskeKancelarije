using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using IS_AdvokatskeKancelarije.Repositories.Trials;

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

            //User
            CreateMap<User, ProfileDTO>();
            CreateMap<ProfileDTO, User>();

            //Client
            CreateMap<Client, ClientDTO>();
            CreateMap<ClientDTO, Client>();

            //Document
            CreateMap<Document, DocumentDTO>();
            CreateMap<DocumentDTO, Document>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            //Case
            CreateMap<Case, CaseDTO>();
            CreateMap<CaseDTO, Case>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            //Meeting
            CreateMap<Meeting, MeetingDTO>();
            CreateMap<MeetingDTO, Meeting>();

            //Invoice 
            CreateMap<Invoice,InvoiceDTO>();
            CreateMap<InvoiceDTO, Invoice>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));


            //Trial
            CreateMap<Trial, TrialDTO>();
            CreateMap<TrialDTO, Trial>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
