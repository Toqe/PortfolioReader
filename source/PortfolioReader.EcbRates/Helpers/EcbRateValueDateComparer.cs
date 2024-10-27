using System.Collections.Generic;
using Toqe.PortfolioReader.EcbRates.Models;

namespace Toqe.PortfolioReader.EcbRates.Helpers
{
    public class EcbRateValueDateComparer : IComparer<EcbRateValue>
    {
        public int Compare(EcbRateValue x, EcbRateValue y)
        {
            return x.Date.CompareTo(y.Date);
        }
    }
}
