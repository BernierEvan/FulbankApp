# FulbankApp — v1

Résumé
------
FulbankApp est une application WPF (.NET 10) fournissant une vue principale de type "game-like" (déplacement d'un personnage animé sur un Canvas, détection de collisions, effets de masque et animations d'UI). Ce dépôt contient le code source pour la version initiale (v1) : logique de rendu (HomeView), utilitaires d'animation (AnimationHelper) et configuration WPF (App.xaml).

But principal
------------
- Fournir une interface riche et animée (Canvas, masque, personnage animé).
- Moteur simple de collision et gestion de profondeur (Z-index) derrière/avant éléments.
- Animations réutilisables (fade, scale, zoom, pulse).
- Navigation déclenchée par commandes / boutons.

Prérequis
---------
- Visual Studio 2026 (Community/Professional/Enterprise) avec le workload "Développement d'applications de bureau .NET" (WPF).
- .NET SDK 10.0 (matching TargetFramework `net10.0-windows`).
- Outils : Git (pour cloner), dotnet CLI (optionnel).
- Packages NuGet (gérés par le projet) :
  - `Microsoft.Data.SqlClient` (v6.1.3)
  - `WpfAnimatedGif` (v2.0.2)

Installation et exécution
------------------------
1. Cloner le dépôt :
   - `git clone <repo-url>`
2. Ouvrir la solution `FulbankApp.sln` dans Visual Studio 2026 (ou ouvrir le dossier dans VS Code + extension C# si vous préférez, mais VS est recommandé pour WPF).
3. Restaurer les packages NuGet (Visual Studio le fait automatiquement, ou `dotnet restore`).
4. Build : `dotnet build` ou via Visual Studio (Clean puis Rebuild).
5. Lancer : F5 (Debug) ou Ctrl+F5 (Release).

Alternatives CLI :
- `dotnet restore`
- `dotnet build -c Debug`
- `dotnet run --project FulbankApp` (fonctionne si l'exécutable WPF est correctement configuré dans l'environnement CLI)

Structure du projet
-------------------
- `FulbankApp/` — projet WPF principal
  - `App.xaml` / `App.xaml.cs` — point d'entrée WPF, ressources et `StartupUri`
  - `MainWindow.xaml` / `MainWindow.xaml.cs` — fenêtre principale (doit exister, voir section Résolution de problème)
  - `View/` — vues et UserControls (ex. `HomeView.xaml`, code?behind `HomeView.xaml.cs`)
  - `Helpers/AnimationHelper.cs` — fonctions d'animation centralisées
  - `Models/`, `Services/`, `Resources/` — (selon organisation du projet)
  - `FulbankApp.csproj` — SDK: `Microsoft.NET.Sdk`, `UseWPF=true`

Fichiers clés disponibles (v1)
------------------------------
- `App.xaml` : définit `StartupUri="MainWindow.xaml"` et référence plusieurs `ResourceDictionary` pour styles.
- `HomeView.xaml.cs` : logique de la vue Home — timer de mise à jour, déplacement, collisions, masque, animations, interaction boutons.
- `Helpers/AnimationHelper.cs` : utilitaires d'animation : fade, scale, zoom, pulse, etc.

Problème connu (et racine du crash rencontré)
---------------------------------------------
Exception observée :
- `System.IO.IOException: Impossible de trouver la ressource 'MainWindow.xaml'.`

Racine :
- Le fichier XAML `MainWindow.xaml` est absent du projet (ou renommé/mal configuré), mais `App.xaml` utilise `StartupUri="MainWindow.xaml"`. WPF tente de charger cette ressource comme ressource embarquée (pack URI). Si le fichier n'est pas inclus dans l'assembly (Build Action ? `Page`), l'entrée de ressource manquera dans le manifeste et provoquera l'IOException au démarrage. Le crash survient lors de l'appel `App.InitializeComponent()`.

Comment résoudre immédiatement
-----------------------------
- Vérifier que `MainWindow.xaml` existe à la racine du projet et que sa propriété `Build Action` est `Page`.
- Si `MainWindow.xaml` est dans un sous-dossier, mettre `StartupUri` sur le chemin relatif correct (ex. `Views/MainWindow.xaml`) ou utiliser pack URI complet `/FulbankApp;component/Views/MainWindow.xaml`.
- S'assurer que `x:Class` dans `MainWindow.xaml` correspond au namespace et au nom de classe du code-behind.
- Faire Clean puis Rebuild.

Fichier d'exemple pour restaurer `MainWindow` (copier/collez si manquant)
-----------------------------------------------------------------------
Si `MainWindow.xaml` n'existe pas, créer ces deux fichiers (ajuster namespace si nécessaire).

---

Explication complète et détaillée du projet (rapport / devoir)
=============================================================

Introduction générale
---------------------
FulbankApp est conçue comme une application WPF qui combine des éléments d'interface utilisateur traditionnels et des mécanismes de rendu "type jeu" : un Canvas sert d'espace de jeu où un personnage animé se déplace, rencontre des obstacles, et déclenche des interactions (boutons). Le code de la version 1 (v1) met l'accent sur une implémentation pragmatique côté code?behind — la logique métier et la logique d'affichage sont implémentées directement dans les UserControls et helpers, facilitant la mise en place rapide de prototypes interactifs.

Objectifs pédagogiques du devoir
--------------------------------
- Présenter l'architecture d'une application WPF intégrant rendu 2D simple, gestion d'entrées et animations.
- Expliquer en détail le flux d'exécution (démarrage, chargement des ressources, initialisation de la vue).
- Décrire les composants principaux, leur rôle, leurs interactions et les choix d'implémentation.
- Proposer diagnostics, vérifications et correctifs pour les erreurs courantes (ressources manquantes, build action).
- Présenter pistes d'amélioration et tests recommandés.

Architecture et composants (détail)
----------------------------------

1) App.xaml / App.xaml.cs
- Rôle : point d'entrée XAML de l'application, définit les ressources globales et la fenêtre de démarrage (`StartupUri`).
- Comportement au démarrage :
  - Le runtime WPF appelle `App.InitializeComponent()` (généré) qui lit le XAML et tente d'instancier la fenêtre désignée par `StartupUri`.
  - Cette opération charge la ressource XAML via un pack URI et attend que le BAML correspondant soit présent dans l'assembly.
  - Si la ressource est absente (MainWindow manquant ou non compilé en tant que `Page`), une `IOException` est levée — racine du crash observé.

2) MainWindow.xaml / MainWindow.xaml.cs (fenêtre hôte)
- Rôle : conteneur d'accueil, hôte des vues (ici `HomeView`). Doit appeler `InitializeComponent()` pour charger sa structure.
- Bonnes pratiques :
  - S'assurer que le fichier XAML et son code-behind partagent le même namespace et nom de classe (`x:Class`).
  - `Build Action` = `Page` pour XAML afin de générer le BAML embarqué.
  - Utiliser `WindowStartupLocation="CenterScreen"` et définir des tailles minimales adaptées.

3) HomeView.xaml / HomeView.xaml.cs (UserControl principal)
- Rôle : implémente la logique de l'interface "game-like".
- Composants privés clés :
  - `DispatcherTimer _movementTimer` : timer UI servant de boucle logique (tick périodique).
  - Flags de direction et `_pressedKeys` : gestion des entrées clavier (flèches + WASD).
  - `_playerCharacter` : wrapper graphique (classe externe) pour dessiner et animer le personnage.
  - `_obstacles` : collection de `Rectangle` définis en XAML représentant des colliders statiques.
  - `_collisionService` : service pour détecter intersections entre rectangles et effectuer calculs géométriques (coin transformé, point dans polygone).
  - `MaskRect` et Background : création d'un effet visuel de masque du bureau (capture d'une portion du background en tenant compte des transformations).

- Méthodes principales et leur fonctionnement :
  - `InitializeObstacles()` : collecte les rectangles nommés (définis en XAML) et les ajoute à la liste d'obstacles.
  - `InitializeCharacter()` : crée `_playerCharacter`, position initiale et charge les animations GIF (via `WpfAnimatedGif` ou autre).
  - `SetupAnimations()` : initialise animations visuelles constantes (pulsation de halos).
  - `OnWindowLoaded()` : focus sur `MainCanvas`, abonnements pour mise à jour du mask.
  - `OnWindowKeyDown/OnWindowKeyUp()` : mise à jour d'un ensemble de touches pressées et des flags de direction — ceci permet une logique fiable de mouvements combinés (diagonales).
  - `OnMovementTick()` : boucle principale appelée à chaque tick ; appelle `UpdatePlayerMovement()`, `UpdateCharacterAnimation()` et `CheckButtonCollisions()`.

- Logique de mouvement et collisions (`UpdatePlayerMovement`) :
  - Calcule vélocités X et Y suivant flags.
  - Normalise la diagonale (facteur 1/?2) pour éviter vitesse supérieure lors de déplacement diagonal.
  - Construit une "hitbox" réduite (insets) à partir de `Player.ActualWidth`/Height et `Constants.HITBOX_SHRINK`.
  - Effectue test de collision séparé pour l'axe X puis Y :
    - Test horizontal : simule déplacement sur X ? si collision détectée, annule translation X.
    - Test vertical : simule déplacement sur Y (avec X potentiellement ajusté) ? si collision détectée, annule translation Y.
    - Cette séparation évite que la collision sur un axe bloque le mouvement sur l'autre inutilement.
  - Clamp la position dans les bornes du Canvas via `Math.Max/Min`.
  - Applique la position via `Canvas.SetLeft/SetTop`.
  - Synchronise visuel animé et met à jour Z-index selon position relative au mask pour simuler profondeur.

- Détection de profondeur (`UpdatePlayerDepth`) :
  - Récupère le centre du player.
  - Obtient coins transformés du `MaskRect` via `_collisionService.GetTransformedCorners(MaskRect)` (gère rotation).
  - Détermine si le centre du joueur se trouve à l'intérieur du polygone du mask.
  - Compare la coordonnée Y du joueur avec le centre Y du mask pour définir l'ordre d'affichage (Z-index) : devant/dérrière.

- Rendu du Mask (`UpdateMaskRectBrush`) :
  - Calcule transform affine inverse (world ? local) en résolvant une petite matrice 2x2 basée sur coins transformés.
  - Crée un `VisualBrush` de l'élément `Background` avec `MatrixTransform` pour aligner la portion correcte.
  - Dessine la portion dans un `RenderTargetBitmap` (bitmap rendu côté CPU/GDI) et l'assigne à `MaskRect.Fill`.
  - Le code contient des gardes (width/height > 0, vérifications nulles) et un throttling basé sur `Constants.MASK_UPDATE_THROTTLE_MS` pour limiter le coût.

- Interactions boutons :
  - `CheckButtonCollision` compare la bounding box du player avec celle des boutons et déclenche `button.RaiseEvent(Button.ClickEvent)` pour simuler un clic lors d'intersection.

4) AnimationHelper.cs
- Fournit utilitaires :
  - `FadeIn` / `FadeOut` : animations d'opacité.
  - `AnimateScale` : applique un `ScaleTransform` à un élément et anime ScaleX/ScaleY.
  - `SimulateButtonPress` : simule appui sur bouton (scale).
  - `ZoomOnCanvasButton` / `ResetCanvasZoom` : calcule translation nécessaire pour centrer un zoom sur un élément du Canvas et anime Scale/Translate du Canvas et overlay.
  - `CreatePulseAnimation` : pulsation continue utilisée pour effets halo.
- Implémentation pseudo-générique via `AnimateDoubleProperty(Animatable target, DependencyProperty property, double toValue, int durationMs)` — méthode privée pour réduire duplication.

Composants manquants ou non fournis dans l'extrait
---------------------------------------------------
- `AnimatedCharacter` : classe responsable du rendu des GIFs, de la gestion des directions/états d'animation (`SetDirection`, `SetIdle`, `LoadGifs`), et des propriétés `Width/Height/Position` exposées.
- `CollisionService` : implémente `IsCollidingWithAny(Rect testRect, List<Rectangle> obstacles)` et `GetTransformedCorners(FrameworkElement element)` et `IsPointInPolygon(Point, Point[])`.
- `Constants` : conteneur des constantes globales (vitesses, tailles, durées, tolérances).
- `RelayCommand` : implémentation d'`ICommand` utilisée pour `NavigateCommand`.

Flux d'exécution complet (séquence)
----------------------------------
1. Lancer l'application — CLR charge assemblies et exécute point d'entrée.
2. `App.InitializeComponent()` lit `App.xaml`.
3. WPF lit `StartupUri` (ex. `MainWindow.xaml`) et tente de charger la ressource BAML correspondante depuis l'assembly.
   - Si BAML absent ? `IOException` (Impossible de trouver la ressource).
4. Si `MainWindow` chargé, son XAML est instancié puis code-behind `MainWindow()` exécute `InitializeComponent()` pour la fenêtre.
5. La fenêtre contient `HomeView` — lors de son instanciation :
   - `InitializeComponent()` charge éléments visuels (Canvas, Player, MaskRect, boutons, Rectangles obstacles).
   - Le constructeur `HomeView()` initialise obstacles, personnage, animations, démarre `_movementTimer`, et abonne events clavier/layout.
6. La boucle timer commence à appeler `OnMovementTick()` sur le thread UI (Dispatcher).
7. Les entrées clavier modifient flags, la boucle gère déplacement, collisions, animations visuelles et interactions boutons.

Stratégies de test et de débogage
---------------------------------
- Reproduire l'IOException : supprimer ou renommer `MainWindow.xaml`, lancer l'app ? vérifier message d'erreur.
- Vérifier manifest de l'assembly : inspecter l'assembly généré pour présence de `*.baml`.
- Points d'arrêt : `OnMovementTick`, `UpdateMaskRectBrush`, `CollisionService` pour valider coordonnées et coins.
- Tester collisions : créer scènes simples avec un obstacle et déplacer le player contre pour vérifier stop/slide.
- Mesurer performance : profiler pour vérifier coût de `RenderTargetBitmap.Render`, throttler plus si nécessaire.

Bonnes pratiques et améliorations proposées
-------------------------------------------
- Séparer logique métier (mouvement, collisions) dans des services ou ViewModels pour testabilité.
- Injecter `DispatcherTimer`/IClock pour simuler ticks en tests.
- Remplacer `RenderTargetBitmap` si problématique pour performances par shaders/GPU si nécessaire.
- Ajouter logging (`ILogger`) pour tracer erreurs de ressources au démarrage.
- Vérifier toutes les ResourceDictionary référencées dans `App.xaml` pour éviter XAML manquants.
- Ajouter validations null et exceptions contrôlées au lieu de crash applicatif.

Checklist de correction pour l'erreur principale
------------------------------------------------
- [ ] Vérifier présence de `MainWindow.xaml` dans l'explorateur de solutions.
- [ ] Propriété `Build Action` = `Page`.
- [ ] `x:Class` valide et correspond au code-behind.
- [ ] Rebuild (Clean + Rebuild).
- [ ] Si MainWindow volontairement supprimé : modifier `App.xaml` pour pointer vers une vue existante ou créer MainWindow minimal.

Conclusion
----------
La v1 de FulbankApp assemble plusieurs concepts WPF avancés : rendu dans un Canvas, synchronisation d'animation, gestion de masque visuel, collisions et animations réutilisables. Le défaut principal observé est lié à l'absence ou la mauvaise configuration de la fenêtre de démarrage (`MainWindow.xaml`) — problème simple à corriger en restaurant le fichier XAML ou en adaptant `App.xaml`. Pour évoluer vers une application maintenable et testable, il est recommandé d'extraire la logique dans des services/ViewModels et d'ajouter instrumentation (logging, tests unitaires/integration).

---

Si vous souhaitez, je peux ajouter ou restaurer automatiquement `MainWindow.xaml` et `MainWindow.xaml.cs` dans le projet pour corriger le crash de démarrage.

**Voulez-vous que je crée MainWindow.xaml et son code-behind ?**