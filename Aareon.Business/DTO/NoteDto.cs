namespace Aareon.Business.DTO
{
    public class NoteDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int? TicketId { get; set; }
        public PersonDto Owner { get; set; }
    }
}