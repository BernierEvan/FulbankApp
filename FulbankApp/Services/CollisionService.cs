using FulbankApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FulbankApp.Services
{
    /// <summary>
    /// Service de détection de collisions géométriques
    /// </summary>
    public class CollisionService
    {
        /// <summary>
        /// Vérifie si un rectangle collisionne avec un obstacle transformé
        /// </summary>
        public bool IsCollidingWithTransformedRectangle(Rect playerRect, Rectangle obstacle)
        {
            var playerGeometry = new RectangleGeometry(playerRect);
            var obstacleGeometry = CreateTransformedGeometry(obstacle);

            var intersection = Geometry.Combine(playerGeometry, obstacleGeometry, GeometryCombineMode.Intersect, null);
            return !intersection.IsEmpty();
        }

        /// <summary>
        /// Vérifie si un rectangle collisionne avec plusieurs obstacles
        /// </summary>
        public bool IsCollidingWithAny(Rect playerRect, IEnumerable<Rectangle> obstacles)
        {
            return obstacles.Any(obstacle => IsCollidingWithTransformedRectangle(playerRect, obstacle));
        }

        /// <summary>
        /// Crée une géométrie transformée pour un rectangle
        /// </summary>
        private RectangleGeometry CreateTransformedGeometry(Rectangle rectangle)
        {
            var geometry = new RectangleGeometry(new Rect(0, 0, rectangle.Width, rectangle.Height));
            var transform = BuildTransformGroup(rectangle);
            geometry.Transform = transform;
            return geometry;
        }

        /// <summary>
        /// Construit le TransformGroup complet pour un rectangle
        /// </summary>
        private TransformGroup BuildTransformGroup(Rectangle rectangle)
        {
            var transformGroup = new TransformGroup();
            var renderOrigin = rectangle.RenderTransformOrigin;

            double originX = rectangle.Width * renderOrigin.X;
            double originY = rectangle.Height * renderOrigin.Y;

            // Translation vers l'origine pour rotation
            transformGroup.Children.Add(new TranslateTransform(-originX, -originY));

            // RenderTransform de l'obstacle (rotation, scale, etc.)
            if (rectangle.RenderTransform != null && rectangle.RenderTransform != Transform.Identity)
            {
                transformGroup.Children.Add(rectangle.RenderTransform);
            }

            // Translation retour depuis l'origine
            transformGroup.Children.Add(new TranslateTransform(originX, originY));

            // Position dans le canvas
            double canvasLeft = Canvas.GetLeft(rectangle);
            double canvasTop = Canvas.GetTop(rectangle);
            transformGroup.Children.Add(new TranslateTransform(canvasLeft, canvasTop));

            return transformGroup;
        }

        /// <summary>
        /// Récupère les coins transformés d'un rectangle
        /// </summary>
        public Point[] GetTransformedCorners(Rectangle rectangle)
        {
            double width = rectangle.Width;
            double height = rectangle.Height;

            Point[] corners = new Point[]
            {
                new Point(0, 0),
                new Point(width, 0),
                new Point(width, height),
                new Point(0, height)
            };

            var transform = BuildTransformGroup(rectangle);

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = transform.Transform(corners[i]);
            }

            return corners;
        }

        /// <summary>
        /// Vérifie si un point est dans un polygone (ray casting algorithm)
        /// </summary>
        public bool IsPointInPolygon(Point point, Point[] polygon)
        {
            int j = polygon.Length - 1;
            bool isInside = false;

            for (int i = 0; i < polygon.Length; i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                               (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
                j = i;
            }

            return isInside;
        }

        /// <summary>
        /// Vérifie si deux segments de ligne se croisent
        /// </summary>
        public bool DoLineSegmentsIntersect(Point p1, Point p2, Point p3, Point p4)
        {
            double denominator = (p2.X - p1.X) * (p4.Y - p3.Y) - (p2.Y - p1.Y) * (p4.X - p3.X);

            if (Math.Abs(denominator) < Constants.COLLISION_TOLERANCE)
                return false;

            double t = ((p3.X - p1.X) * (p4.Y - p3.Y) - (p3.Y - p1.Y) * (p4.X - p3.X)) / denominator;
            double u = ((p3.X - p1.X) * (p2.Y - p1.Y) - (p3.Y - p1.Y) * (p2.X - p1.X)) / denominator;

            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }

        /// <summary>
        /// Vérifie l'intersection entre deux polygones
        /// </summary>
        public bool DoPolygonsIntersect(Point[] poly1, Point[] poly2)
        {
            // Vérifier si un coin d'un polygone est dans l'autre
            foreach (var corner in poly1)
            {
                if (IsPointInPolygon(corner, poly2))
                    return true;
            }

            foreach (var corner in poly2)
            {
                if (IsPointInPolygon(corner, poly1))
                    return true;
            }

            // Vérifier l'intersection des arêtes
            return CheckEdgeIntersections(poly1, poly2);
        }

        /// <summary>
        /// Vérifie si les arêtes de deux polygones se croisent
        /// </summary>
        private bool CheckEdgeIntersections(Point[] poly1, Point[] poly2)
        {
            for (int i = 0; i < poly1.Length; i++)
            {
                Point p1 = poly1[i];
                Point p2 = poly1[(i + 1) % poly1.Length];

                for (int j = 0; j < poly2.Length; j++)
                {
                    Point p3 = poly2[j];
                    Point p4 = poly2[(j + 1) % poly2.Length];

                    if (DoLineSegmentsIntersect(p1, p2, p3, p4))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Calcule le rectangle de hitbox réduit
        /// </summary>
        public Rect GetHitboxRect(Rect originalRect, double shrink = Constants.HITBOX_SHRINK)
        {
            return new Rect(
                originalRect.X + shrink,
                originalRect.Y + shrink,
                Math.Max(0, originalRect.Width - shrink * 2),
                Math.Max(0, originalRect.Height - shrink * 2)
            );
        }
    }
}