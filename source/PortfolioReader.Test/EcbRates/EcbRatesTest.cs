using Toqe.PortfolioReader.EcbRates;
using Toqe.PortfolioReader.EcbRates.Models;
using Xunit;

namespace Toqe.PortfolioReader.Test.EcbRates
{
    public class EcbRatesTest
    {
        [Fact]
        public void TestParse()
        {
            var reader = new EcbRateReader();
            var basePath = new TestHelper().GetTestDataBasePath();
            var filename = Path.Combine(basePath.FullName, "eurofxref-hist.xml");
            var xml = File.ReadAllText(filename);
            
            var rates = reader.Parse(xml);
            
            Assert.Equal(90, rates.Count);

            var rate = rates.First();
            Assert.Equal(1.0938M, rate.Rate);
            Assert.Equal(new DateTime(2024, 10, 11), rate.Date);
            Assert.Equal("USD", rate.Currency);

            rate = rates.Last();
            Assert.Equal(19.3282M, rate.Rate);
            Assert.Equal(new DateTime(2024, 10, 9), rate.Date);
            Assert.Equal("ZAR", rate.Currency);
        }

        [Fact]
        public void TestRateProvider()
        {
            var reader = new EcbRateReader();
            var basePath = new TestHelper().GetTestDataBasePath();
            var filename = Path.Combine(basePath.FullName, "eurofxref-hist.xml");
            var xml = File.ReadAllText(filename);
            var rates = reader.Parse(xml);
            var rateProvider = new EcbRateProvider(rates);

            var rate = rateProvider.Get("USD", new DateTime(2024, 10, 11));
            Assert.Equal(1.0938m, rate);

            rate = rateProvider.Get("USD", new DateTime(2024, 10, 12));
            Assert.Equal(1.0938m, rate);

            rate = rateProvider.Get("USD", new DateTime(2024, 10, 20));
            Assert.Equal(1.0938m, rate);

            rate = rateProvider.Get("USD", new DateTime(2034, 1, 1));
            Assert.Equal(1.0938m, rate);

            rate = rateProvider.Get("USD", new DateTime(2024, 10, 10));
            Assert.Equal(1.0932m, rate);

            rate = rateProvider.Get("USD", new DateTime(2024, 10, 9));
            Assert.Equal(1.0957m, rate);

            rate = rateProvider.Get("USD", new DateTime(2024, 10, 8));
            Assert.Equal(1.0957m, rate);

            rate = rateProvider.Get("USD", new DateTime(2000, 12, 31));
            Assert.Equal(1.0957m, rate);
        }

        [Fact]
        public void TestBinarySearch()
        {
            var provider = new EcbRateProvider(new List<EcbRateValue>());
            var date = new DateTime(2024, 10, 14);

            var rates = new EcbRateValue[0];
            var index = provider.BinarySearch(rates, date);
            Assert.Equal(0, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 14) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(0, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 14) },
                new EcbRateValue { Date = new DateTime(2024, 10, 15) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(0, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 13) },
                new EcbRateValue { Date = new DateTime(2024, 10, 14) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(1, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 12) },
                new EcbRateValue { Date = new DateTime(2024, 10, 13) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(1, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 15) },
                new EcbRateValue { Date = new DateTime(2024, 10, 16) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(0, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 11) },
                new EcbRateValue { Date = new DateTime(2024, 10, 12) },
                new EcbRateValue { Date = new DateTime(2024, 10, 13) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(2, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 12) },
                new EcbRateValue { Date = new DateTime(2024, 10, 13) },
                new EcbRateValue { Date = new DateTime(2024, 10, 14) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(2, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 13) },
                new EcbRateValue { Date = new DateTime(2024, 10, 14) },
                new EcbRateValue { Date = new DateTime(2024, 10, 15) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(1, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 14) },
                new EcbRateValue { Date = new DateTime(2024, 10, 15) },
                new EcbRateValue { Date = new DateTime(2024, 10, 16) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(0, index);

            rates = new EcbRateValue[]
            {
                new EcbRateValue { Date = new DateTime(2024, 10, 15) },
                new EcbRateValue { Date = new DateTime(2024, 10, 16) },
                new EcbRateValue { Date = new DateTime(2024, 10, 17) },
            };

            index = provider.BinarySearch(rates, date);
            Assert.Equal(0, index);
        }
    }
}
