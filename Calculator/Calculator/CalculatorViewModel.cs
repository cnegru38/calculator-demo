using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Calculator
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private double firstNumber = 0;
        private double secondNumber = 0;
        private string operation = "";
        private bool isNewEntry = false;
        private bool digitGroupingEnabled = false;
        private Stack<double> memoryStack = new Stack<double>();
        private string clipboardMemory = "";
        public ObservableCollection<string> MemoryContents { get; } = new ObservableCollection<string>();

        private readonly CalculatorData _calculatorData = new CalculatorData();

        public string Result
        {
            get => _calculatorData.Result;
            set
            {
                _calculatorData.Result = value;
                OnPropertyChanged(nameof(Result)); // Notify UI of changes
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Command getters
        public ICommand DigitCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand ReciprocalCommand { get; }
        public ICommand SquareCommand { get; }
        public ICommand SquareRootCommand { get; }
        public ICommand PercentCommand { get; }
        public ICommand DecimalCommand { get; }
        public ICommand NegativeCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand MemoryStoreCommand { get; }
        public ICommand MemoryRecallCommand { get; }
        public ICommand MemoryClearCommand { get; }
        public ICommand MemoryAddCommand { get; }
        public ICommand MemorySubtractCommand { get; }
        public ICommand MemoryViewCommand { get; }
        public ICommand ConvertToHexCommand { get; }
        public ICommand ConvertToDecCommand { get; }
        public ICommand ConvertToOctCommand { get; }
        public ICommand ConvertToBinCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand DigitGroupingCommand { get; }
        public ICommand AboutCommand { get; }

        #endregion
        public CalculatorViewModel()
        {
            #region Command setters

            DigitCommand = new RelayCommand<string>(AppendDigit);
            OperationCommand = new RelayCommand<string>(StoreOperation);
            EqualsCommand = new RelayCommand(PerformEquals);
            ClearCommand = new RelayCommand(Clear);
            ClearEntryCommand = new RelayCommand(ClearEntry);
            ReciprocalCommand = new RelayCommand(PerformReciprocal);
            SquareCommand = new RelayCommand(PerformSquare);
            SquareRootCommand = new RelayCommand(PerformSquareRoot);
            PercentCommand = new RelayCommand(PerformPercentage);
            DecimalCommand = new RelayCommand(AppendDecimal);
            NegativeCommand = new RelayCommand(ToggleNegative);
            BackspaceCommand = new RelayCommand(Backspace);
            MemoryStoreCommand = new RelayCommand(MemoryStore);
            MemoryRecallCommand = new RelayCommand(MemoryRecall);
            MemoryClearCommand = new RelayCommand(MemoryClear);
            MemoryAddCommand = new RelayCommand(MemoryAdd);
            MemorySubtractCommand = new RelayCommand(MemorySubtract);
            MemoryViewCommand = new RelayCommand(MemoryView);
            ConvertToHexCommand = new RelayCommand(ConvertToHex);
            ConvertToDecCommand = new RelayCommand(ConvertToDec);
            ConvertToOctCommand = new RelayCommand(ConvertToOct);
            ConvertToBinCommand = new RelayCommand(ConvertToBin);
            CutCommand = new RelayCommand(Cut);
            CopyCommand = new RelayCommand(Copy);
            PasteCommand = new RelayCommand(Paste);
            DigitGroupingCommand = new RelayCommand(ToggleDigitGrouping);
            AboutCommand = new RelayCommand(About);

            #endregion
        }

        #region Digit functions
        private void AppendDigit(string digit)
        {
            if (isNewEntry)
            {
                Result = digit;
                isNewEntry = false;
            }
            else if (Result.Length < 14)
            {
                Result = Result == "0" ? digit : Result + digit;
            }
        }

        #endregion

        #region Basic operations
        private void StoreOperation(string op)
        {
            if (double.TryParse(Result, out double currentNumber))
            {
                if (!string.IsNullOrEmpty(operation) && !isNewEntry)
                {
                    // Perform the previous operation first
                    secondNumber = currentNumber;
                    double result = PerformOperation();
                    Result = FormatResult(result);
                    firstNumber = result;
                }
                else
                {
                    firstNumber = currentNumber;
                }

                operation = op;
                isNewEntry = true;
            }
        }

        private void PerformEquals()
        {
            if (isNewEntry)
            {
                firstNumber = double.Parse(Result);
            }
            else if (double.TryParse(Result, out secondNumber))
            {
                isNewEntry = true;
            }

            double result = PerformOperation();
            Result = result.ToString();

            firstNumber = result;
        }

        private double PerformOperation()
        {
            double result = operation switch
            {
                "+" => firstNumber + secondNumber,
                "-" => firstNumber - secondNumber,
                "*" => firstNumber * secondNumber,
                "/" => secondNumber != 0 ? firstNumber / secondNumber : double.NaN,
                _ => firstNumber,
            };

            // Convert result to string to check length
            string resultStr = result.ToString();

            // If result is too long, round it appropriately
            if (resultStr.Length > 14)
            {
                // Try rounding to a reasonable number of decimal places
                int decimalPlaces = Math.Max(0, 14 - resultStr.Split('.')[0].Length - 1);
                result = Math.Round(result, decimalPlaces);
            }

            return result;
        }

        #endregion

        #region Complex operations
        private void PerformReciprocal()
        {
            if (double.TryParse(Result, out double value) && value != 0)
            {
                Result = FormatResult(1 / value);
            }
            else
            {
                Result = "NaN";
            }
        }

        private void PerformSquare()
        {
            if (double.TryParse(Result, out double value))
            {
                Result = FormatResult(value * value);
            }
        }

        private void PerformSquareRoot()
        {
            if (double.TryParse(Result, out double value) && value >= 0)
            {
                Result = FormatResult(Math.Sqrt(value));
            }
            else
            {
                Result = "NaN";
            }
        }

        private void PerformPercentage()
        {
            if (double.TryParse(Result, out double value))
            {
                // If an operation is in progress, calculate percentage relative to firstNumber
                if (!string.IsNullOrEmpty(operation))
                {
                    value = firstNumber * value / 100.0;
                }
                else
                {
                    value = value / 100.0;
                }

                Result = FormatResult(value);
                isNewEntry = true;
            }
        }

        #endregion

        #region Decimal functionality
        private void AppendDecimal()
        {
            if (!Result.Contains("."))
            {
                Result += ".";
            }
        }

        #endregion

        #region Negative functionality

        private void ToggleNegative()
        {
            if (Result != "0")
            {
                Result = Result.StartsWith("-") ? Result.Substring(1) : "-" + Result;
            }
        }

        #endregion

        #region Clearing functions
        private void Backspace()
        {
            Result = Result.Length > 1 ? Result.Substring(0, Result.Length - 1) : "0";
        }

        private void Clear()
        {
            Result = "0";
            firstNumber = 0;
            secondNumber = 0;
            operation = "";
            isNewEntry = false;
        }
        private void ClearEntry()
        {
            Result = "0";
            isNewEntry = true;
        }

        #endregion

        #region Memory functions
        private void MemoryStore()
        {
            if (double.TryParse(Result, out double value))
            {
                memoryStack.Push(value);
            }
        }

        private void MemoryRecall()
        {
            if (memoryStack.Count > 0)
            {
                Result = memoryStack.Peek().ToString();
                isNewEntry = true;
            }
        }

        private void MemoryClear()
        {
            memoryStack.Clear();
        }

        private void MemoryAdd()
        {
            if (double.TryParse(Result, out double value))
            {
                memoryStack.Push(memoryStack.Count > 0 ? memoryStack.Pop() + value : value);
            }
        }

        private void MemorySubtract()
        {
            if (double.TryParse(Result, out double value))
            {
                memoryStack.Push(memoryStack.Count > 0 ? memoryStack.Pop() - value : -value);
            }
        }

        private void MemoryView()
        {
            if (memoryStack.Count > 0)
            {
                StringBuilder message = new StringBuilder("Memory Stack:\n");

                foreach (var value in memoryStack)
                {
                    message.AppendLine(value.ToString());
                }

                MessageBox.Show(message.ToString(), "Memory View");
            }
            else
            {
                MessageBox.Show("Memory is empty", "Memory View");
            }
        }

        #endregion

        #region Conversion functions
        private void ConvertToHex()
        {
            if (int.TryParse(Result, out int value))
            {
                Result = value.ToString("X"); // Convert to hexadecimal
            }
        }

        private void ConvertToDec()
        {
            if (int.TryParse(Result, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
            {
                Result = value.ToString(); // Convert hexadecimal to decimal
            }
        }

        private void ConvertToOct()
        {
            if (int.TryParse(Result, out int value))
            {
                Result = Convert.ToString(value, 8); // Convert to octal
            }
        }

        private void ConvertToBin()
        {
            if (int.TryParse(Result, out int value))
            {
                Result = Convert.ToString(value, 2); // Convert to binary
            }
        }

        #endregion

        #region Tools

        #region Clipboard memory functions
        private void Cut()
        {
            clipboardMemory = Result;
            Result = "0";
            isNewEntry = true;
        }

        private void Copy()
        {
            clipboardMemory = Result;
        }

        private void Paste()
        {
            if (!string.IsNullOrEmpty(clipboardMemory))
            {
                Result = clipboardMemory;
                isNewEntry = true;
            }
            else
            {
                MessageBox.Show("Clipboard memory is empty.", "Paste Error");
            }
        }

        #endregion

        #region Digit grouping
        private void ToggleDigitGrouping()
        {
            digitGroupingEnabled = !digitGroupingEnabled;
            if (double.TryParse(Result, out double value))
            {
                Result = digitGroupingEnabled ? value.ToString("N0", CultureInfo.InvariantCulture) : value.ToString();
            }
        }

        private string FormatResult(double result)
        {
            string resultStr = result.ToString();
            if (resultStr.Length > 14)
            {
                int decimalPlaces = Math.Max(0, 14 - resultStr.Split('.')[0].Length - 1);
                result = Math.Round(result, decimalPlaces);
                resultStr = result.ToString();
                if (resultStr.Length > 14)
                {
                    resultStr = result.ToString("E10");
                }
            }
            return resultStr;
        }

        #endregion

        #region About
        private void About()
        {
            MessageBox.Show("Name: Cosmin-Stefan Negru\nhttps://github.com/cnegru38", "About");
        }

        #endregion

        #endregion
    }
}
