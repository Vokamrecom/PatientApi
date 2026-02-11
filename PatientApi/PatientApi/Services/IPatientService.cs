using PatientApi.DTOs;

namespace PatientApi.Services
{
    public interface IPatientService
    {
        Task<PatientResponse?> GetPatientByIdAsync(Guid id);
        Task<PatientResponse> CreatePatientAsync(PatientRequest request);
        Task<PatientResponse?> UpdatePatientAsync(Guid id, PatientRequest request);
        Task<PatientResponse?> DeletePatientAsync(Guid id);
        Task<List<PatientResponse>> SearchByBirthDateAsync(string birthDate);
    }
}
