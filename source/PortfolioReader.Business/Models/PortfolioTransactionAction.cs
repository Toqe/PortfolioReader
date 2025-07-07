using System;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Models
{
    public class PortfolioTransactionAction
    {
        public PortfolioTransactionActionType Type { get; set; }

        public TransactionWrapper Transaction { get; set; }

        public TransactionUnitWrapper TransactionUnit { get; set; }

        public double Amount { get; set; }

        public DateTime Date { get; set; }

        public double Shares { get; set; }

        public PPortfolio Portfolio { get; set; }

        public PSecurity Security { get; set; }

        public PortfolioTransactionAction Clone()
        {
            return new PortfolioTransactionAction
            {
                Type = this.Type,
                Transaction = this.Transaction,
                TransactionUnit = this.TransactionUnit,
                Amount = this.Amount,
                Date = this.Date,
                Shares = this.Shares,
                Portfolio = this.Portfolio,
                Security = this.Security,
            };
        }
    }
}
