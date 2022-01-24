using System.ComponentModel.DataAnnotations;

namespace Aareon.Api.Models
{
    public class TicketModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(4000)]
        public string Content { get; set; }
    }
}