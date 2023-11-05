using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http {
    public class HttpCommandDataClient : ICommandDataClient {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration) {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendPlatformToCommand(PlatformReadDto platformReadDto) {
            
            // Create payload and serialize before sending
            var httpContent = new StringContent(JsonSerializer.Serialize(platformReadDto), Encoding.UTF8, "application/json");

            // Perform Post HTTP request, set BASE URL in configuration
            var response = await _httpClient.PostAsync($"{_configuration["CommandService"]}", httpContent);
        
            if(response.IsSuccessStatusCode) {
                Console.WriteLine("--> Sync POST to CommandService success");
            } else {
                Console.WriteLine("--> Sync POST to CommandService unsuccessful");
            }
        }
    }
}