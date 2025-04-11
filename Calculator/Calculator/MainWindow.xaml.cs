using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new CalculatorViewModel();

            string iconRelativePath = @"Images\Icon.png";
            string iconFullPath = Path.GetFullPath(iconRelativePath);
            this.Icon = BitmapFrame.Create(new Uri(iconFullPath, UriKind.Absolute));
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
