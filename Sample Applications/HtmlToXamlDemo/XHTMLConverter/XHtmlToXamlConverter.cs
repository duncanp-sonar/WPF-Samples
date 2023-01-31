using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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

        private Stack<XamlOutputElementInfo> outputXamlElementStack = new Stack<XamlOutputElementInfo>();

        /// <summary>
        /// Information about a single XAML output element written to the new stream of XAML.
        /// </summary>
        private struct XamlOutputElementInfo
        {
            public XamlOutputElementInfo(string htmlElementName, bool isInline, bool supportsInlines)
            {
                HtmlElementName= htmlElementName;
                IsInline= isInline;
                SupportsInlines= supportsInlines;
            }

            /// <summary>
            /// True if the XAML element corresponds directly to a specific HTML element in the
            /// source document, otherwise false
            /// </summary>
            public bool MapsDirectlyToHtmlElement => !string.IsNullOrEmpty(HtmlElementName);

            /// <summary>
            /// The HTML element that the XAML element corresponds to
            /// </summary>
            public string HtmlElementName { get; }

            /// <summary>
            /// True if the XAML element needs to be parented under an element that supports inlines
            /// </summary>
            public bool IsInline { get; }

            /// <summary>
            /// True if the XAML element supports inlines as children (e.g. Run, text), otherwise false
            /// </summary>
            public bool SupportsInlines { get; }
        }

        private string Process(string data)
        {
            data = CleanHtml(data);

            var sb = new StringBuilder();
            writer = CreateXmlWriter(sb);

            reader = CreateXmlReader(data);

            try
            {
                
                writer.WriteStartElement("FlowDocument", XamlNamespace);
                writer.WriteAttributeString("xml", "space", null, "preserve");
                writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");

                outputXamlElementStack.Push(new XamlOutputElementInfo("html root", false, false));

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
                            WriteText(reader.Value);
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

                            XamlOutputElementInfo xamlOutputElement;
                            do
                            {
                                xamlOutputElement = outputXamlElementStack.Pop();
                                writer.WriteEndElement();
                            } while (xamlOutputElement.HtmlElementName != reader.Name);

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

            Debug.Assert(outputXamlElementStack.Count == 1
                && outputXamlElementStack.Peek().HtmlElementName == "html root",
                "Expecting the stack to contain the initial flow document element");

            return sb.ToString();
        }

        private bool tableAlternateRow;

        private void PushOutputElementInfo(string htmlElementName, bool isInline, bool supportsInlines)
        {
            outputXamlElementStack.Push(new XamlOutputElementInfo(htmlElementName, isInline, supportsInlines));
        }

        private void ProcessElement()
        {
            switch(reader.Name)
            {
                case "a":
                    WriteInlineElementStart("Hyperlink");

                    var href = reader.GetAttribute("href");
                    writer.WriteAttributeString("NavigateUri", href);

                    break;

                case "blockquote":
                    writer.WriteStartElement("Section");
                    writer.WriteAttributeString("Margin", "16,0,0,0");
                    writer.WriteAttributeString("Background", "LightGray");

                    PushOutputElementInfo("blockquote", false, false);

                    break;

                case "br":
                    // This is an empty element, so there is nothing to push onto the stack.
                    WriteEmptyElement("LineBreak");

                    break;

                case "code":
                    WriteInlineElementStart("Span");

                    break;

                case "em":
                    WriteInlineElementStart("Italic");
                    break;

                case "h2":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "20pt");

                    PushOutputElementInfo("h2", false, true);
                    break;

                case "h3":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "18pt");

                    PushOutputElementInfo("h3", false, true);
                    break;

                case "li":
                    writer.WriteStartElement("ListItem");
                    PushOutputElementInfo("li", false, false);

                    break;

                case "ol":
                    WriteBlockElementStart("List");
                    //writer.WriteStartElement("List");
                    writer.WriteAttributeString("MarkerStyle", "Decimal");

                    PushOutputElementInfo("ol", false, false);

                    break;

                case "p" :
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("xml", "space", null, "default");

                    PushOutputElementInfo("p", false, true);

                    break;

                case "pre":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "8pt");
                    writer.WriteAttributeString("TextAlignment", "Left");
                    writer.WriteAttributeString("FontFamily", "Courier New");

                    PushOutputElementInfo("pre", false, true);
                    break;

                case "strong":
                    WriteInlineElementStart("Bold");

                    break;

                case "ul":
                    WriteBlockElementStart("List");
                    writer.WriteAttributeString("MarkerStyle", "Disc");

                    PushOutputElementInfo("ul", false, false);

                    break;

                case "table":
                    WriteBlockElementStart("Table");
                    writer.WriteAttributeString("BorderThickness", "1,1,1,1");
                    writer.WriteAttributeString("BorderBrush", "Black");

                    PushOutputElementInfo("table", false, false);

                    break;

                case "colgroup":
                    writer.WriteStartElement("Table.Columns");
                    PushOutputElementInfo("colgroup", false, false);

                    break;

                case "col":
                    // This is an empty element, so there is nothing to push onto the stack.
                    WriteEmptyElement("TableColumn");

                    break;

                case "thead":
                    writer.WriteStartElement("TableRowGroup");
                    writer.WriteAttributeString("Background", "LightGray");
                    writer.WriteAttributeString("FontWeight", "Bold");

                    PushOutputElementInfo("thead", false, false);

                    break;

                case "tr":
                    writer.WriteStartElement("TableRow");
                    tableAlternateRow = !tableAlternateRow;

                    PushOutputElementInfo("tr", false, false);

                    break;

                case "th":
                    writer.WriteStartElement("TableCell");
                    writer.WriteAttributeString("BorderThickness", "0,0,0,1");
                    writer.WriteAttributeString("BorderBrush", "Black");
                    PushOutputElementInfo("th", false, false);
                    
                    break;

                case "tbody":
                    tableAlternateRow = true;
                    writer.WriteStartElement("TableRowGroup");
                    PushOutputElementInfo("tbody", false, false);

                    break;

                case "td":
                    writer.WriteStartElement("TableCell");
                    PushOutputElementInfo("td", false, false);

                    if (tableAlternateRow)
                    {
                        writer.WriteAttributeString("Background", "LightBlue");
                    }

                    break;

                default:
                    Debug.Fail("Unexpected element type: " + reader.Name);
                    writer.WriteStartElement(reader.Name);
                    writer.WriteEndElement();
                    break;
            }
        }

        private void WriteEmptyElement(string name, string value = null)
        {
            Debug.Assert(reader.IsEmptyElement);
            writer.WriteElementString(name, value);
        }

        private void WriteInlineElementStart(string elementName)
        {
            // If we are writing an inline element, we need a parent element that supports inlines.

            // This might/might not be the case.
            // e.g. <li><p> some text ... </p><li>      -> <p> does support inlines, so we can just write the text
            // e.g. <li> some text ... </li>            -> <li> does not support inlines directly.
            EnsureCurrentOutputSupportsInlines();

            writer.WriteStartElement(elementName);
            PushOutputElementInfo(reader.Name, true, true);
        }

        private void WriteText(string text)
        {
            // If we are writing an inline element, we need a parent element that supports inlines
            EnsureCurrentOutputSupportsInlines();

            writer.WriteString(text);
        }
        private void EnsureCurrentOutputSupportsInlines()
        {
            var current = outputXamlElementStack.Peek();
            if (current.SupportsInlines) { return; }

            writer.WriteStartElement("Paragraph");
            PushOutputElementInfo(null, false, true);
        }

        private void WriteBlockElementStart(string elementName)
        {
            EnsureCurrentOutputSupportsBlocks();
            writer.WriteStartElement(elementName);
        }

        private void EnsureCurrentOutputSupportsBlocks()
        {
            var current = outputXamlElementStack.Peek();
            // Assumes supporting blocks and inlines are mutually exclusive, which
            // is the case for all the WPF document classes we are using.
            if (!current.SupportsInlines) { return; }

            // If we are in an element that supports inlines, we can't add another child element
            // that supports blocks because there are no WPF classes that do that.
            // Instead, all we can do is close the current element(s) recursively until we find
            // an existing output element that does support blocks.
            //
            // However, we can only walk back as far as the nearest parent output element that was
            // directly mapped to an html element (otherwise we'll fail later when we try to process
            // the matching closing html token).
            // In other words, we can only walk back up the stack closing "extra" elements we added
            // ourselves, that don't map to a specific html tag.

            // e.g. nested lists - c_s1749.desc
            // 1. <ol> 
            // 2.     <li> type name, spelling of built-in types with more than one type-specifier:
            // 3.        <ol>
            // 4.             <li> signedness - <code>signed</code> or <code>unsigned</code> </li>

            // For line 2, we will generate:
            // a.  <List>           <-- mapped to html <ol>
            // b.    <ListItem>     <-- mapped to html <li>
            // c.      <Paragraph>  <-- extra element added by us, not mapped to an html element
            // d.        type name, spelling of..       <-- text from the html

            // In the example above, we add the extra <Paragraph> XAML opening tag, since we need
            // a paragraph to hold the text.
            // However, we then encounter the html <ol> element, which translates to another
            // XAML <List>. "List" is a block element, so we can't host it under <Paragraph>.
            // So, we close the <Paragraph> element and look at its parent, <ListItem>.
            // "ListItem" can contain blocks, so we can now add the new <List> opening tag.
            // Note: the <ListItem> in line 2 is as far back as we can check, since it is mapped
            // directly to an html element (<li>).

            while (current.HtmlElementName == null && current.SupportsInlines)
            {
                writer.WriteEndElement();
                outputXamlElementStack.Pop();

                current = outputXamlElementStack.Peek();
            }

            if (current.SupportsInlines)
            {
                throw new InvalidOperationException("Invalid state: can't find an element that supports blocks");
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

        private static Regex cleanCol = new Regex("(?<element>(<col\\s*)|(col\\s+[^/^>]*))>", RegexOptions.Compiled);
        private static Regex cleanBr = new Regex("(?<element>(<br\\s*)|(br\\s+[^/^>]*))>", RegexOptions.Compiled);

        private static string CleanHtml(string text)
        {
            var cleaned = text.Replace("&nbsp;", "&#160;");
            cleaned = cleanCol.Replace(cleaned, "${element}/>");
            cleaned = cleanBr.Replace(cleaned, "${element}/>");

            return cleaned;
        }

    }
}
