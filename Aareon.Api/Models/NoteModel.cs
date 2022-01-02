using System.ComponentModel.DataAnnotations;

namespace Aareon.Api.Models
{
    public class NoteModel
    {
        [Required]
        public int? TicketId { get; set; }
        
        [Required]
        [MinLength(1)]
        [MaxLength(4000)]
        public string Content { get; set; }
    }
}