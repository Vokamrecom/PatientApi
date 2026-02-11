using PatientApi.Entities;

namespace PatientApi.Services
{
    public interface IDateSearchService
    {
        IQueryable<Patient> ApplyDateFilter(IQueryable<Patient> query, string birthDate);
    }
}
