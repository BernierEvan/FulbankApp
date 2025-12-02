using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private string _skinNameString = string.Empty;
        private int _skinIndex = 0;

        private static readonly string[] _availableSkins = new[]
        {
            Constants.DEFAULT_MALE_SKIN,
            Constants.DEFAULT_FEMALE_SKIN,
            Constants.THREE_PIECE_MAN_SKIN,
            Constants.SAD_EMPLOYEE_SKIN,
            Constants.WEIRD_TURTLE_SKIN
        };

        #endregion

        #region Events

        /// <summary>
        /// Déclenché lorsque l'image est cliquée.
        /// </summary>
        public event EventHandler Clicked;

        /// <summary>
        /// Déclenché lorsque le skin a été changé : param = nom du skin.
        /// </summary>
        public event Action<string> SkinChanged;

        #endregion

        #region Properties

        public Direction CurrentDirection => _currentDirection;
        public double Speed { get; set; }

        public Point Position
        {
            get => new Point(Canvas.GetLeft(_characterImage), Canvas.GetTop(_characterImage));
            set
            {
                Canvas.SetLeft(_characterImage, value.X);
                Canvas.SetTop(_characterImage, value.Y);
            }
        }

        public double Width
        {
            get => _characterImage.Width;
            set => _characterImage.Width = value;
        }

        public double Height
        {
            get => _characterImage.Height;
            set => _characterImage.Height = value;
        }

        #endregion

        #region Constructor

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
                Height = height,
                Stretch = Stretch.Uniform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                IsHitTestVisible = true,
                Cursor = Cursors.Hand
            };

            // Meilleure qualité de rendu
            RenderOptions.SetBitmapScalingMode(_characterImage, BitmapScalingMode.HighQuality);

            Canvas.SetLeft(_characterImage, initialX);
            Canvas.SetTop(_characterImage, initialY);

            // Assurer que l'image soit au-dessus pour pouvoir cliquer dessus
            Panel.SetZIndex(_characterImage, 1000);

            // Click handler : notifier et changer le skin (cycle)
            _characterImage.MouseLeftButtonDown += (s, e) =>
            {
                try
                {
                    Clicked?.Invoke(this, EventArgs.Empty);
                    CycleSkin();
                    e.Handled = true;
                }
                catch
                {
                    // ne pas laisser l'UI planter
                }
            };

            _parentCanvas.Children.Add(_characterImage);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Charge les GIFs et images idle pour le skin indiqué.
        /// Méthode robuste : essaye d'abord le pack:// URI puis tente un chemin sur disque (Constants.CHARACTERS_BASE_PATH).
        /// </summary>
        public void LoadGifs(string skinName = null)
        {
            skinName ??= Constants.DEFAULT_MALE_SKIN;
            _skinNameString = skinName;

            _walkingGifs.Clear();
            _idleImages.Clear();

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                LoadGifForDirection(direction, skinName);
                LoadIdleImageForDirection(direction, skinName);
            }

            // Défaut sur idle down si disponible
            SetIdle(Direction.Down);
        }

        public void SetDirection(Direction direction)
        {
            _currentDirection = direction;

            if (_walkingGifs.TryGetValue(direction, out var walkingGif) && walkingGif != null)
            {
                ImageBehavior.SetAnimatedSource(_characterImage, walkingGif);
                return;
            }

            if (_idleImages.TryGetValue(direction, out var idleFallback) && idleFallback != null)
            {
                ImageBehavior.SetAnimatedSource(_characterImage, null);
                _characterImage.Source = idleFallback;
            }
        }

        public void SetIdle(Direction direction)
        {
            _currentDirection = direction;

            if (_idleImages.TryGetValue(direction, out var idleImage) && idleImage != null)
            {
                ImageBehavior.SetAnimatedSource(_characterImage, null);
                _characterImage.Source = idleImage;
            }
        }

        public void Move(Direction direction, double deltaTime)
        {
            _currentDirection = direction;

            (double dx, double dy) = GetMovementVector(direction);

            if (dx != 0 && dy != 0)
            {
                const double diagonalFactor = 0.70710678118;
                dx *= diagonalFactor;
                dy *= diagonalFactor;
            }

            dx *= Speed * deltaTime;
            dy *= Speed * deltaTime;

            MoveBy(dx, dy);
            SetDirection(direction);
        }

        /// <summary>
        /// Force l'application d'un skin particulier.
        /// </summary>
        public void ApplySkin(string skinName)
        {
            if (string.IsNullOrWhiteSpace(skinName)) return;
            LoadGifs(skinName);
            SkinChanged?.Invoke(skinName);
        }

        /// <summary>
        /// Parcours la liste de skins disponibles (cycle) et l'applique.
        /// </summary>
        public void CycleSkin()
        {
            _skinIndex = (_skinIndex + 1) % _availableSkins.Length;
            var newSkin = _availableSkins[_skinIndex];
            LoadGifs(newSkin);
            SkinChanged?.Invoke(newSkin);
        }

        public void RemoveFromCanvas()
        {
            if (_parentCanvas != null && _parentCanvas.Children.Contains(_characterImage))
            {
                _parentCanvas.Children.Remove(_characterImage);
            }
        }

        #endregion

        #region Private Methods - Loading

        private void LoadGifForDirection(Direction direction, string skinName)
        {
            string fileName = GetWalkGifFileName(direction);
            string packUri = Constants.GetCharacterWalkGifPath(skinName, GetDirectionKey(direction));
            BitmapImage bmp = TryLoadBitmap(packUri, BuildFallbackPath(skinName, "animations", "walk", "gifs", fileName));

            if (bmp != null)
            {
                _walkingGifs[direction] = bmp;
            }
        }

        private void LoadIdleImageForDirection(Direction direction, string skinName)
        {
            string fileName = GetIdleImageFileName(direction);
            string packUri = Constants.GetCharacterIdlePath(skinName, GetDirectionKey(direction));
            BitmapImage bmp = TryLoadBitmap(packUri, BuildFallbackPath(skinName, "idle", fileName));

            if (bmp != null)
            {
                _idleImages[direction] = bmp;
            }
        }

        private static string GetDirectionKey(Direction direction) => direction switch
        {
            Direction.Up => "up",
            Direction.Down => "down",
            Direction.Left => "left",
            Direction.Right => "right",
            Direction.UpLeft => "up_left",
            Direction.UpRight => "up_right",
            Direction.DownLeft => "down_left",
            Direction.DownRight => "down_right",
            _ => "down"
        };

        private BitmapImage TryLoadBitmap(string packUri, string fallbackFilePath)
        {
            if (!string.IsNullOrWhiteSpace(packUri))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(packUri, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch
                {
                    // essayer fallback
                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(fallbackFilePath) && File.Exists(fallbackFilePath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fallbackFilePath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private string BuildFallbackPath(string skinName, params string[] segments)
        {
            try
            {
                var parts = new List<string> { Constants.CHARACTERS_BASE_PATH, skinName };
                parts.AddRange(segments);
                return Path.Combine(parts.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Helpers - Filenames & Movement

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