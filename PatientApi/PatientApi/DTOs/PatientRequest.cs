using System.ComponentModel.DataAnnotations;

namespace PatientApi.DTOs
{
    public class PatientRequest
    {
        [Required]
        public NameDto Name { get; set; } = new NameDto();

        public string Gender { get; set; } = "unknown";

        [Required]
        public DateTime BirthDate { get; set; }

        public bool Active { get; set; } = true;
    }
}
