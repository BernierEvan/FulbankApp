using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FulbankApp.Helpers
{
    /// <summary>
    /// Classe utilitaire pour gérer les animations WPF de manière centralisée
    /// </summary>
    public static class AnimationHelper
    {
        #region Animations de Fondu (Fade)

        /// <summary>
        /// Anime l'apparition progressive d'un élément (opacité 0 → 1)
        /// </summary>
        /// <param name="element">L'élément à animer</param>
        /// <param name="durationMs">Durée de l'animation en millisecondes</param>
        /// <param name="onComplete">Action à exécuter à la fin de l'animation (optionnel)</param>
        public static void FadeIn(UIElement element, int durationMs = Constants.FADE_IN_DURATION_MS, Action onComplete = null)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            if (onComplete != null)
            {
                animation.Completed += (s, e) => onComplete();
            }

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        /// <summary>
        /// Anime la disparition progressive d'un élément (opacité 1 → 0)
        /// </summary>
        /// <param name="element">L'élément à animer</param>
        /// <param name="durationMs">Durée de l'animation en millisecondes</param>
        /// <param name="onComplete">Action à exécuter à la fin de l'animation (optionnel)</param>
        public static void FadeOut(UIElement element, int durationMs = Constants.FADE_OUT_DURATION_MS, Action onComplete = null)
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            if (onComplete != null)
            {
                animation.Completed += (s, e) => onComplete();
            }

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        #endregion

        #region Animations de Mise à l'Échelle (Scale)

        /// <summary>
        /// Anime la mise à l'échelle d'un élément
        /// Crée automatiquement un ScaleTransform si nécessaire
        /// </summary>
        /// <param name="element">L'élément à mettre à l'échelle</param>
        /// <param name="targetScale">Échelle cible (1.0 = taille normale)</param>
        /// <param name="durationMs">Durée de l'animation en millisecondes</param>
        /// <param name="easingFunction">Fonction d'accélération/décélération (optionnel)</param>
        public static void AnimateScale(UIElement element, double targetScale, int durationMs, EasingFunctionBase easingFunction = null)
        {
            // Vérifier si un ScaleTransform existe déjà
            ScaleTransform scaleTransform;

            if (element.RenderTransform is ScaleTransform existingTransform)
            {
                scaleTransform = existingTransform;
            }
            else
            {
                // Créer un nouveau ScaleTransform
                scaleTransform = new ScaleTransform(1, 1);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5); // Centre de l'élément
            }

            // Créer les animations pour X et Y
            var animationX = new DoubleAnimation
            {
                To = targetScale,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = easingFunction
            };

            var animationY = new DoubleAnimation
            {
                To = targetScale,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = easingFunction
            };

            // Appliquer les animations
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animationX);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animationY);
        }

        /// <summary>
        /// Simule un effet de pression sur un bouton (scale down puis retour)
        /// </summary>
        /// <param name="button">Le bouton à animer</param>
        public static void SimulateButtonPress(Button button)
        {
            var scaleTransform = new ScaleTransform(1, 1);
            button.RenderTransform = scaleTransform;
            button.RenderTransformOrigin = new Point(0.5, 0.5);

            // Animation: réduction puis retour à la normale
            var animation = new DoubleAnimation
            {
                From = Constants.BUTTON_PRESS_SCALE,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(Constants.BUTTON_PRESS_DURATION_MS)
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        #endregion

        #region Animations de Zoom sur Canvas

        /// <summary>
        /// Effectue un zoom centré sur un bouton dans un Canvas
        /// </summary>
        /// <param name="button">Le bouton sur lequel zoomer</param>
        /// <param name="canvas">Le Canvas parent</param>
        /// <param name="canvasScale">Le ScaleTransform du Canvas</param>
        /// <param name="canvasTranslate">Le TranslateTransform du Canvas</param>
        /// <param name="darkOverlay">L'overlay sombre à afficher (optionnel)</param>
        /// <param name="targetScale">Facteur de zoom (1.5 = 150%)</param>
        /// <param name="durationMs">Durée de l'animation</param>
        public static void ZoomOnCanvasButton(
            Button button,
            Canvas canvas,
            ScaleTransform canvasScale,
            TranslateTransform canvasTranslate,
            FrameworkElement darkOverlay,
            double targetScale = Constants.ZOOM_CLICK_SCALE,
            int durationMs = Constants.ZOOM_ANIMATION_DURATION_MS)
        {
            // Afficher l'overlay sombre si fourni
            if (darkOverlay != null)
            {
                darkOverlay.Opacity = Constants.DARK_OVERLAY_OPACITY;
            }

            // Calculer le centre du bouton
            double centerX = Canvas.GetLeft(button) + (button.Width / 2);
            double centerY = Canvas.GetTop(button) + (button.Height / 2);

            // Calculer la translation nécessaire pour centrer le zoom
            double translateX = -(centerX * (targetScale - 1));
            double translateY = -(centerY * (targetScale - 1));

            // Appliquer les animations
            AnimateDoubleProperty(canvasScale, ScaleTransform.ScaleXProperty, targetScale, durationMs);
            AnimateDoubleProperty(canvasScale, ScaleTransform.ScaleYProperty, targetScale, durationMs);
            AnimateDoubleProperty(canvasTranslate, TranslateTransform.XProperty, translateX, durationMs);
            AnimateDoubleProperty(canvasTranslate, TranslateTransform.YProperty, translateY, durationMs);
        }

        /// <summary>
        /// Réinitialise le zoom du Canvas (retour à l'échelle 1.0)
        /// </summary>
        /// <param name="canvasScale">Le ScaleTransform du Canvas</param>
        /// <param name="canvasTranslate">Le TranslateTransform du Canvas</param>
        /// <param name="darkOverlay">L'overlay sombre à masquer (optionnel)</param>
        /// <param name="durationMs">Durée de l'animation</param>
        public static void ResetCanvasZoom(
            ScaleTransform canvasScale,
            TranslateTransform canvasTranslate,
            FrameworkElement darkOverlay,
            int durationMs = Constants.ZOOM_ANIMATION_DURATION_MS)
        {
            // Masquer l'overlay sombre
            if (darkOverlay != null)
            {
                darkOverlay.Opacity = 0;
            }

            // Réinitialiser le zoom et la position
            AnimateDoubleProperty(canvasScale, ScaleTransform.ScaleXProperty, 1.0, durationMs);
            AnimateDoubleProperty(canvasScale, ScaleTransform.ScaleYProperty, 1.0, durationMs);
            AnimateDoubleProperty(canvasTranslate, TranslateTransform.XProperty, 0, durationMs);
            AnimateDoubleProperty(canvasTranslate, TranslateTransform.YProperty, 0, durationMs);
        }

        #endregion

        #region Animations Diverses

        /// <summary>
        /// Crée une animation de pulsation infinie (scale qui varie en boucle)
        /// </summary>
        /// <param name="element">L'élément à animer</param>
        /// <param name="minScale">Échelle minimale</param>
        /// <param name="maxScale">Échelle maximale</param>
        /// <param name="durationSeconds">Durée d'un cycle en secondes</param>
        public static void CreatePulseAnimation(
            UIElement element,
            double minScale = Constants.HALO_PULSE_MIN_SCALE,
            double maxScale = Constants.HALO_PULSE_MAX_SCALE,
            int durationSeconds = 1)
        {
            // Vérifier si un ScaleTransform existe déjà
            ScaleTransform scaleTransform;

            if (element.RenderTransform is ScaleTransform existingTransform)
            {
                scaleTransform = existingTransform;
            }
            else
            {
                scaleTransform = new ScaleTransform(1, 1);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            // Créer l'animation avec AutoReverse et répétition infinie
            var animation = new DoubleAnimation
            {
                From = minScale,
                To = maxScale,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        #endregion

        #region Méthodes Utilitaires Privées

        /// <summary>
        /// Anime une propriété de type Double sur un objet Animatable
        /// FIX: Utilise Animatable au lieu de DependencyObject
        /// </summary>
        /// <param name="target">L'objet cible (doit être Animatable)</param>
        /// <param name="property">La propriété à animer</param>
        /// <param name="toValue">La valeur cible</param>
        /// <param name="durationMs">Durée de l'animation</param>
        private static void AnimateDoubleProperty(
            Animatable target,
            DependencyProperty property,
            double toValue,
            int durationMs)
        {
            var animation = new DoubleAnimation
            {
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                FillBehavior = FillBehavior.HoldEnd // Maintenir la valeur finale
            };

            target.BeginAnimation(property, animation);
        }

        #endregion
    }
}