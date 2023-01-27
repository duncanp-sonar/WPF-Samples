using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HtmlToXamlDemo.XHTMLConverter
{
    public class XHtmlToXamlConverter
    {
        public static string Convert(string xhtml)
            => new XHtmlToXamlConverter().Process(xhtml);

        private XHtmlToXamlConverter() { }

        private string Process(string data)
        {
            var sb = new StringBuilder();

            using (var writer = CreateXmlWriter(sb))
            using (XmlReader reader = CreateXmlReader(data))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Console.WriteLine("Start Element {0}", reader.Name);
                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("Text Node: {0}",
                                     reader.Value);
                            break;
                        case XmlNodeType.EndElement:
                            Console.WriteLine("End Element {0}", reader.Name);
                            break;
                        default:
                            Console.WriteLine("Other node {0} with value {1}",
                                            reader.NodeType, reader.Value);
                            break;
                    }
                }
            }

            return sb.ToString();
        }

        private static XmlReader CreateXmlReader(string data)
        {
            var settings = new XmlReaderSettings
            {
                ConformanceLevel= ConformanceLevel.Fragment
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
                Indent = true
            };

            return XmlWriter.Create(stringWriter, settings);
        }

    }
}
