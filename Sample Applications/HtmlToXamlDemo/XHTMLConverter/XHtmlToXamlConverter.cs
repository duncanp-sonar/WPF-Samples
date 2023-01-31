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


        /// <summary>
        /// Stack of currently open XAML elements
        /// </summary>
        /// <remarks>We need some information about the current structure so we can check whether some 
        /// operations are valid e.g. can be we add text to the current element?</remarks>
        private Stack<XamlOutputElementInfo> outputXamlElementStack = new Stack<XamlOutputElementInfo>();

        /// <summary>
        /// Used to add background colour to alternate rows in a table
        /// </summary>
        private bool tableAlternateRow;

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

                outputXamlElementStack.Push(new XamlOutputElementInfo("html root", false));

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

                var current = outputXamlElementStack.Pop();
                Debug.Assert(outputXamlElementStack.Count == 0
                    && current.HtmlElementName == "html root",
                    "Expecting the stack to contain the initial flow document element");
                writer.WriteEndElement(); // FlowDocument
            }
            finally
            {
                reader.Close();
                writer.Close();
            }

            return sb.ToString();
        }


        private void PushOutputElementInfo(string htmlElementName, bool supportsInlines)
        {
            outputXamlElementStack.Push(new XamlOutputElementInfo(htmlElementName, supportsInlines));
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

                    PushOutputElementInfo("blockquote", false);

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

                    PushOutputElementInfo("h2", true);
                    break;

                case "h3":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "18pt");

                    PushOutputElementInfo("h3", true);
                    break;

                case "li":
                    writer.WriteStartElement("ListItem");
                    PushOutputElementInfo("li", false);

                    break;

                case "ol":
                    WriteBlockElementStart("List");
                    //writer.WriteStartElement("List");
                    writer.WriteAttributeString("MarkerStyle", "Decimal");

                    PushOutputElementInfo("ol", false);

                    break;

                case "p" :
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("xml", "space", null, "default");

                    PushOutputElementInfo("p", true);

                    break;

                case "pre":
                    writer.WriteStartElement("Paragraph");
                    writer.WriteAttributeString("FontSize", "8pt");
                    writer.WriteAttributeString("TextAlignment", "Left");
                    writer.WriteAttributeString("FontFamily", "Courier New");

                    PushOutputElementInfo("pre", true);
                    break;

                case "strong":
                    WriteInlineElementStart("Bold");

                    break;

                case "ul":
                    WriteBlockElementStart("List");
                    writer.WriteAttributeString("MarkerStyle", "Disc");

                    PushOutputElementInfo("ul", false);

                    break;

                case "table":
                    WriteBlockElementStart("Table");
                    writer.WriteAttributeString("BorderThickness", "1,1,1,1");
                    writer.WriteAttributeString("BorderBrush", "Black");

                    PushOutputElementInfo("table", false);

                    break;

                case "colgroup":
                    writer.WriteStartElement("Table.Columns");
                    PushOutputElementInfo("colgroup", false);

                    break;

                case "col":
                    // This is an empty element, so there is nothing to push onto the stack.
                    WriteEmptyElement("TableColumn");

                    break;

                case "thead":
                    writer.WriteStartElement("TableRowGroup");
                    writer.WriteAttributeString("Background", "LightGray");
                    writer.WriteAttributeString("FontWeight", "Bold");

                    PushOutputElementInfo("thead", false);

                    break;

                case "tr":
                    writer.WriteStartElement("TableRow");
                    tableAlternateRow = !tableAlternateRow;

                    PushOutputElementInfo("tr", false);

                    break;

                case "th":
                    writer.WriteStartElement("TableCell");
                    writer.WriteAttributeString("BorderThickness", "0,0,0,1");
                    writer.WriteAttributeString("BorderBrush", "Black");
                    PushOutputElementInfo("th", false);
                    
                    break;

                case "tbody":
                    tableAlternateRow = true;
                    writer.WriteStartElement("TableRowGroup");
                    PushOutputElementInfo("tbody", false);

                    break;

                case "td":
                    writer.WriteStartElement("TableCell");
                    PushOutputElementInfo("td", false);

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
            // e.g. <li> <bold>some text ... </bold></li>            -> <li> does not support inlines directly.
            EnsureCurrentOutputSupportsInlines();

            writer.WriteStartElement(elementName);
            PushOutputElementInfo(reader.Name, true);
        }

        private void WriteText(string text)
        {
            // If we are writing an inline element, we need a parent element that supports text directly.
            // e.g. <li> some text ... </li>            -> <li> does not support inlines directly.
            EnsureCurrentOutputSupportsInlines();

            // Note: we could explicitly wrap the text in a <Run>. However, that will happen implicitly
            // when the XAML is parsed, and it won't make any difference to the rendered output.
            writer.WriteString(text);
        }
        private void WriteBlockElementStart(string elementName)
        {
            EnsureCurrentOutputSupportsBlocks();
            writer.WriteStartElement(elementName);
        }

        private void EnsureCurrentOutputSupportsInlines()
        {
            var current = outputXamlElementStack.Peek();
            if (current.SupportsInlines) { return; }

            // If the current XAML class doesn't support inlines then we assume that
            // it supports blocks, and add Paragraph.
            // Paragraph is a type of Block that supports Inlines.

            // Note that there are some XAML classes where this situation won't be valid
            // e.g. <Table>text
            // In this case, adding a Paragraph to a Table directly is not valid i.e. the
            // input HTML document is not valid. If the input document isn't valid then 
            // we don't try to produce a valid XAML document from it.

            writer.WriteStartElement("Paragraph");
            PushOutputElementInfo(null, true);
        }

        private void EnsureCurrentOutputSupportsBlocks()
        {
            var current = outputXamlElementStack.Peek();
            if (current.SupportsBlocks) { return; }

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

            // This produces the following XAML:
            // a.  <List>           <-- mapped to html <ol>
            // b.    <ListItem>     <-- mapped to html <li>
            //
            //                      // Next, we want to add the text from line 2. However, we
            //                      // can't add text to ListItem since it only accepts blocks.
            //                      // So, we add a Paragraph, which is a block that can contain
            //                      // Inlines e.g. text.
            //                      
            // c.      <Paragraph>  <-- extra element added by us, not mapped to an html element
            // d.        type name, spelling of..       <-- text from the html
            //
            //                      // Next, we want to handle the <ol> tag, which translates to
            //                      // a XAML "List". List is a block, which we can't add to a
            //                      // Paragraph. So, we need to walk back up the list of extra XAML
            //                      // elements we have opened and close them, until we reach an
            //                      // XAML class that supports Blocks.
            // e.      </Paragraph> <-- close the paragraph we opened to contain the text.
            //                      // The current XAML element is now the ListItem. This does
            //                      // accepts Blocks, so we can stop looking.
            // f.      <List>       <-- mapped to the nested html <li>

            // To summarise, in the example above, we add the extra <Paragraph> XAML opening tag,
            // since we need a paragraph to hold the text under the ListItem.
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
