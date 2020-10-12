using AutoMapper;
using VeracodeMessageQueue.Models;
using VeracodeService.Models;

namespace VeraData.Profiles
{
    public class BuildProfile : Profile
    {
        public BuildProfile()
        {
            CreateMap<BuildInfoBuildType, Build>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.build_id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.version))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.analysis_unit[0].status));
        }
    }
}
