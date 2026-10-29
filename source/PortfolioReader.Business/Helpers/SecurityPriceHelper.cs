using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Helpers;

public static class SecurityPriceHelper
{
    /// <summary>
    /// Returns the historical prices of the security ordered by date, plus its latest price
    /// (<see cref="PSecurity.Latest"/>, e.g. from a manual or intraday update in Portfolio Performance)
    /// if that one is newer than the newest historical price. Portfolio Performance stores
    /// the latest price separately and uses it the same way.
    /// </summary>
    public static List<PHistoricalPrice> GetOrderedPricesIncludingLatest(PSecurity security)
    {
        var result = security.Prices.OrderBy(x => x.Date).ToList();
        var latest = security.Latest;

        if (latest != null && (result.Count == 0 || latest.Date > result[^1].Date))
        {
            result.Add(new PHistoricalPrice { Date = latest.Date, Close = latest.Close });
        }

        return result;
    }
}
