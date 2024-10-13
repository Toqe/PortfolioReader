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
                this.UpdatePriceAndMarketValueAndRemoveIfZero(portfolio.SecurityValues);
                portfolio.SecurityValues = portfolio.SecurityValues.OrderBy(x => x.Security.Name).ToList();
            }

            return result;
        }

        public List<SecurityValueModel> GetCurrentSecuritiesValues(
            PClient client)
        {
            var securityMap = new Dictionary<string, SecurityValueModel>();

            foreach (var transaction in client.Transactions)
            {
                var wrapper = new TransactionWrapper(transaction, client);

                if (wrapper.Security != null)
                {
                    this.UpdateSecurityValue(wrapper, securityMap);
                }
            }

            var result = securityMap.Values
                .OrderBy(x => x.Security.Name)
                .ToList();

            this.UpdatePriceAndMarketValueAndRemoveIfZero(result);

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

        private void UpdateSecurityValue(
            TransactionWrapper wrapper,
            Dictionary<string, SecurityValueModel> securityMap)
        {
            if (!securityMap.TryGetValue(wrapper.Transaction.Security, out var securityValue))
            {
                securityValue = new SecurityValueModel
                {
                    Security = wrapper.Security,
                    Shares = 0,
                };

                securityMap.Add(wrapper.Transaction.Security, securityValue);
            }

            securityValue.Shares += wrapper.GetSharesEffect();
        }

        private void UpdatePriceAndMarketValueAndRemoveIfZero(List<SecurityValueModel> securityValues)
        {
            foreach (var securityValue in securityValues.ToList())
            {
                if (PortfolioProtobufDataConverter.Instance.IsSharesZero(securityValue.Shares))
                {
                    securityValues.Remove(securityValue);
                    continue;
                }

                var latestPrice = securityValue.Security.Prices.OrderByDescending(x => x.Date).FirstOrDefault();

                if (latestPrice != null)
                {
                    securityValue.Price = PortfolioProtobufDataConverter.Instance.ConvertPriceClose(latestPrice.Close);
                    securityValue.MarketValue = securityValue.Shares * securityValue.Price;
                }
            }
        }
    }
}
