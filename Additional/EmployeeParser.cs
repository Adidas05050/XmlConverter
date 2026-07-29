using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlTransformer;

namespace XmlConverter.Additional
{
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
}
