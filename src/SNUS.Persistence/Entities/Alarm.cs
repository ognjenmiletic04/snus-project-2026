using SNUS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Persistence.Entities
{
    public class Alarm
    {
        private long id;
        private int sensorId;
        private Sensor sensor = null!;
        private long sensorReadingId;
        private SensorReading sensorReading = null!;
        private AlarmPriority priority;
        private double temperature;
        private DateTime occurredAtUtc;
        private DateTime createdAtUtc = DateTime.UtcNow;

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        public int SensorId
        {
            get { return sensorId; }
            set { sensorId = value; }
        }

        public Sensor Sensor
        {
            get { return sensor; }
            set { sensor = value; }
        }

        public long SensorReadingId
        {
            get { return sensorReadingId; }
            set { sensorReadingId = value; }
        }

        public SensorReading SensorReading
        {
            get { return sensorReading; }
            set { sensorReading = value; }
        }

        public AlarmPriority Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public double Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public DateTime OccurredAtUtc
        {
            get { return occurredAtUtc; }
            set { occurredAtUtc = value; }
        }

        public DateTime CreatedAtUtc
        {
            get { return createdAtUtc; }
            set { createdAtUtc = value; }
        }
    }
}
