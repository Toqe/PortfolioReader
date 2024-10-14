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
    }
}
