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

        #region Private Properties
        private readonly DispatcherTimer _movementTimer;
        private bool _isMovingUp, _isMovingDown, _isMovingLeft, _isMovingRight;
        private readonly HashSet<Key> _pressedKeys = new();
        private AnimatedCharacter _playerCharacter;
        private DateTime _lastUpdateTime;
        private readonly List<Rectangle> _obstacles = new();
        private readonly CollisionService _collisionService = new();
        private DateTime _lastMaskUpdate = DateTime.MinValue;
        private double _previousMaskLeft = double.NaN;
        private double _previousMaskTop = double.NaN;
        private double _previousMaskAngle = double.NaN;
        #endregion

        #endregion

        #region Constructeur

        /// Initialise la fenêtre et tous ses composants
        public HomeView()
        {
            InitializeComponent();
            InitializeObstacles();
            InitializeCharacter();
            SetupAnimations();

            _movementTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Constants.MOVEMENT_TIMER_INTERVAL_MS) };
            _movementTimer.Tick += OnMovementTick;
            _movementTimer.Start();

            this.Loaded += OnWindowLoaded;
            this.Focusable = true;
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
            string destination = button.Tag?.ToString();

            // 1. FREEZE INPUT IMMEDIATELY
            _movementTimer?.Stop();
            MainCanvas.IsEnabled = false;

            // 2. FADE OUT
            var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            fadeOut.Completed += (s, args) =>
            {
                // 3. EXECUTE NAVIGATION COMMAND
                var mainVM = Application.Current.MainWindow.DataContext as MainViewModel;
                if (mainVM != null && !string.IsNullOrEmpty(destination))
                {
                    if (mainVM.NavigateCommand.CanExecute(destination))
                        mainVM.NavigateCommand.Execute(destination);
                }
            };

            MainCanvas.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        // --- CORRECTION : BOUTON AVEC ZOOM ---
        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            string destination = button.Tag?.ToString();

            // Centrage du zoom
            Point center = button.TransformToAncestor(MainCanvas)
                                 .Transform(new Point(button.ActualWidth / 2, button.ActualHeight / 2));
            MainCanvas.RenderTransformOrigin = new Point(center.X / MainCanvas.ActualWidth, center.Y / MainCanvas.ActualHeight);

            _movementTimer?.Stop();
            MainCanvas.IsEnabled = false;

            var storyboard = CreateButtonClickZoomAnimation();

            storyboard.Completed += (s, args) =>
            {
                // APPEL AU MAINVIEWMODEL
                var mainVM = Application.Current.MainWindow.DataContext as MainViewModel;
                if (mainVM != null && !string.IsNullOrEmpty(destination))
                {
                    if (mainVM.NavigateCommand.CanExecute(destination))
                        mainVM.NavigateCommand.Execute(destination);
                }
            };

            storyboard.Begin();
        }

        private Storyboard CreateButtonClickZoomAnimation()
        {
            var sb = new Storyboard();
            AddZoomAnimation(sb, 1.0, 1.3, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            AddZoomAnimation(sb, 1.3, 1.0, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            AddZoomAnimation(sb, 1.0, 15, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));

            var fade = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1.0)) { BeginTime = TimeSpan.FromSeconds(2.0) };
            Storyboard.SetTarget(fade, MainCanvas);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);
            return sb;
        }

        // Méthode helper pour choisir le bon squelette
        


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

        private void AddZoomAnimation(Storyboard sb, double from, double to, TimeSpan begin, TimeSpan dur)
        {
            var sx = new DoubleAnimation(from, to, dur) { BeginTime = begin };
            var sy = new DoubleAnimation(from, to, dur) { BeginTime = begin };
            Storyboard.SetTarget(sx, MainCanvas); Storyboard.SetTarget(sy, MainCanvas);
            Storyboard.SetTargetProperty(sx, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(sy, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            sb.Children.Add(sx); sb.Children.Add(sy);
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

        

        

        
        #endregion
    }
}
