using System.Collections.Generic;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Helpers
{
    public class PHistoricalPriceDateComparer : IComparer<PHistoricalPrice>
    {
        public int Compare(PHistoricalPrice x, PHistoricalPrice y)
        {
            return x.Date.CompareTo(y.Date);
        }
    }
}
