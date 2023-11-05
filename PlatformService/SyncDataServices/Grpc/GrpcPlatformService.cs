using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

// Implementing a service class based on the proto file we created
namespace PlatformService.SyncDataServices.Grpc {
    // GrpcPlatform is the name of service specified in the proto file
    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase {
        private readonly IPlatformRepository _repository;
        private readonly IMapper _mapper;

        public GrpcPlatformService(IPlatformRepository repository, IMapper mapper) {
            _repository = repository;
            _mapper = mapper;
        }

        // Returning list of platforms according to the grpc call:
        // rpc GetAllPlatforms (GetAllRequest) returns (PlatformResponse);
        public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context) {
            var response = new PlatformResponse();
            var platforms = _repository.GetAllPlatforms();

            foreach(var platform in platforms) {
                response.Platform.Add(_mapper.Map<GrpcPlatformModel>(platform));
            }

            return Task.FromResult(response);
        }

    }
}