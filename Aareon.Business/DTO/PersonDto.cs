namespace Aareon.Business.DTO
{
    public class PersonDto
    {
        public int Id { get; set; }
        
        public string Forename { get; set; }

        public string Surname { get; set; }

        public string FullName { get; set; }

        public bool IsAdmin { get; set; }
    }
}