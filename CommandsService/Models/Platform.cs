using System.ComponentModel.DataAnnotations;

namespace CommandsService.Models {
    public class Platform {

        [Key]
        [Required]
        public int Id { get; set; }

        // Platform ID
        [Required]
        public int ExternalId { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<Command> Commands {get; set;} = new List<Command>();
    }
}