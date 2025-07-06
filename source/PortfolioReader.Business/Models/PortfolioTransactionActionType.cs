namespace Toqe.PortfolioReader.Business.Models
{
    public enum PortfolioTransactionActionType
    {
        Unknown = 0,
        Buy,
        Sell,
        Tax,
        Fee,
        Dividend,
        DividendTax,
        Inbound,
        Outbound,
    }
}
