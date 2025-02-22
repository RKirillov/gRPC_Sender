using GrpcServices;
using AutoMapper;
using gRPC_Sender.Entity;

namespace gRPC_Sender.Mapper
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<AdkuEntity, GrpcServices.Entity>()
                .ForMember(dest => dest.TagName, opt => opt.MapFrom(src => src.TagName))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
        }
    }
}
