namespace Aareon.Data.Entities
{
    public class Note : DbEntity
    {
        public int? TicketId { get; set; }
        public string Content { get; set; }
        public int PersonId { get; set; }
        
        public virtual Ticket Ticket { get; set; }
        public virtual Person Owner { get; set; }
    }
}