using FulbankApp.Helpers;
using FulbankApp.Models;
using FulbankApp.Services;
using FulbankApp.View.Skeletons;
using FulbankApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO; // ajouté pour fallback chargement image
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Numeric = System.Numerics;

namespace FulbankApp.View
{
    /// <summary>
    /// Logique d'interaction pour HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        #region Private Properties

        // === GESTION DU MOUVEMENT ===
        private readonly DispatcherTimer _movementTimer;
        private bool _isMovingUp;
        private bool _isMovingDown;
        private bool _isMovingLeft;
        private bool _isMovingRight;
        private readonly HashSet<Key> _pressedKeys = new();

        // === PERSONNAGE ANIMÉ ===
        private AnimatedCharacter _playerCharacter;
        private DateTime _lastUpdateTime;

        // === DÉTECTION DE COLLISIONS ===
        private readonly List<Rectangle> _obstacles = new();
        private readonly CollisionService _collisionService = new();

        // === RENDU ET MASQUE ===
        private DateTime _lastMaskUpdate = DateTime.MinValue;
        private double _previousMaskLeft = double.NaN;
        private double _previousMaskTop = double.NaN;
        private double _previousMaskAngle = double.NaN;

        // === VIEWMODEL FOR CHANGES ===

        public ICommand NavigateCommand { get; private set; }

        #endregion

        #region Constructeur

        /// Initialise la fenêtre et tous ses composants
        public HomeView()
        {
            InitializeComponent();

            // Configuration initiale
            InitializeObstacles();
            InitializeCharacter();
            SetupAnimations();

            // FIX: Initialiser le timer après tous les autres composants
            _movementTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Constants.MOVEMENT_TIMER_INTERVAL_MS)
            };
            _movementTimer.Tick += OnMovementTick;
            _movementTimer.Start();

            // Événements : laisser l'attachement clavier au MainCanvas dans OnWindowLoaded
            this.Loaded += OnWindowLoaded;
            this.Focusable = true;

            // ViewModel
            NavigateCommand = new RelayCommand(ExecuteNavigation);
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Remplit la liste des obstacles pour la détection de collisions
        /// Tous ces rectangles sont définis dans home.xaml
        /// </summary>
        private void InitializeObstacles()
        {
            _obstacles.Clear();

            // Ajouter tous les obstacles par catégorie
            _obstacles.AddRange(new[]
            {
                // Bureaux
                DeskObstacle1, DeskObstacle2, DeskObstacle3,
                DeskObstacle4, DeskObstacle5, DeskObstacle6,
                
                // Murs
                WallObstacle1, WallObstacle2, WallObstacle3,
                WallObstacle4, WallObstacle5, WallObstacle6,
                
                // Canapés
                CouchObstacle1, CouchObstacle2, CouchObstacle3, CouchObstacle4,
                
                // Tables
                TableObstacle1,
                
                // Plantes
                PlantObstacle1, PlantObstacle2, PlantObstacle3, PlantObstacle4,
                
                // Poufs
                PoufObstacle1, PoufObstacle2, PoufObstacle4,
                PoufObstacle5, PoufObstacle6, PoufObstacle7,
                
                // Seaux/Buckets
                BucketObstacle1, BucketObstacle2, BucketObstacle3, BucketObstacle4
            });
        }

        /// <summary>
        /// Crée et positionne le personnage animé
        /// </summary>
        private void InitializeCharacter()
        {
            const double startX = 450;
            const double startY = 450;

            // Crée AnimatedCharacter (visuel géré par AnimatedCharacter)
            _playerCharacter = new AnimatedCharacter(
                MainCanvas,
                Constants.CHARACTER_RENDER_WIDTH,
                Constants.CHARACTER_RENDER_HEIGHT,
                startX,
                startY
            );

            // S'abonner aux changements de skin pour mettre à jour le placeholder Player
            _playerCharacter.SkinChanged += OnPlayerSkinChanged;

            // Récupérer le skin sélectionné (stocké depuis le Login)
            string selectedSkin = null;
            try
            {
                if (Application.Current != null && Application.Current.Properties.Contains("SelectedSkin"))
                {
                    selectedSkin = Application.Current.Properties["SelectedSkin"] as string;
                }
            }
            catch
            {
                // ignore
            }

            if (string.IsNullOrWhiteSpace(selectedSkin))
                selectedSkin = Constants.DEFAULT_MALE_SKIN;

            // Charger les animations et idle du skin sélectionné
            try
            {
                _playerCharacter.LoadGifs(selectedSkin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur LoadGifs pour '{selectedSkin}': {ex.Message}");
            }

            // Mettre à jour le placeholder Player affiché dans le XAML
            UpdatePlayerPlaceholder(selectedSkin);

            _lastUpdateTime = DateTime.Now;
        }

        // Corrected signature : event Action<string> => handler takes single string parameter
        private void OnPlayerSkinChanged(string newSkin)
        {
            // Mettre à jour le placeholder (UI thread)
            Dispatcher.Invoke(() => UpdatePlayerPlaceholder(newSkin));
        }

        private void UpdatePlayerPlaceholder(string skinName)
        {
            if (string.IsNullOrWhiteSpace(skinName)) return;

            if (Player is Image img)
            {
                try
                {
                    string idleUri = Constants.GetCharacterIdlePath(skinName, "down");
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(idleUri, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    img.Source = bmp;
                }
                catch
                {
                    // Fallback vers le chemin disque si pack URI non disponible
                    try
                    {
                        // Qualifier System.IO.Path pour lever l'ambiguïté avec System.Windows.Shapes.Path
                        string file = System.IO.Path.Combine(Constants.CHARACTERS_BASE_PATH, skinName, "idle", "idle_down.png");
                        if (System.IO.File.Exists(file))
                        {
                            var bmp2 = new BitmapImage();
                            bmp2.BeginInit();
                            bmp2.UriSource = new Uri(file, UriKind.Absolute);
                            bmp2.CacheOption = BitmapCacheOption.OnLoad;
                            bmp2.EndInit();
                            img.Source = bmp2;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdatePlayerPlaceholder error: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Configure les animations de base (pulsation des halos)
        /// </summary>
        private void SetupAnimations()
        {
            // Animation de pulsation pour les deux halos lumineux
            AnimationHelper.CreatePulseAnimation(Halo, durationSeconds: 1);
            AnimationHelper.CreatePulseAnimation(Halo2, durationSeconds: 1);
        }

        #endregion

        #region Cycle de Vie de la Fenêtre

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // S'assurer que le Canvas a le focus pour recevoir les événements clavier
            if (MainCanvas != null)
            {
                MainCanvas.Focusable = true;
                MainCanvas.Focus();
                Keyboard.Focus(MainCanvas);

                // Attacher les événements clavier sur le Canvas (recevra les touches si canvas a le focus)
                MainCanvas.KeyDown += OnWindowKeyDown;
                MainCanvas.KeyUp += OnWindowKeyUp;
            }

            // Configurer le rendu du masque
            MaskRect.LayoutUpdated += OnMaskRectLayoutUpdated;
            UpdateMaskRectBrush();
        }

        // Nouveau gestionnaire pour cliquer et donner le focus au canvas
        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MainCanvas == null) return;
            MainCanvas.Focus();
            Keyboard.Focus(MainCanvas);
            e.Handled = false;
        }
                
        #endregion

        #region Masque & Zoom (implémentations ajoutées)

        private void OnMaskRectLayoutUpdated(object sender, EventArgs e)
        {
            if (MaskRect == null) return;

            DateTime now = DateTime.Now;

            // Throttle rapide pour éviter des mises à jour excessives pendant le layout
            if ((now - _lastMaskUpdate).TotalMilliseconds < 100) return;

            double left = Canvas.GetLeft(MaskRect);
            double top = Canvas.GetTop(MaskRect);
            double angle = 0;

            if (MaskRect.RenderTransform is RotateTransform rt)
            {
                angle = rt.Angle;
            }
            else if (MaskRect.RenderTransform is TransformGroup tg)
            {
                foreach (var child in tg.Children)
                {
                    if (child is RotateTransform r)
                    {
                        angle = r.Angle;
                        break;
                    }
                }
            }

            if (double.IsNaN(_previousMaskLeft) ||
                Math.Abs(left - _previousMaskLeft) > 0.5 ||
                Math.Abs(top - _previousMaskTop) > 0.5 ||
                Math.Abs(angle - _previousMaskAngle) > 0.5)
            {
                UpdateMaskRectBrush();
                _previousMaskLeft = left;
                _previousMaskTop = top;
                _previousMaskAngle = angle;
                _lastMaskUpdate = now;
            }
        }

        private void UpdateMaskRectBrush()
        {
            if (MaskRect == null) return;

            try
            {
                // Implémentation simple et robuste : gradient radial pour simuler un halo/masque.
                // Ceci évite les dépendances complexes tout en fournissant un rendu visuel acceptable.
                var gradient = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5
                };
                gradient.GradientStops.Add(new GradientStop(Color.FromArgb(220, 255, 255, 255), 0.0));
                gradient.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1.0));

                MaskRect.Fill = gradient;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateMaskRectBrush error: {ex.Message}");
            }
        }

        private void ZoomOnButton(Button button)
        {
            if (MainCanvas == null || button == null) return;

            try
            {
                var storyboard = new Storyboard();

                var scaleX = new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleY = new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(scaleX, MainCanvas);
                Storyboard.SetTarget(scaleY, MainCanvas);
                Storyboard.SetTargetProperty(scaleX,
                    new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleY,
                    new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

                storyboard.Children.Add(scaleX);
                storyboard.Children.Add(scaleY);

                storyboard.Begin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ZoomOnButton error: {ex.Message}");
            }
        }

        private void ResetCanvasZoom()
        {
            if (MainCanvas == null) return;

            try
            {
                var storyboard = new Storyboard();

                var scaleX = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleY = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(scaleX, MainCanvas);
                Storyboard.SetTarget(scaleY, MainCanvas);
                Storyboard.SetTargetProperty(scaleX,
                    new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleY,
                    new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

                storyboard.Children.Add(scaleX);
                storyboard.Children.Add(scaleY);

                storyboard.Begin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResetCanvasZoom error: {ex.Message}");
            }
        }

        #endregion

        #region Gestion des Entrées Clavier

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            _pressedKeys.Add(e.Key);

            switch (e.Key)
            {
                case Key.Left:
                case Key.A:
                    _isMovingLeft = true;
                    break;

                case Key.Right:
                case Key.D:
                    _isMovingRight = true;
                    break;

                case Key.Up:
                case Key.W:
                    _isMovingUp = true;
                    break;

                case Key.Down:
                case Key.S:
                    _isMovingDown = true;
                    break;
            }
        }

        private void OnWindowKeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);

            switch (e.Key)
            {
                case Key.Left:
                case Key.A:
                    _isMovingLeft = false;
                    break;

                case Key.Right:
                case Key.D:
                    _isMovingRight = false;
                    break;

                case Key.Up:
                case Key.W:
                    _isMovingUp = false;
                    break;

                case Key.Down:
                case Key.S:
                    _isMovingDown = false;
                    break;
            }
        }

        #endregion

        #region Boucle de Jeu (Game Loop)

        private void OnMovementTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double deltaTime = (now - _lastUpdateTime).TotalSeconds;
            if (deltaTime > 0.5) deltaTime = 0.5;
            _lastUpdateTime = now;

            UpdatePlayerMovement(deltaTime);
            UpdateCharacterAnimation();
            CheckButtonCollisions();
        }

        private void UpdatePlayerMovement(double deltaTime)
        {
            if (Player == null || _playerCharacter == null) return;

            double currentLeft = Canvas.GetLeft(Player);
            double currentTop = Canvas.GetTop(Player);

            double speedPxs = _playerCharacter.Speed * deltaTime;

            double dirX = 0;
            double dirY = 0;
            if (_isMovingLeft) dirX -= 1;
            if (_isMovingRight) dirX += 1;
            if (_isMovingUp) dirY -= 1;
            if (_isMovingDown) dirY += 1;

            if (dirX == 0 && dirY == 0)
                return;

            if (dirX != 0 && dirY != 0)
            {
                const double diagonalFactor = 0.70710678118;
                dirX *= diagonalFactor;
                dirY *= diagonalFactor;
            }

            double deltaX = dirX * speedPxs;
            double deltaY = dirY * speedPxs;

            double hitboxWidth = Math.Max(0, Player.ActualWidth - (Constants.HITBOX_SHRINK * 2));
            double hitboxHeight = Math.Max(0, Player.ActualHeight - (Constants.HITBOX_SHRINK * 2));

            double newLeft = currentLeft + deltaX;
            var testRectX = new Rect(
                newLeft + Constants.HITBOX_SHRINK,
                currentTop + Constants.HITBOX_SHRINK,
                hitboxWidth,
                hitboxHeight
            );

            if (_collisionService.IsCollidingWithAny(testRectX, _obstacles))
            {
                newLeft = currentLeft;
                deltaX = 0;
            }

            double newTop = currentTop + deltaY;
            var testRectY = new Rect(
                newLeft + Constants.HITBOX_SHRINK,
                newTop + Constants.HITBOX_SHRINK,
                hitboxWidth,
                hitboxHeight
            );

            if (_collisionService.IsCollidingWithAny(testRectY, _obstacles))
            {
                newTop = currentTop;
                deltaY = 0;
            }

            newLeft = Math.Max(0, Math.Min(newLeft, Constants.CANVAS_WIDTH - Player.ActualWidth));
            newTop = Math.Max(0, Math.Min(newTop, Constants.CANVAS_HEIGHT - Player.ActualHeight));

            Canvas.SetLeft(Player, newLeft);
            Canvas.SetTop(Player, newTop);

            // Synchroniser le personnage animé avec le rectangle de collision
            SynchronizeAnimatedCharacter(newLeft, newTop);

            UpdatePlayerDepth();
        }

        #endregion

        #region Synchronisation & Animation du personnage

        private void SynchronizeAnimatedCharacter(double playerLeft, double playerTop)
        {
            if (_playerCharacter == null) return;

            double playerCenterX = playerLeft + (Player.ActualWidth / 2.0);
            double playerCenterY = playerTop + (Player.ActualHeight / 2.0);

            double characterX = playerCenterX - (_playerCharacter.Width / 2.0);
            double characterY = playerCenterY - (_playerCharacter.Height / 1.4);

            _playerCharacter.Position = new Point(characterX, characterY);
        }

        private void UpdateCharacterAnimation()
        {
            if (_playerCharacter == null) return;

            bool isMoving = _isMovingUp || _isMovingDown || _isMovingLeft || _isMovingRight;

            if (isMoving)
            {
                Direction direction = DetermineDirection();
                _playerCharacter.SetDirection(direction);
            }
            else
            {
                _playerCharacter.SetIdle(_playerCharacter.CurrentDirection);
            }
        }

        private Direction DetermineDirection()
        {
            if (_isMovingUp && _isMovingLeft) return Direction.UpLeft;
            if (_isMovingUp && _isMovingRight) return Direction.UpRight;
            if (_isMovingDown && _isMovingLeft) return Direction.DownLeft;
            if (_isMovingDown && _isMovingRight) return Direction.DownRight;

            if (_isMovingUp) return Direction.Up;
            if (_isMovingDown) return Direction.Down;
            if (_isMovingLeft) return Direction.Left;
            if (_isMovingRight) return Direction.Right;

            return Direction.Down;
        }

        #endregion

        #region Set / Change Skin

        /// <summary>
        /// Charge les animations GIF et l'image idle du skin donné,
            /// et met à jour l'image "Player" pour afficher l'idle initial.
        /// </summary>
        public void SetPlayerSkin(string skinName)
        {
            if (string.IsNullOrWhiteSpace(skinName)) skinName = Constants.DEFAULT_MALE_SKIN;

            // Charger les gifs/idle dans le modèle AnimatedCharacter
            try
            {
                _playerCharacter.LoadGifs(skinName);
            }
            catch (Exception ex)
            {
                // Ne pas faire planter l'UI si assets manquent
                System.Diagnostics.Debug.WriteLine($"Erreur LoadGifs pour '{skinName}': {ex.Message}");
            }

            // Mettre à jour le placeholder Player (image statique)
            UpdatePlayerPlaceholder(skinName);
        }

        #endregion

        #region Gestion Profondeur & Boutons (inchangés)

        private void UpdatePlayerDepth()
        {
            double playerCenterY = Canvas.GetTop(Player) + (Player.ActualHeight / 2);
            double playerCenterX = Canvas.GetLeft(Player) + (Player.ActualWidth / 2);
            Point playerCenter = new Point(playerCenterX, playerCenterY);

            Point[] maskCorners = _collisionService.GetTransformedCorners(MaskRect);

            if (_collisionService.IsPointInPolygon(playerCenter, maskCorners))
            {
                double maskCenterY = maskCorners.Average(p => p.Y);
                Panel.SetZIndex(Player, playerCenterY > maskCenterY ? 1 : 3);
                return;
            }

            Panel.SetZIndex(Player, 2);
        }

        private void CheckButtonCollisions()
        {
            CheckButtonCollision(CryptoButton);
            CheckButtonCollision(CryptoButton2);
        }

        private void CheckButtonCollision(Button button)
        {
            if (button.Visibility != Visibility.Visible) return;

            Rect playerRect = new Rect(
                Canvas.GetLeft(Player),
                Canvas.GetTop(Player),
                Player.ActualWidth,
                Player.ActualHeight
            );

            Rect buttonRect = new Rect(
                Canvas.GetLeft(button),
                Canvas.GetTop(button),
                button.Width,
                button.Height
            );

            if (playerRect.IntersectsWith(buttonRect))
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        #endregion

        #region Boutons / Navigation


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            // 1. Figer le jeu : désactiver les mouvements et interactions
            _movementTimer?.Stop();
            MainCanvas.IsEnabled = false;

            // 2. Créer l'animation de fondu de sortie (Fade Out simple)
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(300), // 300ms est fluide et rapide
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            // 3. Au moment où le fondu est terminé, on lance la navigation
            fadeOut.Completed += (s, args) =>
            {
                // On force l'opacité à 0 pour éviter le "flash" avant que la nouvelle page ne charge
                MainCanvas.Opacity = 0;

                // Appel de votre fonction de navigation existante (qui gère le Skeleton)
                NavigateToPage(button);
            };

            // 4. Lancer l'animation sur le Canvas
            MainCanvas.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }


        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            // --- NOUVEAU CODE : CENTRAGE DU ZOOM ---

            // 1. Calculer le centre du bouton (Point(Width/2, Height/2))
            // 2. Transformer ce point pour obtenir ses coordonnées relatives au MainCanvas
            Point centerOfButton = button.TransformToAncestor(MainCanvas)
                                         .Transform(new Point(button.ActualWidth / 2, button.ActualHeight / 2));

            // 3. Convertir en coordonnées relatives (0.0 à 1.0) pour RenderTransformOrigin
            // Ex: Si le canvas fait 1000px et le bouton est à 500px, on veut 0.5
            double originX = centerOfButton.X / MainCanvas.ActualWidth;
            double originY = centerOfButton.Y / MainCanvas.ActualHeight;

            // 4. Appliquer l'origine au Canvas
            MainCanvas.RenderTransformOrigin = new Point(originX, originY);

            // ---------------------------------------

            // 1. Désactiver les interactions
            _movementTimer?.Stop();
            MainCanvas.IsEnabled = false;

            // 2. Créer et lancer l'animation (le zoom partira maintenant du bouton)
            var storyboard = CreateButtonClickZoomAnimation();

            // 3. Définir la fin de l'animation
            storyboard.Completed += (s, args) =>
            {
                MainCanvas.Opacity = 0;
                NavigateToPage(button);
            };

            storyboard.Begin();
        }

        private Storyboard CreateButtonClickZoomAnimation()
        {
            var storyboard = new Storyboard();

            // 1. Zoom In (0s to 1s)
            AddZoomAnimation(storyboard, 1.0, 1.3, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            // 2. Zoom Out (1s to 2s)
            AddZoomAnimation(storyboard, 1.3, 1.0, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            // 3. Huge Zoom (2s to 3s)
            AddZoomAnimation(storyboard, 1.0, 15, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));

            // 4. Fade Out
            // We use FillBehavior.Stop so the animation system releases the 'lock' 
            // on the Opacity property immediately after finishing.
            var fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1.0))
            {
                BeginTime = TimeSpan.FromSeconds(2.0),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn },
                FillBehavior = FillBehavior.Stop // <--- CRITICAL CHANGE
            };

            Storyboard.SetTarget(fadeOutAnimation, MainCanvas);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(fadeOutAnimation);

            return storyboard;
        }

        private async void NavigateToPage(Button button)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main == null) { RestoreCanvasState(); return; }

            string key = button.Tag.ToString();

            // 1. OBTENIR ET AFFICHER LE SQUELETTE
            UserControl skeletonView = GetSkeletonView(key);

            // On affiche le squelette immédiatement
            main.Content = skeletonView;

            // (Optionnel) Force l'UI à se rafraîchir pour afficher le squelette tout de suite
            await Task.Delay(50);

            UserControl realView = null;
            try
            {
                // 2. SIMULATION DE CHARGEMENT / CHARGEMENT RÉEL
                // C'est ici que l'effet Skeleton brille. On attend un peu pour que
                // l'utilisateur voit l'animation, ou le temps que les données arrivent.
                await Task.Delay(800); // 800ms de skeleton pour l'effet fluide

                // 3. CRÉATION DE LA VRAIE VUE
                realView = key switch
                {
                    "BankAccounts" => new BankAccountView(),
                    "Wallet" => new WalletView(),
                    "Transfer" => new TransferView(),
                    "Convert" => new ConvertView(),
                    "Beneficiaries" => new BeneficiariesView(),
                    "Settings" => new SettingsView(),
                    _ => null
                };

                if (realView != null)
                {
                    // Transition vers la vraie vue
                    main.Content = realView;

                    // Animation d'apparition douce de la vraie vue (Fade In)
                    realView.Opacity = 0;
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));
                    realView.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
                else
                {
                    RestoreCanvasState();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur nav: {ex.Message}");
                RestoreCanvasState();
            }
        }

        // Méthode helper pour choisir le bon squelette
        private UserControl GetSkeletonView(string key)
        {
            // Idéalement, retournez un squelette spécifique par page.
            // Pour l'instant, on peut retourner un squelette générique ou spécifique.

            switch (key)
            {
                case "Wallet":
                    return new WalletSkeletonView(); // Celui qu'on a créé

                case "BankAccounts":
                    // return new BankAccountsSkeletonView();
                    return new WalletSkeletonView(); // Recyclage temporaire

                default:
                    // Un squelette générique par défaut
                    return new WalletSkeletonView();
            }
        }


        private void RestoreCanvasState()
        {
            // Remettre l'origine du zoom au centre de l'écran par défaut
            MainCanvas.RenderTransformOrigin = new Point(0.5, 0.5); // <--- AJOUT IMPORTANT

            // Reset Opacity to visible
            MainCanvas.Opacity = 1.0;

            // Reset Zoom
            ResetCanvasZoom();

            // Re-enable interaction
            MainCanvas.IsEnabled = true;
            _movementTimer?.Start();

            // Redonner le focus au canvas pour que le clavier remarche immédiatement
            MainCanvas.Focus();
        }

        private void AddZoomAnimation(Storyboard storyboard, double from, double to, TimeSpan beginTime, TimeSpan duration)
        {
            var scaleXAnimation = new DoubleAnimation(from, to, duration) { BeginTime = beginTime };
            var scaleYAnimation = new DoubleAnimation(from, to, duration) { BeginTime = beginTime };

            Storyboard.SetTarget(scaleXAnimation, MainCanvas);
            Storyboard.SetTarget(scaleYAnimation, MainCanvas);

            Storyboard.SetTargetProperty(scaleXAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleYAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
        }

        // --- EMPTY EVENT HANDLERS (Clean these up!) ---
        private void Button_MouseEnter(object sender, MouseEventArgs e) { }
        private void Button_MouseLeave(object sender, MouseEventArgs e) { }
        private void Button2_MouseEnter(object sender, MouseEventArgs e) { }
        private void Button2_MouseLeave(object sender, MouseEventArgs e) { }

        #endregion


        #region Navigation (inchangés)

        private void ExecuteNavigation()
        {
            MessageBox.Show("Navigation exécutée (Code-Behind)");
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            var storyboard = CreateButtonClickZoomAnimation();
            storyboard.Completed += async (s, args) =>
            {
                
            };
            storyboard.Begin();
        }

        

        
        #endregion
    }
}
