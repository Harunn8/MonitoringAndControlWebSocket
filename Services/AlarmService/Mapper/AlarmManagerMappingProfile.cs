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
                .ForMember(d => d.Id, opt => opt.MapFrom(p => p.Id))
                .ForMember(d => d.AlarmName, opt => opt.MapFrom(p => p.AlarmName))
                .ForMember(d => d.AlarmDescription, opt => opt.MapFrom(p => p.AlarmDescription))
                .ForMember(d => d.Severity, opt => opt.MapFrom(p => p.Severity))
                .ForMember(d => d.AlarmCreateTime, opt => opt.MapFrom(p => p.AlarmCreateTime))
                .ForMember(d => d.FixedDate, opt => opt.MapFrom(p => p.FixedDate))
                .ForMember(d => d.DeviceId, opt => opt.MapFrom(p => p.DeviceId))
                .ForMember(d => d.IsAlarmActive, opt => opt.MapFrom(p => p.IsAlarmActive))
                .ForMember(d => d.IsAlarmFixed, opt => opt.MapFrom(p => p.IsAlarmFixed))
                .ForMember(d => d.IsMasked, opt => opt.MapFrom(p => p.IsMasked))
                .ForMember(d => d.AlarmCondition, opt => opt.MapFrom(p => p.AlarmCondition))
                .ForMember(d => d.AlarmThreshold, opt => opt.MapFrom(p => p.AlarmThreshold))
                .ForMember(d => d.DeviceType, opt => opt.MapFrom(p => p.DeviceType));
        }

    }
}
