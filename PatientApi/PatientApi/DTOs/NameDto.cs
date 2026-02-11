using System.ComponentModel.DataAnnotations;

namespace PatientApi.DTOs
{
    public class NameDto
    {
        public Guid? Id { get; set; }
        public string? Use { get; set; }
        
        [Required]
        public string Family { get; set; } = string.Empty;
        
        public ICollection<string> Given { get; set; } = new List<string>();
    }
}
