using FulbankApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FulbankApp.View
{
    /// <summary>
    /// Logique d'interaction pour WalletView.xaml
    /// </summary>
    public partial class WalletView : UserControl 
    {
        public WalletView()
        {
            InitializeComponent();
            Opacity = 1.0;
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
