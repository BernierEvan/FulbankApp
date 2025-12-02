using FulbankApp.Helpers;
using FulbankApp.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
// Supprimez les using INotifyPropertyChanged si vous les aviez
// ...

namespace FulbankApp.ViewModels
{
    // ➡️ HÉRITE DE BASEVIEWMODEL 
    // (qui contient maintenant tout le code de INotifyPropertyChanged)
    public class MainViewModel : BaseViewModel
    {
        #region Navigation

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                // ➡️ Utilisation de SetProperty hérité
                SetProperty(ref _currentViewModel, value);
            }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            // Initialiser la première vue (ex: Login)
            CurrentViewModel = new LoginViewModel();

            // La commande de navigation est ici
            NavigateCommand = new RelayCommand<string>(ExecuteNavigation);
        }


        private void ExecuteNavigation(object parameter)
        {
            string viewName = parameter?.ToString();

            switch (viewName)
            {
                case "BankAccounts":
                    CurrentViewModel = new BankAccountViewModel();
                    break;
                case "Wallet":
                    CurrentViewModel = new WalletViewModel();
                    break;
                case "Transfer":
                    CurrentViewModel = new TransferViewModel();
                    break;
                case "Convert":
                    CurrentViewModel = new ConvertViewModel();
                    break;
                case "Beneficiaries":
                    CurrentViewModel = new BeneficiariesViewModel();
                    break;
                case "Settings":
                    CurrentViewModel = new SettingsViewModel();
                    break;
                // Ajoutez d'autres cas pour Settings, etc.
                default:
                    // Gérer les cas par défaut ou les erreurs
                    break;
            }
        }

        #endregion


    }
}