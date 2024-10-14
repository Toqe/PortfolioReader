using System;

namespace Toqe.PortfolioReader.EcbRates.Models
{
    public class EcbRateValue
    {
        public string Currency { get; set; }

        public decimal Rate { get; set; }

        public DateTime Date { get; set; }
    }
}
