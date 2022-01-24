using System.ComponentModel.DataAnnotations;

namespace Aareon.Api.Models
{
    public class PersonModel
    {
        [Required]
        [MinLength(1)]
        public string Forename { get; set; }

        [Required]
        [MinLength(1)]
        public string Surname { get; set; }

        public bool IsAdmin { get; set; } = false;
    }
}