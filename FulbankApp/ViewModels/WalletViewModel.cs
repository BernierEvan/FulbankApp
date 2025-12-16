using FulbankApp.Helpers;
using FulbankApp.Models.WalletModels;
using FulbankApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace FulbankApp.ViewModels
{
    public class WalletViewModel : BaseViewModel
    {
        private readonly IWalletRepository _walletRepository;
        private readonly UserRepository _userRepository;

        public ObservableCollection<WalletClass> Wallets { get; } = new ObservableCollection<WalletClass>();

        private WalletClass _selectedWallet;
        public WalletClass SelectedWallet
        {
            get => _selectedWallet;
            set => SetProperty(ref _selectedWallet, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddWalletCommand { get; }
        public ICommand DeleteWalletCommand { get; }
    }
}

        /*public WalletViewModel() : this(new WalletRepository()) { }

        public WalletViewModel(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
            _userRepository = new UserRepository();

            RefreshCommand = new RelayCommand(LoadWallets);
            AddWalletCommand = new RelayCommand<object>(param => AddWallet(param?.ToString()));
            DeleteWalletCommand = new RelayCommand(RemoveSelectedWallet);

            // charger immédiatement
            LoadWallets();
        }

        private int GetCurrentUserId()
        {
            try
            {
                string username = Thread.CurrentPrincipal?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    username = Constants.CURRENT_USER_NAME;

                var user = _userRepository.GetByUsername(username);
                if (user == null) return 0;
                if (int.TryParse(user.Id, out int id)) return id;
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public void LoadWallets()
        {
            Wallets.Clear();
            int userId = GetCurrentUserId();
            if (userId == 0) return;

            var wallets = _walletRepository.GetWalletsByUserId(userId);
            foreach (var w in wallets)
                Wallets.Add(w);
        }

        private void AddWallet(string labelOrNull)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                MessageBox.Show("Utilisateur non identifié.");
                return;
            }

            string label = !string.IsNullOrWhiteSpace(labelOrNull) ? labelOrNull : $"Wallet {DateTime.Now:yyyyMMddHHmmss}";

            var model = new WalletClass { Label = label, Balance = 0m, UserId = userId };
            int newId = _walletRepository.Add(model);
            if (newId > 0)
            {
                model.Id = newId;
                Wallets.Add(model);
            }
            else
            {
                MessageBox.Show("Impossible de créer le wallet.");
            }
        }

        private void RemoveSelectedWallet()
        {
            if (SelectedWallet == null) return;

            var confirm = MessageBox.Show($"Supprimer le wallet '{SelectedWallet.Label}' ?", "Confirmation", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            bool ok = _walletRepository.Remove(SelectedWallet.Id);
            if (ok)
            {
                Wallets.Remove(SelectedWallet);
                SelectedWallet = null;
            }
            else
            {
                MessageBox.Show("Suppression impossible.");
            }
        }
    }
}
        */