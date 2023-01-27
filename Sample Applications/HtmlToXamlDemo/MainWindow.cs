// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using HtmlToXamlDemo.XHTMLConverter;
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
        public override void EndInit()
        {
            base.EndInit();

            var html = @"<p>Shared naming conventions allow teams to collaborate efficiently. This rule checks that all function names match a provided regular expression.</p>
<h2>Noncompliant Code Example</h2>
<p>With default provided regular expression: <code>^[a-z][a-zA-Z0-9]*$</code>:</p>
<pre>
void DoSomething (void);
</pre>
<h2>Compliant Solution</h2>
<pre>
void doSomething (void);
</pre>";
            myTextBox.Text = html;
        }

        public void ConvertContent(object sender, RoutedEventArgs e)
        {
            var converted = HtmlToXamlConverter.ConvertHtmlToXaml(myTextBox.Text, true);
            txtConverted.Text = converted;

            var convertedNew = XHtmlToXamlConverter.Convert(myTextBox.Text);
            txtConvertedNew.Text = convertedNew;


            // Can't shared the document between controls, so we need to create a new
            // instance each time.
            docReader.Document = TryCreateDoc(converted);
            docScrollViewer.Document = TryCreateDoc(converted);
            docPgeViewer.Document = TryCreateDoc(converted);

            //docReader.Document = TryCreateDoc(convertedNew);
            //docScrollViewer.Document = TryCreateDoc(convertedNew);
            //docPgeViewer.Document = TryCreateDoc(convertedNew);

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