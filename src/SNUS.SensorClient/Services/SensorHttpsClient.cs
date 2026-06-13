using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Json;
using SNUS.Shared.DTOs;

namespace SNUS.SensorClient.Services
{
    public class SensorHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseAddress = "http://localhost:5225/api/ingest";

        public SensorHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<(bool Success, string? ErrorMessage)> SendReadingAsync(SensorReadingRequestDto readingDto)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_baseAddress, readingDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SERVER GREŠKA] Status: {response.StatusCode}, Detalji: {errorContent}");
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KOMUNIKACIJA GREŠKA] Nije moguće kontaktirati server. Poruka: {ex.Message}");
                return (false, null);
            }
        }
    }
}