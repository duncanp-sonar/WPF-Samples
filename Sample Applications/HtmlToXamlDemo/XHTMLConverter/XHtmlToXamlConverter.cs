using System;
using System.IO;
using System.Text;
using System.Xml;

namespace HtmlToXamlDemo.XHTMLConverter
{
    public class XHtmlToXamlConverter
    {
        private static readonly string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        public static string Convert(string xhtml)
            => new XHtmlToXamlConverter().Process(xhtml);


        private XmlWriter writer;
        private XmlReader reader;

        private string Process(string data)
        {
            data = data.Replace("&nbsp;", "&#160;");

            var sb = new StringBuilder();
            writer = CreateXmlWriter(sb);

            reader = CreateXmlReader(data);

            try
            {
                writer.WriteStartElement("FlowDocument", XamlNamespace);
                writer.WriteAttributeString("xml", "space", null, "preserve");
                writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Console.WriteLine("Start Element {0}", reader.Name);
                            ProcessElement();

                            break;

                        case XmlNodeType.Text:
                            Console.WriteLine("Text Node: {0}", reader.Value);
                            writer.WriteString(reader.Value);
                            break;

                        case XmlNodeType.Whitespace:
                            Console.WriteLine("Text Node: {0}", reader.Value);
                            writer.WriteString(reader.Value);
                            break;

                        case XmlNodeType.SignificantWhitespace:
                            Console.WriteLine("Text Node: {0}", reader.Value);
                            writer.WriteString(reader.Value);
                            break;

                        case XmlNodeType.EndElement:
                            Console.WriteLine("End Element {0}", reader.Name);
                            writer.WriteEndElement();
                            
                            // TODO - special cases: sometimes the HTML tag translates to multiple
                            // XAML tags, so we need to write more than one end element.
                            if(reader.Name == "li")
                            {
                                writer.WriteEndElement();
                            }
                            break;
                        default:
                            Console.WriteLine("Other node {0} with value {1}",
                                            reader.NodeType, reader.Value);
                            break;
                    }
                }

                writer.WriteEndElement(); // FlowDocument
            }
            finally
            {
                reader.Close();
                writer.Close();
            }

            return sb.ToString();
        }

        private void ProcessElement()
        {
            switch(reader.Name)
            {
                case "a":
                    writer.WriteStartElement("Hyperlink");
                    var href = reader.GetAttribute("href");
                    writer.WriteAttributeString("NavigateUri", href);
                    break;

                case "blockquote":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("Margin", "16,0,0,0");
                    break;

                case "code":
                    writer.WriteStartElement("Run");
                    break;

                case "em":
                    writer.WriteStartElement("Run");
                    writer.WriteAttributeString("FontStyle", "italic");
                    break;

                case "h2":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "20pt");
                    break;

                case "h3":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "18pt");
                    break;

                case "li":
                    writer.WriteStartElement("ListItem");
                    writer.WriteStartElement("Paragraph");
                    break;

                case "ol":
                    writer.WriteStartElement("List");
                    writer.WriteAttributeString("MarkerStyle", "Decimal");
                    break;

                case "p" : writer.WriteStartElement("Paragraph");
                    break;

                case "pre":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "8pt");
                    writer.WriteAttributeString("TextAlignment", "Left");
                    writer.WriteAttributeString("FontFamily", "Courier New");
                    break;

                case "strong":
                    writer.WriteStartElement("Run");
                    writer.WriteAttributeString("FontWeight", "bold");
                    break;

                case "ul":
                    writer.WriteStartElement("List");
                    writer.WriteAttributeString("MarkerStyle", "Disc");
                    break;

                default:
                    writer.WriteStartElement(reader.Name);
                    writer.WriteEndElement();
                    break;
            }
        }

        private static XmlReader CreateXmlReader(string data)
        {
            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = false,
            };

            var stream = new StringReader(data);
            return XmlReader.Create(stream, settings);
        }

        private static XmlWriter CreateXmlWriter(StringBuilder sb)
        {
            var stringWriter = new StringWriter(sb);

            var settings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
                CloseOutput = true,
                WriteEndDocumentOnClose = true
            };

            var writer = XmlWriter.Create(stringWriter, settings);
            return writer;
        }
    }
}
