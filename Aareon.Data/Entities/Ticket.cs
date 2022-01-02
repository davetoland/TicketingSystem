using System.Collections.Generic;

namespace Aareon.Data.Entities
{
    public class Ticket : DbEntity
    {
        public string Content { get; set; }

        public int PersonId { get; set; }
        
        // If these classes were scaffolded in a Db First approach, then we wouldn't
        // want to annotate them like this as they'd be overwritten during the next scaffold.
        // In that case, they'd be generated as partial classes enabling us to create our
        // own corresponding partials of them containing these annotations there instead... 
        public virtual Person Owner { get; set; }
        public virtual IEnumerable<Note> Notes { get; set; }
    }
}