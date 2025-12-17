using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FulbankApp.Helpers;

namespace FulbankApp.View
{
    public partial class HomeView : UserControl
    {
        private void RestoreCanvasState()
        {
            if (MainCanvas != null)
            {
                MainCanvas.IsEnabled = true;
                MainCanvas.BeginAnimation(UIElement.OpacityProperty, null);
                MainCanvas.Opacity = 1.0;
                MainCanvas.RenderTransformOrigin = new Point(0.5, 0.5);
                MainCanvas.Focusable = true;
                MainCanvas.Focus();
                Keyboard.Focus(MainCanvas);
            }

            if (DarkOverlay != null)
            {
                DarkOverlay.Opacity = 0;
            }

            ResetCanvasZoom();

            if (_movementTimer == null)
            {
                _movementTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(Constants.MOVEMENT_TIMER_INTERVAL_MS)
                };
                _movementTimer.Tick += OnMovementTick;
            }

            if (!_movementTimer.IsEnabled)
            {
                _movementTimer.Start();
            }

            _pressedKeys?.Clear();
            _isMovingUp = _isMovingDown = _isMovingLeft = _isMovingRight = false;
        }

        private Storyboard CreateButtonClickZoomAnimation()
        {
            var storyboard = new Storyboard();

            if (CanvasScale != null)
            {
                var easing = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                var scaleX = new DoubleAnimation
                {
                    To = Constants.ZOOM_CLICK_SCALE,
                    Duration = TimeSpan.FromMilliseconds(Constants.ZOOM_ANIMATION_DURATION_MS),
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.HoldEnd
                };

                var scaleY = new DoubleAnimation
                {
                    To = Constants.ZOOM_CLICK_SCALE,
                    Duration = TimeSpan.FromMilliseconds(Constants.ZOOM_ANIMATION_DURATION_MS),
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTarget(scaleX, CanvasScale);
                Storyboard.SetTargetProperty(scaleX, new PropertyPath(ScaleTransform.ScaleXProperty));
                Storyboard.SetTarget(scaleY, CanvasScale);
                Storyboard.SetTargetProperty(scaleY, new PropertyPath(ScaleTransform.ScaleYProperty));

                storyboard.Children.Add(scaleX);
                storyboard.Children.Add(scaleY);
            }

            if (DarkOverlay != null)
            {
                var overlayAnimation = new DoubleAnimation
                {
                    To = Constants.DARK_OVERLAY_OPACITY,
                    Duration = TimeSpan.FromMilliseconds(Constants.ZOOM_ANIMATION_DURATION_MS / 2),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTarget(overlayAnimation, DarkOverlay);
                Storyboard.SetTargetProperty(overlayAnimation, new PropertyPath(UIElement.OpacityProperty));
                storyboard.Children.Add(overlayAnimation);
            }

            return storyboard;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) => HandleButtonHover(sender as Button);

        private void Button_MouseLeave(object sender, MouseEventArgs e) => HandleButtonLeave();

        private void Button2_MouseEnter(object sender, MouseEventArgs e) => HandleButtonHover(sender as Button);

        private void Button2_MouseLeave(object sender, MouseEventArgs e) => HandleButtonLeave();

        private void HandleButtonHover(Button? button)
        {
            if (button == null)
            {
                return;
            }

            ZoomOnButton(button);
        }

        private void HandleButtonLeave()
        {
            ResetCanvasZoom();
        }
    }
}