using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PatientApi.Entities
{
    public class GivenName
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        [JsonIgnore]
        public Patient Patient { get; set; }
    }
}
