using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SNUS.SensorClient.Models;
using SNUS.Shared.DTOs;
using SNUS.Shared.Enums;

namespace SNUS.SensorClient.Services
{
    public class SensorSimulator
    {
        private readonly SensorHttpClient _httpClient;
        private readonly List<LocalSensor> _sensors;
        private readonly Dictionary<string, CancellationTokenSource> _activeTasks;
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _systemCts;

        public SensorSimulator(SensorHttpClient httpClient)
        {
            _httpClient = httpClient;
            _systemCts = new CancellationTokenSource();
            _sensors = new List<LocalSensor>();
            _activeTasks = new Dictionary<string, CancellationTokenSource>();

          
            _sensors.Add(new LocalSensor("SENZOR_01", 15, 45, DataQuality.GOOD, 25, 35, 42));
            _sensors.Add(new LocalSensor("SENZOR_02", 20, 100, DataQuality.GOOD, 50, 75, 90));
            _sensors.Add(new LocalSensor("SENZOR_03", -10, 30, DataQuality.GOOD, 10, 18, 26));
            _sensors.Add(new LocalSensor("SENZOR_04", 50, 150, DataQuality.GOOD, 80, 110, 135));
            _sensors.Add(new LocalSensor("SENZOR_05", 0, 60, DataQuality.GOOD, 20, 40, 55));
            _sensors.Add(new LocalSensor("SENZOR_06", 10, 70, DataQuality.GOOD, 25, 45, 60)); // Rezerva 1
            _sensors.Add(new LocalSensor("SENZOR_07", -5, 40, DataQuality.GOOD, 15, 25, 35));  // Rezerva 2
            _sensors.Add(new LocalSensor("SENZOR_08", 30, 120, DataQuality.GOOD, 60, 85, 105)); // Rezerva 3
        }

        public async Task StartSimulationAsync()
        {
            Console.WriteLine("=== SIMULACIJA POKRENUTA (DYNAMIC SPAWNING) ===");
            Console.WriteLine("Pritisni 'B' u glavnom programu za blokadu NASUMIČNOG aktivnog senzora.\n");


            _ = Task.Run(() => StartBalancingLoopAsync(_systemCts.Token));
        }

        private async Task StartBalancingLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                BalanceActiveSensors();
                await Task.Delay(1000, token); 
            }
        }

        private void BalanceActiveSensors()
        {
            lock (_lock)
            {
               
                foreach (var sensor in _sensors)
                {
                    if (sensor.BlockedUntilUtc.HasValue && sensor.BlockedUntilUtc.Value <= DateTime.UtcNow)
                    {
                        sensor.BlockedUntilUtc = null;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n[SISTEM]  Istekla blokada. Senzor {sensor.Id} je ponovo dostupan u bazenu.");
                        Console.ResetColor();
                    }
                }

               
                var blockedSensorIds = _sensors.Where(s => s.IsBlocked()).Select(s => s.Id).ToList();
                foreach (var id in blockedSensorIds)
                {
                    if (_activeTasks.ContainsKey(id))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"[SPAWNER] Gasim Task za {id} jer je blokiran.");
                        Console.ResetColor();

                        _activeTasks[id].Cancel();
                        _activeTasks.Remove(id);
                    }
                }


                int currentActiveCount = _activeTasks.Count;

                if (currentActiveCount < 5)
                {
                    int howManyToSpawn = 5 - currentActiveCount;

                    
                    var availablePool = _sensors
                        .Where(s => !s.IsBlocked() && !_activeTasks.ContainsKey(s.Id))
                        .Take(howManyToSpawn)
                        .ToList();

                    foreach (var sensor in availablePool)
                    {
                        var cts = new CancellationTokenSource();
                        _activeTasks[sensor.Id] = cts;

            
                        _ = Task.Run(() => RunSensorAsync(sensor, cts.Token));

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[SPAWNER]  Pokrenut zamenski senzor: {sensor.Id} da popuni mesto!");
                        Console.ResetColor();
                    }
                }
            }
        }

        private async Task RunSensorAsync(LocalSensor sensor, CancellationToken token)
        {
            var random = new Random();

            try
            {
                while (!token.IsCancellationRequested)
                {
                   
                    double temperature = sensor.GenerateRandomTemperature();
                    AlarmPriority priority = sensor.EvaluateAlarm(temperature);
                    long msgId = sensor.GetNextMessageId();

                    DateTime trenutnoVreme = DateTime.UtcNow;
                    string timestampStr = trenutnoVreme.ToString("o");

   
                    string plainTempText = temperature.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    string encryptedPayload = sensor.Crypto.Encrypt(plainTempText);
                    string dataToSign = $"{sensor.Id}|{msgId}|{encryptedPayload}|{timestampStr}";
                    string digitalSignature = sensor.Crypto.SignData(dataToSign);
                    string publicKey = sensor.Crypto.GetPublicKeyBase64();

                    IspisiPorukuUKonzoli(sensor.Id, temperature, priority);

                    var dto = new SensorReadingRequestDto
                    {
                        SensorId = sensor.Id,
                        Temperature = Math.Round(temperature, 2),
                        TimestampUtc = trenutnoVreme,
                        MessageId = msgId,
                        DataQuality = sensor.Quality,
                        AlarmPriority = priority,
                        EncryptedPayload = encryptedPayload,
                        DigitalSignature = digitalSignature,
                        PublicKey = publicKey
                    };

                    await _httpClient.SendReadingAsync(dto);

                   
                    int delaySeconds = random.Next(2, 6);
                    await Task.Delay(delaySeconds * 1000, token);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }


        public void TriggerTemporaryBlock()
        {
            lock (_lock)
            {
               
                if (!_activeTasks.Any())
                {
                    Console.WriteLine("[SISTEM] Nema aktivnih senzora za blokiranje.");
                    return;
                }

                var random = new Random();
                var activeIds = _activeTasks.Keys.ToList();
                string randomActiveId = activeIds[random.Next(activeIds.Count)];

                var targetSensor = _sensors.FirstOrDefault(s => s.Id == randomActiveId);
                if (targetSensor != null)
                {
                    targetSensor.BlockedUntilUtc = DateTime.UtcNow.AddSeconds(30);

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\n[SISTEM]  PRIVREMENA BLOKADA: Izabran nasumični aktivni {targetSensor.Id} i blokiran na 30 sekundi!");
                    Console.ResetColor();
                }
            }
        }

        private void IspisiPorukuUKonzoli(string id, double temp, AlarmPriority priority)
        {
            lock (Console.Out)
            {
                Console.Write($"[{DateTime.Now:HH:mm:ss}] Senzor {id}: {temp:F2}°C | ");

                switch (priority)
                {
                    case AlarmPriority.Priority1:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(" ALARM: Priority 1 (Blagi rast)");
                        break;
                    case AlarmPriority.Priority2:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(" ALARM: Priority 2 (Visoka temp!)");
                        break;
                    case AlarmPriority.Priority3:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" ALARM: Priority 3 (KRITIČNO!!!)");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(" Status OK");
                        break;
                }
                Console.ResetColor();
            }
        }
    }
}