using System;

namespace Toqe.PortfolioReader.Business.Protobuf
{
    public class PortfolioProtobufDataConverter
    {
        public static PortfolioProtobufDataConverter Instance { get; } = new PortfolioProtobufDataConverter();

        public DateTime ConvertPriceDate(long value)
        {
            return new DateTime(1970, 1, 1).AddDays(value);
        }

        public double ConvertPriceClose(long value)
        {
            return this.ConvertDouble(value, 8);
        }

        public double ConvertTransactionShares(long value)
        {
            return this.ConvertDouble(value, 8);
        }

        public double ConvertTransactionAmount(long value)
        {
            return this.ConvertDouble(value, 2);
        }

        public bool IsSharesZero(double shares)
        {
            return Math.Abs(shares) < 0.00000001;
        }

        private double ConvertDouble(long value, int pow)
        {
            return value / Math.Pow(10, pow);
        }
    }
}
