using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SNUS.SensorClient.Models;
using SNUS.Shared.Enums; 

namespace SNUS.SensorClient.Services
{
    public class DynamicSensorSimulator
    {
        private readonly List<LocalSensor> _allSensors;
        private readonly Dictionary<string, CancellationTokenSource> _activeTasks;
        private readonly object _lock = new object();
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl = "http://localhost:5225/api/ingest"; 

        public DynamicSensorSimulator(List<LocalSensor> sensors)
        {
            _allSensors = sensors;
            _activeTasks = new Dictionary<string, CancellationTokenSource>();
            _httpClient = new HttpClient();
        }

        public async Task StartSimulationAsync(CancellationToken systemToken)
        {
            while (!systemToken.IsCancellationRequested)
            {
                BalanceActiveSensors();
                await Task.Delay(1000, systemToken); 
            }
        }

        private void BalanceActiveSensors()
        {
            lock (_lock)
            {
          
                var blockedSensorIds = _allSensors
                    .Where(s => s.IsBlocked())
                    .Select(s => s.Id)
                    .ToList();

                foreach (var id in blockedSensorIds)
                {
                    if (_activeTasks.ContainsKey(id))
                    {
                        Console.WriteLine($"[SPAWNER] Gasim rad za {id} jer je privremeno blokiran.");
                        _activeTasks[id].Cancel();
                        _activeTasks.Remove(id);
                    }
                }

           
                int currentActiveCount = _activeTasks.Count;

                if (currentActiveCount < 5)
                {
                    int howManyToSpawn = 5 - currentActiveCount;

                    var availablePool = _allSensors
                        .Where(s => !s.IsBlocked() && !_activeTasks.ContainsKey(s.Id))
                        .Take(howManyToSpawn)
                        .ToList();

                    foreach (var sensor in availablePool)
                    {
                        var cts = new CancellationTokenSource();
                        _activeTasks[sensor.Id] = cts;

                        Task.Run(() => RunSensorAsync(sensor, cts.Token));
                        Console.WriteLine($"[SPAWNER]  Pokrenut rezervni senzor: {sensor.Id} da popuni mesto do 5 aktivnih!");
                    }
                }
            }
        }

        private async Task RunSensorAsync(LocalSensor sensor, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
 
                    double temperature = sensor.GenerateRandomTemperature();
                    AlarmPriority priority = sensor.EvaluateAlarm(temperature);
                    int msgId = sensor.GetNextMessageId();

                    DateTime trenutnoVreme = DateTime.UtcNow;
                    string timestampStr = trenutnoVreme.ToString("o");

                    string plainTempText = temperature.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    string encryptedPayload = sensor.Crypto.Encrypt(plainTempText);
                    string dataToSign = $"{sensor.Id}|{msgId}|{encryptedPayload}|{timestampStr}";
                    string digitalSignature = sensor.Crypto.SignData(dataToSign);
                    string publicKey = sensor.Crypto.GetPublicKeyBase64();

                    IspisiPorukuUKonzoli(sensor.Id, temperature, priority);

                    var dto = new
                    {
                        SensorId = sensor.Id,
                        Temperature = Math.Round(temperature, 2),
                        TimestampUtc = trenutnoVreme,
                        MessageId = (long)msgId,
                        DataQuality = sensor.Quality,
                        AlarmPriority = priority,
                        EncryptedPayload = encryptedPayload,
                        DigitalSignature = digitalSignature,
                        PublicKey = publicKey
                    };

                    try
                    {

                        var response = await _httpClient.PostAsJsonAsync(_serverUrl, dto, token);

                        if (!response.IsSuccessStatusCode)
                        {
                            string greskaDetonacija = await response.Content.ReadAsStringAsync();
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine($"[SERVER GRESKA] Status: {response.StatusCode}, Detalji: {greskaDetonacija}");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MREZA GRESKA] Neuspešno slanje za {sensor.Id}: {ex.Message}");
                    }

                    int randomDelay = new Random().Next(1000, 10000);
                    await Task.Delay(randomDelay, token);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void IspisiPorukuUKonzoli(string sensorId, double temp, AlarmPriority priority)
        {
            string vreme = DateTime.Now.ToString("HH:mm:ss");
            string statusTekst = priority == AlarmPriority.None ? "Status OK" : $"ALARM: {priority}";

            switch (priority)
            {
                case AlarmPriority.Priority1:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case AlarmPriority.Priority2:
                    Console.ForegroundColor = ConsoleColor.DarkYellow; 
                    break;
                case AlarmPriority.Priority3:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            Console.WriteLine($"[{vreme}] Senzor {sensorId}: {temp:F2}°C | {statusTekst}");
            Console.ResetColor();
        }
    }
}