using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using Toqe.PortfolioReader.EcbRates.Models;

namespace Toqe.PortfolioReader.EcbRates
{
    public class EcbRateReader
    {
        private const string DefaultEcbRatesDownloadUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

        public List<EcbRateValue> Parse(string xml)
        {
            var xDoc = XDocument.Parse(xml);
            var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            xmlNamespaceManager.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
            xmlNamespaceManager.AddNamespace("x", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");

            var nodes = xDoc.XPathSelectElements("/gesmes:Envelope/x:Cube/x:Cube/x:Cube", xmlNamespaceManager);
            var result = new List<EcbRateValue>(nodes.Count());

            foreach (var node in nodes)
            {
                var rate = new EcbRateValue
                {
                    Date = DateTime.ParseExact(node.Parent.Attribute("time").Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Currency = node.Attribute("currency").Value,
                    Rate = decimal.Parse(node.Attribute("rate").Value, CultureInfo.InvariantCulture),
                };

                result.Add(rate);
            }

            return result;
        }

        public async Task<List<EcbRateValue>> Download(string url = DefaultEcbRatesDownloadUrl)
        {
            var xmlString = await this.DownloadXml(url);
            var result = this.Parse(xmlString);
            return result;
        }

        private async Task<string> DownloadXml(string url)
        {
            var ecbRequest = await new HttpClient().GetAsync(url);
            ecbRequest.EnsureSuccessStatusCode();
            var ecbRatesXml = await ecbRequest.Content.ReadAsStringAsync();
            return ecbRatesXml;
        }
    }
}
