using System.Xml;

namespace InvestmentReporting.Import.AlphaDirectMyBroker {
	public sealed class XmlSanitizer {
		public XmlDocument Sanitize(XmlDocument report) {
			var sanitizedXml    = report.OuterXml.Replace("xmlns=\"MyBroker\"", string.Empty);
			var sanitizedReport = new XmlDocument();
			sanitizedReport.LoadXml(sanitizedXml);
			return sanitizedReport;
		}
	}
}