using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;
using System.Xml.Xsl;

namespace XmlTransformer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите XML файл (data1)",
                Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
                StatusText.Text = $"Выбран файл: {Path.GetFileName(openFileDialog.FileName)}";
            }
        }

        private void MagicButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FilePathTextBox.Text))
                {
                    StatusText.Text = "⚠️ Ошибка: Не выбран XML файл!";
                    return;
                }

                string xmlFilePath = FilePathTextBox.Text;
                string xslFilePath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "instruction.xls");

                if (!File.Exists(xmlFilePath))
                {
                    StatusText.Text = "⚠️ Ошибка: XML файл не найден!";
                    return;
                }

                if (!File.Exists(xslFilePath))
                {
                    StatusText.Text = "⚠️ Ошибка: Файл instruction.xsl не найден рядом с XML!";
                    return;
                }

                // Выполняем XSLT преобразование
                string outputFilePath = Path.Combine(
                    Path.GetDirectoryName(xmlFilePath),
                    $"{Path.GetFileNameWithoutExtension(xmlFilePath)}_transformed.xml"
                );

                PerformXsltTransformation(xmlFilePath, xslFilePath, outputFilePath);

                StatusText.Text = $"✅ Преобразование выполнено! Результат: {Path.GetFileName(outputFilePath)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка: {ex.Message}";
            }
        }

        private void PerformXsltTransformation(string xmlPath, string xslPath, string outputPath)
        {
            try
            {
                // Загружаем XSLT
                var xslt = new XslCompiledTransform();
                xslt.Load(xslPath);

                // Выполняем преобразование
                using (var writer = XmlWriter.Create(outputPath, new XmlWriterSettings { Indent = true }))
                {
                    xslt.Transform(xmlPath, writer);
                }
            }
            catch (XmlException ex)
            {
                throw new Exception($"Ошибка в XML/XSL файле: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при преобразовании: {ex.Message}");
            }
        }
    }
}