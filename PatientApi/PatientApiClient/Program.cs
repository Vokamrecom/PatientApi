using System.Text.Json;
using System.Text;

namespace PatientApiClient
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            string apiUrl = "http://patientapi:5000/api/patients";

            Console.WriteLine("Waiting for API to be ready...");
            var maxWaitAttempts = 30;
            var waitDelay = TimeSpan.FromSeconds(2);
            
            for (int attempt = 0; attempt < maxWaitAttempts; attempt++)
            {
                try
                {
                    var healthCheck = await _client.GetAsync($"{apiUrl.Replace("/api/patients", "")}/swagger/index.html");
                    if (healthCheck.IsSuccessStatusCode || healthCheck.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("API is ready!");
                        break;
                    }
                }
                catch
                {
                }
                
                if (attempt < maxWaitAttempts - 1)
                {
                    Console.WriteLine($"Waiting for API... (attempt {attempt + 1}/{maxWaitAttempts})");
                    await Task.Delay(waitDelay);
                }
            }

            var genders = new[] { "male", "female", "other", "unknown" };
            var firstNames = new[] { "Иван", "Мария", "Алексей", "Анна", "Дмитрий", "Елена", "Сергей", "Ольга" };
            var middleNames = new[] { "Иванович", "Петрович", "Сергеевич", "Александрович", "Ивановна", "Петровна", "Сергеевна" };
            var lastNames = new[] { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов" };

            for (int i = 1; i <= 100; i++)
            {
                var patient = new
                {
                    name = new
                    {
                        id = Guid.NewGuid(),
                        use = "official",
                        family = lastNames[_random.Next(lastNames.Length)],
                        given = new List<string> 
                        { 
                            firstNames[_random.Next(firstNames.Length)],
                            middleNames[_random.Next(middleNames.Length)]
                        }
                    },
                    gender = genders[_random.Next(genders.Length)],
                    birthDate = DateTime.Now.AddDays(-_random.Next(1, 3650)).ToString("yyyy-MM-ddTHH:mm:ss"),
                    active = i % 3 != 0
                };

                var json = JsonSerializer.Serialize(patient, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _client.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Patient {i} added successfully.");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to add Patient {i}: {response.StatusCode} - {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding Patient {i}: {ex.Message}");
                }
            }

            Console.WriteLine("Finished adding patients.");
        }
    }
}