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

        public PSecurity Security { get; set; }
    }
}
