// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using HtmlToXamlDemo.XHTMLConverter;
using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace HtmlToXamlDemo
{

    public class SampleFile
    {
        public SampleFile(string fullPath, string displayName)
        {
            FullPath = fullPath;
            DisplayName = displayName;
        }

        public string FullPath { get; }
        public string DisplayName { get; }
    }


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
</pre>
<p>
<a href=""https://wiki.sei.cmu.edu/confluence/x/1DdGBQ"">CERT, MSC52-J.</a> - Finish every set of statements associated with a case label with a
  break statement 
</p>
";
            myTextBox.Text = html;

            cbSampleFiles.ItemsSource = GetSampleFiles();


            var input = @"
<p>To ensure future code portability, obsolete POSIX functions should be removed. Those functions, with their replacements are listed below:</p>
<table>
  <colgroup>
    <col style=""width: 50%;"">
    <col style=""width: 50%;"">
  </colgroup>
 ";
            var exp = new Regex("<(col[\\s>])[^>]*>");
            
            exp = new Regex("<col[/s>]>");

            exp = new Regex("(<col\\s*>|<col\\s+>)");


            exp = new Regex("(<col\\s*[^/^>]*>)");

            exp = new Regex("(<col\\s*[^/^>]*>)");

            exp = new Regex("(<col\\s*>)"); // <col     >

            exp = new Regex("(<col\\s*>)|(col\\s+[^/]*>)");
            exp = new Regex("(?<test>(<col\\s*)|(col\\s+[^/]*))>");

            var matches = exp.Matches(input);
            var modified = Regex.Replace(input, "(<col\\s*>)|(col\\s+[^/]*>)", "$1XXX");

            modified = Regex.Replace(input, "(?<element>(<col\\s*)|(col\\s+[^/]*))>", "${element}/>");


            exp = new Regex("(?<element>((<col\\s*)|(col\\s+[^/^>]*)))>");

            modified = exp.Replace(input, "${element}/>");


        }

        private SampleFile[] GetSampleFiles()
        {
            var sampleDir =Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "SampleFiles");
            var files = Directory.GetFiles(sampleDir, "*.*");

            return files.Select(x => new SampleFile(x, Path.GetFileName(x))).ToArray();
        }

        public void ConvertContent(object sender, RoutedEventArgs e)
        {
            ConvertHtmlToXaml(myTextBox.Text);
        }

        private void ConvertHtmlToXaml(string text)
        {
            var converted = HtmlToXamlConverter.ConvertHtmlToXaml(text, true);
            txtConverted.Text = converted;

            // Can't shared the document between controls, so we need to create a new
            // instance each time.
            docReader.Document = TryCreateDoc(converted);
            docScrollViewer.Document = TryCreateDoc(converted);
            docPgeViewer.Document = TryCreateDoc(converted);

            string convertedNew = "";
            try
            {
                convertedNew = XHtmlToXamlConverter.Convert(myTextBox.Text);
                txtConvertedNew.Text = convertedNew;

                docReaderNew.Document = TryCreateDoc(convertedNew);
                docScrollViewerNew.Document = TryCreateDoc(convertedNew);
                docPgeViewerNew.Document = TryCreateDoc(convertedNew);
            }
            catch (Exception ex)
            {
                txtConvertedNew.Text = ex.Message + Environment.NewLine + Environment.NewLine + convertedNew;
            }
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

        private void cbSampleFiles_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var filePath = cbSampleFiles.SelectedItem as SampleFile;
            if (filePath != null && File.Exists(filePath.FullPath))
            {
                var text = File.ReadAllText(filePath.FullPath);

                myTextBox.Text = text;
                ConvertHtmlToXaml(myTextBox.Text);
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            const string rootFileDirectory = "d:\\repos\\sq\\slvs\\src\\rules\\embedded";
            var files = Directory.GetFiles(rootFileDirectory, "*.desc", SearchOption.AllDirectories);

            txtTestResults.Text = "";
            LogTestOutput("Files: " + files.Length);

            testFilesFailed = 0;

            foreach (var file in files)
            {
                TestConverter(file);
            }

            LogTestOutput("Files: " + files.Length);
            LogTestOutput("Failed: " + testFilesFailed);

        }

        private int testFilesFailed = 0;

        private void TestConverter(string file)
        {
            var text = File.ReadAllText(file);

            try
            {
                var xaml = XHtmlToXamlConverter.Convert(text);
                TryCreateDoc(xaml);
            }
            catch (Exception ex)
            {
                LogTestOutput($"Failed: {file}, {ex.Message}");
                testFilesFailed++;
            }
        }

        private void LogTestOutput(string text)
        {
            var allText = txtTestResults.Text + Environment.NewLine + text;
            txtTestResults.Text = allText;
            txtTestResults.SelectionStart= txtTestResults.Text.Length;
        }
    }
}