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
            this.DataContext = new WalletViewModel();
            Opacity = 1.0;
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main == null) return;

            var btn = sender as Button;
            string key = btn.Tag.ToString();

            switch (key)
            {
                case "Home":
                    main.Content = new HomeView();
                    break;
                default:
                    break;
            }
        }

    }
}
