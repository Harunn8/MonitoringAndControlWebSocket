using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Models;
using Services.AlarmService.Responses;

namespace Services.AlarmService.Mapper
{
    public class AlarmManagerMappingProfile : Profile
    {
        public AlarmManagerMappingProfile()
        {
            CreateMap<AlarmModel, AlarmResponse>();
        }

    }
}
