# Documentation : Helpers

Ce document fournit la documentation exhaustive pour les classes situées dans le dossier `Helpers` de l'application FulbankApp. Ces classes fournissent des méthodes utilitaires globales, des constantes de configuration matérielles/logicielles, et l'implémentation des commandes MVVM.

---

## 1. `AnimationHelper` (dans `AnimationHelper.cs`)
**Espace de nom :** `FulbankApp.Helpers`

Classe utilitaire statique pour centraliser, standardiser et exécuter les animations d'interface utilisateur WPF (Windows Presentation Foundation) de façon propre.

### Méthodes
- **`FadeIn(UIElement element, int durationMs, Action onComplete)`** : Apparition progressive modifiant l'Opacité (`Opacity 0 → 1`).
- **`FadeOut(UIElement element, int durationMs, Action onComplete)`** : Disparition progressive modifiant l'Opacité (`Opacity 1 → 0`).
- **`AnimateScale(UIElement element, double targetScale, int durationMs, EasingFunctionBase easingFunction)`** :
  Agrandit ou rétrécit dynamiquement un élément d'interface via un `ScaleTransform`. S'assure que le RenderTransformOrigin est bien ancré au centre (`Point(0.5, 0.5)`).
- **`SimulateButtonPress(Button button)`** :
  Création d'un feedback visuel utilisateur : réduit la taille du composant (`0.9x`) très rapidement puis le remet à sa taille initiale (`1.0`).
- **`ZoomOnCanvasButton(...)`** / **`ResetCanvasZoom(...)`** :
  Mise en focus d'une zone de l'écran (avec assombrissement optionnel de l'arrière plan `darkOverlay`). Calcule la translation sur l'axe X et Y pour recentrer l'objectif sur le bouton donné.
- **`CreatePulseAnimation(UIElement element, double minScale, double maxScale, int durationSeconds)`** :
  Animation cyclique infinie (`RepeatBehavior.Forever`, `AutoReverse = true`) pour faire clignoter/respirer un élément (ex: bouton de validation d'un transfert bancaire).
- **`AnimateDoubleProperty(...)`** (*Privée*) : Exécute physiquement via le moteur WPF une `DoubleAnimation` sur une `DependencyProperty` quelconque.

---

## 2. `Constants` (dans `Constants.cs`)
**Espace de nom :** `FulbankApp.Helpers`

Super-structure listant de manière globale les propriétés de configuration de l'application. Elle agit comme registre figé des comportements par défaut de l'interface et du jeu.

### Propriétés & Régions clés
- **`Animation Timings`** : Durées standards en MS pour homogénéiser les rendus (`FADE_IN_DURATION_MS = 400`, `BUTTON_PRESS_DURATION_MS = 100`, etc.).
- **`Gameplay` / `Character Dimensions`** : Configuration du moteur de balade 2D (vitesse en pixels/sec, taille de vue 1920x1080).
- **`File Paths` & `Pack URIs`** :
  - Chemins absolus de développement (ex: `C:\Users\BERNIER\...`). *Attention métier: La présence de chemins absolus en dur vers un dossier source précis brise la portabilité au déploiement du jeu/app.*
  - URIs de pack (ressources WPF compilées `pack://application:...`) pour charger les GIFs locaux sans erreur.
- **`CURRENT USER`** : Pseudo de session mémorisée (ex: variable globale). *Note : Archicturalement "anti-pattern" : le fait d'avoir des variables globales (`BankAmount`, `CryptosAmount`, etc.) en `Constants` est potentiellement une anomalie de l'ancien système local avant le passage vers `SupabaseService`.*

---

## 3. `RelayCommand` et `RelayCommand<T>` (dans `RelayCommand.cs`)
**Espace de nom :** `FulbankApp.Helpers`

Implémentation standard indispensable du pattern MVVM en WPF pour l'interface `ICommand`. Elle redirige la logique du UI-Event (un Clic Bouton par exemple) vers une méthode encapsulée dans le ViewModel.

### Classes
- **`RelayCommand`** : Commande sans argument optionnel.
  - Constructeur : `Action execute`, `Func<bool> canExecute`.
- **`RelayCommand<T>`** : Commande qui transfert les `CommandParameter` donnés depuis le XAML (par exemple : transférer l'objet `BankAccount` en cliquant sur le bouton de sélection).

### Méthodes respectées par ICommand
- **`CanExecute(object parameter)`** : Détermine (souvent dynamiquement évalué) si le bouton associé dans le XAML doit être autorisé ou grisé (Disabled). Par exemple: `return UserBalance >= TransferAmount`.
- **`Execute(object parameter)`** :  Le bloc code fonctionnel à exécuter si `CanExecute` a retourné `true`.
- **`CanExecuteChanged`** :  Connecté à `CommandManager.RequerySuggested` de WPF, force le moteur à ré-évaluer l'accessibilité des boutons de l'UI dès qu'une touche est frappée au clavier ou qu'un clic de souris a lieu.
