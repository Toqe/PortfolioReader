using System;
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
            var bigInt = new BigInteger(value.Value, isBigEndian: true, isUnsigned: false);
            return (decimal)bigInt / (decimal)Math.Pow(10, value.Scale);
        }

        public PDecimalValue ConvertPDecimalValue(decimal value)
        {
            uint fixedPrecision = 10;
            uint fixedScale = 10;

            var scaledDecimal = value * (decimal)Math.Pow(10, fixedScale);
            var bigInt = new BigInteger(scaledDecimal);
            var valueBytes = bigInt.ToByteArray(isBigEndian: true, isUnsigned: false);

            return new PDecimalValue
            {
                Scale = fixedScale,
                Precision = fixedPrecision,
                Value = valueBytes
            };
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
