using System.Collections.Generic;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Models
{
    public class PortfolioValuesModel
    {
        public PPortfolio Portfolio { get; set; }

        public List<SecurityValueModel> SecurityValues { get; set; } = new List<SecurityValueModel>();
    }
}
