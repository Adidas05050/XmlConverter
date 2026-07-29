using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlConverter.Additional
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
}
