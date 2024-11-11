using System.IO;
using System.IO.Compression;
using ProtoBuf;

namespace Toqe.PortfolioReader.Business.Protobuf
{
    public class PortfolioProtobufWriter
    {
        public void WriteClient(string filename, PClient client)
        {
            using var zipFile = ZipFile.Open(filename, ZipArchiveMode.Create);
            var entry = zipFile.CreateEntry("data.portfolio", CompressionLevel.Optimal);
            using var stream = entry.Open();
            this.WriteClient(stream, client);
        }

        public void WriteClient(Stream stream, PClient client)
        {
            var expectedSignature = PortfolioProtobufDefaults.ExpectedSignature;
            stream.Write(expectedSignature, 0, expectedSignature.Length);
            Serializer.Serialize(stream, client);
        }
    }
}
