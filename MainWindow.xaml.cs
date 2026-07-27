using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace XmlTransformer
{
    /// <summary>
    /// Модель записи для таблицы
    /// </summary>
    public class RecordModel : INotifyPropertyChanged
    {
        private string _name;
        private string _surname;
        private decimal _amount;
        private string _month;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Surname
        {
            get => _surname;
            set
            {
                if (_surname != value)
                {
                    _surname = value;
                    OnPropertyChanged(nameof(Surname));
                }
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public string Month
        {
            get => _month;
            set
            {
                if (_month != value)
                {
                    _month = value;
                    OnPropertyChanged(nameof(Month));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RecordModel()
        {
            Name = "Ivan";
            Surname = "Ivanov";
            Amount = 0;
            Month = "january";
        }

        public RecordModel(string name, string surname, decimal amount, string month)
        {
            Name = name;
            Surname = surname;
            Amount = amount;
            Month = month;
        }
    }

    public partial class MainWindow : Window
    {
        private ObservableCollection<RecordModel> _records = new ObservableCollection<RecordModel>();
        private string _currentFilePath;
        private string _xmlStructureType; // "flat" или "grouped"

        // Список месяцев для ComboBox
        public List<string> MonthsList { get; } = new List<string>
        {
            "january", "february", "march", "april", "may", "june",
            "july", "august", "september", "october", "november", "december"
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            RecordsDataGrid.ItemsSource = _records;
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
                _currentFilePath = openFileDialog.FileName;
                StatusText.Text = $"Выбран файл: {Path.GetFileName(openFileDialog.FileName)}";
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FilePathTextBox.Text))
                {
                    StatusText.Text = "⚠️ Ошибка: Не выбран XML файл!";
                    return;
                }

                if (!File.Exists(FilePathTextBox.Text))
                {
                    StatusText.Text = "⚠️ Ошибка: XML файл не найден!";
                    return;
                }

                LoadXmlData(FilePathTextBox.Text);
                StatusText.Text = $"✅ Загружено {_records.Count} записей";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Детали ошибки:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newRecord = new RecordModel();
            _records.Add(newRecord);

            // Прокручиваем к новой записи
            RecordsDataGrid.ScrollIntoView(newRecord);
            RecordsDataGrid.SelectedItem = newRecord;

            UpdateStatistics();
            StatusText.Text = "✅ Добавлена новая запись";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentFilePath))
                {
                    // Если файл не выбран, предлагаем сохранить как
                    var saveDialog = new SaveFileDialog
                    {
                        Title = "Сохранить XML файл",
                        Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
                        FileName = "data1.xml"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        _currentFilePath = saveDialog.FileName;
                        FilePathTextBox.Text = _currentFilePath;
                    }
                    else
                    {
                        return;
                    }
                }

                SaveXmlData(_currentFilePath);
                StatusText.Text = $"✅ Данные сохранены в {Path.GetFileName(_currentFilePath)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка сохранения: {ex.Message}";
                MessageBox.Show($"Детали ошибки:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var record = button?.Tag as RecordModel;

            if (record != null)
            {
                if (MessageBox.Show($"Удалить запись для {record.Name} {record.Surname}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _records.Remove(record);
                    UpdateStatistics();
                    StatusText.Text = "🗑️ Запись удалена";
                }
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


        /// <summary>
        /// Загрузка данных из XML файла
        /// </summary>
        private void LoadXmlData(string filePath)
        {
            _records.Clear();

            XDocument xmlDoc = XDocument.Load(filePath);
            XElement root = xmlDoc.Root;

            // Определяем структуру XML
            var firstElement = root.Elements().FirstOrDefault();

            if (firstElement != null && firstElement.Name == "item")
            {
                // Плоская структура: <item name="..." surname="..." amount="..." mount="..."/>
                _xmlStructureType = "flat";
                LoadFlatStructure(root);
            }
            else if (firstElement != null && MonthsList.Contains(firstElement.Name.LocalName.ToLower()))
            {
                // Группированная структура: <month><item .../></month>
                _xmlStructureType = "grouped";
                LoadGroupedStructure(root);
            }
            else
            {
                throw new Exception("Неизвестная структура XML файла");
            }

            UpdateStatistics();
        }

        /// <summary>
        /// Загрузка плоской структуры
        /// </summary>
        private void LoadFlatStructure(XElement root)
        {
            foreach (var item in root.Elements("item"))
            {
                string name = item.Attribute("name")?.Value ?? "Неизвестно";
                string surname = item.Attribute("surname")?.Value ?? "Неизвестно";
                string month = item.Attribute("mount")?.Value ?? "january";

                decimal amount = 0;
                string amountStr = item.Attribute("amount")?.Value ?? "0";
                // Поддержка разных форматов чисел (с запятой и точкой)
                amountStr = amountStr.Replace(',', '.');
                decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);

                _records.Add(new RecordModel(name, surname, amount, month));
            }
        }

        /// <summary>
        /// Загрузка группированной структуры
        /// </summary>
        private void LoadGroupedStructure(XElement root)
        {
            foreach (var monthElement in root.Elements())
            {
                string monthName = monthElement.Name.LocalName.ToLower();

                foreach (var item in monthElement.Elements("item"))
                {
                    string name = item.Attribute("name")?.Value ?? "Неизвестно";
                    string surname = item.Attribute("surname")?.Value ?? "Неизвестно";

                    decimal amount = 0;
                    string amountStr = item.Attribute("amount")?.Value ?? "0";
                    amountStr = amountStr.Replace(',', '.');
                    decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);

                    _records.Add(new RecordModel(name, surname, amount, monthName));
                }
            }
        }

        /// <summary>
        /// Сохранение данных в XML файл
        /// </summary>
        private void SaveXmlData(string filePath)
        {
            XElement root = new XElement("Pay");

            if (_xmlStructureType == "flat")
            {
                // Сохраняем в плоской структуре
                foreach (var record in _records)
                {
                    var item = new XElement("item");
                    item.SetAttributeValue("name", record.Name);
                    item.SetAttributeValue("surname", record.Surname);
                    item.SetAttributeValue("amount", record.Amount.ToString("0.00", CultureInfo.InvariantCulture));
                    item.SetAttributeValue("mount", record.Month);
                    root.Add(item);
                }
            }
            else if (_xmlStructureType == "grouped")
            {
                // Сохраняем в группированной структуре
                var grouped = _records.GroupBy(r => r.Month);

                foreach (var group in grouped)
                {
                    var monthElement = new XElement(group.Key.ToLower());

                    foreach (var record in group)
                    {
                        var item = new XElement("item");
                        item.SetAttributeValue("name", record.Name);
                        item.SetAttributeValue("surname", record.Surname);
                        item.SetAttributeValue("amount", record.Amount.ToString("0.00", CultureInfo.InvariantCulture));
                        item.SetAttributeValue("mount", record.Month);
                        monthElement.Add(item);
                    }

                    root.Add(monthElement);
                }
            }
            else
            {
                // По умолчанию сохраняем в плоской структуре
                foreach (var record in _records)
                {
                    var item = new XElement("item");
                    item.SetAttributeValue("name", record.Name);
                    item.SetAttributeValue("surname", record.Surname);
                    item.SetAttributeValue("amount", record.Amount.ToString("0.00", CultureInfo.InvariantCulture));
                    item.SetAttributeValue("mount", record.Month);
                    root.Add(item);
                }
            }

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                root
            );

            doc.Save(filePath);
        }

        /// <summary>
        /// Обновление статистики
        /// </summary>
        private void UpdateStatistics()
        {
            RecordsCountText.Text = $"Записей: {_records.Count}";
            decimal total = _records.Sum(r => r.Amount);
            TotalAmountText.Text = $"Общая сумма: {total:N2}";
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