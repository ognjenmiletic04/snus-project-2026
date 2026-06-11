using SNUS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Persistence.Entities
{
    public class SensorReading
    {
        private long id;
        private int sensorId;
        private Sensor sensor = null!;
        private double temperature;
        private DateTime measuredAtUtc;
        private DateTime receivedAtUtc = DateTime.UtcNow;
        private long messageId;
        private DataQuality dataQuality;
        private AlarmPriority alarmPriority = AlarmPriority.None;
        private bool isConsensus = false;

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

        public double Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public DateTime MeasuredAtUtc
        {
            get { return measuredAtUtc; }
            set { measuredAtUtc = value; }
        }

        public DateTime ReceivedAtUtc
        {
            get { return receivedAtUtc; }
            set { receivedAtUtc = value; }
        }

        public long MessageId
        {
            get { return messageId; }
            set { messageId = value; }
        }

        public DataQuality DataQuality
        {
            get { return dataQuality; }
            set { dataQuality = value; }
        }

        public AlarmPriority AlarmPriority
        {
            get { return alarmPriority; }
            set { alarmPriority = value; }
        }

        public bool IsConsensus
        {
            get { return isConsensus; }
            set { isConsensus = value; }
        }
    }
}
