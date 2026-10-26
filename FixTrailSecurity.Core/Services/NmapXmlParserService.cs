using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
namespace FixTrailSecurity.Core.Services
{
    public class NmapXmlParserService
    {
        public const int MaxParsedFindings = 10_000;
        private const long MaxXmlCharacters = 5 * 1024 * 1024;

        public List<ParsedFinding> ParseNmapXml(Stream xmlStream)
        {
            var findings = new List<ParsedFinding>();
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    MaxCharactersInDocument = MaxXmlCharacters,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true
                };
                using var reader = XmlReader.Create(xmlStream, settings);
                var doc = XDocument.Load(reader, LoadOptions.None);
                var hosts = doc.Descendants("host");
                foreach (var host in hosts)
                {
                    var address = host.Element("address")?.Attribute("addr")?.Value ?? "Unknown Host";
                    var ports = host.Descendants("port");
                    foreach (var port in ports)
                    {
                        var state = port.Element("state")?.Attribute("state")?.Value;
                        if (state != "open") continue;
                        var portId = port.Attribute("portid")?.Value;
                        var protocol = port.Attribute("protocol")?.Value;
                        var service = port.Element("service")?.Attribute("name")?.Value ?? "unknown";
                        var product = port.Element("service")?.Attribute("product")?.Value ?? "";
                        var version = port.Element("service")?.Attribute("version")?.Value ?? "";
                        var serviceDetails = string.IsNullOrWhiteSpace(product) ? service : $"{product} {version}".Trim();
                        if (findings.Count >= MaxParsedFindings)
                            throw new InvalidOperationException($"Nmap import exceeds the maximum of {MaxParsedFindings} findings.");

                        findings.Add(new ParsedFinding
                        {
                            Title = $"Open Port: {portId}/{protocol} ({serviceDetails})",
                            Description = $"Host {address} has port {portId} open running {serviceDetails}.",
                            Source = "Nmap",
                            SourceSeverity = "Info" // Open ports are generally Info/Low unless matched with CVEs
                        });
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse Nmap XML file.", ex);
            }
            return findings;
        }
    }
    public class ParsedFinding
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string SourceSeverity { get; set; } = string.Empty;
    }
}
