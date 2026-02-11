using PatientApi.DTOs;
using PatientApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace PatientApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new patient", Description = "Adds a new patient to the database.")]
        [SwaggerResponse(201, "Patient created successfully.", typeof(PatientResponse))]
        [SwaggerResponse(400, "Invalid input.")]
        public async Task<ActionResult<PatientResponse>> CreatePatient(PatientRequest request)
        {
            if (string.IsNullOrEmpty(request.Name?.Family) || request.BirthDate == default)
                return BadRequest("Name.Family and BirthDate are required fields.");

            try
            {
                var response = await _patientService.CreatePatientAsync(request);
                return CreatedAtAction(nameof(GetPatient), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating patient: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Retrieves a patient by ID", Description = "Gets the details of a specific patient using their unique ID.")]
        [SwaggerResponse(200, "Patient found.", typeof(PatientResponse))]
        [SwaggerResponse(404, "Patient not found.")]
        public async Task<ActionResult<PatientResponse>> GetPatient(Guid id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();

            return Ok(patient);
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search patients by birthDate", Description = "Finds patients by birthDate based on HL7 FHIR search parameters. Supports prefixes: eq, ne, lt, gt, le, ge, sa, eb, ap")]
        [SwaggerResponse(200, "Patients found.", typeof(List<PatientResponse>))]
        [SwaggerResponse(400, "Invalid date format or parameter.")]
        public async Task<ActionResult<List<PatientResponse>>> SearchByBirthDate([FromQuery] string birthDate)
        {
            if (string.IsNullOrEmpty(birthDate))
            {
                return BadRequest("birthDate parameter is required");
            }

            try
            {
                var patients = await _patientService.SearchByBirthDateAsync(birthDate);
                return Ok(patients);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching patients: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Updates a patient", Description = "Updates the details of an existing patient.")]
        [SwaggerResponse(200, "Patient updated successfully.", typeof(PatientResponse))]
        [SwaggerResponse(400, "Invalid input.")]
        [SwaggerResponse(404, "Patient not found.")]
        public async Task<ActionResult<PatientResponse>> UpdatePatient(Guid id, PatientRequest request)
        {
            if (string.IsNullOrEmpty(request.Name?.Family))
                return BadRequest("Name.Family is required.");

            try
            {
                var patient = await _patientService.UpdatePatientAsync(id, request);
                if (patient == null) return NotFound();

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating patient: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Deletes a patient", Description = "Removes a patient from the database using their unique ID.")]
        [SwaggerResponse(200, "Patient deleted successfully.", typeof(PatientResponse))]
        [SwaggerResponse(404, "Patient not found.")]
        public async Task<ActionResult<PatientResponse>> DeletePatient(Guid id)
        {
            try
            {
                var patient = await _patientService.DeletePatientAsync(id);
                if (patient == null) return NotFound();

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting patient: {ex.Message}");
            }
        }
    }
}
