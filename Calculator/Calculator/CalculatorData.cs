using System;
using System.ComponentModel;

namespace Calculator
{
    class CalculatorData : INotifyPropertyChanged
    {
        private string result;

        public CalculatorData()
        {
            result = "0";
        }

        public string Result
        {
            get => result;
            set
            {
                if (value.Length > 14)
                    return;

                if (result != value)
                {
                    result = value;
                    NotifyPropertyChanged(nameof(Result));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
