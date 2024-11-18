using Toqe.PortfolioReader.Business.Protobuf;
using Xunit;

namespace Toqe.PortfolioReader.Test.Protobuf
{
    public class PortfolioProtobufDataConverterTest
    {
        [Fact]
        public void TestDecimalValueConversion()
        {
            var converter = new PortfolioProtobufDataConverter();

            var value = new PDecimalValue { Precision = 0, Scale = 0, Value = new byte[] { 9, 24, 78, 114, 160, 0 } };
            var convertedValue = converter.ConvertDecimalValue(value);
            Assert.Equal(10000000000000m, convertedValue);

            value = new PDecimalValue { Precision = 10, Scale = 10, Value = new byte[] { 2, 12, 199, 250, 118 } };
            convertedValue = converter.ConvertDecimalValue(value);
            Assert.Equal(0.8804366966m, convertedValue);
            Assert.Equal(1.1358m, Math.Round(1 / convertedValue, 4));

            value = new PDecimalValue { Precision = 10, Scale = 10, Value = new byte[] { 1, 148, 224, 162, 49 } };
            convertedValue = converter.ConvertDecimalValue(value);
            Assert.Equal(0.6792716849m, convertedValue);
            Assert.Equal(1.4722m, Math.Round(1 / convertedValue, 4, MidpointRounding.AwayFromZero));
        }

        [Fact]
        public void TestPDecimalToDecimalValueConversion()
        {
            var converter = new PortfolioProtobufDataConverter();
            uint fixedPrecision = 10;
            uint fixedScale = 10;

            var value = 0.8804366966m;
            var convertedValue = converter.ConvertPDecimalValue(value);
            Assert.Equal(fixedPrecision, convertedValue.Precision);
            Assert.Equal(fixedScale, convertedValue.Scale);
            Assert.Equal(new byte[] { 2, 12, 199, 250, 118 }, convertedValue.Value);

            value = 0.6792716849m;
            convertedValue = converter.ConvertPDecimalValue(value);
            Assert.Equal(fixedPrecision, convertedValue.Precision);
            Assert.Equal(fixedScale, convertedValue.Scale);
            Assert.Equal(new byte[] { 1, 148, 224, 162, 49 }, convertedValue.Value);
        }
    }
}
