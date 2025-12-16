using FulbankApp.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FulbankApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            // Initialisation
            CurrentViewModel = new HomeViewModel();
            NavigateCommand = new RelayCommand<string>(async (p) => await Navigate(p));
        }

        public async Task Navigate(string pageKey)
        {
            if (string.IsNullOrEmpty(pageKey)) return;

            IsLoading = true;
            await Task.Delay(100); // Laisser l'UI afficher le loader

            try
            {
                // Task.Run<object> pour éviter les erreurs de type du compilateur
                object nextView = await Task.Run<object>(() =>
                {
                    // Simulation de chargement
                    System.Threading.Thread.Sleep(1000);

                    switch (pageKey)
                    {
                        case "Home": return new HomeViewModel();
                        case "Wallet": return new WalletViewModel();
                        case "BankAccounts": return new BankAccountViewModel();
                        case "Transfer": return new TransferViewModel();
                        case "Convert": return new ConvertViewModel();
                        case "Beneficiaries": return new BeneficiariesViewModel();
                        case "Settings": return new SettingsViewModel();
                        case "Login": return new LoginViewModel();
                        default: return null;
                    }
                });

                // Retour sur le thread UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (nextView != null) CurrentViewModel = nextView;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}");
            }
            finally
            {
                await Task.Delay(200);
                IsLoading = false;
            }
        }
    }
}