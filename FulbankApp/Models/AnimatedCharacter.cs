using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FulbankApp.Helpers;
using WpfAnimatedGif;

namespace FulbankApp.Models
{
    /// <summary>
    /// Directions possibles pour le personnage animé
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    /// <summary>
    /// Représente un personnage animé avec gestion des GIFs et images statiques
    /// </summary>
    public class AnimatedCharacter : IDisposable
    {
        #region Fields

        private readonly Canvas _parentCanvas;
        private readonly Image _characterImage;
        private readonly Dictionary<Direction, BitmapImage> _walkingGifs;
        private readonly Dictionary<Direction, BitmapImage> _idleImages;
        private Direction _currentDirection;
        private string _skinNameString = "";
        private const string _skinNameConst = ""; 

        #endregion

        #region Properties

        /// <summary>
        /// Direction actuelle du personnage
        /// </summary>
        public Direction CurrentDirection => _currentDirection;

        /// <summary>
        /// Vitesse de déplacement en pixels par seconde
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Position actuelle du personnage sur le canvas
        /// </summary>
        public Point Position
        {
            get => new Point(Canvas.GetLeft(_characterImage), Canvas.GetTop(_characterImage));
            set
            {
                Canvas.SetLeft(_characterImage, value.X);
                Canvas.SetTop(_characterImage, value.Y);
            }
        }

        /// <summary>
        /// Largeur du personnage
        /// </summary>
        public double Width
        {
            get => _characterImage.Width;
            set => _characterImage.Width = value;
        }

        /// <summary>
        /// Hauteur du personnage
        /// </summary>
        public double Height
        {
            get => _characterImage.Height;
            set => _characterImage.Height = value;
        }

       

        #endregion

        #region Constructor

        /// <summary>
        /// Crée un nouveau personnage animé
        /// </summary>
        /// <param name="parentCanvas">Canvas parent où afficher le personnage</param>
        /// <param name="width">Largeur du personnage</param>
        /// <param name="height">Hauteur du personnage</param>
        /// <param name="initialX">Position X initiale</param>
        /// <param name="initialY">Position Y initiale</param>
        public AnimatedCharacter(Canvas parentCanvas, double width = 64, double height = 64, double initialX = 0, double initialY = 0)
        {
            _parentCanvas = parentCanvas ?? throw new ArgumentNullException(nameof(parentCanvas));
            _walkingGifs = new Dictionary<Direction, BitmapImage>();
            _idleImages = new Dictionary<Direction, BitmapImage>();
            _currentDirection = Direction.Down;
            Speed = Constants.CHARACTER_SPEED_PPS;
            _skinNameString = Constants.DEFAULT_MALE_SKIN;

            _characterImage = new Image
            {
                Width = width,
                Height = height
            };

            Canvas.SetLeft(_characterImage, initialX);
            Canvas.SetTop(_characterImage, initialY);

            _parentCanvas.Children.Add(_characterImage);
        }

        // Ligne à corriger (~30)
        public void LoadGifs(string skinName = null)
        {
            skinName ??= Constants.DEFAULT_MALE_SKIN;

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                LoadGifForDirection(direction, skinName);
                LoadIdleImageForDirection(direction, skinName);
            }

            SetIdle(Direction.Down);
        }

        /// <summary>
        /// Change la direction et affiche l'animation de marche correspondante
        /// </summary>
        public void SetDirection(Direction direction)
        {
            _currentDirection = direction;

            if (_walkingGifs.TryGetValue(direction, out var walkingGif))
            {
                ImageBehavior.SetAnimatedSource(_characterImage, walkingGif);
            }
        }

        /// <summary>
        /// Affiche l'image statique (idle) pour une direction donnée
        /// </summary>
        public void SetIdle(Direction direction)
        {
            _currentDirection = direction;

            if (_idleImages.TryGetValue(direction, out var idleImage))
            {
                // Arrêter le GIF actuel
                ImageBehavior.SetAnimatedSource(_characterImage, null);
                // Afficher l'image statique
                _characterImage.Source = idleImage;
            }
        }

        /// <summary>
        /// Déplace le personnage dans une direction pendant un temps donné
        /// </summary>
        /// <param name="direction">Direction du mouvement</param>
        /// <param name="deltaTime">Temps écoulé en secondes</param>
        public void Move(Direction direction, double deltaTime)
        {
            _currentDirection = direction;

            // Calcul du vecteur de déplacement
            (double dx, double dy) = GetMovementVector(direction);

            // Normalisation pour les diagonales
            if (dx != 0 && dy != 0)
            {
                const double diagonalFactor = 0.70710678118; // 1 / sqrt(2)
                dx *= diagonalFactor;
                dy *= diagonalFactor;
            }

            // Déplacement réel en pixels
            dx *= Speed * deltaTime;
            dy *= Speed * deltaTime;

            MoveBy(dx, dy);
            SetDirection(direction);
        }

        /// <summary>
        /// Retire le personnage du canvas
        /// </summary>
        public void RemoveFromCanvas()
        {
            if (_parentCanvas.Children.Contains(_characterImage))
            {
                _parentCanvas.Children.Remove(_characterImage);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Charge le GIF de marche pour une direction donnée
        /// </summary>
        private void LoadGifForDirection(Direction direction, string skinName)
        {
            string gifFileName = GetWalkGifFileName(direction);
            string uriPath = Constants.GetCharacterWalkGifPath(skinName, gifFileName.Replace("walk_", "").Replace(".gif", ""));

            try
            {
                var uri = new Uri(uriPath, UriKind.Absolute);
                _walkingGifs[direction] = new BitmapImage(uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du GIF {gifFileName} : {ex.Message}");
            }
        }

        /// <summary>
        /// Charge l'image statique pour une direction donnée
        /// </summary>
        private void LoadIdleImageForDirection(Direction direction, string skinName)
        {
            string idleFileName = GetIdleImageFileName(direction);
            string uriPath = Constants.GetCharacterIdlePath(skinName, idleFileName.Replace("idle_", "").Replace(".png", ""));

            try
            {
                var uri = new Uri(uriPath, UriKind.Absolute);
                _idleImages[direction] = new BitmapImage(uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'image {idleFileName} : {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne le nom de fichier du GIF de marche selon la direction
        /// </summary>
        private static string GetWalkGifFileName(Direction direction) => direction switch
        {
            Direction.Up => "walk_up.gif",
            Direction.Down => "walk_down.gif",
            Direction.Left => "walk_left.gif",
            Direction.Right => "walk_right.gif",
            Direction.UpLeft => "walk_up_left.gif",
            Direction.UpRight => "walk_up_right.gif",
            Direction.DownLeft => "walk_down_left.gif",
            Direction.DownRight => "walk_down_right.gif",
            _ => "walk_down.gif"
        };

        /// <summary>
        /// Retourne le nom de fichier de l'image idle selon la direction
        /// </summary>
        private static string GetIdleImageFileName(Direction direction) => direction switch
        {
            Direction.Up => "idle_up.png",
            Direction.Down => "idle_down.png",
            Direction.Left => "idle_left.png",
            Direction.Right => "idle_right.png",
            Direction.UpLeft => "idle_up_left.png",
            Direction.UpRight => "idle_up_right.png",
            Direction.DownLeft => "idle_down_left.png",
            Direction.DownRight => "idle_down_right.png",
            _ => "idle_down.png"
        };

        /// <summary>
        /// Retourne le vecteur de mouvement normalisé pour une direction
        /// </summary>
        private static (double dx, double dy) GetMovementVector(Direction direction) => direction switch
        {
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            Direction.UpLeft => (-1, -1),
            Direction.UpRight => (1, -1),
            Direction.DownLeft => (-1, 1),
            Direction.DownRight => (1, 1),
            _ => (0, 0)
        };

        /// <summary>
        /// Déplace le personnage de manière relative
        /// </summary>
        private void MoveBy(double deltaX, double deltaY)
        {
            double currentX = Canvas.GetLeft(_characterImage);
            double currentY = Canvas.GetTop(_characterImage);

            Canvas.SetLeft(_characterImage, currentX + deltaX);
            Canvas.SetTop(_characterImage, currentY + deltaY);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            RemoveFromCanvas();
            GC.SuppressFinalize(this);
        }

        #endregion

        
    }
       
}