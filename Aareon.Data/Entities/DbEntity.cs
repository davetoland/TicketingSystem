using System.ComponentModel.DataAnnotations;

namespace Aareon.Data.Entities
{
    public class DbEntity
    {
        [Key]
        public int Id { get; set; }
    }
}