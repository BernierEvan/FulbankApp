# Documentation : Services

Ce document fournit la documentation exhaustive pour les classes situées dans le dossier `Services` de l'application FulbankApp, contenant la logique métier, la persistance des données réseau, la sécurité locale et des services utilitaires (comme les collisions pour la partie graphique).

---

## 1. `CollisionService` (dans `CollisionService.cs`)
**Espace de nom :** `FulbankApp.Services`

Fournit la logique pour détecter les collisions entre le joueur (personnage animé) et les obstacles de l'interface, en prenant en compte les transformations géométriques.

### Méthodes
- **`IsCollidingWithTransformedRectangle(Rect playerRect, Rectangle obstacle)`** (`bool`) :
  Vérifie si la "*hitbox*" du joueur (`playerRect`) se superpose avec la géométrie transformée de l'obstacle.
- **`IsCollidingWithAny(Rect playerRect, IEnumerable<Rectangle> obstacles)`** (`bool`) :
  Vérifie rapidement si le joueur entre en collision avec au moins un élément d'une liste d'obstacles.
- **`CreateTransformedGeometry(Rectangle rectangle)`** (`RectangleGeometry` - *Privée*) :
  Génère le modèle 2D de l'obstacle intégrant ses éventuelles transformations (rotations).
- **`BuildTransformGroup(Rectangle rectangle)`** (`TransformGroup` - *Privée*) :
  Agrège les modifications appliquées sur l'obstacle dans la vue (translation, rotation, coordonnées d'origine) pour les appliquer au moteur de collision.
- **`GetTransformedCorners(Rectangle rectangle)`** (`Point[]`) :
  Retourne les coordonnées des 4 coins d'un polygone.
- **`IsPointInPolygon(Point point, Point[] polygon)`** (`bool`) :
  Algorithme *Ray casting* (lancer de rayon). Utilisé pour savoir si un point X,Y est géographiquement à l'intérieur d'un espace.
- **`DoLineSegmentsIntersect(Point p1, Point p2, Point p3, Point p4)`** (`bool`) :
  Calcul mathématique complexe vérifiant si 2 vecteurs se croisent. Étape clé de la détection de collision avancée.
- **`DoPolygonsIntersect(Point[] poly1, Point[] poly2)`** (`bool`) :
  Gère l'intersection totale entre deux formes.
- **`GetHitboxRect(Rect originalRect, double shrink)`** (`Rect`) :
  Réduit la zone de collision effective ("*hitbox*") d'une valeur donnée. Évite les collisions "pixel perfect" parfois trop punitives (améliore le gameplay / UX).

---

## 2. `LocalAuth` (dans `LocalAuth.cs`)
**Espace de nom :** `FulbankApp.Services`

Gère l'authentification locale pour stocker de façon persistante sur le poste client les données d'identification simples, via un fichier `auth.json`.

### Propriétés
- **`Username`** (`string`) : Nom d'utilisateur identifié.
- **`PinHash`** (`string`) : Signature cryptographique du code PIN de l'utilisateur (Sécurité : le PIN en clair ne doit jamais être stocké !).
- **`HasLoggedBefore`** (`bool`) : Indicateur pour afficher un écran d'accueil différent si l'utilisateur est déjà venu.

### Méthodes
- **`Save()`** : Sérialise l'instance courante en format JSON et l'écrit physiquement dans un fichier non chiffré (`auth.json`). *NB: Bien que le PIN soit haché, stocker le JSON en clair peut poser un risque physique mineur si l'OS n'est pas sécurisé*.
- **`Load()`** (`LocalAuth` - statique) : Désérialise et charge l'instance en mémoire. Retourne `null` si le fichier n'existe pas.

---

## 3. `SupabaseService` (dans `SupabaseService.cs`)
**Espace de nom :** `FulbankApp.Services`

Service critique (Architecture de type "*Singleton*"). Centralise toutes les communications Cloud/Backend sur l'infrastructure [Supabase](https://supabase.com/). Il encapsule les appels base de données réseau et la logique métier bancaire principale (transfert, historisation).

### Propriétés
- **`Instance`** (`SupabaseService` - *statique*) : Accès unique en Singleton.
- **`CurrentUser`** (`User`) : Utilisateur actuellement authentifié (Session active).
- **`IsAuthenticated`** (`bool`) : Retourne `true` si un utilisateur est connecté.

### Méthodes - Authentification
- **`InitializeAsync()`** (`Task<bool>`) : Initialise le client Supabase avec l'URL/Clé depuis les variables d'environnement.
- **`SignUpAsync(...)`** (`Task<(bool success, string error)>`) : Gère de bout-en-bout l'inscription de l'utilisateur : vérification Auth Supabase `->` Création de la ligne en BDD `Users` `->` Création de la configuration `UserSettings`. C'est une pseudo-transaction métier.
- **`SignInAsync(string username, string password)`** (`Task<(bool success, string error)>`) : Authentifie un utilisateur, mappe le compte Supabase Auth à l'entité de la base de données.
- **`SignOutAsync()`** (`Task`) : Détruit la session active en cours.

### Méthodes - Récupération des données (Lecture)
- **`GetBankAccountsAsync()`** (`Task<List<BankAccount>>`) : Récupère la liste des comptes bancaires de l'utilisateur connecté via un filtre de sécurité asynchrone (`x.UserId == CurrentUser.Id`).
- **`GetCryptoWalletsAsync()`** (`Task<List<CryptoWallet>>`) : Idem pour les cryptomonnaies.
- **`GetBeneficiariesAsync()`** (`Task<List<Beneficiary>>`) : Idem pour la liste des bénéficiaires autorisés pour faire un virement.
- **`GetTransactionsAsync(int limit = 50)`** (`Task<List<Transaction>>`) : Récupère et trie (du plus récent au plus ancien) l'historique de paiement. Limite par défaut à 50 par sécurité et performance (pagination métier implicite).
- **`GetSettingsAsync()`** (`Task<UserSettings>`) : Charge les préférences.

### Méthodes - Mutation des données bancaires (Écriture)
- **`AddBeneficiaryAsync(...)`** (`Task<bool>`) : Ajoute un IBAN de confiance. Vérifie que la requête ne vient que de l'utilisateur sessionnel.
- **`DeleteBeneficiaryAsync(Guid id)`** (`Task<bool>`) : Supprime un bénéficiaire. L'aspect sécurité `x.UserId == CurrentUser.Id` empêche la faille IDOR (supprimer le bénéficiaire d'un autre client).
- **`CreateTransferAsync(...)`** (`Task<bool>`) : **[Cœur d'opération bancaire]** 
  - Crée la trace en BDD (`Transaction`) indiquant un débit (`Amount = -amount`).
  - Décrémente la valeur `Balance` de l'entité `BankAccount`.
  - Effectue un `.Update(account)` pour figer ce solde. 
  *NB: Cette méthode lève de graves risques de concurrence réseau (Race Condition) car elle n'utilise pas de transactions SQL Supabase imbriquée ni de vérification préalable `if(account.Balance < amount) return false;`. C'est un axe d'amélioration critique de la logique métier.*
- **`UpdateSettingsAsync(...)`**, **`UpdateUserSkinAsync(...)`** : Remplacent ou mettent à jour les méta-données de profil. Maintiennent `UpdatedAt`.
