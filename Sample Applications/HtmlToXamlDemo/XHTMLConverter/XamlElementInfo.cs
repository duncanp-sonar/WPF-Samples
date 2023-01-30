namespace HtmlToXamlDemo.XHTMLConverter
{
    internal class XamlElementInfo
    {

        public static readonly XamlElementInfo Paragrah = new XamlElementInfo("Paragraph", true, false);
        public static readonly XamlElementInfo Table = new XamlElementInfo("Table", false, false);

        public XamlElementInfo(string xamlElement, bool supportsInlines, bool requiresInlines)
        {
            XamlElementName = xamlElement;
            SupportsInlines = supportsInlines;
            RequiresInlines = requiresInlines;
        }

        /// <summary>
        /// The HTML element that the XAML element corresponds to
        /// </summary>
        public string XamlElementName { get; }

        /// <summary>
        /// True if the XAML element supports inlines as children (e.g. Run, text), otherwise false
        /// </summary>
        public bool SupportsInlines { get; }

        /// <summary>
        /// True if the XAML element needs to be parented under an element that supports inlines
        /// </summary>
        public bool RequiresInlines { get; }

    }

}
