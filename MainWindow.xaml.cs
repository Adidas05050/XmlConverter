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

                    List<string> monthNames = new List<string>
                        {
                            "January", "February", "March", "April", "May", "June",
                            "July", "August", "September", "October", "November", "December"
                        };
                    monthNames = monthNames.Select(e => e.ToLower()).ToList();

                    foreach (var salaryItem in empElement.Elements("salary"))
                    {
                        var month = salaryItem.Attribute("mount").Value.ToString().ToLower();
                        var monthIndex = monthNames.FindIndex(e => e == month);
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

    public partial class MainWindow : Window
    {
        // Коллекция для привязки к DataGrid
        private ObservableCollection<EmployeeData> _employeesCollection =
            new ObservableCollection<EmployeeData>();

        public MainWindow()
        {
            InitializeComponent();

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

                // Обновляем информационные поля
                UpdateStatistics(employees);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка обновления GUI: {ex.Message}";
            }
        }

        /// <summary>
        /// Обновляет статистику (количество сотрудников, общая сумма)
        /// </summary>
        private void UpdateStatistics(List<EmployeeData> employees)
        {
            if (employees == null || employees.Count == 0)
            {
                EmployeesCountText.Text = "Сотрудников: 0";
                TotalSumText.Text = "Общая сумма: 0";
                return;
            }

            // Количество сотрудников
            EmployeesCountText.Text = $"Сотрудников: {employees.Count}";

            // Общая сумма по всем сотрудникам за все месяцы
            double totalAll = employees.Sum(e => e.TotalSum);
            TotalSumText.Text = $"Общая сумма: {totalAll:N2}";
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