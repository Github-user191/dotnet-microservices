using PlatformService.Models;

namespace PlatformService.Data {
    class PlatformRepository : IPlatformRepository {
        private readonly AppDBContext _context;

        public PlatformRepository(AppDBContext context) {
            _context = context;
        }

        public void CreatePlatform(Platform platform) {
            if(platform == null) {
                throw new ArgumentNullException();
            }

            _context.Platforms.Add(platform);
        }

        public IEnumerable<Platform> GetAllPlatforms() {
            return _context.Platforms.ToList();
        }

        public Platform GetPlatformById(int Id) {
            return _context.Platforms.FirstOrDefault(p => p.Id == Id);
        }

        public bool SaveChanges() {
            return _context.SaveChanges() >= 0;
        }
    }
}