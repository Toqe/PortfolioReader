using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Toqe.PortfolioReader.Business.Protobuf
{
    public class PortfolioProtobufDataConverter
    {
        public static PortfolioProtobufDataConverter Instance { get; } = new PortfolioProtobufDataConverter();

        public DateTime ConvertPriceDate(long value)
        {
            return new DateTime(1970, 1, 1).AddDays(value);
        }

        public long ConvertPriceDate(DateTime value)
        {
            return (long)Math.Round((value - new DateTime(1970, 1, 1)).TotalDays, MidpointRounding.AwayFromZero);
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

        public decimal ConvertDecimalValue(PDecimalValue value)
        {
            var bigInt = new BigInteger(Enumerable.Repeat((byte)0, 1).Concat(value.Value.AsEnumerable()).Reverse().ToArray());
            var bigIntDecimal = decimal.Parse(bigInt.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            return bigIntDecimal * (decimal)Math.Pow(10, -value.Scale);
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
