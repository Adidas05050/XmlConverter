using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlConverter.Additional
{
    /// <summary>
    /// Класс для хранения информации о сотруднике
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
}
