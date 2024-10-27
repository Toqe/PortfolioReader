using System;
using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.EcbRates.Helpers;
using Toqe.PortfolioReader.EcbRates.Models;

namespace Toqe.PortfolioReader.EcbRates
{
    public class EcbRateProvider
    {
        private static EcbRateValueDateComparer comparer = new EcbRateValueDateComparer();

        private Dictionary<string, List<EcbRateValue>> currencyToEcbRateValuesMap;

        public EcbRateProvider(
            List<EcbRateValue> rates)
        {
            currencyToEcbRateValuesMap = rates
                .GroupBy(x => x.Currency)
                .ToDictionary(x => x.Key, x => x.OrderBy(x => x.Date).ToList());
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

        internal int BinarySearch(List<EcbRateValue> rates, DateTime date)
        {
            var index = rates.ToList().BinarySearch(new EcbRateValue { Date = date }, comparer);

            if (index >= 0)
            {
                return index;
            }

            if (~index == rates.Count)
            {
                return Math.Max(0, rates.Count - 1);
            }

            return ~index;
        }
    }
}
