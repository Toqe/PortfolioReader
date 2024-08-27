using Toqe.PortfolioReader.Business.Protobuf;
using Xunit;

namespace Toqe.PortfolioReader.Test.Protobuf
{
    public class PortfolioProtobufReaderTest
    {
        [Fact]
        public void TestReadClient()
        {
            var basePath = this.GetSourceBasePath();
            var reader = new PortfolioProtobufReader();
            var client = reader.ReadClient(Path.Combine(basePath.FullName, "PortfolioReader.Test", "TestData", "test.portfolio"));

            Assert.Equal("EUR", client.baseCurrency);

            Assert.Equal(2, client.Accounts.Count);

            var konto = client.Accounts.FirstOrDefault(x => x.Name == "konto");
            Assert.NotNull(konto);
            Assert.Equal("EUR", konto.currencyCode);

            var konto2 = client.Accounts.FirstOrDefault(x => x.Name == "konto2");
            Assert.NotNull(konto2);
            Assert.Equal("EUR", konto2.currencyCode);

            Assert.Equal(2, client.Portfolios.Count);

            var depot = client.Portfolios.FirstOrDefault(x => x.Name == "depot");
            Assert.NotNull(depot);
            Assert.Equal(konto.Uuid, depot.referenceAccount);

            var depot2 = client.Portfolios.FirstOrDefault(x => x.Name == "depot2");
            Assert.NotNull(depot2);
            Assert.Equal(konto2.Uuid, depot2.referenceAccount);

            Assert.Equal(2, client.Securities.Count);

            var sap = client.Securities.FirstOrDefault(x => x.Name == "SAP SE");
            Assert.NotNull(sap);

            var allianz = client.Securities.FirstOrDefault(x => x.Name == "Allianz SE");
            Assert.NotNull(allianz);

            Assert.Equal(17, client.Transactions.Count);

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 1));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Deposit, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(10000, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 2));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Removal, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(100.02, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 3));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Interest, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.Security);
                Assert.Equal(1, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(1.22, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
                var transactionUnit = transaction.Units.FirstOrDefault();
                Assert.NotNull(transactionUnit);
                Assert.Equal(PTransactionUnit.Type.Tax, transactionUnit.type);
                var transactionUnitWrapper = new TransactionUnitWrapper(transactionUnit);
                Assert.Equal(0.01, transactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 4));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.InterestCharge, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(5.23, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 5));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Fee, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(0.02, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Equal(sap, transactionWrapper.Security);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 6));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.FeeRefund, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(3.21, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Equal(sap, transactionWrapper.Security);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 7));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Tax, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(5.43, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.Security);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 8, 1, 23, 0));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.TaxRefund, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.Portfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(8.76, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Equal(sap, transactionWrapper.Security);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 7, 9, 1, 23, 0));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.CashTransfer, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Equal(konto2.Uuid, transaction.otherAccount);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(9.99, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Equal(konto2, transactionWrapper.OtherAccount);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.Security);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 1));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Purchase, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Equal(depot.Uuid, transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(2, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(1916.28, transactionWrapper.Amount);
                Assert.Equal(10, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Equal(depot, transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(sap, transactionWrapper.Security);
                var taxTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Tax);
                Assert.NotNull(taxTransactionUnit);
                var taxTransactionUnitWrapper = new TransactionUnitWrapper(taxTransactionUnit);
                Assert.Equal(0.45, taxTransactionUnitWrapper.Amount);
                var feeTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Fee);
                Assert.NotNull(feeTransactionUnit);
                var feeTransactionUnitWrapper = new TransactionUnitWrapper(feeTransactionUnit);
                Assert.Equal(1.23, feeTransactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 2));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Sale, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Equal(depot.Uuid, transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(2, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(184.48, transactionWrapper.Amount);
                Assert.Equal(1, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Equal(depot, transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(sap, transactionWrapper.Security);
                var taxTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Tax);
                Assert.NotNull(taxTransactionUnit);
                var taxTransactionUnitWrapper = new TransactionUnitWrapper(taxTransactionUnit);
                Assert.Equal(0.45, taxTransactionUnitWrapper.Amount);
                var feeTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Fee);
                Assert.NotNull(feeTransactionUnit);
                var feeTransactionUnitWrapper = new TransactionUnitWrapper(feeTransactionUnit);
                Assert.Equal(1.23, feeTransactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 3));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.Dividend, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(2, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(9.83, transactionWrapper.Amount);
                Assert.Equal(9, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(sap, transactionWrapper.Security);
                var taxTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Tax);
                Assert.NotNull(taxTransactionUnit);
                var taxTransactionUnitWrapper = new TransactionUnitWrapper(taxTransactionUnit);
                Assert.Equal(0.05, taxTransactionUnitWrapper.Amount);
                var feeTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Fee);
                Assert.NotNull(feeTransactionUnit);
                var feeTransactionUnitWrapper = new TransactionUnitWrapper(feeTransactionUnit);
                Assert.Equal(0.12, feeTransactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 4));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.TaxRefund, transaction.type);
                Assert.Equal(konto.Uuid, transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Empty(transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(8.76, transactionWrapper.Amount);
                Assert.Equal(0, transactionWrapper.Shares);
                Assert.Equal(konto, transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Null(transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(sap, transactionWrapper.Security);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 5));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.InboundDelivery, transaction.type);
                Assert.Empty(transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Equal(depot.Uuid, transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(2, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(362.47, transactionWrapper.Amount);
                Assert.Equal(2, transactionWrapper.Shares);
                Assert.Null(transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Equal(depot, transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(sap, transactionWrapper.Security);
                var taxTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Tax);
                Assert.NotNull(taxTransactionUnit);
                var taxTransactionUnitWrapper = new TransactionUnitWrapper(taxTransactionUnit);
                Assert.Equal(0.02, taxTransactionUnitWrapper.Amount);
                var feeTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Fee);
                Assert.NotNull(feeTransactionUnit);
                var feeTransactionUnitWrapper = new TransactionUnitWrapper(feeTransactionUnit);
                Assert.Equal(0.01, feeTransactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 6));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.OutboundDelivery, transaction.type);
                Assert.Empty(transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Equal(depot.Uuid, transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(sap.Uuid, transaction.Security);
                Assert.Equal(2, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(92.37, transactionWrapper.Amount);
                Assert.Equal(0.5, transactionWrapper.Shares);
                Assert.Null(transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Equal(depot, transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(sap, transactionWrapper.Security);
                var taxTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Tax);
                Assert.NotNull(taxTransactionUnit);
                var taxTransactionUnitWrapper = new TransactionUnitWrapper(taxTransactionUnit);
                Assert.Equal(0.22, taxTransactionUnitWrapper.Amount);
                var feeTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Fee);
                Assert.NotNull(feeTransactionUnit);
                var feeTransactionUnitWrapper = new TransactionUnitWrapper(feeTransactionUnit);
                Assert.Equal(0.11, feeTransactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 10));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.InboundDelivery, transaction.type);
                Assert.Empty(transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Equal(depot.Uuid, transaction.Portfolio);
                Assert.Empty(transaction.otherPortfolio);
                Assert.Equal(allianz.Uuid, transaction.Security);
                Assert.Equal(2, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(1932.73, transactionWrapper.Amount);
                Assert.Equal(7, transactionWrapper.Shares);
                Assert.Null(transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Equal(depot, transactionWrapper.Portfolio);
                Assert.Null(transactionWrapper.OtherPortfolio);
                Assert.Equal(allianz, transactionWrapper.Security);
                var taxTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Tax);
                Assert.NotNull(taxTransactionUnit);
                var taxTransactionUnitWrapper = new TransactionUnitWrapper(taxTransactionUnit);
                Assert.Equal(0.02, taxTransactionUnitWrapper.Amount);
                var feeTransactionUnit = transaction.Units.FirstOrDefault(x => x.type == PTransactionUnit.Type.Fee);
                Assert.NotNull(feeTransactionUnit);
                var feeTransactionUnitWrapper = new TransactionUnitWrapper(feeTransactionUnit);
                Assert.Equal(0.01, feeTransactionUnitWrapper.Amount);
            }

            {
                var transaction = client.Transactions.FirstOrDefault(x => x.Date == new DateTime(2024, 8, 11));
                Assert.NotNull(transaction);
                Assert.Equal(PTransaction.Type.SecurityTransfer, transaction.type);
                Assert.Empty(transaction.Account);
                Assert.Empty(transaction.otherAccount);
                Assert.Equal(depot.Uuid, transaction.Portfolio);
                Assert.Equal(depot2.Uuid, transaction.otherPortfolio);
                Assert.Equal(allianz.Uuid, transaction.Security);
                Assert.Equal(0, transaction.Units.Count);
                var transactionWrapper = new TransactionWrapper(transaction, client);
                Assert.Equal(1380.50, transactionWrapper.Amount);
                Assert.Equal(5, transactionWrapper.Shares);
                Assert.Null(transactionWrapper.Account);
                Assert.Null(transactionWrapper.OtherAccount);
                Assert.Equal(depot, transactionWrapper.Portfolio);
                Assert.Equal(depot2, transactionWrapper.OtherPortfolio);
                Assert.Equal(allianz, transactionWrapper.Security);
            }
        }

        private DirectoryInfo GetSourceBasePath()
        {
            var basePath = new DirectoryInfo(".");

            while (basePath.Name != "source")
            {
                basePath = basePath.Parent;

                if (basePath == null)
                {
                    throw new InvalidOperationException("Base path not found");
                }
            }

            return basePath;
        }
    }
}
