using FulbankApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FulbankApp.View
{
    /// <summary>
    /// Logique d'interaction pour TransferView.xaml
    /// </summary>
    public partial class TransferView : UserControl
    {
        public TransferView()
        {
            InitializeComponent();
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow?.DataContext is not MainViewModel mainViewModel)
            {
                return;
            }

            if (sender is not Button button)
            {
                return;
            }

            var destination = button.Tag as string;
            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }

            if (mainViewModel.NavigateCommand.CanExecute(destination))
            {
                mainViewModel.NavigateCommand.Execute(destination);
            }
        }
    }
}
