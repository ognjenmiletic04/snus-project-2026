using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNUS.Persistence.Entities
{
    public class ConsensusValue
    {
        private long id;
        private double value;
        private DateTime periodStartUtc;
        private DateTime periodEndUtc;
        private DateTime calculatedAtUtc = DateTime.UtcNow;
        private int usedReadingsCount;
        private string algorithm = "Initial";

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public DateTime PeriodStartUtc
        {
            get { return periodStartUtc; }
            set { periodStartUtc = value; }
        }

        public DateTime PeriodEndUtc
        {
            get { return periodEndUtc; }
            set { periodEndUtc = value; }
        }

        public DateTime CalculatedAtUtc
        {
            get { return calculatedAtUtc; }
            set { calculatedAtUtc = value; }
        }

        public int UsedReadingsCount
        {
            get { return usedReadingsCount; }
            set { usedReadingsCount = value; }
        }

        public string Algorithm
        {
            get { return algorithm; }
            set { algorithm = value; }
        }
    }
}
