using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using iText.Html2pdf;

namespace TreatmentPDF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string? _filePath;

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HTML Files|*.html"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
            }
        }

        private void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            TextBlock statusTextBlock = (TextBlock)FindName("statusTextBlock");
            CheckBox deletedAllLinks = (CheckBox)FindName("deletedAllLinks");

            if (deletedAllLinks?.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(_filePath))
                {
                    try
                    {
                        statusTextBlock.Text = "Начало обработки...";

                        var htmlDoc = new HtmlDocument();
                        htmlDoc.Load(_filePath);

                        var textNodes = htmlDoc.DocumentNode.SelectNodes("//text()");

                        if (textNodes != null)
                        {
                            Parallel.ForEach(textNodes, node =>
                            {
                                var words = node.InnerText.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int i = 0; i < words.Length; i++)
                                {
                                    if (Regex.IsMatch(words[i], @"\b(http|https):", RegexOptions.IgnoreCase))
                                    {
                                        words[i] = string.Empty;
                                    }
                                }

                                node.InnerHtml = string.Join(" ", words.Where(word => !string.IsNullOrEmpty(word)));
                            });

                            htmlDoc.Save(_filePath);

                            var pdfFilePath = Path.ChangeExtension(_filePath, ".pdf");
                            using (var pdfStream = new FileStream(pdfFilePath, FileMode.Create))
                            {
                                ConverterProperties properties = new ConverterProperties();
                                HtmlConverter.ConvertToPdf(File.ReadAllText(_filePath), pdfStream, properties);
                            }

                            statusTextBlock.Text = $"Обработка завершена.\nФайл сохранен по пути {pdfFilePath}";
                        }
                        else
                        {
                            statusTextBlock.Text = "Нет текстовых узлов для обработки.";
                        }
                    }
                    catch (Exception ex)
                    {
                        statusTextBlock.Text = $"Произошла ошибка: {ex.Message}";
                    }
                }
            }
        }
    }
}
