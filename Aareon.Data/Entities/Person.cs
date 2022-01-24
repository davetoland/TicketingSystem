using System.Collections.Generic;

namespace Aareon.Data.Entities
{
    public class Person : DbEntity
    {
        public string Forename { get; set; }

        public string Surname { get; set; }

        public bool IsAdmin { get; set; }
        
        public virtual IEnumerable<Ticket> Tickets { get; set; }
        public virtual IEnumerable<Note> Notes { get; set; }
    }
}