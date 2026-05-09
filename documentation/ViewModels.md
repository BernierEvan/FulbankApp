# Documentation : ViewModels

Ce document fournit la documentation pour le système de `ViewModels` de l'application FulbankApp, respectant le Modèle-Vue-VueModèle (MVVM).

---

## Architecture MVVM
Les ViewModels de ce dossier constituent la passerelle entre les données du système (`Models`, `Services`) et l'interface utilisateur XAML (`Views`). 
Leur rôle principal est d'isoler toute la logique présentationnelle afin que les Vues "CodeBehind" restent les plus vides possible.

### `BaseViewModel`
Tous les ViewModels héritent de cette classe (ou implémentent `INotifyPropertyChanged`). 
Elle fournit la méthode `OnPropertyChanged(string propertyName)` qui permet à WPF d'être informé dynamiquement du changement interne d'une variable métier et de rafraîchir l'interface (DataBinding) de manière automatique, sans besoin de rafraîchissement manuel de la page.

### `ViewModelCommand`
Une possible variante alias du `RelayCommand` définie pour le système spécifique de navigation des pages ou les entrées/sorties locales au namespace courant.

---

## Vue d'ensemble des ViewModels par Écrans

1. **`MainViewModel.cs`** : Moteur de navigation principal de l'application (Sidebar/Header). Gère l'encapsulation de la vue active et assure les commandes pour changer de sous-vue au clic (Accueil, Comptes, etc.).
2. **`LoginViewModel.cs`** : Gère la logique de saisie de formulaires d'authentification, les appels à `SupabaseService.SignInAsync`, et les indicateurs de chargement ou les erreurs rouges UI.
3. **`HomeViewModel.cs`** : Synthétise le tableau de bord (Vue d'ensemble des soldes agrégés, historique partiel limité des transactions).
4. **`BankAccountViewModel.cs`** : Permet au client de visualiser en détail le graphe ou le relevé unique d'un compte courant ou épargne.
5. **`BeneficiariesViewModel.cs`** : Gère l'ajout, la validation d'IBAN et la suppression réseau de `Beneficiary` via `Command` tout en mettant à jour la liste affichée dans la GUI.`
6. **`TransferViewModel.cs`** : **[Cœur des opérations bancaires]**.
   - Gère un formulaire composé de (Compte débité + Bénéficiaire cible + Montant + Motivation).
   - Intégre la vérification instantanée (`CanExecute` métier) : Vérification que `Montant > 0`, et que `Montant < Solde disponible`.
7. **`ConvertViewModel.cs`** : Gère l'interface de conversion (taux de change, achat/vente crypto vs monnaie réelle).
8. **`WalletViewModel.cs`** : Résumé strict du portefeuille d'investissement Crypto / Trading.
9. **`SettingsViewModel.cs`** : Commandes liées aux flags utilisateurs dans `UserSettings`, changements de skin ou alertes de mot de passe à deux facteurs.

---

## Interactions Modèles Bancaires / ViewModels

Chaque ViewModel intègre des propriétés observables (`ObservableCollection<T>`) pour lister sans conflit multi-threading des objets tels que :
```csharp
public ObservableCollection<Transaction> TransactionsList { get; set; }
```
Une mise à jour depuis un bloc asynchrone (`Task`) doit souvent passer par le répartiteur UI (WPF Dispatcher) afin de prévenir les plantages liés à des écritures d'interfaces sur des processus non prioritaires.

