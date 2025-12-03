1. Logiciels utilisés / nécessaires

  Outil principal de développement:

  - Visual Studio 2026
  - Langage utilisé : C# (.NET 10).
  - Framework : WPF (Windows Presentation Foundation).
  - Modèle d’architecture : MVVM.
  - SQL Server Express 2022
  - SQL Server Management Studio (SSMS)
  - Git / GitHub

  Extensions (Nuggets) :
  - Microsoft.Data.SqlClient
  - WpfAnimatedGif

Comment implémenter la base de données sur un autre SQL Server

  - Installer SQL Server Express
  - Se connecter à l’instance :
      localhost\SQLEXPRESS
  - Cliquer sur Nouvelle requête
  - Récupérer le fichier script.sql (Github)
  - Ouvrir le script script.sql
  - Exécuter

Mettre à jour la chaîne de connexion

Dans App.config du projet :

<connectionStrings>
  <add name="FulbankDB"
       connectionString="Server=localhost\SQLEXPRESS;Database=FulbankDB;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
