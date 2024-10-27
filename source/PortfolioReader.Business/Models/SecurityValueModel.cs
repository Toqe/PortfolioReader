using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business.Models
{
    public class SecurityValueModel
    {
        public PSecurity Security { get; set; }

        public double Shares { get; set; }

        public double? MarketValue { get; set; }

        public double? Price { get; set; }

        public SecurityValueModel Clone()
        {
            return new SecurityValueModel
            {
                MarketValue = this.MarketValue,
                Price = this.Price,
                Security = this.Security,
                Shares = this.Shares,
            };
        }
    }
}
