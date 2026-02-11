namespace PatientApi.DTOs
{
    public class PatientResponse
    {
        public Guid Id { get; set; }
        public NameDto Name { get; set; } = new NameDto();
        public string Gender { get; set; } = "unknown";
        public DateTime BirthDate { get; set; }
        public bool Active { get; set; } = true;
    }
}
