# PatientApi

REST API application for managing Patient entities.

## Requirements

- .NET 6.0
- Docker and Docker Compose
- PostgreSQL (runs via Docker Compose)

## Project Structure

- `PatientApi/PatientApi` - main REST API application
- `PatientApi/PatientApiClient` - console application for generating test data

## Quick Start

### Running the Project

Clone the repository:
```bash
git clone <repository_url>
```

Navigate to the project directory:
```bash
cd PatientApi
```

Set up the project and PostgreSQL by running the following command:
```bash
docker-compose up --build
```

**Note:** The console application will automatically start after the API starts and generate 100 patients. To manually run the data generation:
```bash
docker-compose run --rm patientapiclient
```

- Swagger UI: http://localhost:5000/swagger


## API Methods

### CRUD Operations

1. **POST** `/api/patients` - Create a new patient
2. **GET** `/api/patients/{id}` - Get patient by ID
3. **PUT** `/api/patients/{id}` - Update patient
4. **DELETE** `/api/patients/{id}` - Delete patient

### Search by birthDate

**GET** `/api/patients/search?birthDate={prefix}{date}`

Supported prefixes according to HL7 FHIR:
- `eq` - equals (default if prefix is not specified)
- `ne` - not equals
- `lt` - less than
- `gt` - greater than
- `le` - less than or equal
- `ge` - greater than or equal
- `sa` - starts after
- `eb` - ends before
- `ap` - approximately (10% tolerance)

#### Request Examples:

- `GET /api/patients/search?birthDate=eq2024-01-13` - patients born on January 13, 2024
- `GET /api/patients/search?birthDate=ge2024-01-13` - patients born on or after January 13, 2024
- `GET /api/patients/search?birthDate=lt2024-01-13T10:00:00` - patients born before 10:00 AM on January 13, 2024
- `GET /api/patients/search?birthDate=ge2024` - patients born in 2024 or later
- `GET /api/patients/search?birthDate=eq2024-01` - patients born in January 2024

#### Supported Date Formats:
- Full date and time: `yyyy-MM-ddTHH:mm:ss`
- Date only: `yyyy-MM-dd`
- Year and month: `yyyy-MM`
- Year only: `yyyy`

## Postman Collection

Import the `PatientApi.postman_collection.json` file into Postman to test all API methods.


### Running Without Docker

1. Install PostgreSQL locally
2. Update the connection string in `appsettings.json`
3. Run the API:
   ```bash
   cd PatientApi/PatientApi
   dotnet run
   ```
4. Run the client to generate data:
   ```bash
   cd PatientApi/PatientApiClient
   dotnet run
   ```

## Technologies

- .NET 6.0
- ASP.NET Core Web API
- Entity Framework Core 6.0 (Npgsql 6.0)
- PostgreSQL
- Swagger/OpenAPI
- Docker & Docker Compose
