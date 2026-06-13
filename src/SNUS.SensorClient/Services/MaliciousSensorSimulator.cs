using System;
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

        public async Task AttackWithBadSignatureAsync()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[HAKER] Pokrećem napad 'X': Šaljem modifikovane podatke sa lažnim potpisom...");
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


        public async Task AttackWithDDoSAsync()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[HAKER] POKREĆEM DDoS NAPAD 'Y': Ispaljujem 15 brzih uzastopnih zahteva za redom...");
            Console.ResetColor();


            var maliciousSensor = new LocalSensor("SENZOR_02", 20, 100, DataQuality.GOOD, 50, 75, 90);

            for (int i = 1; i <= 15; i++)
            {
                double temperature = 55.0 + i;
                string plainTempText = temperature.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                string encryptedPayload = maliciousSensor.Crypto.Encrypt(plainTempText);

                long msgId = DateTime.UtcNow.Ticks + i;
                DateTime trenutnoVreme = DateTime.UtcNow;
                string timestampStr = trenutnoVreme.ToString("o");

                string dataToSign = $"{maliciousSensor.Id}|{msgId}|{encryptedPayload}|{timestampStr}";
                string digitalSignature = maliciousSensor.Crypto.SignData(dataToSign);
                string publicKey = maliciousSensor.Crypto.GetPublicKeyBase64();

                var dto = new SensorReadingRequestDto
                {
                    SensorId = maliciousSensor.Id,
                    Temperature = Math.Round(temperature, 2),
                    TimestampUtc = trenutnoVreme,
                    MessageId = msgId,
                    DataQuality = DataQuality.GOOD,
                    AlarmPriority = AlarmPriority.None,
                    EncryptedPayload = encryptedPayload,
                    PublicKey = publicKey,
                    DigitalSignature = digitalSignature
                };

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[HAKER] Šaljem poruku {i}/15 za {maliciousSensor.Id}...");
                Console.ResetColor();


                await _httpClient.SendReadingAsync(dto);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[HAKER] DDoS napad 'Y' priveden kraju.\n");
            Console.ResetColor();
        }
    }
}