using System;
using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.EcbRates.Models;

namespace Toqe.PortfolioReader.EcbRates
{
    public class EcbRateProvider
    {
        private Dictionary<string, EcbRateValue[]> currencyToEcbRateValuesMap;

        public EcbRateProvider(
            List<EcbRateValue> rates)
        {
            currencyToEcbRateValuesMap = rates
                .GroupBy(x => x.Currency)
                .ToDictionary(x => x.Key, x => x.OrderBy(x => x.Date).ToArray());
        }

        public decimal Get(string currency, DateTime date)
        {
            var found = this.currencyToEcbRateValuesMap.TryGetValue(currency, out var currencyBucket);

            if (!found)
            {
                throw new InvalidOperationException($"Currency '{currency}' not found.");
            }

            var index = this.BinarySearch(currencyBucket, date);
            return currencyBucket[index].Rate;
        }

        internal int BinarySearch(EcbRateValue[] rates, DateTime date)
        {
            int lo = 0, hi = rates.Length - 1;

            while (lo <= hi)
            {
                var median = lo + (hi - lo >> 1);
                var medianDate = rates[median].Date;
                var num = medianDate.CompareTo(date);

                if (num == 0)
                {
                    return median;
                }

                if (lo == hi)
                {
                    return lo;
                }

                if (num < 0)
                {
                    lo = median + 1;
                }
                else
                {
                    hi = median - 1;
                }
            }

            return lo;
        }
    }
}
