using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VeracodeMessageQueue.Models;
using VeracodeService.Models;

namespace VeracodeMessageQueue.Profiles
{
    public interface IMappingService
    {
        App[] Apps(AppType[] entities);
        Build[] Scans(BuildInfoBuildType[] builds);
        Flaw[] Flaws(FlawType[] entities);
    }

    public class MappingService : IMappingService
    {
        private readonly IMapper _mapper;
        public MappingService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public App[] Apps(AppType[] entities) => entities.Select(x => _mapper.Map<App>(x)).ToArray();
        public Build[] Scans(BuildInfoBuildType[] entities) => entities.Select(x => _mapper.Map<Build>(x)).ToArray();
        public Flaw[] Flaws(FlawType[] entities) => entities.Select(x => _mapper.Map<Flaw>(x)).ToArray();
    }
}
