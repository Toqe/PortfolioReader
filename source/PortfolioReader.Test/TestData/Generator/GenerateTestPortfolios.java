import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.math.BigDecimal;
import java.nio.file.Path;
import java.time.LocalDate;
import java.util.zip.Deflater;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

import com.google.protobuf.Any;
import com.google.protobuf.ByteString;
import com.google.protobuf.NullValue;
import com.google.protobuf.Timestamp;

import name.abuchen.portfolio.model.proto.v1.*;

/**
 * Generates the synthetic test files "generated.portfolio" and "generated-with-extensions.portfolio"
 * (fake data only) with the protobuf classes of Portfolio Performance itself, i.e. with the same
 * serializer Portfolio Performance uses when saving a file. Output is deterministic.
 *
 * The data tries to set every field of client.proto at least once, including edge cases
 * (negative numbers, empty optional strings, default values inside oneofs, unicode, empty messages).
 * The second file additionally contains PClient.extensions (field 99), which our C# classes don't
 * know (commented out in pp-client.proto) and therefore must keep as unknown fields.
 *
 * Run with Java 22+ from this directory (adjust the Portfolio Performance installation path/versions):
 *   set PP=D:\Prog\PortfolioPerformance\portfolio\plugins
 *   java -cp "%PP%\name.abuchen.portfolio_0.89.0.jar;%PP%\com.google.protobuf_4.36.2.jar" GenerateTestPortfolios.java ..
 */
public class GenerateTestPortfolios
{
    private static final String SECURITY_1 = "11111111-1111-1111-1111-111111111111";
    private static final String SECURITY_2 = "22222222-2222-2222-2222-222222222222";
    private static final String ACCOUNT_1 = "33333333-3333-3333-3333-333333333333";
    private static final String ACCOUNT_2 = "44444444-4444-4444-4444-444444444444";
    private static final String PORTFOLIO_1 = "55555555-5555-5555-5555-555555555555";
    private static final String PORTFOLIO_2 = "66666666-6666-6666-6666-666666666666";

    private static int transactionCounter = 0;

    public static void main(String[] args) throws IOException
    {
        Path outDir = Path.of(args.length > 0 ? args[0] : ".");

        PClient client = buildClient().build();
        write(outDir.resolve("generated.portfolio"), client);

        PClient clientWithExtensions = buildClient()
                        .addExtensions(Any.pack(PKeyValue.newBuilder().setKey("plugin-setting")
                                        .setValue(PAnyValue.newBuilder().setString("Wert äöü")).build()))
                        .addExtensions(Any.newBuilder().setTypeUrl("type.example.com/acme.ThirdPartyData")
                                        .setValue(ByteString.copyFrom(new byte[] { 0x08, (byte) 0x96, 0x01, 0x12,
                                                        0x03, 'a', 'b', 'c', (byte) 0xfa, 0x06, 0x00 })))
                        .addExtensions(Any.newBuilder().setTypeUrl("type.example.com/acme.Empty"))
                        .build();
        write(outDir.resolve("generated-with-extensions.portfolio"), clientWithExtensions);
    }

    private static void write(Path file, PClient client) throws IOException
    {
        // same container as Portfolio Performance's ClientFactory (unencrypted binary, compressed)
        try (OutputStream out = new FileOutputStream(file.toFile()); ZipOutputStream zip = new ZipOutputStream(out))
        {
            zip.setLevel(Deflater.BEST_COMPRESSION);
            zip.putNextEntry(new ZipEntry("data.portfolio"));
            zip.write(new byte[] { 'P', 'P', 'P', 'B', 'V', '1' });
            client.writeTo(zip);
            zip.closeEntry();
        }

        System.out.println("Written " + file.toAbsolutePath());
    }

    private static PClient.Builder buildClient()
    {
        transactionCounter = 0;

        PClient.Builder client = PClient.newBuilder().setVersion(70).setBaseCurrency("EUR");

        client.addSecurities(security1());
        client.addSecurities(security2());
        client.addSecurities(PSecurity.newBuilder().setUuid("77777777-7777-7777-7777-777777777777")
                        .setName("Minimal Security (no optional fields)"));

        client.addAccounts(PAccount.newBuilder().setUuid(ACCOUNT_1).setName("Girokonto").setCurrencyCode("EUR")
                        .setNote("Notiz mit Umlauten äöüß und Emoji 📈").addAttributes(stringAttribute("logo", "x"))
                        .setUpdatedAt(timestamp(1_700_000_000L, 123_000_000)));
        client.addAccounts(PAccount.newBuilder().setUuid(ACCOUNT_2).setName("USD Account").setCurrencyCode("USD")
                        .setNote("").setIsRetired(true).setUpdatedAt(timestamp(0, 0)));

        client.addPortfolios(PPortfolio.newBuilder().setUuid(PORTFOLIO_1).setName("Depot").setNote("Depot-Notiz")
                        .setReferenceAccount(ACCOUNT_1).addAttributes(stringAttribute("broker", "Fake Bank"))
                        .setUpdatedAt(timestamp(1_700_000_001L, 100)));
        client.addPortfolios(PPortfolio.newBuilder().setUuid(PORTFOLIO_2).setName("Altes Depot").setIsRetired(true)
                        .setReferenceAccount(ACCOUNT_2));

        addTransactions(client);
        addPlans(client);

        client.addWatchlists(PWatchlist.newBuilder().setName("Beobachten").addSecurities(SECURITY_1)
                        .addSecurities(SECURITY_2));
        client.addWatchlists(PWatchlist.newBuilder().setName("Leer"));

        client.addTaxonomies(taxonomy());
        client.addTaxonomies(PTaxonomy.newBuilder().setId("tax-empty").setName("Empty taxonomy"));

        client.addDashboards(PDashboard.newBuilder().setId("dash-1").setName("Dashboard")
                        .putConfiguration("REPORTING_PERIOD", "L1Y0").putConfiguration("CONFIG_B", "")
                        .addColumns(PDashboard.Column.newBuilder().setWeight(2)
                                        .addWidgets(PDashboard.Widget.newBuilder().setType("HEADING")
                                                        .setLabel("Überschrift"))
                                        .addWidgets(PDashboard.Widget.newBuilder().setType("TOTAL_SUM").setLabel("Sum")
                                                        .putConfiguration("DATA_SERIES", "Client-totals")
                                                        .putConfiguration("REPORTING_PERIOD", "L5Y0")))
                        .addColumns(PDashboard.Column.newBuilder().setWeight(-1)));

        client.putProperties("security-chart-details", "SCALING_LINEAR,INVESTMENT");
        client.putProperties("empty-value", "");
        client.putProperties("ümlaut-key", "ümlaut-value");

        client.setSettings(PSettings.newBuilder()
                        .addBookmarks(PBookmark.newBuilder().setLabel("Onvista").setPattern(
                                        "https://www.onvista.de/aktien/{isin}"))
                        .addBookmarks(PBookmark.newBuilder().setLabel("").setPattern(""))
                        .addAttributeTypes(PAttributeType.newBuilder().setId("ter").setName("Gesamtkostenquote (TER)")
                                        .setColumnLabel("TER").setSource("source").setTarget(
                                                        "name.abuchen.portfolio.model.Security")
                                        .setType("java.lang.Double")
                                        .setConverterClass(
                                                        "name.abuchen.portfolio.model.AttributeType$PercentConverter")
                                        .setProperties(PMap.newBuilder().addEntries(PKeyValue.newBuilder()
                                                        .setKey("p").setValue(PAnyValue.newBuilder().setBool(true)))))
                        .addAttributeTypes(PAttributeType.newBuilder().setId("logo").setName("Logo")
                                        .setColumnLabel("Logo").setTarget("name.abuchen.portfolio.model.Account")
                                        .setType("name.abuchen.portfolio.model.ImageManager")
                                        .setConverterClass(
                                                        "name.abuchen.portfolio.model.AttributeType$ImageConverter")
                                        .setProperties(PMap.newBuilder()))
                        .addConfigurationSets(PConfigurationSet.newBuilder().setKey("StatementOfAssetsViewer")
                                        .setUuid("cfg-1").setName("Standard").setData("col1=1;col2=0"))
                        .addConfigurationSets(PConfigurationSet.newBuilder().setKey("empty")));

        return client;
    }

    private static PSecurity.Builder security1()
    {
        PSecurity.Builder security = PSecurity.newBuilder().setUuid(SECURITY_1).setOnlineId("online-1")
                        .setName("Fake Aktiengesellschaft").setCurrencyCode("EUR").setTargetCurrencyCode("USD")
                        .setNote("Mehrzeilige\nNotiz").setIsin("DE0000000001").setTickerSymbol("FAKE.DE")
                        .setWkn("000001").setCalendar("de").setFeed("YAHOO").setFeedURL("https://example.com/feed")
                        .setLatestFeed("MANUAL").setLatestFeedURL("https://example.com/latest");

        LocalDate start = LocalDate.of(2024, 1, 1);
        for (int i = 0; i < 400; i++)
        {
            // values in "price * 10^8", like Portfolio Performance stores them
            security.addPrices(PHistoricalPrice.newBuilder().setDate(start.plusDays(i).toEpochDay())
                            .setClose(10_000_000_000L + i * 12_345_678L));
        }

        security.setLatest(PFullHistoricalPrice.newBuilder().setDate(start.plusDays(400).toEpochDay())
                        .setClose(15_000_000_000L).setHigh(15_500_000_000L).setLow(14_900_000_000L)
                        .setVolume(1_234_567L));

        security.addAttributes(stringAttribute("ter", "0.2"));
        security.addAttributes(PKeyValue.newBuilder().setKey("long").setValue(PAnyValue.newBuilder().setInt64(-5)));
        security.addAttributes(PKeyValue.newBuilder().setKey("double").setValue(PAnyValue.newBuilder().setDouble(3.14)));
        security.addAttributes(PKeyValue.newBuilder().setKey("zero-int").setValue(PAnyValue.newBuilder().setInt32(0)));
        security.addAttributes(PKeyValue.newBuilder().setKey("false").setValue(PAnyValue.newBuilder().setBool(false)));
        security.addAttributes(PKeyValue.newBuilder().setKey("empty").setValue(PAnyValue.newBuilder().setString("")));
        security.addAttributes(
                        PKeyValue.newBuilder().setKey("null").setValue(PAnyValue.newBuilder().setNull(NullValue.NULL_VALUE)));
        security.addAttributes(PKeyValue.newBuilder().setKey("no-value"));

        security.addEvents(PSecurityEvent.newBuilder().setType(PSecurityEvent.Type.STOCK_SPLIT)
                        .setDate(LocalDate.of(2024, 6, 1).toEpochDay()).setDetails("2:1"));
        security.addEvents(PSecurityEvent.newBuilder().setType(PSecurityEvent.Type.NOTE)
                        .setDate(LocalDate.of(2024, 7, 1).toEpochDay()).setDetails("Hauptversammlung")
                        .setSource("manual"));
        security.addEvents(PSecurityEvent.newBuilder().setType(PSecurityEvent.Type.DIVIDEND_PAYMENT)
                        .setDate(LocalDate.of(2024, 5, 2).toEpochDay()).setDetails("")
                        .addData(PAnyValue.newBuilder().setInt64(LocalDate.of(2024, 5, 6).toEpochDay()))
                        .addData(PAnyValue.newBuilder().setString("EUR"))
                        .addData(PAnyValue.newBuilder().setInt64(1140)).setSource("divvydiary.com"));

        security.addProperties(PKeyValue.newBuilder().setKey("FEED").setValue(PAnyValue.newBuilder()
                        .setMap(PMap.newBuilder()
                                        .addEntries(PKeyValue.newBuilder().setKey("GENERIC-JSON-PATH-DATE")
                                                        .setValue(PAnyValue.newBuilder().setString("$.date")))
                                        .addEntries(PKeyValue.newBuilder().setKey("nested")
                                                        .setValue(PAnyValue.newBuilder().setMap(PMap.newBuilder()))))));

        security.setUpdatedAt(timestamp(1_730_000_000L, 999_999_900));
        return security;
    }

    private static PSecurity.Builder security2()
    {
        return PSecurity.newBuilder().setUuid(SECURITY_2).setName("Fake US Corp.").setCurrencyCode("USD")
                        .setIsin("US0000000002").setNote("").setFeed("").setIsRetired(true)
                        .addPrices(PHistoricalPrice.newBuilder().setDate(-1).setClose(-1))
                        .addPrices(PHistoricalPrice.newBuilder().setDate(0).setClose(0))
                        .addPrices(PHistoricalPrice.newBuilder().setDate(Long.MAX_VALUE).setClose(Long.MIN_VALUE))
                        .setLatest(PFullHistoricalPrice.newBuilder()) // empty, but present
                        .setUpdatedAt(timestamp(-62_135_596_800L, 0)); // 0001-01-01
    }

    private static void addTransactions(PClient.Builder client)
    {
        // purchase with fee, tax and forex gross value
        client.addTransactions(transaction(PTransaction.Type.PURCHASE).setPortfolio(PORTFOLIO_1).setAccount(ACCOUNT_1)
                        .setOtherUuid("other-1").setOtherUpdatedAt(timestamp(1_700_000_100L, 500))
                        .setDate(timestamp(1_704_103_200L, 0)).setCurrencyCode("EUR").setAmount(100_550)
                        .setShares(10_00000000L).setSecurity(SECURITY_1).setNote("Kauf").setSource("import.pdf")
                        .addUnits(PTransactionUnit.newBuilder().setType(PTransactionUnit.Type.FEE).setAmount(495)
                                        .setCurrencyCode("EUR"))
                        .addUnits(PTransactionUnit.newBuilder().setType(PTransactionUnit.Type.TAX).setAmount(55)
                                        .setCurrencyCode("EUR"))
                        .addUnits(PTransactionUnit.newBuilder().setType(PTransactionUnit.Type.GROSS_VALUE)
                                        .setAmount(100_000).setCurrencyCode("EUR").setFxAmount(108_000)
                                        .setFxCurrencyCode("USD").setFxRateToBase(decimal("0.9259259259"))));

        client.addTransactions(transaction(PTransaction.Type.SALE).setPortfolio(PORTFOLIO_1).setAccount(ACCOUNT_1)
                        .setOtherUuid("other-2").setOtherUpdatedAt(timestamp(1_700_000_200L, 0))
                        .setDate(timestamp(1_710_000_000L, 0)).setCurrencyCode("EUR").setAmount(50_000)
                        .setShares(2_50000000L).setSecurity(SECURITY_1));

        client.addTransactions(transaction(PTransaction.Type.INBOUND_DELIVERY).setPortfolio(PORTFOLIO_2)
                        .setDate(timestamp(1_600_000_000L, 0)).setCurrencyCode("USD").setAmount(0).setShares(1)
                        .setSecurity(SECURITY_2));
        client.addTransactions(transaction(PTransaction.Type.OUTBOUND_DELIVERY).setPortfolio(PORTFOLIO_2)
                        .setDate(timestamp(1_600_000_001L, 0)).setCurrencyCode("USD").setAmount(1).setShares(0)
                        .setSecurity(SECURITY_2).setNote(""));

        client.addTransactions(transaction(PTransaction.Type.SECURITY_TRANSFER).setPortfolio(PORTFOLIO_1)
                        .setOtherPortfolio(PORTFOLIO_2).setOtherUuid("other-3")
                        .setOtherUpdatedAt(timestamp(1_700_000_300L, 0)).setDate(timestamp(1_720_000_000L, 0))
                        .setCurrencyCode("EUR").setAmount(20_000).setShares(1_00000000L).setSecurity(SECURITY_1));

        client.addTransactions(transaction(PTransaction.Type.CASH_TRANSFER).setAccount(ACCOUNT_1)
                        .setOtherAccount(ACCOUNT_2).setOtherUuid("other-4")
                        .setOtherUpdatedAt(timestamp(1_700_000_400L, 0)).setDate(timestamp(1_720_000_100L, 0))
                        .setCurrencyCode("EUR").setAmount(30_000)
                        .addUnits(PTransactionUnit.newBuilder().setType(PTransactionUnit.Type.GROSS_VALUE)
                                        .setAmount(30_000).setCurrencyCode("EUR").setFxAmount(0)
                                        .setFxCurrencyCode("").setFxRateToBase(PDecimalValue.newBuilder())));

        client.addTransactions(transaction(PTransaction.Type.DIVIDEND).setAccount(ACCOUNT_1)
                        .setDate(timestamp(1_714_953_600L, 0)).setCurrencyCode("EUR").setAmount(1_140)
                        .setShares(10_00000000L).setSecurity(SECURITY_1)
                        .setExDate(PLocalDateTime.newBuilder().setEpochDay(LocalDate.of(2024, 5, 2).toEpochDay())
                                        .setSecondOfDay(3600))
                        .addUnits(PTransactionUnit.newBuilder().setType(PTransactionUnit.Type.TAX).setAmount(300)
                                        .setCurrencyCode("EUR")));

        // remaining account transaction types
        long date = 1_725_000_000L;
        for (PTransaction.Type type : new PTransaction.Type[] { PTransaction.Type.DEPOSIT, PTransaction.Type.REMOVAL,
                        PTransaction.Type.INTEREST, PTransaction.Type.INTEREST_CHARGE, PTransaction.Type.TAX,
                        PTransaction.Type.TAX_REFUND, PTransaction.Type.FEE, PTransaction.Type.FEE_REFUND })
        {
            client.addTransactions(transaction(type).setAccount(ACCOUNT_1).setDate(timestamp(date, 0))
                            .setCurrencyCode("EUR").setAmount(1_000 + transactionCounter));
            date += 86_400;
        }

        // negative amount and empty optional strings
        client.addTransactions(transaction(PTransaction.Type.DEPOSIT).setAccount("").setPortfolio("")
                        .setDate(timestamp(-1L, 0)).setCurrencyCode("").setAmount(-12_345).setShares(-1)
                        .setSecurity("").setSource(""));
    }

    private static void addPlans(PClient.Builder client)
    {
        client.addPlans(PInvestmentPlan.newBuilder().setName("Sparplan").setNote("monatlich").setSecurity(SECURITY_1)
                        .setPortfolio(PORTFOLIO_1).setAccount(ACCOUNT_1).addAttributes(stringAttribute("a", "b"))
                        .setAutoGenerate(true).setDate(LocalDate.of(2024, 1, 15).toEpochDay()).setInterval(1)
                        .setAmount(10_000).setFees(150).setTaxes(0).addTransactions("tx-1").addTransactions("tx-2")
                        .setType(PInvestmentPlan.Type.PURCHASE_OR_DELIVERY));
        client.addPlans(PInvestmentPlan.newBuilder().setName("Einzahlung").setAccount(ACCOUNT_1).setDate(0)
                        .setInterval(103).setAmount(5_000).setTaxes(12).setType(PInvestmentPlan.Type.DEPOSIT));
        client.addPlans(PInvestmentPlan.newBuilder().setName("Entnahme").setAccount(ACCOUNT_2).setInterval(12)
                        .setAmount(1).setType(PInvestmentPlan.Type.REMOVAL));
        client.addPlans(PInvestmentPlan.newBuilder().setName("Zins").setAccount(ACCOUNT_2)
                        .setType(PInvestmentPlan.Type.INTEREST));
    }

    private static PTaxonomy.Builder taxonomy()
    {
        return PTaxonomy.newBuilder().setId("tax-1").setName("Asset-Allokation").setSource("template")
                        .addDimensions("Dimension 1").addDimensions("")
                        .addClassifications(PTaxonomy.Classification.newBuilder().setId("root").setName("Root")
                                        .setColor("#000000").setWeight(10000).setRank(0))
                        .addClassifications(PTaxonomy.Classification.newBuilder().setId("equity").setParentId("root")
                                        .setName("Aktien").setNote("Notiz").setColor("#ff0000").setWeight(7000)
                                        .setRank(1)
                                        .addData(PKeyValue.newBuilder().setKey("sortOrder")
                                                        .setValue(PAnyValue.newBuilder().setInt32(-7)))
                                        .addAssignments(PTaxonomy.Assignment.newBuilder()
                                                        .setInvestmentVehicle(SECURITY_1).setWeight(10000).setRank(2)
                                                        .addData(stringAttribute("k", "v")))
                                        .addAssignments(PTaxonomy.Assignment.newBuilder()
                                                        .setInvestmentVehicle(ACCOUNT_1).setWeight(5000)))
                        .addClassifications(PTaxonomy.Classification.newBuilder().setId("cash").setParentId("root")
                                        .setName("Liquidität").setNote("").setColor("").setWeight(3000).setRank(2));
    }

    private static PTransaction.Builder transaction(PTransaction.Type type)
    {
        transactionCounter++;
        return PTransaction.newBuilder().setUuid(String.format("tx-%04d", transactionCounter)).setType(type)
                        .setUpdatedAt(timestamp(1_730_000_000L + transactionCounter, transactionCounter * 1000));
    }

    private static PKeyValue.Builder stringAttribute(String key, String value)
    {
        return PKeyValue.newBuilder().setKey(key).setValue(PAnyValue.newBuilder().setString(value));
    }

    // nanos must be a multiple of 100: our C# classes use DateTime (100ns precision) for timestamps,
    // finer values are rounded on write (accepted limitation, see Protobuf/README.txt)
    private static Timestamp timestamp(long seconds, int nanos)
    {
        return Timestamp.newBuilder().setSeconds(seconds).setNanos(nanos).build();
    }

    private static PDecimalValue decimal(String value)
    {
        // same encoding as Portfolio Performance's ProtobufWriter.asDecimalValue
        BigDecimal decimal = new BigDecimal(value);
        return PDecimalValue.newBuilder().setScale(decimal.scale()).setPrecision(decimal.precision())
                        .setValue(ByteString.copyFrom(decimal.unscaledValue().toByteArray())).build();
    }
}
