using Toqe.PortfolioReader.Business;
using Toqe.PortfolioReader.Business.Protobuf;
using Xunit;

namespace Toqe.PortfolioReader.Test
{
    public class ClientManagerTest
    {
        [Fact]
        public void TestGetCurrentPortfoliosValues()
        {
            var basePath = new TestHelper().GetTestDataBasePath();
            var reader = new PortfolioProtobufReader();
            var filename = Path.Combine(basePath.FullName, "test.portfolio");
            var client = reader.ReadClient(filename);

            var manager = new ClientManager();
            var model = manager.GetCurrentPortfoliosValues(client, null);

            Assert.NotNull(model);
            Assert.Equal(2, model.Portfolios.Count);

            {
                var depot = model.Portfolios[0];
                Assert.NotNull(depot);
                Assert.Equal("depot", depot.Portfolio.Name);

                Assert.Equal(2, depot.SecurityValues.Count);

                var allianz = depot.SecurityValues[0];
                Assert.NotNull(allianz);
                Assert.Equal("Allianz SE", allianz.Security.Name);
                Assert.Equal(2, allianz.Shares);
                Assert.Equal(550, allianz.MarketValue);

                var sap = depot.SecurityValues[1];
                Assert.NotNull(sap);
                Assert.Equal("SAP SE", sap.Security.Name);
                Assert.Equal(10.5, sap.Shares);
                Assert.Equal(2039.10, sap.MarketValue);
            }

            {
                var depot2 = model.Portfolios[1];
                Assert.NotNull(depot2);
                Assert.Equal("depot2", depot2.Portfolio.Name);

                Assert.Equal(1, depot2.SecurityValues.Count);

                var allianz = depot2.SecurityValues[0];
                Assert.NotNull(allianz);
                Assert.Equal("Allianz SE", allianz.Security.Name);
                Assert.Equal(5, allianz.Shares);
                Assert.Equal(1375, allianz.MarketValue);
            }
        }

        [Fact]
        public void TestGetPortfoliosValuesHistory()
        {
            var basePath = new TestHelper().GetTestDataBasePath();
            var reader = new PortfolioProtobufReader();
            var filename = Path.Combine(basePath.FullName, "test.portfolio");
            var client = reader.ReadClient(filename);

            var manager = new ClientManager();
            var model = manager.GetSecuritiesValuesHistory(client, client.Transactions, null);

            Assert.NotNull(model);

            var startDate = new DateTime(2024, 7, 1);

            for (var i = 0; i < 31; i++)
            {
                var xEntry = model.Skip(i).First();
                Assert.Equal(startDate.AddDays(i), xEntry.Key);
                Assert.Empty(xEntry.Value);
            }
            
            var entry = model.Skip(31).First();
            Assert.Equal(new DateTime(2024, 8, 1), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(10, entry.Value.First().Shares);
            Assert.Equal(191.46d, entry.Value.First().Price);
            Assert.Equal(1914.6d, entry.Value.First().MarketValue);

            entry = model.Skip(32).First();
            Assert.Equal(new DateTime(2024, 8, 2), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(9, entry.Value.First().Shares);
            Assert.Equal(186.16d, entry.Value.First().Price);
            Assert.Equal(1675.44d, entry.Value.First().MarketValue);

            entry = model.Skip(33).First();
            Assert.Equal(new DateTime(2024, 8, 3), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(9, entry.Value.First().Shares);
            Assert.Equal(186.16d, entry.Value.First().Price);
            Assert.Equal(1675.44d, entry.Value.First().MarketValue);

            entry = model.Skip(34).First();
            Assert.Equal(new DateTime(2024, 8, 4), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(9, entry.Value.First().Shares);
            Assert.Equal(186.16d, entry.Value.First().Price);
            Assert.Equal(1675.44d, entry.Value.First().MarketValue);

            entry = model.Skip(35).First();
            Assert.Equal(new DateTime(2024, 8, 5), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(11, entry.Value.First().Shares);
            Assert.Equal(181.22d, entry.Value.First().Price);
            Assert.Equal(1993.42d, entry.Value.First().MarketValue);

            entry = model.Skip(36).First();
            Assert.Equal(new DateTime(2024, 8, 6), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(10.5, entry.Value.First().Shares);
            Assert.Equal(185.40d, entry.Value.First().Price);
            Assert.Equal(1946.7d, entry.Value.First().MarketValue);

            entry = model.Skip(37).First();
            Assert.Equal(new DateTime(2024, 8, 7), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(10.5, entry.Value.First().Shares);
            Assert.Equal(187.98d, entry.Value.First().Price);
            Assert.Equal(1973.79d, entry.Value.First().MarketValue);

            entry = model.Skip(38).First();
            Assert.Equal(new DateTime(2024, 8, 8), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(10.5, entry.Value.First().Shares);
            Assert.Equal(188.28d, entry.Value.First().Price);
            Assert.Equal(1976.94d, entry.Value.First().MarketValue);

            entry = model.Skip(39).First();
            Assert.Equal(new DateTime(2024, 8, 9), entry.Key);
            Assert.Equal(1, entry.Value.Count);
            Assert.Equal("SAP SE", entry.Value.First().Security.Name);
            Assert.Equal(10.5, entry.Value.First().Shares);
            Assert.Equal(189.92d, entry.Value.First().Price);
            Assert.Equal(1994.16d, entry.Value.First().MarketValue);

            entry = model.Skip(40).First();
            Assert.Equal(new DateTime(2024, 8, 10), entry.Key);
            Assert.Equal(2, entry.Value.Count);
            var sapValue = entry.Value.First(x => x.Security.Name == "SAP SE");
            Assert.Equal(10.5, sapValue.Shares);
            Assert.Equal(189.92d, sapValue.Price);
            Assert.Equal(1994.16d, sapValue.MarketValue);
            var allianzValue = entry.Value.First(x => x.Security.Name == "Allianz SE");
            Assert.Equal(7, allianzValue.Shares);
            Assert.Equal(255.9d, allianzValue.Price);
            Assert.Equal(1791.3d, allianzValue.MarketValue);
            Assert.Equal(allianzValue, entry.Value.First());
            Assert.Equal(sapValue, entry.Value.Last());

            entry = model.Skip(41).First();
            Assert.Equal(new DateTime(2024, 8, 11), entry.Key);
            Assert.Equal(2, entry.Value.Count);
            sapValue = entry.Value.First(x => x.Security.Name == "SAP SE");
            Assert.Equal(10.5, sapValue.Shares);
            Assert.Equal(189.92d, sapValue.Price);
            Assert.Equal(1994.16d, sapValue.MarketValue);
            allianzValue = entry.Value.First(x => x.Security.Name == "Allianz SE");
            Assert.Equal(7, allianzValue.Shares);
            Assert.Equal(255.9d, allianzValue.Price);
            Assert.Equal(1791.3d, allianzValue.MarketValue);
        }
    }
}
