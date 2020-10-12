using AutoMapper;
using VeracodeMessageQueue.Models;
using VeracodeService;
using VeracodeService.Models;

namespace VeraData.Profiles
{
    public class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<AppType, App>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.app_id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.app_name));
        }
    }
}
