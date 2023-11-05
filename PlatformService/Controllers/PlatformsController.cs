using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase {
        private readonly IPlatformRepository _platformRepository;
        private readonly IMessageBusClient _messageBusClient;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;

        public PlatformsController(IPlatformRepository platformRepository, IMapper mapper, ICommandDataClient commandDataClient, IMessageBusClient messageBusClient) {
            _platformRepository = platformRepository;
            _mapper = mapper;
            _messageBusClient = messageBusClient;
            _commandDataClient = commandDataClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms() {
            Console.WriteLine("Getting platforms xxxxxxxxxxxxxxx..");

            var platforms = _platformRepository.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet("{Id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int Id) {
            var platform = _platformRepository.GetPlatformById(Id);

            if(platform == null) {
                return NotFound();
            }

            return Ok(_mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto) {
            // Map from PlatformCreateDto to Platform Model to save the model to the DB
            var platformModel = _mapper.Map<Platform>(platformCreateDto);
            _platformRepository.CreatePlatform(platformModel);
            _platformRepository.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);

            // Sending message Synchronously
            try {
                Console.WriteLine($"--> Sending message Synchronously");
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            } catch(Exception ex) {
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }

            // Sending message Asynchronously with RabbitMQ
            // Doesnt depend on the CommandService being up as messages will be sent to the Queue
            try {
                Console.WriteLine($"--> Sending message Asynchronously");
                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
                // Type of Event message to send to Queue
                platformPublishedDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(platformPublishedDto);
                 
            } catch(Exception ex) {
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }

            // Returns URI we can retrieve the created resource at
            return CreatedAtRoute(nameof(GetPlatformById) , 
                new {Id = platformReadDto.Id}, platformReadDto);
        }
        
    }
}