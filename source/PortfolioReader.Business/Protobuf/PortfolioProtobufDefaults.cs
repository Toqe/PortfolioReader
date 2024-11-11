using System.Linq;

namespace Toqe.PortfolioReader.Business.Protobuf
{
    public static class PortfolioProtobufDefaults
    {
        public static byte[] ExpectedSignature { get; } = new char[] { 'P', 'P', 'P', 'B', 'V', '1' }.Select(x => (byte)x).ToArray();
    }
}
