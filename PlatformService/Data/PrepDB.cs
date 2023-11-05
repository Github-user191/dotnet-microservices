using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data {
    public static class PrepDB {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder, bool isProd) {
            using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope()) {
                SeedData(serviceScope.ServiceProvider.GetService<AppDBContext>(), isProd);
            }
        }

        private static void SeedData(AppDBContext context, bool isProd) {

            if(isProd) {
                Console.WriteLine("Attempting to run migrations");

                try {
                    context.Database.Migrate();
                }
                catch (Exception ex) {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }


            if(!context.Platforms.Any()) {
                Console.WriteLine("Seeding data new ver...");
                context.Platforms.AddRange(
                    new Platform() {Name = "Dot Net", Publisher = "Microsoft", Cost = "Free"},
                    new Platform() {Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free"},
                    new Platform() {Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free"}
                );

                context.SaveChanges();

            } else {
                Console.WriteLine("We already have data");
            }
        }
    }
}