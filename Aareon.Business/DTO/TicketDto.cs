using System.Collections.Generic;

namespace Aareon.Business.DTO
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public PersonDto Owner { get; set; }
    }
}