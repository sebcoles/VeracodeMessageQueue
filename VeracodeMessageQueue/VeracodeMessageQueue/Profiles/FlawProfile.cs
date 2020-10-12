using AutoMapper;
using System;
using VeracodeMessageQueue.Models;
using VeracodeService.Models;

namespace VeraData.Profiles
{
    public class FlawProfile : Profile
    {
        public FlawProfile()
        {
            CreateMap<FlawType, Flaw>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ParseInt(src.issueid)))
            .ForMember(dest => dest.MitigationStatus, opt => opt.MapFrom(src => ParseInt(src.line)))
            .ForMember(dest => dest.RemediationStatus, opt => opt.MapFrom(src => src.remediation_status));
        }

        public int ParseInt(string value)
        {
            return Int32.Parse(value);
        }
    }
}
