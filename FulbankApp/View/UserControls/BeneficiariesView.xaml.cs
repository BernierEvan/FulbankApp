using FulbankApp.Data;
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
    /// Logique d'interaction pour BeneficiariesView.xaml
    /// </summary>
    public partial class BeneficiariesView : UserControl
    {
        public BeneficiariesView()
        {
            InitializeComponent();
            this.DataContext = new BeneficiariesViewModel();
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
