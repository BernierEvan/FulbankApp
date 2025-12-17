using FulbankApp.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FulbankApp.ViewModels
{
    public enum NavigationPresentation
    {
        GlobalLoading,
        Skeleton
    }

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
            NavigateCommand = new RelayCommand<string>(async (p) => await Navigate(p, NavigationPresentation.GlobalLoading));
        }

        public async Task Navigate(string pageKey, NavigationPresentation presentation = NavigationPresentation.GlobalLoading)
        {
            if (string.IsNullOrWhiteSpace(pageKey)) return;

            bool useGlobalLoader = presentation == NavigationPresentation.GlobalLoading;

            if (useGlobalLoader)
            {
                IsLoading = true;
                await Task.Delay(100); // Laisser l'UI afficher le loader
            }

            try
            {
                // Task.Run<object> pour éviter les erreurs de type du compilateur
                object nextView = await Task.Run<object>(() =>
                {
                    // Simulation de chargement
                    System.Threading.Thread.Sleep(1000);

                    return pageKey switch
                    {
                        "Home" => new HomeViewModel(),
                        "Wallet" => new WalletViewModel(),
                        "BankAccounts" => new BankAccountViewModel(),
                        "Transfer" => new TransferViewModel(),
                        "Convert" => new ConvertViewModel(),
                        "Beneficiaries" => new BeneficiariesViewModel(),
                        "Settings" => new SettingsViewModel(),
                        "Login" => new LoginViewModel(),
                        _ => null
                    };
                });

                // Retour sur le thread UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (nextView != null)
                    {
                        CurrentViewModel = nextView;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}");
            }
            finally
            {
                if (useGlobalLoader)
                {
                    await Task.Delay(200);
                    IsLoading = false;
                }
            }
        }
    }
}