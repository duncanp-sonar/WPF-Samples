// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace HtmlToXamlDemo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void ConvertContent(object sender, RoutedEventArgs e)
        {
            var converted = HtmlToXamlConverter.ConvertHtmlToXaml(myTextBox.Text, true);
            txtConverted.Text = converted;

            // Can't shared the document between controls, so we need to create a new
            // instance each time.
            docReader.Document = TryCreateDoc(converted);
            docScrollViewer.Document = TryCreateDoc(converted);
            docPgeViewer.Document = TryCreateDoc(converted);
        }

        private static FlowDocument TryCreateDoc(string text)
        {
            var xaml = XamlReader.Parse(text);
            return xaml as FlowDocument;
        }

        public void CopyXaml(object sender, RoutedEventArgs e)
        {
            myTextBox.SelectAll();
            myTextBox.Copy();
        }

        public void ConvertContent2(object sender, RoutedEventArgs e)
        {
            myTextBox2.Text = HtmlFromXamlConverter.ConvertXamlToHtml(myTextBox2.Text);
            MessageBox.Show("Content Conversion Complete!");
        }

        public void CopyHtml(object sender, RoutedEventArgs e)
        {
            myTextBox2.SelectAll();
            myTextBox2.Copy();
        }
    }
}