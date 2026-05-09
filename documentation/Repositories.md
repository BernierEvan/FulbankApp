# Documentation : Repositories

Ce document fournit la documentation exhaustive pour les classes situées dans le dossier `Repositories` de l'application FulbankApp. L'architecture s'appuie sur le *Repository Pattern* pour s'abstraire de la couche d'accès direct à la base de données relationnelle (`SQL Server`). 
*Note de sécurité métier : Les flux réseaux bas niveaux semblent reposer sur SQLClient en parallèle de Supabase, indiquant possiblement une double utilisation (Legacy & Moderne).*

---

## 1. `RepositoryBase` (dans `RepositoryBase.cs`)
**Espace de nom :** `FulbankApp.Repositories`

Classe abstraite de base qui gère exclusivement la connexion brute au serveur SQL.

### Propriétés
- **`_connectionString`** (`string`, *privée, lecture seule*) : Chaîne de connexion codée en dur (IP `172.16.119.44`, database `testDB`, user `fulbank_user`). *Attention métier: Les identifiants sont exposés en clair dans le code, ce qui constitue une dette technique de sécurité à externaliser (Variables d'environnement / Secret Manager).*

### Méthodes
- **`GetConnection()`** (`SqlConnection`, *protégée*) : Instancie et retourne une connexion `SqlConnection` vers la base de données. Elle ne l'ouvre pas : c'est au repository enfant de l'ouvrir d'utiliser le pattern `using (...)` pour assurer sa fermeture en cas d'exception.

---

## 2. `UserRepository` (dans `UserRepository.cs`)
**Espace de nom :** `FulbankApp.Repositories`

Gère les opérations (CRUD) pour les entités métier `UserModel`. Implémente l'interface `IuserRepository`.

### Méthodes - Authentification & Sécurité
- **`AuthenticateUser(NetworkCredential credential)`** (`bool`) :
  Vérifie dans la BDD que l'utilisateur existe et que le mot de passe correspond.
  **Logique Métier Bancaire - Étape de vérification :**
  1. Récupère de la table `[User]` la colonne `[password]` (qui est en réalité le hash crypto, et non le texte clair) ciblée pour l'utilisateur fourni (via `ExecuteScalar()`).
  2. Hash le mot de passe utilisateur saisi (`credential.Password`).
  3. Compare en mémoire stricte si les deux hash sont rigoureusement identiques. S'ils sont égaux, la vérification logique et sémantique autorise le login.
  4. La sécurité prévient les injections SQL grâce au paramètre `@username` (via `SqlParameter`).
  
- **`HashPassword(string password)`** (`string`, *privée*) :
  Produit la signature cryptographique SHA-256 du mot de passe.
  *Note Inline de développement : L'algorithme SHA256 n'est plus suffisant pour le stockage bancaire car soumis aux attaques par force brute / rainbow tables. La recommandation inline indique une transition vers `bcrypt` ou `Argon2`.*
  **Retour:** Renvoie le hash d'octets traduit en chaîne hexadécimale (`x2`).

### Méthodes - Récupération de Données
- **`GetByUsername(string username)`** (`UserModel`) :
  Récupère les informations complètes d'un `UserModel` à partir de l'identifiant de connexion.
  **Logique Métier Bancaire - Anonymisation :** Lors du mapping `SqlDataReader` vers l'objet C#, la propriété `Password` est délibérément vidée (`string.Empty`). Il est vital sur le plan bancaire et réglementaire que les credentials circulent le moins possible en RAM ou en sérialisation (protection des données internes).

### Méthodes - Implémentations en attente (`NotImplementedException`)
Ce repository définit le contrat `IuserRepository`, mais certaines opérations restent explicitement non implémentées et lèveront une exception en l'état :
- `Add(UserModel userModel)`
- `Edit(UserModel userModel)`
- `GetById(int id)`
- `GetByAll()`
- `Remove(int id)`
Ces composants font probablement l'objet d'une migration logicielle vers Supabase (les opérations d'écriture de création de comptes étant déjà implémentées dans `SupabaseService.cs`).
