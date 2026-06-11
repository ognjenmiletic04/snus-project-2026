using SNUS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Shared.DTOs
{
    public class SensorReadingRequestDto
    {
        private string sensorId = string.Empty;
        private double temperature;
        private DateTime timestampUtc;
        private long messageId;
        private DataQuality dataQuality = DataQuality.GOOD;
        private AlarmPriority alarmPriority = AlarmPriority.None;

        public string SensorId
        {
            get { return sensorId; }
            set { sensorId = value; }
        }

        public double Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public DateTime TimestampUtc
        {
            get { return timestampUtc; }
            set { timestampUtc = value; }
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
    }
}
