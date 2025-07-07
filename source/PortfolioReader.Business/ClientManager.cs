using System;
using System.Collections.Generic;
using System.Linq;
using Toqe.PortfolioReader.Business.Helpers;
using Toqe.PortfolioReader.Business.Models;
using Toqe.PortfolioReader.Business.Protobuf;

namespace Toqe.PortfolioReader.Business
{
    public class ClientManager
    {
        private static readonly PHistoricalPriceDateComparer comparer = new PHistoricalPriceDateComparer();

        public List<PortfolioValuesModel> GetCurrentPortfoliosValues(
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

            var result = portfolioMap.Values
                .OrderBy(x => x.Portfolio.Name)
                .ToList();

            foreach (var portfolio in result)
            {
                this.UpdatePriceAndMarketValueAndRemoveIfZero(client, portfolio.SecurityValues, DateTime.Today, exchangeRateProvider, null);
                portfolio.SecurityValues = portfolio.SecurityValues.OrderBy(x => x.Security.Name).ToList();
            }

            return result;
        }

        public Dictionary<DateTime, List<PortfolioValuesModel>> GetPortfoliosValuesHistory(
            PClient client,
            IEnumerable<PTransaction> transactions,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider,
            DateTime? endDate = null)
        {
            var result = new Dictionary<DateTime, List<PortfolioValuesModel>>();

            if (!transactions.Any())
            {
                return result;
            }

            var dateOrderedTransactions = transactions.OrderBy(x => x.Date).ToList();

            var startDate = dateOrderedTransactions.First().Date.Value.Date;
            endDate = (endDate ?? dateOrderedTransactions.Last().Date).Value.Date;

            Dictionary<PSecurity, List<PHistoricalPrice>> securityPricesCache = new Dictionary<PSecurity, List<PHistoricalPrice>>();

            Func<PSecurity, List<PHistoricalPrice>> orderedSecurityPricesPerSecurityProvider = security =>
            {
                if (!securityPricesCache.ContainsKey(security))
                {
                    securityPricesCache.Add(security, security.Prices.OrderBy(x => x.Date).ToList());
                }

                return securityPricesCache[security];
            };

            var portfolioMap = new Dictionary<string, PortfolioValuesModel>();

            foreach (var portfolio in client.Portfolios)
            {
                var portfolioValue = new PortfolioValuesModel
                {
                    Portfolio = portfolio,
                };

                portfolioMap.Add(portfolio.Uuid, portfolioValue);
            }

            var currentTransactionIndex = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                while (currentTransactionIndex < dateOrderedTransactions.Count
                       && dateOrderedTransactions[currentTransactionIndex].Date.Value.Date == currentDate.Date)
                {
                    var transaction = dateOrderedTransactions[currentTransactionIndex];
                    var wrapper = new TransactionWrapper(transaction, client);

                    if (wrapper.Security != null)
                    {
                        this.UpdatePortfolioValue(wrapper.Portfolio, wrapper, portfolioMap);
                        this.UpdatePortfolioValue(wrapper.OtherPortfolio, wrapper, portfolioMap);
                    }

                    currentTransactionIndex++;
                }

                var portfoliosOnDate = portfolioMap.Values
                    .OrderBy(x => x.Portfolio.Name)
                    .Select(x => x.Clone())
                    .ToList();

                foreach (var portfolio in portfoliosOnDate)
                {
                    this.UpdatePriceAndMarketValueAndRemoveIfZero(client, portfolio.SecurityValues, currentDate, exchangeRateProvider, orderedSecurityPricesPerSecurityProvider);
                    portfolio.SecurityValues = portfolio.SecurityValues.OrderBy(x => x.Security.Name).ToList();
                }

                if (result.ContainsKey(currentDate))
                {
                    result.Remove(currentDate);
                }

                result.Add(currentDate, portfoliosOnDate);

                currentDate = currentDate.AddDays(1).Date;
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

            this.UpdatePriceAndMarketValueAndRemoveIfZero(client, result, priceDate, exchangeRateProvider, null);

            return result;
        }

        public Dictionary<DateTime, List<SecurityValueModel>> GetSecuritiesValuesHistory(
            PClient client,
            IEnumerable<PTransaction> transactions,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider,
            DateTime? endDate = null)
        {
            var result = new Dictionary<DateTime, List<SecurityValueModel>>();

            if (!transactions.Any())
            {
                return result;
            }

            var dateOrderedTransactions = transactions.OrderBy(x => x.Date).ToList();

            var startDate = dateOrderedTransactions.First().Date.Value.Date;
            endDate = (endDate ?? dateOrderedTransactions.Last().Date).Value.Date;

            var securityMap = new Dictionary<string, SecurityValueModel>();
            Dictionary<PSecurity, List<PHistoricalPrice>> securityPricesCache = new Dictionary<PSecurity, List<PHistoricalPrice>>();

            Func<PSecurity, List<PHistoricalPrice>> orderedSecurityPricesPerSecurityProvider = security =>
            {
                if (!securityPricesCache.ContainsKey(security))
                {
                    securityPricesCache.Add(security, security.Prices.OrderBy(x => x.Date).ToList());
                }

                return securityPricesCache[security];
            };

            var currentTransactionIndex = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                while (currentTransactionIndex < dateOrderedTransactions.Count
                    && dateOrderedTransactions[currentTransactionIndex].Date.Value.Date == currentDate.Date)
                {
                    var transaction = dateOrderedTransactions[currentTransactionIndex];
                    var wrapper = new TransactionWrapper(transaction, client);

                    if (wrapper.Security != null)
                    {
                        this.UpdateSecurityValue(wrapper, securityMap);
                    }

                    currentTransactionIndex++;
                }

                var securitiesOnDate = securityMap.Values
                        .OrderBy(x => x.Security.Name)
                        .Select(x => x.Clone())
                        .ToList();

                this.UpdatePriceAndMarketValueAndRemoveIfZero(client, securitiesOnDate, currentDate, exchangeRateProvider, orderedSecurityPricesPerSecurityProvider);

                if (result.ContainsKey(currentDate))
                {
                    result.Remove(currentDate);
                }

                result.Add(currentDate, securitiesOnDate);

                currentDate = currentDate.AddDays(1).Date;
            }

            return result;
        }

        public ClassificationRebalancingModel GetClassificationRebalancing(
            PClient client,
            PTaxonomy taxonomy,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider,
            PortfolioTransactionStatus portfolioTransactionStatus = null)
        {
            var root = taxonomy.Classifications.SingleOrDefault(x => string.IsNullOrWhiteSpace(x.parentId));

            var currentSecuritiesValues = this.GetCurrentSecuritiesValues(client, exchangeRateProvider);
            var classificationValuesCache = new Dictionary<string, (double marketValue, double dividends, double purchaseValue, double realisedGains, List<SecurityValueModel>)>();

            (double marketValue, double dividends, double purchaseValue, double realisedGains, List<SecurityValueModel> securities) GetClassificationMarketValueAndSecurities(
                PTaxonomy.Classification c)
            {
                if (classificationValuesCache.TryGetValue(c.Id, out var cacheEntry))
                {
                    return cacheEntry;
                }

                double marketValue = 0;
                double dividends = 0;
                double purchaseValue = 0;
                double realisedGains = 0;
                var securityValues = new List<SecurityValueModel>();

                foreach (var assignment in c.Assignments)
                {
                    var securityValue = currentSecuritiesValues.FirstOrDefault(x => x.Security.Uuid == assignment.investmentVehicle);

                    if (securityValue != null)
                    {
                        marketValue += securityValue?.MarketValue ?? 0;
                        securityValues.Add(securityValue);
                    }

                    if (portfolioTransactionStatus?.ActionsPerSecurity.TryGetValue(assignment.investmentVehicle, out var x) == true)
                    {
                        if (x.actions.Any())
                        {
                            if (securityValue == null)
                            {
                                securityValue = new SecurityValueModel { Security = x.security };
                                securityValues.Add(securityValue);
                            }

                            securityValue.Dividends += Math.Round(
                                x.actions
                                    .Where(x => x.Type is PortfolioTransactionActionType.Dividend)
                                    .Sum(x => x.Amount),
                                2,
                                MidpointRounding.AwayFromZero);
                        }

                        var portfolios = x.actions
                            .Select(x => x.Portfolio)
                            .Where(x => x != null)
                            .Distinct()
                            .ToList();

                        foreach (var portfolio in portfolios)
                        {
                            var actions = x.actions.Where(x => x.Portfolio == portfolio);

                            if (actions.Any())
                            {
                                var fifo = new List<(double shares, double amount)>();
                                double currentPurchaseValue = 0;
                                double currentRealisedGains = 0;

                                var fifoActions = actions
                                    .Where(x => x.Type
                                        is PortfolioTransactionActionType.Inbound
                                        or PortfolioTransactionActionType.Outbound
                                        or PortfolioTransactionActionType.Buy
                                        or PortfolioTransactionActionType.Sell)
                                    .OrderBy(x => x.Date)
                                    .ToList();

                                foreach (var action in fifoActions)
                                {
                                    switch (action.Type)
                                    {
                                        case PortfolioTransactionActionType.Buy:
                                        case PortfolioTransactionActionType.Inbound:
                                            fifo.Add((action.Shares, action.Amount));
                                            currentPurchaseValue += action.Amount;
                                            break;

                                        case PortfolioTransactionActionType.Sell:
                                        case PortfolioTransactionActionType.Outbound:
                                            var todoShares = action.Shares;

                                            while (todoShares > 0)
                                            {
                                                var fifoAction = fifo.First();

                                                if (fifoAction.shares > todoShares)
                                                {
                                                    var inAmount = fifoAction.amount * todoShares / fifoAction.shares;
                                                    var outAmount = action.Amount * todoShares / action.Shares;
                                                    fifoAction.amount -= inAmount;
                                                    fifoAction.shares -= todoShares;
                                                    fifoAction.shares = Math.Round(fifoAction.shares, 5, MidpointRounding.AwayFromZero);
                                                    fifo[0] = fifoAction;
                                                    currentPurchaseValue -= inAmount;
                                                    todoShares = 0;

                                                    if (action.Type == PortfolioTransactionActionType.Sell)
                                                    {
                                                        currentRealisedGains += outAmount - inAmount;
                                                    }
                                                }
                                                else
                                                {
                                                    var inAmount = fifoAction.amount;
                                                    var outAmount = action.Amount * fifoAction.shares / action.Shares;
                                                    fifo.RemoveAt(0);
                                                    currentPurchaseValue -= inAmount;
                                                    todoShares -= fifoAction.shares;
                                                    todoShares = Math.Round(todoShares, 5, MidpointRounding.AwayFromZero);

                                                    if (action.Type == PortfolioTransactionActionType.Sell)
                                                    {
                                                        currentRealisedGains += outAmount - inAmount;
                                                    }
                                                }
                                            }

                                            break;
                                    }
                                }

                                securityValue.PurchaseValue += Math.Round(
                                    currentPurchaseValue,
                                    2,
                                    MidpointRounding.AwayFromZero);

                                securityValue.RealisedGains += Math.Round(
                                    currentRealisedGains,
                                    2,
                                    MidpointRounding.AwayFromZero);
                            }
                        }

                        dividends += securityValue.Dividends;
                        purchaseValue += securityValue.PurchaseValue;
                        realisedGains += securityValue.RealisedGains;
                    }
                }

                var children = taxonomy.Classifications.Where(x => x.parentId == c.Id);

                foreach (var child in children)
                {
                    var childEntry = GetClassificationMarketValueAndSecurities(child);
                    marketValue += childEntry.marketValue;
                    dividends += childEntry.dividends;
                    purchaseValue += childEntry.purchaseValue;
                    realisedGains += childEntry.realisedGains;
                }

                marketValue = Math.Round(marketValue, 2, MidpointRounding.AwayFromZero);
                dividends = Math.Round(dividends, 2, MidpointRounding.AwayFromZero);
                purchaseValue = Math.Round(purchaseValue, 2, MidpointRounding.AwayFromZero);
                realisedGains = Math.Round(realisedGains, 2, MidpointRounding.AwayFromZero);

                var result = (marketValue, dividends, purchaseValue, realisedGains, securityValues);
                classificationValuesCache.Add(c.Id, result);
                return result;
            }

            ClassificationRebalancingModel Build(
                PTaxonomy.Classification classification,
                ClassificationRebalancingModel parent,
                double totalValue)
            {
                var marketValueAndSecurities = GetClassificationMarketValueAndSecurities(classification);
                var result = new ClassificationRebalancingModel();
                result.Classification = classification;
                result.MarketValue = marketValueAndSecurities.marketValue;
                result.Dividends = marketValueAndSecurities.dividends;
                result.PurchaseValue = marketValueAndSecurities.purchaseValue;
                result.RealisedGains = marketValueAndSecurities.realisedGains;
                result.SecurityValues = marketValueAndSecurities.securities;
                result.SupposedWeightPercentage = classification.Weight / 100d;
                result.SupposedTotalWeightPercentage = parent == null ? 100 : parent.SupposedTotalWeightPercentage * result.SupposedWeightPercentage / 100;
                result.SupposedValue = Math.Round(result.SupposedTotalWeightPercentage * totalValue / 100, 2, MidpointRounding.AwayFromZero);
                result.CurrentTotalWeightPercentage = result.MarketValue / totalValue * 100;
                result.CurrentWeightPercentage = parent == null ? 100 : Math.Round(result.MarketValue / parent.MarketValue * 100, 2, MidpointRounding.AwayFromZero);

                var children = taxonomy.Classifications
                    .Where(x => x.parentId == classification.Id)
                    .OrderBy(x => x.Name);

                foreach (var child in children)
                {
                    var childResult = Build(child, result, totalValue);
                    childResult.Parent = result;
                    result.Children.Add(childResult);
                }

                return result;
            }

            return Build(root, null, GetClassificationMarketValueAndSecurities(root).Item1);
        }

        public PortfolioTransactionStatus GetPortfolioTransactionStatus(
            PClient client,
            IEnumerable<PTransaction> transactionsEnumerable,
            Func<(string currency, DateTime date), decimal> exchangeRateProvider,
            PortfolioTransactionStatus initialStatus = null)
        {
            var result = initialStatus ?? new PortfolioTransactionStatus();

            var transactions = transactionsEnumerable
                .Select(x => new TransactionWrapper(x, client))
                .OrderBy(x => x.Transaction.Date)
                .ToList();

            void AddAction(PortfolioTransactionAction action)
            {
                result.Actions.Add(action);

                if (action.Security != null)
                {
                    if (!result.ActionsPerSecurity.ContainsKey(action.Security.Uuid))
                    {
                        result.ActionsPerSecurity[action.Security.Uuid] = (action.Security, new List<PortfolioTransactionAction>());
                    }

                    result.ActionsPerSecurity[action.Security.Uuid].actions.Add(action);
                }
            }

            foreach (var transaction in transactions)
            {
                var action = new PortfolioTransactionAction
                {
                    Transaction = transaction,
                    Date = transaction.Transaction.Date.Value,
                    Amount = transaction.Amount,
                    Security = transaction.Security,
                    Shares = transaction.Shares,
                    Portfolio = transaction.Portfolio,
                };

                switch (transaction.Transaction.type)
                {
                    case PTransaction.Type.Purchase:
                        action.Type = PortfolioTransactionActionType.Buy;
                        break;

                    case PTransaction.Type.Sale:
                        action.Type = PortfolioTransactionActionType.Sell;
                        break;

                    case PTransaction.Type.OutboundDelivery:
                        action.Type = PortfolioTransactionActionType.Outbound;
                        break;

                    case PTransaction.Type.InboundDelivery:
                        action.Type = PortfolioTransactionActionType.Inbound;
                        break;

                    case PTransaction.Type.Tax:
                        action.Type = PortfolioTransactionActionType.Tax;
                        break;

                    case PTransaction.Type.Fee:
                        action.Type = PortfolioTransactionActionType.Fee;
                        break;

                    case PTransaction.Type.TaxRefund:
                        action.Type = PortfolioTransactionActionType.Tax;
                        action.Amount *= -1;
                        break;

                    case PTransaction.Type.FeeRefund:
                        action.Type = PortfolioTransactionActionType.Fee;
                        action.Amount *= -1;
                        break;

                    case PTransaction.Type.Dividend:
                        action.Type = PortfolioTransactionActionType.Dividend;
                        break;

                    case PTransaction.Type.SecurityTransfer:
                        action.Type = PortfolioTransactionActionType.Outbound;
                        AddAction(action);
                        action = action.Clone();
                        action.Type = PortfolioTransactionActionType.Inbound;
                        action.Portfolio = transaction.OtherPortfolio;
                        break;

                    case PTransaction.Type.Deposit:
                    case PTransaction.Type.Removal:
                        continue;

                    default:
                        throw new NotImplementedException($"unknown transaction type {transaction.Transaction.type}");
                }

                AddAction(action);

                var units = transaction.Transaction.Units
                    .Select(x => new TransactionUnitWrapper(x));

                foreach (var unit in units)
                {
                    var unitAction = new PortfolioTransactionAction
                    {
                        Transaction = transaction,
                        TransactionUnit = unit,
                        Date = transaction.Transaction.Date.Value,
                        Amount = unit.Amount,
                        Security = transaction.Security,
                    };

                    switch (unit.TransactionUnit.type)
                    {
                        case PTransactionUnit.Type.Fee:
                            unitAction.Type = PortfolioTransactionActionType.Fee;

                            if (transaction.Transaction.type == PTransaction.Type.Sale)
                            {
                                unitAction.Type = PortfolioTransactionActionType.SellFeeOrTax;
                                action.Amount += unit.Amount;
                            }
                            else if (transaction.Transaction.type == PTransaction.Type.Dividend)
                            {
                                unitAction.Type = PortfolioTransactionActionType.DividendFeeOrTax;
                                action.Amount += unit.Amount;
                            }
                            else if (transaction.Transaction.type == PTransaction.Type.Purchase)
                            {
                                action.Amount -= unit.Amount;
                            }

                            break;

                        case PTransactionUnit.Type.Tax:
                            unitAction.Type = PortfolioTransactionActionType.Tax;

                            if (transaction.Transaction.type == PTransaction.Type.Sale)
                            {
                                unitAction.Type = PortfolioTransactionActionType.SellFeeOrTax;
                                action.Amount += unit.Amount;
                            }
                            else if (transaction.Transaction.type == PTransaction.Type.Dividend)
                            {
                                unitAction.Type = PortfolioTransactionActionType.DividendFeeOrTax;
                                action.Amount += unit.Amount;
                            }
                            else if (transaction.Transaction.type == PTransaction.Type.Purchase)
                            {
                                action.Amount -= unit.Amount;
                            }

                            break;

                        case PTransactionUnit.Type.GrossValue:
                            // do nothing
                            break;

                        default:
                            throw new NotImplementedException($"unknown transaction unit type {unit.TransactionUnit.type}");
                    }

                    AddAction(unitAction);
                }
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
            Func<(string currency, DateTime date), decimal> exchangeRateProvider,
            Func<PSecurity, List<PHistoricalPrice>> orderedSecurityPricesPerSecurityProvider)
        {
            foreach (var securityValue in securityValues.ToList())
            {
                if (PortfolioProtobufDataConverter.Instance.IsSharesZero(securityValue.Shares))
                {
                    securityValues.Remove(securityValue);
                    continue;
                }

                securityValue.Shares = Math.Round(securityValue.Shares, 5, MidpointRounding.AwayFromZero);

                PHistoricalPrice latestPrice = null;
                var orderedPrices = orderedSecurityPricesPerSecurityProvider?.Invoke(securityValue.Security);

                if (orderedPrices == null)
                {
                    orderedPrices = securityValue.Security.Prices.OrderBy(x => x.Date).ToList();
                }

                if (orderedPrices != null && orderedPrices.Any())
                {
                    var index = orderedPrices.BinarySearch(
                        new PHistoricalPrice { Date = PortfolioProtobufDataConverter.Instance.ConvertPriceDate(priceDate) },
                        comparer);

                    if (index >= 0)
                    {
                        latestPrice = orderedPrices[index];
                    }
                    else if (~index == orderedPrices.Count)
                    {
                        latestPrice = orderedPrices.LastOrDefault();
                    }
                    else
                    {
                        latestPrice = orderedPrices[Math.Max(0, (~index) - 1)];
                    }
                }

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
