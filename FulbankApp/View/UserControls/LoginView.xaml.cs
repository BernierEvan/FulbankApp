using FulbankApp.Helpers;
using FulbankApp.ViewModels;
using System;
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
        private LoginViewModel ViewModel => DataContext as LoginViewModel;

        public LoginView()
        {
            InitializeComponent();

            // S'abonner aux événements du ViewModel
            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.RequestResetZoom += OnRequestResetZoom;
                ViewModel.RequestSkinChangeAnimation += OnRequestSkinChange;
                ViewModel.RequestSkinPanelDisplay += OnRequestSkinPanelDisplay;
            }

            // Animer l'apparition des halos
            AnimateHalosPulse();

            // Animer les boutons au survol
            SetupButtonHoverAnimations();
        }

        #region Event Handlers - Personnage

        private void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            // Animation de rebond
            var bounceStoryboard = CharacterButton.FindResource("BounceStoryboard") as Storyboard;
            bounceStoryboard?.Begin();

            // Afficher le panneau de sélection de skin
            ViewModel?.EnterSkinSelectionCommand.Execute(null);
        }

        #endregion

        #region Event Handlers - ViewModel

        private void OnRequestResetZoom()
        {
            AnimationHelper.ResetCanvasZoom(
                CanvasScale,
                CanvasTranslate,
                DarkOverlay
            );
        }

        private void OnRequestSkinChange(string skinName)
        {
            // Charger la nouvelle image de skin
            try
            {
                string imagePath = Constants.GetCharacterIdlePath(skinName, "down");
                CharacterImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch
            {
                // Si l'image n'est pas trouvée, garder l'ancienne
            }
        }

        private void OnRequestSkinPanelDisplay()
        {
            ShowSkinSelectionPanel();
        }

        #endregion

        #region Skin Selection Panel

        private void ShowSkinSelectionPanel()
        {
            // Effacer le contenu précédent
            SkinContainer.Children.Clear();

            // Créer un bouton pour chaque skin disponible
            foreach (string skin in ViewModel.AvailableSkins)
            {
                var button = CreateSkinButton(skin);
                SkinContainer.Children.Add(button);
            }

            // Animer l'apparition du panneau
            SkinSelectionPanel.Visibility = Visibility.Visible;

            var storyboard = new Storyboard();

            // Animation de l'opacité
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            Storyboard.SetTarget(fadeIn, SkinSelectionPanel);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));

            // Animation du scale
            var scaleX = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleX, SkinSelectionPanel);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            var scaleY = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleY, SkinSelectionPanel);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);
            storyboard.Begin();
        }

        private Button CreateSkinButton(string skinName)
        {
            var button = new Button
            {
                Width = 150,
                Height = 150,
                Margin = new Thickness(15),
                Background = new SolidColorBrush(Color.FromArgb(100, 26, 26, 46)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(207, 255, 4)),
                BorderThickness = new Thickness(2),
                Cursor = Cursors.Hand,
                Tag = skinName
            };

            // Image du skin
            var image = new Image
            {
                Source = LoadSkinPreview(skinName),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(10)
            };

            button.Content = image;
            button.Click += SkinButton_Click;

            // Animation au survol
            button.MouseEnter += (s, e) =>
            {
                var btn = s as Button;
                var scaleTransform = new ScaleTransform(1, 1);
                btn.RenderTransform = scaleTransform;
                btn.RenderTransformOrigin = new Point(0.5, 0.5);

                var anim = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(200));
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 255));
            };

            button.MouseLeave += (s, e) =>
            {
                var btn = s as Button;
                if (btn.RenderTransform is ScaleTransform st)
                {
                    var anim = new DoubleAnimation(1.1, 1, TimeSpan.FromMilliseconds(200));
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }

                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(207, 255, 4));
            };

            return button;
        }

        private ImageSource LoadSkinPreview(string skinName)
        {
            try
            {
                string path = Constants.GetCharacterIdlePath(skinName, "down");
                return new BitmapImage(new Uri(path, UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }

        private void SkinButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string skinName)
            {
                ViewModel.ChangeSkinCommand.Execute(skinName);
                CloseSkinPanel_Click(null, null);
            }
        }

        private void CloseSkinPanel_Click(object sender, RoutedEventArgs e)
        {
            var storyboard = new Storyboard();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(fadeOut, SkinSelectionPanel);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, args) =>
            {
                SkinSelectionPanel.Visibility = Visibility.Collapsed;
                OnRequestResetZoom();
            };

            storyboard.Begin();
        }

        #endregion

        #region Animations

        private void AnimateHalosPulse()
        {
            // Animation de pulsation pour les halos
            AnimationHelper.CreatePulseAnimation(Halo, 1.0, 1.2, 2);
            AnimationHelper.CreatePulseAnimation(Halo2, 1.0, 1.15, 1);
        }

        private void SetupButtonHoverAnimations()
        {
            // Les animations de survol sont déjà définies dans le XAML via les Triggers
            // Cette méthode peut être utilisée pour des animations supplémentaires si nécessaire
        }

        #endregion
    }
}