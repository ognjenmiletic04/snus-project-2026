using SNUS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Persistence.Entities
{
    public class Sensor
    {
        private int id;
        private string externalId = string.Empty;
        private DataQuality dataQuality = DataQuality.GOOD;
        private bool isActive;
        private DateTime createdAtUtc = DateTime.UtcNow;
        private DateTime? lastMessageAtUtc;
        private DateTime? blockedUntilUtc;
        private ICollection<SensorReading> readings = new List<SensorReading>();
        private ICollection<Alarm> alarms = new List<Alarm>();

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string ExternalId
        {
            get { return externalId; }
            set { externalId = value; }
        }

        public DataQuality DataQuality
        {
            get { return dataQuality; }
            set { dataQuality = value; }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public DateTime CreatedAtUtc
        {
            get { return createdAtUtc; }
            set { createdAtUtc = value; }
        }

        public DateTime? LastMessageAtUtc
        {
            get { return lastMessageAtUtc; }
            set { lastMessageAtUtc = value; }
        }

        public DateTime? BlockedUntilUtc
        {
            get { return blockedUntilUtc; }
            set { blockedUntilUtc = value; }
        }

        public ICollection<SensorReading> Readings
        {
            get { return readings; }
            set { readings = value; }
        }

        public ICollection<Alarm> Alarms
        {
            get { return alarms; }
            set { alarms = value; }
        }
    }
}
