using SNUS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Shared.DTOs
{
    public class SensorStatusDto
    {
        private string sensorId = string.Empty;
        private DataQuality dataQuality;
        private bool isActive;
        private DateTime? lastMessageAtUtc;
        private DateTime? blockedUntilUtc;

        public string SensorId
        {
            get { return sensorId; }
            set { sensorId = value; }
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

        public bool IsBlocked
        {
            get
            {
                return blockedUntilUtc.HasValue && blockedUntilUtc.Value > DateTime.UtcNow;
            }
        }
    }
}
