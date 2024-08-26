namespace Toqe.PortfolioReader.Business.Protobuf
{
    public class TransactionUnitWrapper
    {
        public TransactionUnitWrapper(
            PTransactionUnit transactionUnit)
        {
            this.TransactionUnit = transactionUnit;

            var converter = PortfolioProtobufDataConverter.Instance;
            this.Amount = converter.ConvertTransactionAmount(transactionUnit.Amount);
        }

        public double Amount { get; }

        public PTransactionUnit TransactionUnit { get; set; }
    }
}
