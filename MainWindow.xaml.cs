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
    /// Структура для хранения информации о сотруднике
    /// </summary>
    public class EmployeeData : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;
        private double[] _months = new double[12];
        private double _totalSum;

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }

        public double[] Months
        {
            get => _months;
            set
            {
                if (_months != value)
                {
                    _months = value;
                    OnPropertyChanged(nameof(Months));
                    CalculateTotal();
                }
            }
        }

        public double TotalSum
        {
            get => _totalSum;
            private set
            {
                if (_totalSum != value)
                {
                    _totalSum = value;
                    OnPropertyChanged(nameof(TotalSum));
                }
            }
        }

        private void CalculateTotal()
        {
            if (_months != null)
            {
                double sum = 0;
                for (int i = 0; i < 12; i++)
                {
                    sum += _months[i];
                }
                TotalSum = sum;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeeData()
        {
            // Инициализация массива нулями
            _months = new double[12];
        }

        public EmployeeData(string firstName, string lastName) : this()
        {
            FirstName = firstName;
            LastName = lastName;
        }

        /// <summary>
        /// Добавить сумму за конкретный месяц (0-11)
        /// </summary>
        public void AddMonthSum(int monthIndex, double value)
        {
            if (monthIndex >= 0 && monthIndex < 12)
            {
                _months[monthIndex] += value;
                OnPropertyChanged(nameof(Months));
                CalculateTotal();
            }
        }

        /// <summary>
        /// Получить сумму за конкретный месяц (0-11)
        /// </summary>
        public double GetMonthSum(int monthIndex)
        {
            if (monthIndex >= 0 && monthIndex < 12)
                return _months[monthIndex];
            return 0;
        }
    }

    public static class EmployeeParser
    {
        /// <summary>
        /// Парсит XML и заполняет список сотрудников
        /// </summary>
        /// <param name="rootElement">Корневой элемент XML</param>
        /// <returns>Список сотрудников с данными по месяцам</returns>
        public static List<EmployeeData> ParseEmployeesFromXml(XElement rootElement)
        {
            var employees = new List<EmployeeData>();

            try
            {
                // Предполагаем структуру: root/Employees/Employee
                // Или root/Employee - в зависимости от вашего XML
                var employeeElements = rootElement.Descendants("Employee");

                foreach (var empElement in employeeElements)
                {
                    // Получаем имя и фамилию
                    string firstName = empElement.Attribute("name")?.Value ??
                                      "Неизвестно";

                    string lastName = empElement.Attribute("surname")?.Value ??
                                     "Неизвестно";

                    var employee = new EmployeeData(firstName, lastName);

                    foreach (var salaryItem in empElement.Elements("salary"))
                    {
                        var month = salaryItem.Attribute("mount").Value.ToString().ToLower();
                        var monthIndex = MainWindow.MonthsList.FindIndex(e => e == month);
                        if (monthIndex < 0)
                            continue;
                        var amount = Math.Round(float.Parse(salaryItem.Attribute("amount").Value.ToString().Replace('.', ',')), 2);
                        employee.AddMonthSum(monthIndex, amount);
                    }

                    employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при парсинге XML: {ex.Message}");
            }

            return employees;
        }
    }

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

        // Коллекция для привязки к DataGrid
        private ObservableCollection<EmployeeData> _employeesCollection =
            new ObservableCollection<EmployeeData>();

        // Список месяцев для ComboBox
        static public List<string> MonthsList { get; } = new List<string>
        {
            "january", "february", "march", "april", "may", "june",
            "july", "august", "september", "october", "november", "december"
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            RecordsDataGrid.ItemsSource = _records;

            // Привязываем коллекцию к DataGrid
            EmployeesDataGrid.ItemsSource = _employeesCollection;

            // Включаем сортировку для всех колонок
            EmployeesDataGrid.CanUserSortColumns = true;
        }
        /// <summary>
        /// Метод для обновления данных в GUI из списка сотрудников
        /// </summary>
        /// <param name="employees">Список сотрудников</param>
        private void UpdateGuiFromEmployees(List<EmployeeData> employees)
        {
            try
            {
                // Очищаем текущую коллекцию
                _employeesCollection.Clear();

                // Добавляем всех сотрудников
                foreach (var emp in employees)
                {
                    _employeesCollection.Add(emp);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка обновления GUI: {ex.Message}";
            }
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
                    $"Employees.xml"
                );

                PerformXsltTransformation(xmlFilePath, xslFilePath, outputFilePath);

                // Загружаем XML
                XDocument xmlDoc = XDocument.Load(outputFilePath);

                // Парсим сотрудников из XML
                List<EmployeeData> employees = EmployeeParser.ParseEmployeesFromXml(xmlDoc.Root);

                if (employees == null || employees.Count == 0)
                {
                    StatusText.Text = "⚠️ Внимание: Сотрудники не найдены в XML файле!";
                    return;
                }

                // Обновляем GUI
                UpdateGuiFromEmployees(employees);

                AddAllAmount(xmlFilePath);
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
                AddAmountForEmployee(outputPath);
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

        private void AddAllAmount(string xmlPath)
        {
            try
            {
                var xml = XDocument.Load(xmlPath);
                var root = xml.Root;
                SummaryAmount(root, "item");
                root.Save(xmlPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка : {ex.Message}");
            }
        }

        private void AddAmountForEmployee(string xmlPath)
        {
            try
            {
                var xml = XDocument.Load(xmlPath);
                var root = xml.Root;
                foreach (var employee in root.Elements("Employee"))
                    SummaryAmount(employee, "salary");
                root.Save(xmlPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка : {ex.Message}");
            }
        }

        private void SummaryAmount(XElement item, string elementName)
        {
            var result = 0.0;
            foreach (var attr in item.Elements(elementName).Attributes("amount"))
                result += Math.Round(float.Parse(attr.Value.Replace('.', ',')), 2);
            item.Attribute("amount")?.Remove();
            item.Add(new XAttribute("amount", result));
        }
    }
}