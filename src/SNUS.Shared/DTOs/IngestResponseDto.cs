using SNUS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Shared.DTOs
{
    public class IngestResponseDto
    {
        private bool success;
        private string message = string.Empty;
        private long? readingId;
        private string sensorId = string.Empty;
        private AlarmPriority alarmPriority = AlarmPriority.None;
        private long lastValidMessageId;

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public long? ReadingId
        {
            get { return readingId; }
            set { readingId = value; }
        }

        public string SensorId
        {
            get { return sensorId; }
            set { sensorId = value; }
        }

        public AlarmPriority AlarmPriority
        {
            get { return alarmPriority; }
            set { alarmPriority = value; }
        }

        public long LastValidMessageId 
        {
            get { return lastValidMessageId; }
            set { lastValidMessageId = value; }
        }
    }
}