using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Models
{
    public class PortfolioValuesModel
    {
        public PPortfolio Portfolio { get; set; }

        public List<SecurityValueModel> SecurityValues { get; set; } = new List<SecurityValueModel>();

        public PortfolioValuesModel Clone()
        {
            return new PortfolioValuesModel
            {
                Portfolio = this.Portfolio,
                SecurityValues = SecurityValues.Select(x => x.Clone()).ToList(),
            };
        }
    }
}
