using System;
using System.IO;
using System.Linq;
using System.Text;
using FixTrailSecurity.Core.Services;
using Xunit;
namespace FixTrailSecurity.Tests
{
    public class NmapParserTests
    {
        [Fact]
        public void ParseNmapXml_WithValidOpenPorts_ShouldReturnFindings()
        {
            // Arrange
            var xmlFixture = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <nmaprun scanner=""nmap"" args=""nmap -sV -p 22,80 192.168.1.1"">
                <host>
                    <address addr=""192.168.1.1"" addrtype=""ipv4""/>
                    <ports>
                        <port protocol=""tcp"" portid=""22"">
                            <state state=""open"" reason=""syn-ack"" reason_ttl=""64""/>
                            <service name=""ssh"" product=""OpenSSH"" version=""8.2p1""/>
                        </port>
                        <port protocol=""tcp"" portid=""80"">
                            <state state=""closed"" reason=""conn-refused"" reason_ttl=""64""/>
                            <service name=""http""/>
                        </port>
                    </ports>
                </host>
            </nmaprun>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlFixture));
            var parser = new NmapXmlParserService();
            // Act
            var findings = parser.ParseNmapXml(stream);
            // Assert
            Assert.Single(findings); // Only one port is open
            var finding = findings.First();
            Assert.Equal("Open Port: 22/tcp (OpenSSH 8.2p1)", finding.Title);
            Assert.Equal("Nmap", finding.Source);
            Assert.Equal("Info", finding.SourceSeverity);
            Assert.Contains("192.168.1.1", finding.Description);
        }
        [Fact]
        public void ParseNmapXml_WithInvalidXml_ShouldThrowException()
        {
            // Arrange
            var invalidXml = "<nmaprun><host>Unclosed tags";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXml));
            var parser = new NmapXmlParserService();
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => parser.ParseNmapXml(stream));
        }
    }
}
