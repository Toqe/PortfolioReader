using System;
using System.Linq;

namespace Toqe.PortfolioReader.Business.Protobuf
{
    public class TransactionWrapper
    {
        public TransactionWrapper(
            PTransaction transaction,
            PClient client)
        {
            this.Transaction = transaction;

            var converter = PortfolioProtobufDataConverter.Instance;
            this.Amount = converter.ConvertTransactionAmount(transaction.Amount);
            this.Shares = converter.ConvertTransactionShares(transaction.Shares);
            this.Account = Find(transaction.Account, id => client.Accounts.FirstOrDefault(x => x.Uuid == id));
            this.OtherAccount = Find(transaction.otherAccount, id => client.Accounts.FirstOrDefault(x => x.Uuid == id));
            this.Portfolio = Find(transaction.Portfolio, id => client.Portfolios.FirstOrDefault(x => x.Uuid == id));
            this.OtherPortfolio = Find(transaction.otherPortfolio, id => client.Portfolios.FirstOrDefault(x => x.Uuid == id));
            this.Security = Find(transaction.Security, id => client.Securities.FirstOrDefault(x => x.Uuid == id));
        }

        public double Amount { get; }

        public double Shares { get; }

        public PAccount Account { get; }

        public PAccount OtherAccount { get; }

        public PPortfolio Portfolio { get; }

        public PPortfolio OtherPortfolio { get; }

        public PSecurity Security { get; }

        public PTransaction Transaction { get; set; }

        private static T Find<T>(
            string id,
            Func<string, T> byIdAccessor)
        where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return byIdAccessor(id);
        }
    }
}
