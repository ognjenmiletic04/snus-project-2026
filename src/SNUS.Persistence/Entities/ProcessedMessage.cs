using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Persistence.Entities
{
    public class ProcessedMessage
    {
        private long id;
        private int sensorId;
        private Sensor sensor = null!;
        private long messageId;
        private DateTime messageTimestampUtc;
        private DateTime receivedAtUtc = DateTime.UtcNow;

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

        public long MessageId
        {
            get { return messageId; }
            set { messageId = value; }
        }

        public DateTime MessageTimestampUtc
        {
            get { return messageTimestampUtc; }
            set { messageTimestampUtc = value; }
        }

        public DateTime ReceivedAtUtc
        {
            get { return receivedAtUtc; }
            set { receivedAtUtc = value; }
        }
    }
}
