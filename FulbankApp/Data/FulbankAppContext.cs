using Azure;
using FulbankApp.Models.SubscriptionModels;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using FulbankApp.Models;
using FulbankApp.Models.OperationModels;
using FulbankApp.Models.AccountModels;
using FulbankApp.Models.WalletModels;
using FulbankApp.Models.CryptoModels;
using FulbankApp.Models.CheckingAccountModels;
using FulbankApp.Models.SavingModels;
using FulbankApp.Models.WalletCryptoLinkModels;
using FulbankApp.Models.BeneficiariesModels;
using FulbankApp.Models.CardModels;
using FulbankApp.Models.OwnsWalletModels;
using FulbankApp.Models.CardCategoryModels;
using FulbankApp.Models.FiatModels;

namespace FulbankApp.Data
{
    public class FulbankAppContext : DbContext
    {
        // 1. On déclare les tables (DbSet)
        // Assure-toi d'avoir créé les classes Modèles que je t'ai données avant !
        public DbSet<UserClass> Users { get; set; }
        public DbSet<SubscriptionClass> Subscriptions { get; set; }
        public DbSet<AccountClass> Accounts { get; set; }
        public DbSet<BeneficiaryClass> Beneficiaries { get; set; }
        public DbSet<WalletCryptoLinkClass> WalletCryptoLinks { get; set; }
        public DbSet<CheckingAccountClass> CheckingAccounts { get; set; }
        public DbSet<SavingAccountClass> SavingAccounts { get; set; }
        public DbSet<OperationClass> Operations { get; set; }
        public DbSet<WalletClass> Wallets { get; set; }
        public DbSet<CryptoClass> Cryptos { get; set; }
        public DbSet<FiatClass> Fiats { get; set; }
        public DbSet<CardClass> Cards { get; set; }
        public DbSet<OwnsWalletClass> OwnsWallets { get; set; }
        public DbSet<CardCategory> CardCategories { get; set; }



        // 2. On configure la connexion
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // REMPLACE "TON_NOM_PC" par le nom de ton serveur SQL !
            // Tu peux le trouver dans la fenêtre de connexion de SQL Server Management Studio.
            // Si tu utilises LocalDB, c'est souvent : "(localdb)\\mssqllocaldb"

            string connectionString = @"Server=172.16.119.44;Database=FulbankApp;User Id=fulbank_admin;Password=rfeog,eùgroek123;Encrypt=False;";

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        // 3. Configuration avancée (pour les clés spéciales)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gestion de la clé primaire composite pour la table 'ownsWallet'
            modelBuilder.Entity<OwnsWalletClass>()
                .HasKey(ow => new { ow.IdUser, ow.IdWallet });

            // (Optionnel) Si tes noms de tables SQL ont des majuscules ou pluriels différents
            // EF Core est intelligent, mais parfois il faut préciser.
            // Les attributs [Table("...")] dans tes modèles font déjà ce travail.
        }
    }
}