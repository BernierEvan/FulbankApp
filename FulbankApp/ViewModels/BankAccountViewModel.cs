using System.Collections.ObjectModel;
using System.ComponentModel; // Nécessaire pour INotifyPropertyChanged
using System.Linq;
using System.Runtime.CompilerServices; // Nécessaire pour CallerMemberName
using FulbankApp.Data;
using FulbankApp.Models.AccountModels;
using FulbankApp.Models.OperationModels;

namespace FulbankApp.ViewModels
{
    // 1. On ajoute l'interface ici
    public class BankAccountViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<OperationClass> Operations { get; set; }

        // 2. On crée un champ privé pour le compte
        private AccountClass _selectedAccount;

        // 3. On crée la propriété publique avec la notification
        public AccountClass SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                OnPropertyChanged(); // <--- C'est ça qui dit au XAML : "Mets-toi à jour !"
            }
        }

        public BankAccountViewModel()
        {
            Operations = new ObservableCollection<OperationClass>();
            LoadData();
        }

        private void LoadData()
        {
            using (var context = new FulbankAppContext())
            {
                try
                {
                    // Charge le premier compte trouvé
                    var currentAccount = context.Accounts.FirstOrDefault();

                    // Cette ligne va maintenant déclencher la mise à jour de l'interface
                    SelectedAccount = currentAccount;

                    // Si on a trouvé un compte, on charge ses opérations (ou toutes, selon ton besoin)
                    if (currentAccount != null)
                    {
                        var list = context.Operations.ToList();
                        foreach (var operation in list)
                        {
                            Operations.Add(operation);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur : {ex.Message}");
                }
            }
        }

        // 4. Implémentation standard de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}