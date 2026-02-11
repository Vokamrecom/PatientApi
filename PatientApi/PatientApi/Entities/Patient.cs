using System.ComponentModel.DataAnnotations;

namespace PatientApi.Entities
{
    public class Patient
    {
        [Key]
        public Guid Id { get; set; }
        public Guid NameId { get; set; }
        public string NameUse { get; set; } = "official";
        public string Family { get; set; } = string.Empty;
        public IList<GivenName> Given { get; set; } = new List<GivenName>();
        public string Gender { get; set; } = "unknown"; // male, female, other, unknown
        public DateTime BirthDate { get; set; }
        public bool Active { get; set; } = true;
    }
}
