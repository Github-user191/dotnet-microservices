using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Profiles {
    public class PlatformsProfile : Profile {
        public PlatformsProfile() {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<PlatformCreateDto, Platform>();
            // Will be used in Controller when creating a Platform, since it returns a ReadDto, we need to 
            // convert that into a PublishedDto to send to RabbitMQ
            CreateMap<PlatformReadDto, PlatformPublishedDto>();
            // Map Platform Model to a Grpc model we specified in the proto file
            CreateMap<Platform, GrpcPlatformModel>()
                .ForMember(dest => dest.PlatformId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher));
        }

    }
}