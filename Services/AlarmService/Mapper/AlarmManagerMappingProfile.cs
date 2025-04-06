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
            CreateMap<AlarmModel, AlarmResponse>()
                .ForMember(dest => dest.AlarmCreateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.FixedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType.ToString()))
                .ForMember(dest => dest.IsAlarmActive, opt => opt.MapFrom(src => src.IsAlarmActive))
                .ForMember(dest => dest.IsAlarmFixed, opt => opt.MapFrom(src => src.IsAlarmFixed))
                .ForMember(dest => dest.IsMasked, opt => opt.MapFrom(src => src.IsMasked))
                .ForMember(dest => dest.AlarmCondition, opt => opt.MapFrom(src => src.AlarmCondition))
                .ForMember(dest => dest.AlarmThreshold, opt => opt.MapFrom(src => src.AlarmThreshold))
                .ForMember(dest => dest.AlarmName, opt => opt.MapFrom(src => src.AlarmName))
                .ForMember(dest => dest.AlarmDescription, opt => opt.MapFrom(src => src.AlarmDescription))
                .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity))
                .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        }

    }
}
