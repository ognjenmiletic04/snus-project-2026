using System;
using SNUS.Shared.Enums;

namespace SNUS.SensorClient.Models
{
    public class LocalSensor
    {
        private static readonly Random _random = new Random();
        private int _messageCounter = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public Security.CryptoService Crypto { get; private set; } = new Security.CryptoService();
        public string Id { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public DataQuality Quality { get; set; }

        public double Alarm1Limit { get; set; }
        public double Alarm2Limit { get; set; }
        public double Alarm3Limit { get; set; }

        public DateTime? BlockedUntilUtc { get; set; }

        public LocalSensor(string id, double minTemp, double maxTemp, DataQuality quality, double limit1, double limit2, double limit3)
        {
            Id = id;
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            Quality = quality;
            Alarm1Limit = limit1;
            Alarm2Limit = limit2;
            Alarm3Limit = limit3;
        }

        public double GenerateRandomTemperature()
        {
            return _random.NextDouble() * (MaxTemperature - MinTemperature) + MinTemperature;
        }

        public int GetNextMessageId()
        {
            _messageCounter++;
            return _messageCounter;
        }

        public void SyncMessageCounter(long lastServerId)
        {
            if (lastServerId >= _messageCounter)
            {
                _messageCounter = (int)lastServerId;
            }
        }

        public bool IsBlocked()
        {
            return BlockedUntilUtc.HasValue && BlockedUntilUtc.Value > DateTime.UtcNow;
        }

        public void UspostaviBlokadu(int sekunde)
        {
            BlockedUntilUtc = DateTime.UtcNow.AddSeconds(sekunde);
        }

        public AlarmPriority EvaluateAlarm(double temperature)
        {
            if (temperature >= Alarm3Limit)
                return AlarmPriority.Priority3;

            if (temperature >= Alarm2Limit)
                return AlarmPriority.Priority2;

            if (temperature >= Alarm1Limit)
                return AlarmPriority.Priority1;

            return AlarmPriority.None;
        }
    }
}