using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ProtoBuf;

namespace Toqe.PortfolioReader.Business.Protobuf
{
    public class PortfolioProtobufReader
    {
        public PClient ReadClient(string filename)
        {
            using var zipFile = ZipFile.Open(filename, ZipArchiveMode.Read);
            using var stream = zipFile.Entries.First().Open();
            return this.ReadClient(stream);
        }

        public PClient ReadClient(Stream stream)
        {
            var expectedSignature = PortfolioProtobufDefaults.ExpectedSignature;
            var signature = new byte[expectedSignature.Length];
            var readBytes = stream.Read(signature, 0, signature.Length);

            if (readBytes != expectedSignature.Length || !expectedSignature.SequenceEqual(signature))
            {
                throw new InvalidOperationException("Signature is invalid");
            }

            var data = Serializer.Deserialize<PClient>(stream);
            return data;
        }
    }
}
