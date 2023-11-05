using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing {
    public class EventProcessor : IEventProcessor {
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper) {
            _mapper = mapper;
            _scopeFactory = scopeFactory;
        }

        public void ProcessEvent(string message) {
            var eventType = DetermineEventType(message);


            switch(eventType) {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:
                    break; 
            }
        }

        // Determine what type of event was received when a platform is published
        private EventType DetermineEventType(string message) {
            Console.WriteLine("--> Determining event");

            // Getting only the EventType from the message
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(message);
            
            switch(eventType.Event) {
                case "Platform_Published":
                    Console.WriteLine("--> Platform published event detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Could not determine event type");
                    return EventType.Undetermined;

            }
        }

        // Adds a platform record in DB when an event is received for a service that published a platform
        private void addPlatform(string platformPublishedMessage) {
            using (var scope = _scopeFactory.CreateScope()) {
                // Get reference to ICommandRepo because we cant use Dependency Injection
                // The classes have different lifecyle types (Singleton vs Scoped)
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try {
                    var platform = _mapper.Map<Platform>(platformPublishedDto);
                    if(!repo.ExternalPlatformExists(platform.ExternalId)) {
                        repo.CreatePlatform(platform);
                        repo.SaveChanges();
                        Console.WriteLine("--> Platform was added to commands DB!");
                    } else {
                        Console.WriteLine("--> Platform already exists...");

                    }
                } catch(Exception ex) {
                    Console.WriteLine($"--> Could not add Platform to DB: {ex.Message}");
                }
            }
        }

        enum EventType {
            PlatformPublished,
            Undetermined
        }
    }
}