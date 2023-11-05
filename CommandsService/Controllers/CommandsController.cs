using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;

namespace CommandsService.Controllers {

    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase {

        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper){
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId) {
            Console.WriteLine($"--> Hit GetCommandsForPlatform for Platform with ID {platformId}");

            if(!_repository.PlatformExists(platformId)) {
                return NotFound("Platform not found");
            }

            var commands = _repository.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId) {
            Console.WriteLine($"--> Hit GetCommandForPlatform for Platform with ID {platformId} and Command with ID {commandId}");

            if (!_repository.PlatformExists(platformId)) {
                return NotFound("Platform not found");
            }
            var command = _repository.GetCommand(platformId, commandId);

            if(command == null) {
                return NotFound();
            }

            // Map to a CommandReadDto from a Command
            return Ok(_mapper.Map<CommandReadDto>(command));

        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(int platformId, CommandCreateDto commandCreateDto) {
            Console.WriteLine($"--> Hit CreateCommand for Platform with ID {platformId} and command to create {commandCreateDto}");

            if (!_repository.PlatformExists(platformId)) {
                return NotFound("Platform not found");
            }

            // Map to a Command from a CommandCreateDto
            var command = _mapper.Map<Command>(commandCreateDto);

            _repository.CreateCommand(platformId, command);
            _repository.SaveChanges();

            // Map to a CommandReadDto from a Command
            var commandReadDto = _mapper.Map<CommandReadDto>(command);

            return CreatedAtRoute(nameof(GetCommandForPlatform),
                new { platformId = platformId, commandId = commandReadDto.Id }, commandReadDto);




        }

    }
}