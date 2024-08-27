using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.Business.Models;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business
{
    public class ClientManager
    {
        public PortfoliosValuesModel GetCurrentPortfoliosValues(
            PClient client)
        {
            var portfolioMap = new Dictionary<string, PortfolioValuesModel>();

            foreach (var portfolio in client.Portfolios)
            {
                var portfolioValue = new PortfolioValuesModel
                {
                    Portfolio = portfolio,
                };

                portfolioMap.Add(portfolio.Uuid, portfolioValue);
            }

            foreach (var transaction in client.Transactions)
            {
                var wrapper = new TransactionWrapper(transaction, client);

                if (wrapper.Security != null)
                {
                    this.UpdatePortfolioValue(wrapper.Portfolio, wrapper, portfolioMap);
                    this.UpdatePortfolioValue(wrapper.OtherPortfolio, wrapper, portfolioMap);
                }
            }

            var result = new PortfoliosValuesModel
            {
                Portfolios = portfolioMap.Values
                    .OrderBy(x => x.Portfolio.Name)
                    .ToList(),
            };

            foreach (var portfolio in result.Portfolios)
            {
                foreach (var securityValue in portfolio.SecurityValues.ToList())
                {
                    if (securityValue.Shares == 0)
                    {
                        portfolio.SecurityValues.Remove(securityValue);
                        continue;
                    }

                    var latestPrice = securityValue.Security.Prices.OrderByDescending(x => x.Date).FirstOrDefault();

                    if (latestPrice != null)
                    {
                        securityValue.Price = PortfolioProtobufDataConverter.Instance.ConvertPriceClose(latestPrice.Close);
                        securityValue.MarketValue = securityValue.Shares * securityValue.Price;
                    }
                }

                portfolio.SecurityValues = portfolio.SecurityValues.OrderBy(x => x.Security.Name).ToList();
            }

            return result;
        }

        private void UpdatePortfolioValue(
            PPortfolio portfolio,
            TransactionWrapper wrapper,
            Dictionary<string, PortfolioValuesModel> portfolioMap)
        {
            if (portfolio == null)
            {
                return;
            }

            var securityValue = portfolioMap[portfolio.Uuid].SecurityValues
                .FirstOrDefault(x => x.Security.Uuid == wrapper.Security.Uuid);

            if (securityValue == null)
            {
                securityValue = new SecurityValueModel
                {
                    Security = wrapper.Security,
                };

                portfolioMap[portfolio.Uuid].SecurityValues.Add(securityValue);
            }

            securityValue.Shares += wrapper.GetSharesEffectOn(portfolio.Uuid);
        }
    }
}
