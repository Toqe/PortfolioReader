using System.Collections;
using System.IO.Compression;
using ProtoBuf;
using Toqe.PortfolioReader.Business.Protobuf;
using Xunit;

namespace Toqe.PortfolioReader.Test.Protobuf;

public class PortfolioProtobufWriterTest
{
    [Fact]
    public void TestWriteClient()
    {
        var basePath = new TestHelper().GetTestDataBasePath();
        var reader = new PortfolioProtobufReader();
        var readFilename = Path.Combine(basePath.FullName, "test.portfolio");
        var readClient = reader.ReadClient(readFilename);
        var writer = new PortfolioProtobufWriter();
        var writeFilename = Path.Combine(basePath.FullName, "test-written.portfolio");

        if (File.Exists(writeFilename))
        {
            File.Delete(writeFilename);
        }

        writer.WriteClient(writeFilename, readClient);

        var writtenAndReadClient = reader.ReadClient(writeFilename);

        this.Validate(typeof(PClient), readClient, writtenAndReadClient);
    }

    /// <summary>
    /// Reads a file written by Portfolio Performance's serializer, writes it again and checks that the
    /// (uncompressed) data inside the zip is byte-for-byte identical, i.e. no data is lost.
    /// The zip container itself is not compared (timestamps, compression may differ).
    /// The generated*.portfolio files come from TestData/Generator/GenerateTestPortfolios.java.
    /// </summary>
    [Theory]
    [InlineData("test.portfolio")]
    [InlineData("generated.portfolio")]
    [InlineData("generated-with-extensions.portfolio")]
    public void TestReadWriteRoundTripIsLossless(string filename)
    {
        var originalFilename = Path.Combine(new TestHelper().GetTestDataBasePath().FullName, filename);
        var writeFilename = Path.Combine(Path.GetTempPath(), $"roundtrip-{Guid.NewGuid():N}.portfolio");

        try
        {
            var client = new PortfolioProtobufReader().ReadClient(originalFilename);
            new PortfolioProtobufWriter().WriteClient(writeFilename, client);

            var expected = ReadSingleZipEntry(originalFilename);
            var actual = ReadSingleZipEntry(writeFilename);

            Assert.Equal(expected.Name, actual.Name);

            var firstDifference = Enumerable.Range(0, Math.Min(expected.Data.Length, actual.Data.Length))
                .FirstOrDefault(i => expected.Data[i] != actual.Data[i], -1);

            Assert.True(
                firstDifference == -1 && expected.Data.Length == actual.Data.Length,
                $"Written data differs: length {actual.Data.Length} vs. expected {expected.Data.Length}, " +
                $"first different byte at offset {firstDifference}.");
        }
        finally
        {
            File.Delete(writeFilename);
        }
    }

    /// <summary>
    /// PClient.extensions (field 99, google.protobuf.Any) is commented out in our pp-client.proto,
    /// so protobuf-net keeps it as unknown field. Checks that it survives deserialize → serialize → deserialize.
    /// </summary>
    [Fact]
    public void TestExtensionsSurviveRoundTrip()
    {
        const int extensionsField = 99;
        var originalFilename = Path.Combine(new TestHelper().GetTestDataBasePath().FullName, "generated-with-extensions.portfolio");
        var reader = new PortfolioProtobufReader();
        var writer = new PortfolioProtobufWriter();

        var client = reader.ReadClient(originalFilename);
        var extensions = Extensible.GetValues<AnyMessage>(client, extensionsField).ToList();

        Assert.Equal(
            ["type.googleapis.com/name.abuchen.portfolio.PKeyValue", "type.example.com/acme.ThirdPartyData", "type.example.com/acme.Empty"],
            extensions.Select(x => x.TypeUrl));
        Assert.Equal(new byte[] { 0x08, 0x96, 0x01, 0x12, 0x03, (byte)'a', (byte)'b', (byte)'c', 0xfa, 0x06, 0x00 }, extensions[1].Value);
        Assert.Empty(extensions[2].Value);

        using var stream = new MemoryStream();
        writer.WriteClient(stream, client);
        stream.Position = 0;
        var writtenAndReadClient = reader.ReadClient(stream);
        var writtenExtensions = Extensible.GetValues<AnyMessage>(writtenAndReadClient, extensionsField).ToList();

        Assert.Equal(extensions.Count, writtenExtensions.Count);

        for (var i = 0; i < extensions.Count; i++)
        {
            Assert.Equal(extensions[i].TypeUrl, writtenExtensions[i].TypeUrl);
            Assert.Equal(extensions[i].Value, writtenExtensions[i].Value);
        }

        // the extension is unknown to the C# model, so the rest must not have been affected
        Assert.Equal(3, writtenAndReadClient.Securities.Count);
        Assert.Equal(client.Transactions.Count, writtenAndReadClient.Transactions.Count);
    }

    /// <summary>Minimal google.protobuf.Any, only to inspect the unknown extensions field in tests.</summary>
    [ProtoContract]
    private class AnyMessage
    {
        [ProtoMember(1)]
        public string TypeUrl { get; set; } = "";

        [ProtoMember(2)]
        public byte[] Value { get; set; } = [];
    }

    private static (string Name, byte[] Data) ReadSingleZipEntry(string filename)
    {
        using var zipFile = ZipFile.OpenRead(filename);
        var entry = Assert.Single(zipFile.Entries);
        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return (entry.FullName, memoryStream.ToArray());
    }

    private void Validate(Type type, object expected, object actual)
    {
        Assert.True((expected == null) == (actual == null));

        if (expected == null)
        {
            return;
        }

        foreach (var property in type.GetProperties())
        {
            var expectedProp = property.GetValue(expected);
            var actualProp = property.GetValue(actual);

            if (property.PropertyType.Namespace == typeof(PClient).Namespace)
            {
                this.Validate(property.PropertyType, expectedProp, actualProp);
            }
            else if (property.PropertyType.IsGenericType &&
                     property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Assert.True((expectedProp == null) == (actualProp == null));

                if (expectedProp != null)
                {
                    Assert.Equal(((IList)expectedProp).Count, ((IList)actualProp).Count);

                    for (var i = 0; i < ((IList)expectedProp).Count; i++)
                    {
                        this.Validate(property.PropertyType.GetGenericArguments()[0], ((IList)expectedProp)[i], ((IList)actualProp)[i]);
                    }
                }
            }
            else if (property.PropertyType.IsGenericType &&
                     property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Assert.True((expectedProp == null) == (actualProp == null));

                if (expectedProp != null)
                {
                    Assert.Equal(((IDictionary)expectedProp).Count, ((IDictionary)actualProp).Count);
                    Assert.Equal(((IDictionary)expectedProp).Keys, ((IDictionary)actualProp).Keys);

                    foreach (var key in ((IDictionary)expectedProp).Keys)
                    {
                        this.Validate(property.PropertyType.GetGenericArguments()[1], ((IDictionary)expectedProp)[key], ((IDictionary)actualProp)[key]);
                    }
                }
            }
            else
            {
                Assert.Equal(expectedProp, actualProp);
            }
        }
    }
}