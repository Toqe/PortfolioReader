using System;
using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.Business.Models;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business
{
    public class ClientManager
    {
        public PortfoliosValuesModel GetCurrentPortfoliosValues(
            PClient client,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider)
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
                this.UpdatePriceAndMarketValueAndRemoveIfZero(client, portfolio.SecurityValues, DateTime.Today, exchangeRateProvider);
                portfolio.SecurityValues = portfolio.SecurityValues.OrderBy(x => x.Security.Name).ToList();
            }

            return result;
        }

        public List<SecurityValueModel> GetCurrentSecuritiesValues(
            PClient client,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider)
        {
            return this.GetCurrentSecuritiesValues(client, client.Transactions, DateTime.Today, exchangeRateProvider);
        }

        public List<SecurityValueModel> GetCurrentSecuritiesValues(
            PClient client,
            IEnumerable<PTransaction> transactions,
            DateTime priceDate,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider)
        {
            var securityMap = new Dictionary<string, SecurityValueModel>();

            foreach (var transaction in transactions)
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

            this.UpdatePriceAndMarketValueAndRemoveIfZero(client, result, priceDate, exchangeRateProvider);

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

        private void UpdatePriceAndMarketValueAndRemoveIfZero(
            PClient client,
            List<SecurityValueModel> securityValues,
            DateTime priceDate,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider)
        {
            foreach (var securityValue in securityValues.ToList())
            {
                if (PortfolioProtobufDataConverter.Instance.IsSharesZero(securityValue.Shares))
                {
                    securityValues.Remove(securityValue);
                    continue;
                }

                securityValue.Shares = Math.Round(securityValue.Shares, 5, MidpointRounding.AwayFromZero);

                var latestPrice = securityValue.Security.Prices
                    .OrderBy(x => Math.Abs((priceDate - PortfolioProtobufDataConverter.Instance.ConvertPriceDate(x.Date)).TotalDays))
                    .FirstOrDefault();

                if (latestPrice != null)
                {
                    securityValue.Price = PortfolioProtobufDataConverter.Instance.ConvertPriceClose(latestPrice.Close);

                    if (securityValue.Security.currencyCode != client.baseCurrency)
                    {
                        if (exchangeRateProvider == null)
                        {
                            throw new InvalidOperationException(
                                $"Could not calculate value of security '{securityValue.Security.Name}' " +
                                $"because its currency is '{securityValue.Security.currencyCode}' and no exchangeRateProvider is provided.");
                        }

                        var latestPriceDate = PortfolioProtobufDataConverter.Instance.ConvertPriceDate(latestPrice.Date);
                        var rate = exchangeRateProvider((securityValue.Security.currencyCode, latestPriceDate));
                        securityValue.Price = securityValue.Price.Value / (double)rate;
                    }

                    securityValue.MarketValue = securityValue.Shares * securityValue.Price;

                    if (securityValue.MarketValue.HasValue)
                    {
                        securityValue.MarketValue = Math.Round(securityValue.MarketValue.Value, 2, MidpointRounding.AwayFromZero);
                    }
                }
            }
        }
    }
}
