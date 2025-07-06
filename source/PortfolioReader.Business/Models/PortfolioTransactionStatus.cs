using System.Collections.Generic;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Models
{
    public class PortfolioTransactionStatus
    {
        public List<PortfolioTransactionAction> Actions { get; set; } = new List<PortfolioTransactionAction>();

        public Dictionary<string, (PSecurity security, List<PortfolioTransactionAction> actions)>
            ActionsPerSecurity { get; set; } =
            new Dictionary<string, (PSecurity security, List<PortfolioTransactionAction> actions)>();
    }
}
