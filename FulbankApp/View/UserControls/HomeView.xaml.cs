using FulbankApp.Helpers;
using FulbankApp.Models;
using FulbankApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Numeric = System.Numerics;
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
using FulbankApp.ViewModels;

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

        // Propriété qui sera liée au ContentControl dans le XAML de la fenêtre principale
        

        public ICommand NavigateCommand { get; private set; }

        #endregion

        #region Constructeur

        /// Initialise la fenêtre et tous ses composants
        /// 
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

            // Événements de la fenêtre
            this.Loaded += OnWindowLoaded;
            this.KeyDown += OnWindowKeyDown;
            this.KeyUp += OnWindowKeyUp;
            this.Focusable = true;
            this.Focus();

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

            _playerCharacter = new AnimatedCharacter(
                MainCanvas,
                Constants.CHARACTER_RENDER_WIDTH,
                Constants.CHARACTER_RENDER_HEIGHT,
                startX,
                startY
            );

            // Charger les animations GIF du personnage
            _playerCharacter.LoadGifs();

            _lastUpdateTime = DateTime.Now;
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

        /// <summary>
        /// Appelé quand la fenêtre est complètement chargée
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // S'assurer que le Canvas a le focus pour recevoir les événements clavier
            MainCanvas.Focus();

            // Configurer le rendu du masque
            MaskRect.LayoutUpdated += OnMaskRectLayoutUpdated;
            UpdateMaskRectBrush();
        }

        #endregion

        #region Gestion des Entrées Clavier

        /// <summary>
        /// Gère l'appui sur une touche du clavier
        /// </summary>
        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            // Ajouter la touche à l'ensemble des touches pressées
            _pressedKeys.Add(e.Key);

            // Mettre à jour les flags de direction
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

        /// <summary>
        /// Gère le relâchement d'une touche du clavier
        /// </summary>
        private void OnWindowKeyUp(object sender, KeyEventArgs e)
        {
            // Retirer la touche de l'ensemble des touches pressées
            _pressedKeys.Remove(e.Key);

            // Mettre à jour les flags de direction
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

        /// <summary>
        /// Appelé à chaque tick du timer de mouvement (environ 60 FPS)
        /// C'est la boucle principale du jeu
        /// </summary>
        private void OnMovementTick(object sender, EventArgs e)
        {
            UpdatePlayerMovement();
            UpdateCharacterAnimation();
            CheckButtonCollisions();
        }

        /// <summary>
        /// Met à jour la position du joueur avec détection de collisions
        /// </summary>
        private void UpdatePlayerMovement()
        {
            if (Player == null) return;

            // Position actuelle
            double currentLeft = Canvas.GetLeft(Player);
            double currentTop = Canvas.GetTop(Player);

            // Calculer le vecteur de vélocité selon les touches pressées
            double velocityX = 0;
            double velocityY = 0;

            if (_isMovingLeft) velocityX -= Constants.PLAYER_SPEED;
            if (_isMovingRight) velocityX += Constants.PLAYER_SPEED;
            if (_isMovingUp) velocityY -= Constants.PLAYER_SPEED;
            if (_isMovingDown) velocityY += Constants.PLAYER_SPEED;

            // Normalisation pour mouvement diagonal
            // Si on se déplace en diagonale, réduire la vitesse pour éviter d'aller plus vite
            if (velocityX != 0 && velocityY != 0)
            {
                const double diagonalFactor = 0.70710678118; // 1/√2
                velocityX *= diagonalFactor;
                velocityY *= diagonalFactor;
            }

            // Calculer la hitbox réduite du joueur
            double hitboxWidth = Math.Max(0, Player.ActualWidth - (Constants.HITBOX_SHRINK * 2));
            double hitboxHeight = Math.Max(0, Player.ActualHeight - (Constants.HITBOX_SHRINK * 2));

            // === TEST COLLISION HORIZONTALE (axe X) ===
            double newLeft = currentLeft + velocityX;
            var testRectX = new Rect(
                newLeft + Constants.HITBOX_SHRINK,
                currentTop + Constants.HITBOX_SHRINK,
                hitboxWidth,
                hitboxHeight
            );

            // Si collision détectée, annuler le mouvement horizontal
            if (_collisionService.IsCollidingWithAny(testRectX, _obstacles))
            {
                newLeft = currentLeft;
            }

            // === TEST COLLISION VERTICALE (axe Y) ===
            double newTop = currentTop + velocityY;
            var testRectY = new Rect(
                newLeft + Constants.HITBOX_SHRINK,
                newTop + Constants.HITBOX_SHRINK,
                hitboxWidth,
                hitboxHeight
            );

            // Si collision détectée, annuler le mouvement vertical
            if (_collisionService.IsCollidingWithAny(testRectY, _obstacles))
            {
                newTop = currentTop;
            }

            // Limiter la position dans les bornes du canvas
            newLeft = Math.Max(0, Math.Min(newLeft, Constants.CANVAS_WIDTH - Player.ActualWidth));
            newTop = Math.Max(0, Math.Min(newTop, Constants.CANVAS_HEIGHT - Player.ActualHeight));

            // Appliquer la nouvelle position
            Canvas.SetLeft(Player, newLeft);
            Canvas.SetTop(Player, newTop);

            // Synchroniser le personnage animé avec le rectangle de collision
            SynchronizeAnimatedCharacter(newLeft, newTop);

            // Mettre à jour l'ordre de rendu (Z-Index) selon la profondeur
            UpdatePlayerDepth();
        }

        /// <summary>
        /// Synchronise la position du personnage animé avec le rectangle de collision
        /// </summary>
        private void SynchronizeAnimatedCharacter(double playerLeft, double playerTop)
        {
            if (_playerCharacter == null) return;

            // Calculer le centre du rectangle Player
            double playerCenterX = playerLeft + (Player.ActualWidth / 2.0);
            double playerCenterY = playerTop + (Player.ActualHeight / 2.0);

            // Positionner le personnage animé centré sur le Player
            double characterX = playerCenterX - (_playerCharacter.Width / 2.0);
            double characterY = playerCenterY - (_playerCharacter.Height / 1.4);

            _playerCharacter.Position = new Point(characterX, characterY);
        }

        /// <summary>
        /// Met à jour l'animation du personnage selon sa direction de mouvement
        /// </summary>
        private void UpdateCharacterAnimation()
        {
            if (_playerCharacter == null) return;

            bool isMoving = _isMovingUp || _isMovingDown || _isMovingLeft || _isMovingRight;

            if (isMoving)
            {
                // En mouvement: déterminer et appliquer la direction
                Direction direction = DetermineDirection();
                _playerCharacter.SetDirection(direction);
            }
            else
            {
                // Immobile: passer en animation idle dans la dernière direction
                _playerCharacter.SetIdle(_playerCharacter.CurrentDirection);
            }
        }

        /// <summary>
        /// Détermine la direction du personnage selon les touches pressées
        /// Gère les 8 directions (4 cardinales + 4 diagonales)
        /// </summary>
        private Direction DetermineDirection()
        {
            // Diagonales d'abord (priorité si deux touches sont pressées)
            if (_isMovingUp && _isMovingLeft) return Direction.UpLeft;
            if (_isMovingUp && _isMovingRight) return Direction.UpRight;
            if (_isMovingDown && _isMovingLeft) return Direction.DownLeft;
            if (_isMovingDown && _isMovingRight) return Direction.DownRight;

            // Directions cardinales
            if (_isMovingUp) return Direction.Up;
            if (_isMovingDown) return Direction.Down;
            if (_isMovingLeft) return Direction.Left;
            if (_isMovingRight) return Direction.Right;

            // Par défaut: vers le bas
            return Direction.Down;
        }

        #endregion

        #region Gestion de la Profondeur (Z-Index)

        /// <summary>
        /// Met à jour le Z-Index du joueur selon sa position par rapport au MaskRect
        /// Cela crée l'effet de profondeur (le joueur peut passer devant ou derrière le bureau)
        /// </summary>
        private void UpdatePlayerDepth()
        {
            // Calculer le centre du Player
            double playerCenterY = Canvas.GetTop(Player) + (Player.ActualHeight / 2);
            double playerCenterX = Canvas.GetLeft(Player) + (Player.ActualWidth / 2);
            Point playerCenter = new Point(playerCenterX, playerCenterY);

            // Obtenir les coins transformés du MaskRect (avec rotation)
            Point[] maskCorners = _collisionService.GetTransformedCorners(MaskRect);

            // Vérifier si le joueur est dans la zone du masque
            if (_collisionService.IsPointInPolygon(playerCenter, maskCorners))
            {
                // Calculer le centre Y du masque
                double maskCenterY = maskCorners.Average(p => p.Y);

                // Si le joueur est en dessous du centre du masque: devant (Z=1)
                // Si le joueur est au-dessus du centre du masque: derrière (Z=3)
                Panel.SetZIndex(Player, playerCenterY > maskCenterY ? 1 : 3);
                return;
            }

            // Par défaut: joueur au-dessus de tout (Z=2)
            Panel.SetZIndex(Player, 2);
        }

        #endregion

        #region Interactions avec les Boutons

        /// <summary>
        /// Vérifie les collisions entre le joueur et les boutons interactifs
        /// </summary>
        private void CheckButtonCollisions()
        {
            CheckButtonCollision(CryptoButton);
            CheckButtonCollision(CryptoButton2);
        }

        /// <summary>
        /// Vérifie si le joueur entre en collision avec un bouton spécifique
        /// et déclenche son événement Click si c'est le cas
        /// </summary>
        private void CheckButtonCollision(Button button)
        {
            if (button.Visibility != Visibility.Visible) return;

            // Rectangle de collision du joueur
            Rect playerRect = new Rect(
                Canvas.GetLeft(Player),
                Canvas.GetTop(Player),
                Player.ActualWidth,
                Player.ActualHeight
            );

            // Rectangle de collision du bouton
            Rect buttonRect = new Rect(
                Canvas.GetLeft(button),
                Canvas.GetTop(button),
                button.Width,
                button.Height
            );

            // Si intersection détectée, simuler un clic sur le bouton
            if (playerRect.IntersectsWith(buttonRect))
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        

        #endregion

        #region Gestionnaires d'Événements des Boutons

        /// <summary>
        /// Gère le clic sur un bouton (avec animation de zoom)
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Créer et lancer l'animation de zoom au clic
            var storyboard = CreateButtonClickZoomAnimation();
            storyboard.Begin();
        }

        /// <summary>
        /// Gère le survol du bouton Crypto Wallet
        /// </summary>
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ZoomOnButton(CryptoButton);
        }

        /// <summary>
        /// Gère la sortie du survol du bouton Crypto Wallet
        /// </summary>
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetCanvasZoom();
        }

        /// <summary>
        /// Gère le survol du bouton Fiat Wallet
        /// </summary>
        private void Button2_MouseEnter(object sender, MouseEventArgs e)
        {
            ZoomOnButton(CryptoButton2);
        }

        /// <summary>
        /// Gère la sortie du survol du bouton Fiat Wallet
        /// </summary>
        private void Button2_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetCanvasZoom();
        }

        

        #endregion

        #region Animations de Zoom

        /// <summary>
        /// Effectue un zoom centré sur un bouton
        /// </summary>
        private void ZoomOnButton(Button button)
        {
            AnimationHelper.ZoomOnCanvasButton(
                button,
                MainCanvas,
                CanvasScale,
                CanvasTranslate,
                DarkOverlay,
                Constants.ZOOM_CLICK_SCALE,
                Constants.ZOOM_ANIMATION_DURATION_MS
            );
        }

        /// <summary>
        /// Réinitialise le zoom du canvas (retour à l'échelle normale)
        /// </summary>
        private void ResetCanvasZoom()
        {
            AnimationHelper.ResetCanvasZoom(
                CanvasScale,
                CanvasTranslate,
                DarkOverlay,
                Constants.ZOOM_ANIMATION_DURATION_MS
            );
        }

        /// <summary>
        /// Crée une animation de zoom complexe en 3 phases au clic sur un bouton:
        /// 1. Zoom in (1.0 → 1.2)
        /// 2. Zoom out léger (1.2 → 0.9)
        /// 3. Shrink final (0.9 → 0.0)
        /// </summary>
        private Storyboard CreateButtonClickZoomAnimation()
        {
            var storyboard = new Storyboard();

            // Phase 1: Zoom in
            AddZoomAnimation(storyboard, 1.0, 1.2, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            // Phase 2: Zoom out léger
            AddZoomAnimation(storyboard, 1.2, 0.9, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            // Phase 3: Shrink final
            AddZoomAnimation(storyboard, 0.9, 0.0, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.5));

            return storyboard;
        }

        /// <summary>
        /// Ajoute une animation de zoom (ScaleX et ScaleY) au storyboard
        /// </summary>
        private void AddZoomAnimation(Storyboard storyboard, double from, double to, TimeSpan beginTime, TimeSpan duration)
        {
            // Animation pour ScaleX
            var scaleXAnimation = new DoubleAnimation(from, to, duration)
            {
                BeginTime = beginTime
            };

            // Animation pour ScaleY
            var scaleYAnimation = new DoubleAnimation(from, to, duration)
            {
                BeginTime = beginTime
            };

            // Définir les cibles
            Storyboard.SetTarget(scaleXAnimation, MainCanvas);
            Storyboard.SetTarget(scaleYAnimation, MainCanvas);

            // Définir les propriétés à animer
            Storyboard.SetTargetProperty(scaleXAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleYAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            // Ajouter au storyboard
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
        }

        #endregion

        #region Rendu du Masque (MaskRect)

        /// <summary>
        /// Appelé quand le layout du MaskRect est mis à jour
        /// Gère le throttling pour éviter trop de recalculs
        /// </summary>
        private void OnMaskRectLayoutUpdated(object sender, EventArgs e)
        {
            if (MaskRect == null) return;

            DateTime now = DateTime.UtcNow;

            // Throttling: éviter de mettre à jour trop fréquemment
            if (now - _lastMaskUpdate < TimeSpan.FromMilliseconds(Constants.MASK_UPDATE_THROTTLE_MS))
                return;

            // Vérifier si la position ou la rotation a changé significativement
            double left = Canvas.GetLeft(MaskRect);
            double top = Canvas.GetTop(MaskRect);
            double angle = GetMaskRotationAngle();

            bool hasMoved = double.IsNaN(_previousMaskLeft) ||
                          Math.Abs(left - _previousMaskLeft) > Constants.MASK_POSITION_THRESHOLD ||
                          Math.Abs(top - _previousMaskTop) > Constants.MASK_POSITION_THRESHOLD;

            bool hasRotated = double.IsNaN(_previousMaskAngle) ||
                            Math.Abs(angle - _previousMaskAngle) > Constants.MASK_ROTATION_THRESHOLD;

            if (hasMoved || hasRotated)
            {
                // Sauvegarder les nouvelles valeurs
                _previousMaskLeft = left;
                _previousMaskTop = top;
                _previousMaskAngle = angle;
                _lastMaskUpdate = now;

                // Mettre à jour le rendu (en arrière-plan pour ne pas bloquer)
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateMaskRectBrush();
                    UpdatePlayerDepth();
                }), DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Récupère l'angle de rotation actuel du MaskRect
        /// </summary>
        private double GetMaskRotationAngle()
        {
            if (MaskRect.RenderTransform is not TransformGroup transformGroup)
                return 0;

            foreach (var transform in transformGroup.Children)
            {
                if (transform is RotateTransform rotateTransform)
                    return rotateTransform.Angle;
            }

            return 0;
        }

        /// <summary>
        /// Met à jour le brush du MaskRect pour afficher la portion correcte du fond
        /// C'est ce qui crée l'effet de "masque" du bureau
        /// </summary>
        private void UpdateMaskRectBrush()
        {
            if (Background?.Source == null || MaskRect == null || MainCanvas == null)
                return;

            // Obtenir les coins transformés du MaskRect
            Point[] corners = _collisionService.GetTransformedCorners(MaskRect);
            if (corners == null || corners.Length < 4)
                return;

            Point corner0 = corners[0];
            Point corner1 = corners[1];
            Point corner3 = corners[3];

            double width = MaskRect.Width;
            double height = MaskRect.Height;

            if (width <= 0 || height <= 0)
                return;

            // Calculer la matrice de transformation inverse (world → local)
            Matrix worldToLocal = CalculateWorldToLocalMatrix(corner0, corner1, corner3, width, height);

            // Créer un brush avec la transformation
            var visualBrush = new VisualBrush(Background)
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Transform = new MatrixTransform(worldToLocal)
            };

            // Rendre dans un bitmap
            int pixelWidth = Math.Max(1, (int)Math.Ceiling(width));
            int pixelHeight = Math.Max(1, (int)Math.Ceiling(height));

            var renderTarget = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                96,
                96,
                PixelFormats.Pbgra32
            );

            var drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
            }

            renderTarget.Render(drawingVisual);

            // Appliquer le bitmap au MaskRect
            MaskRect.Fill = new ImageBrush(renderTarget)
            {
                Stretch = Stretch.Fill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
        }

        /// <summary>
        /// Calcule la matrice de transformation affine inverse pour mapper
        /// les coordonnées monde vers les coordonnées locales du MaskRect
        /// </summary>
        private Matrix CalculateWorldToLocalMatrix(Point s0, Point s1, Point s3, double width, double height)
        {
            // Points de destination (rectangle local)
            Point d0 = new Point(0, 0);
            Point d1 = new Point(width, 0);
            Point d3 = new Point(0, height);

            // Vecteurs des côtés du rectangle transformé
            Vector u = s1 - s0;
            Vector v = s3 - s0;

            // Vecteurs du rectangle local
            Vector e1 = d1 - d0;
            Vector e2 = d3 - d0;

            // Calculer le déterminant
            double determinant = (u.X * v.Y) - (v.X * u.Y);

            if (Math.Abs(determinant) < Constants.COLLISION_TOLERANCE)
                return Matrix.Identity;

            // Calculer l'inverse de la matrice U
            double invU11 = v.Y / determinant;
            double invU12 = -v.X / determinant;
            double invU21 = -u.Y / determinant;
            double invU22 = u.X / determinant;

            // Calculer les coefficients de la transformation affine
            double a11 = (e1.X * invU11) + (e2.X * invU21);
            double a12 = (e1.X * invU12) + (e2.X * invU22);
            double a21 = (e1.Y * invU11) + (e2.Y * invU21);
            double a22 = (e1.Y * invU12) + (e2.Y * invU22);

            double offsetX = d0.X - (a11 * s0.X + a12 * s0.Y);
            double offsetY = d0.Y - (a21 * s0.X + a22 * s0.Y);

            return new Matrix(a11, a21, a12, a22, offsetX, offsetY);
        }

        #endregion

        #region Navigation



        private void ExecuteNavigation()
        {
            // Logique de navigation (ex: aller à une autre vue)
            MessageBox.Show("Navigation exécutée (Code-Behind)");
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main == null) return;

            var btn = sender as Button;
            string key = btn.Tag.ToString();  // ← récupère "BankAccounts", "Wallet", etc.

            switch (key)
            {
                case "BankAccounts":
                    main.Content = new BankAccountView();
                    break;

                case "Wallet":
                    main.Content = new WalletView();
                    break;

                case "Transfer":
                    main.Content = new TransferView();
                    break;

                case "Convert":
                    main.Content = new ConvertView();
                    break;

                case "Beneficiaries":
                    main.Content = new BeneficiariesView();
                    break;

                case "Settings":
                    main.Content = new SettingsView();
                    break;
            }
        }
        #endregion
    }
}
