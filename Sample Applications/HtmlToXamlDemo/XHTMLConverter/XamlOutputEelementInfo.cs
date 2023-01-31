namespace HtmlToXamlDemo.XHTMLConverter
{
    /// <summary>
    /// Information about a single XAML output element written to the new stream of XAML.
    /// </summary>
    public struct XamlOutputElementInfo
    {
        public XamlOutputElementInfo(string htmlElementName, bool supportsInlines)
        {
            HtmlElementName = htmlElementName;
            SupportsInlines = supportsInlines;
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
        /// True if the XAML element supports inlines as children (e.g. Run, text), otherwise false
        /// </summary>
        public bool SupportsInlines { get; }

        /// <summary>
        /// True if the XAML element supports blocks as children, otherwise false
        /// </summary>
        /// <remarks>This is a convenience method - there are no XAML elements that support both Inlines
        /// and Blocks as children, so we just return the negation of SupportsInLines.
        /// Note that there are some classes that don't support either Inlines or Blocks as children such as 
        /// List and the Table classes. However, as long as the HTML document we are parsing is valid it
        /// won't cause us a problem since we won't need to check SupportsInlines/SupportsBlocks for those
        /// classes.
        /// </remarks>
        public bool SupportsBlocks => !SupportsInlines;
    }
}
