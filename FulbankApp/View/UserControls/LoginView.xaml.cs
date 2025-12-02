using FulbankApp.Helpers;
using FulbankApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FulbankApp.View
{
    public partial class LoginView : UserControl
    {
        private readonly Dictionary<KeyCombo, Button> _keyMap = new();

        private LoginViewModel ViewModel => DataContext as LoginViewModel;

        public LoginView()
        {
            InitializeComponent();
            InitializeKeyMap();
            SetupAnimations();
            this.PreviewKeyDown += OnWindowKeyDown;
            this.Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.RequestResetZoom += OnResetZoomRequested;
                ViewModel.RequestSkinChangeAnimation += ChangeSkinWithAnimation;
                ViewModel.RequestSkinPanelDisplay += ShowSkinSelectionPanel;

                // charger le skin initial s'il existe
                if (!string.IsNullOrWhiteSpace(ViewModel.CurrentSkin))
                    LoadCharacterSkin(ViewModel.CurrentSkin);
            }
        }

        private void SetupAnimations()
        {
            AnimationHelper.CreatePulseAnimation(Halo, durationSeconds: 1);
        }

        private void InitializeKeyMap()
        {
            for (int i = 0; i <= 9; i++)
            {
                Key digitKey = Key.D0 + i;
                Key numpadKey = Key.NumPad0 + i;
                Button button = FindPinButton(i);

                if (button != null)
                {
                    _keyMap[new KeyCombo(digitKey)] = button;
                    _keyMap[new KeyCombo(numpadKey)] = button;
                }
            }

            _keyMap[new KeyCombo(Key.Back)] = BtnPinBack;
            _keyMap[new KeyCombo(Key.Back, ModifierKeys.Control)] = BtnPinErase;
        }

        private Button FindPinButton(int digit) => digit switch
        {
            0 => BtnPin0,
            1 => BtnPin1,
            2 => BtnPin2,
            3 => BtnPin3,
            4 => BtnPin4,
            5 => BtnPin5,
            6 => BtnPin6,
            7 => BtnPin7,
            8 => BtnPin8,
            9 => BtnPin9,
            _ => null
        };

        private void LoadCharacterSkin(string skinName)
        {
            if (string.IsNullOrWhiteSpace(skinName)) return;

            string imagePath = $"{Constants.CHARACTERS_BASE_PATH}/{skinName}/idle/idle_down.png";

            try
            {
                if (!File.Exists(imagePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Image introuvable : {imagePath}");
                    return;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                CharacterImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement du skin : {ex.Message}");
            }
        }

        private async void ChangeSkinWithAnimation(string newSkin)
        {
            if (string.IsNullOrWhiteSpace(newSkin)) return;

            try
            {
                double screenWidth = this.ActualWidth;
                double exitDistance = -(screenWidth + CharacterButton.ActualWidth);

                var exitAnim = new DoubleAnimation
                {
                    From = 0,
                    To = exitDistance,
                    Duration = TimeSpan.FromMilliseconds(Constants.CHARACTER_WALK_DURATION_MS),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                AnimationHelper.FadeOut(CharacterButton, 500);
                CharacterTranslate.BeginAnimation(TranslateTransform.XProperty, exitAnim);

                await System.Threading.Tasks.Task.Delay(Constants.CHARACTER_WALK_DURATION_MS + 100);

                LoadCharacterSkin(newSkin);

                CharacterTranslate.X = exitDistance;

                await System.Threading.Tasks.Task.Delay(100);

                CharacterButton.Opacity = 0;
                CharacterButton.Visibility = Visibility.Visible;

                var enterAnim = new DoubleAnimation
                {
                    From = exitDistance,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(Constants.CHARACTER_WALK_DURATION_MS),
                    EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2, EasingMode = EasingMode.EaseOut }
                };

                CharacterTranslate.BeginAnimation(TranslateTransform.XProperty, enterAnim);
                AnimationHelper.FadeIn(CharacterButton, Constants.CHARACTER_WALK_DURATION_MS);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
                CharacterButton.Opacity = 1;
                CharacterButton.Visibility = Visibility.Visible;
                CharacterTranslate.X = 0;
            }
        }

        private void ShowSkinSelectionPanel()
        {
            SkinContainer.Children.Clear();

            var skins = ViewModel?.AvailableSkins ?? new List<string>();
            foreach (var skin in skins)
            {
                var button = CreateSkinButton(skin);
                SkinContainer.Children.Add(button);
            }

            SkinSelectionPanel.Visibility = Visibility.Visible;
            DarkOverlay.Opacity = Constants.DARK_OVERLAY_OPACITY;

            var scaleAnim = new DoubleAnimation
            {
                From = Constants.SKIN_PANEL_INITIAL_SCALE,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(Constants.SKIN_PANEL_ANIMATION_DURATION_MS),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            AnimationHelper.FadeIn(SkinSelectionPanel, Constants.SKIN_PANEL_ANIMATION_DURATION_MS);
            PanelScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            PanelScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private Button CreateSkinButton(string skinName)
        {
            var button = new Button
            {
                Style = (Style)FindResource("SkinButton"),
                Margin = new Thickness(10),
                Tag = skinName
            };

            var grid = new Grid();

            var image = new System.Windows.Controls.Image
            {
                Width = 225,
                Height = 225,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            string imagePath = $"{Constants.CHARACTERS_BASE_PATH}/{skinName}/idle/idle_down.png";

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                image.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement skin {skinName} : {ex.Message}");
            }

            grid.Children.Add(image);

            if (skinName == ViewModel?.CurrentSkin)
            {
                var checkmark = new TextBlock
                {
                    Text = "✓",
                    FontSize = 40,
                    Foreground = new SolidColorBrush(Colors.LimeGreen),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, -10, -10, 0)
                };
                grid.Children.Add(checkmark);
            }

            button.Content = grid;
            button.Click += OnSkinButtonClick;

            return button;
        }

        private void OnSkinButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string skinName)
            {
                if (skinName != ViewModel?.CurrentSkin)
                {
                    CloseSkinPanel_Click(sender, e);
                    System.Threading.Tasks.Task.Delay(400).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => ChangeSkinWithAnimation(skinName));
                    });
                }
                else
                {
                    ViewModel?.BackCommand.Execute(null);
                    CloseSkinPanel_Click(sender, e);
                }
            }
        }

        /// <summary>
        /// Zoom hover sur un bouton
        /// </summary>
        private void ZoomHoverOnButton(Button button)
        {
            AnimationHelper.ZoomOnCanvasButton(
                button,
                MainCanvas,
                CanvasScale,
                CanvasTranslate,
                DarkOverlay,
                Constants.ZOOM_HOVER_SCALE
            );
        }

        /// <summary>
        /// Zoom click sur un bouton
        /// </summary>
        private void ZoomClickOnButton(Button button)
        {
            AnimationHelper.ZoomOnCanvasButton(
                button,
                MainCanvas,
                CanvasScale,
                CanvasTranslate,
                DarkOverlay,
                Constants.ZOOM_CLICK_SCALE
            );
        }

        /// <summary>
        /// Reset le zoom du canvas
        /// </summary>
        public void ResetZoom()
        {
            AnimationHelper.ResetCanvasZoom(CanvasScale, CanvasTranslate, DarkOverlay);
        }

        private void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            // Animation de bounce
            AnimationHelper.AnimateScale(CharacterButton, Constants.CHARACTER_BOUNCE_SCALE, 200);

            // Ouvrir le panneau de sélection
            ViewModel?.EnterSkinSelection.Execute(null);
            ShowSkinSelectionPanel();
        }

        private void CloseSkinPanel_Click(object sender, RoutedEventArgs e)
        {
            var scaleAnim = new DoubleAnimation
            {
                From = 1,
                To = Constants.SKIN_PANEL_INITIAL_SCALE,
                Duration = TimeSpan.FromMilliseconds(Constants.HALO_ANIMATION_DURATION_MS),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn }
            };

            AnimationHelper.FadeOut(SkinSelectionPanel, Constants.HALO_ANIMATION_DURATION_MS, () =>
            {
                SkinSelectionPanel.Visibility = Visibility.Collapsed;
                DarkOverlay.Opacity = 0;
            });

            PanelScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            PanelScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomClickOnButton(LoginButton);
            ViewModel?.HideBothCommand.Execute(null);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomClickOnButton(RegisterButton);
            ViewModel?.HideBothCommand.Execute(null);
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                AnimationHelper.SimulateButtonPress(button);
                ViewModel.HandlePinInput(button.Content.ToString());
            }
        }

        private void LoginButton_MouseEnter(object sender, MouseEventArgs e) =>
            ZoomHoverOnButton(LoginButton);

        private void RegisterButton_MouseEnter(object sender, MouseEventArgs e) =>
            ZoomHoverOnButton(RegisterButton);

        private void LoginButton_MouseLeave(object sender, MouseEventArgs e) =>
            ResetZoom();

        private void RegisterButton_MouseLeave(object sender, MouseEventArgs e) =>
            ResetZoom();

        private void OnResetZoomRequested() => ResetZoom();

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            var combo = new KeyCombo(e.Key, Keyboard.Modifiers);

            if (_keyMap.TryGetValue(combo, out Button button))
            {
                AnimationHelper.SimulateButtonPress(button);
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Représente une combinaison de touche + modificateur
    /// </summary>
    public struct KeyCombo
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }

        public KeyCombo(Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyCombo other && Key == other.Key && Modifiers == other.Modifiers;
        }

        public override int GetHashCode() => HashCode.Combine(Key, Modifiers);
    }
}

