# Documentation : Models

Ce document fournit la documentation exhaustive pour les classes situées dans le dossier `Models` de l'application FulbankApp, qui représentent les données et entités métier du système bancaire.

---

## 1. `AnimatedCharacter` (dans `AnimatedCharacter.cs`)
**Espace de nom :** `FulbankApp.Models`

Représente un personnage animé pouvant se déplacer dans l'interface, avec la gestion des ressources graphiques (GIFs pour le mouvement, images statiques pour l'inactivité).

### Propriétés
- **`CurrentDirection`** (`Direction`) : Direction actuelle du personnage (lecture seule).
- **`Speed`** (`double`) : Vitesse de déplacement en pixels par seconde.
- **`Position`** (`Point`) : Position actuelle du personnage sur le canevas (lecture et écriture).
- **`Width`** (`double`) : Largeur du personnage.
- **`Height`** (`double`) : Hauteur du personnage.

### Méthodes
- **`AnimatedCharacter(...)`** : Constructeur initialisant les dimensions et la position, puis l'attachant au canevas.
- **`LoadGifs(string skinName)`** : Charge les ressources graphiques (Assets) pour toutes les directions.
- **`SetDirection(Direction direction)`** : Change la direction et active l'animation de marche.
- **`SetIdle(Direction direction)`** : Active l'image statique lorsque le personnage ne bouge pas.
- **`Move(Direction direction, double deltaTime)`** : Déplace le personnage de façon logique, normalisant la vitesse pour les déplacements diagonaux.
- **`RemoveFromCanvas()`** : Retire proprement le contrôle Image du parent.
- **`Dispose()`** : Libère les ressources (implémente IDisposable).

---

## 2. `BankAccount` (dans `BankAccount.cs`)
**Espace de nom :** `FulbankApp.Models`

Représente un compte bancaire dans le système. Contient les informations relatives au compte, au solde, et au propriétaire. C'est l'entité centrale pour les vérifications de soldes.

### Propriétés
- **`Id`** (`Guid`) : Identifiant unique du compte bancaire (Clé primaire).
- **`UserId`** (`Guid`) : Identifiant de l'utilisateur propriétaire du compte (Clé étrangère vers `User`).
- **`AccountNumber`** (`string`) : Numéro de compte bancaire.
- **`Iban`** (`string`) : Code IBAN (International Bank Account Number), crucial pour la validation des virements.
- **`AccountType`** (`string`) : Type de compte (ex: Courant, Épargne).
- **`Balance`** (`decimal`) : Solde actuel du compte. Type décimal requis pour prévenir les pertes de précision dans la logique métier bancaire.
- **`Currency`** (`string`) : Devise du compte (ex: EUR, USD).
- **`CreatedAt`** (`DateTime`) : Date de création du compte.
- **`FormattedBalance`** (`string`) : Propriété calculée retournant le solde formaté avec la devise (ex: "1 000.00 EUR").

---

## 3. `Beneficiary` (dans `Beneficiary.cs`)
**Espace de nom :** `FulbankApp.Models`

Représente un bénéficiaire à qui l'utilisateur peut envoyer des fonds.

### Propriétés
- **`Id`** (`Guid`) : Identifiant unique du bénéficiaire.
- **`UserId`** (`Guid`) : Identifiant du créateur de ce bénéficiaire.
- **`Name`** (`string`) : Nom du bénéficiaire.
- **`Iban`** (`string`) : L'IBAN du bénéficiaire pour le routage de l'argent. Doit être vérifié (format) lors de l'ajout (Logique Métier).
- **`Note`** (`string`) : Commentaire ou description pour le bénéficiaire.
- **`CreatedAt`** (`DateTime`) : Date d'ajout du bénéficiaire.

---

## 4. `CryptoWallet` (dans `CryptoWallet.cs`)
**Espace de nom :** `FulbankApp.Models`

Représente un portefeuille de cryptomonnaies d'un utilisateur.

### Propriétés
- **`Id`** (`Guid`) : Identifiant unique du portefeuille.
- **`UserId`** (`Guid`) : Utilisateur propriétaire du portefeuille.
- **`CryptoCode`** (`string`) : Symbole de la cryptomonnaie (ex: BTC, ETH).
- **`Amount`** (`decimal`) : Montant détenu. Unité décimale utilisée, précision importante pour les montants fractionnés.
- **`WalletAddress`** (`string`) : Adresse blockchain du portefeuille.
- **`CreatedAt`** (`DateTime`) : Date de création.
- **`UpdatedAt`** (`DateTime`) : Date de la dernière modification (dépôt ou retrait).
- **`FormattedAmount`** (`string`) : Formate le solde, généralement sur 8 décimales pour les cryptos (ex: "0.00500000 BTC").

---

## 5. `Transaction` (dans `Transaction.cs`)
**Espace de nom :** `FulbankApp.Models`

Historise toutes les opérations monétaires (Dépôts, Retraits, Virements).
L'audit est primordial : une fois créée, une transaction est généralement immuable dans la logique bancaire.

### Propriétés
- **`Id`** (`Guid`) : Identifiant unique.
- **`UserId`** (`Guid`) : Acteur ayant initié la transaction.
- **`AccountId`** (`Guid?`) : Compte source ou destination de l'argent (Optionnel si transaction globale).
- **`Type`** (`string`) : `Transfer`, `Deposit`, `Withdrawal`, etc.
- **`Amount`** (`decimal`) : La valeur de la transaction. Positive (entrée) ou négative (sortie).
- **`Currency`** (`string`) : La monnaie échangée.
- **`RecipientName`** (`string`) : Nom du destinataire (Pour les virements).
- **`RecipientIban`** (`string`) : IBAN du destinataire (Prouve la destination des fonds).
- **`Description`** (`string`) : Motif de la transaction.
- **`Status`** (`string`) : État (`Pending`, `Completed`, `Failed`). Permet de gérer la validation asynchrone des fonds.
- **`CreatedAt`** (`DateTime`) : Timestamp exact.

### Propriétés calculées
- **`FormattedAmount`** (`string`) : Affiche `+` ou `-` avec la valeur absolue.
- **`FormattedDate`** (`string`) : Formatage simple de la date `dd/MM/yyyy`.

---

## 6. `User` (dans `User.cs`)
**Espace de nom :** `FulbankApp.Models`

Utilisateur principal de l'application Fulbank.

### Propriétés
- **`Id`, `Username`, `Email`** : Identifiants personnels et d'accès.
- **`FirstName`, `LastName`** : Identité.
- **`Birthdate`** (`DateTime?`) : Vérification d'âge légal requise pour les opérations de crédit/crypto.
- **`ProfilePictureUrl`, `CurrentSkin`** : Personnalisation esthétique de l'app et du `AnimatedCharacter`.
- **`FullName`** (`string`) : Propriété calculée (`FirstName + " " + LastName`).

---

## 7. `UserSettings` (dans `UserSettings.cs`)
**Espace de nom :** `FulbankApp.Models`

Préférences de notification et de sécurité de l'utilisateur.

### Propriétés
- **`TwoFactorEnabled`** (`bool`) : Métier : essentiel. Si activé, requiert une étape supplémentaire lors du login.
- **`SmsNotifications`, `EmailNotifications`, `ConnectionAlerts`** : Flags pour le déclenchement de webhooks ou mails de service.
