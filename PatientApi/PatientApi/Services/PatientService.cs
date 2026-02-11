using Microsoft.EntityFrameworkCore;
using PatientApi.DTOs;
using PatientApi.Entities;

namespace PatientApi.Services
{
    public class PatientService : IPatientService
    {
        private readonly DataContext _context;
        private readonly IDateSearchService _dateSearchService;

        public PatientService(DataContext context, IDateSearchService dateSearchService)
        {
            _context = context;
            _dateSearchService = dateSearchService;
        }

        public async Task<PatientResponse?> GetPatientByIdAsync(Guid id)
        {
            var patient = await _context.Patients
                .Include(p => p.Given)
                .FirstOrDefaultAsync(x => x.Id == id);

            return patient == null ? null : MapToResponse(patient);
        }

        public async Task<PatientResponse> CreatePatientAsync(PatientRequest request)
        {
            var patientId = Guid.NewGuid();
            var nameId = request.Name?.Id ?? Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                NameId = nameId,
                NameUse = request.Name?.Use ?? "official",
                Family = request.Name?.Family ?? string.Empty,
                Gender = request.Gender ?? "unknown",
                BirthDate = ConvertToUtc(request.BirthDate),
                Active = request.Active,
                Given = CreateGivenNames(request.Name?.Given, patientId)
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return MapToResponse(patient);
        }

        public async Task<PatientResponse?> UpdatePatientAsync(Guid id, PatientRequest request)
        {
            var patient = await _context.Patients
                .Include(x => x.Given)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (patient == null) return null;

            patient.NameId = request.Name?.Id ?? patient.NameId;
            patient.NameUse = request.Name?.Use ?? patient.NameUse;
            patient.Family = request.Name?.Family ?? patient.Family;
            patient.Gender = request.Gender ?? patient.Gender;
            patient.BirthDate = ConvertToUtc(request.BirthDate);
            patient.Active = request.Active;

            patient.Given.Clear();
            patient.Given = CreateGivenNames(request.Name?.Given, patient.Id);

            await _context.SaveChangesAsync();

            return MapToResponse(patient);
        }

        public async Task<PatientResponse?> DeletePatientAsync(Guid id)
        {
            var patient = await _context.Patients
                .Include(x => x.Given)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (patient == null) return null;

            var response = MapToResponse(patient);
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<List<PatientResponse>> SearchByBirthDateAsync(string birthDate)
        {
            var query = _context.Patients.Include(p => p.Given);
            var filteredQuery = _dateSearchService.ApplyDateFilter(query, birthDate);
            var patients = await filteredQuery.ToListAsync();

            return patients.Select(MapToResponse).ToList();
        }

        private DateTime ConvertToUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }

        private List<GivenName> CreateGivenNames(ICollection<string>? givenNames, Guid patientId)
        {
            return givenNames?.Select(x => new GivenName
            {
                Name = x,
                PatientId = patientId
            }).ToList() ?? new List<GivenName>();
        }

        private PatientResponse MapToResponse(Patient patient)
        {
            return new PatientResponse
            {
                Id = patient.Id,
                Name = new NameDto
                {
                    Id = patient.NameId,
                    Use = patient.NameUse,
                    Family = patient.Family,
                    Given = patient.Given.Select(x => x.Name).ToList()
                },
                Gender = patient.Gender,
                BirthDate = patient.BirthDate,
                Active = patient.Active
            };
        }
    }
}
