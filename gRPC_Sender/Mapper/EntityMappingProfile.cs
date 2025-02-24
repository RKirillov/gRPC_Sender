using GrpcServices;
using AutoMapper;
using gRPC_Sender.Entity;
using Google.Protobuf.WellKnownTypes;

namespace gRPC_Sender.Mapper
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<AdkuEntity, GrpcServices.Entity>()
                .ForMember(dest => dest.TagName, opt => opt.MapFrom(src => src.TagName))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src.DateTime, DateTimeKind.Utc))))
                .ForMember(dest => dest.DateTimeUTC, opt => opt.MapFrom(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src.DateTimeUTC, DateTimeKind.Utc))))
                .ForMember(dest => dest.RegisterType, opt => opt.MapFrom(src => (GrpcServices.RegisterType)src.RegisterType));

        }
    }
}
