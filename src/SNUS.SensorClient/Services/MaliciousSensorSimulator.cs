using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNUS.SensorClient.Models;
using SNUS.Shared.DTOs;
using SNUS.Shared.Enums;

namespace SNUS.SensorClient.Services
{
    public class MaliciousSensorSimulator
    {
        private readonly SensorHttpClient _httpClient;

        public MaliciousSensorSimulator(SensorHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AttackSystemAsync()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n [HAKER] Pokrećem napad: Šaljem modifikovane podatke sa lažnim potpisom...");
            Console.ResetColor();

            var fakeSensor = new LocalSensor("SENZOR_01", 15, 45, DataQuality.GOOD, 25, 35, 42);

            string encryptedPayload = fakeSensor.Crypto.Encrypt("150.00");
            long msgId = DateTime.UtcNow.Ticks;

            var dto = new SensorReadingRequestDto
            {
                SensorId = fakeSensor.Id,
                Temperature = 150.00,
                TimestampUtc = DateTime.UtcNow,
                MessageId = msgId,
                DataQuality = DataQuality.GOOD,
                AlarmPriority = AlarmPriority.Priority3,

                EncryptedPayload = encryptedPayload,
                PublicKey = fakeSensor.Crypto.GetPublicKeyBase64(),

                DigitalSignature = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 })
            };

            await _httpClient.SendReadingAsync(dto);
        }
    }
}