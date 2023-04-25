using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Helpers
{
    public static class PointHelper
    {
        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt(
                Math.Pow(a.X - b.X, 2) +
                Math.Pow(a.Y - b.Y, 2)
                );
        }

        public static Point CalculateCentroid(List<Point> points)
        {
            Point centroid = new Point();

            foreach (var point in points)
            {
                centroid.X += point.X;
                centroid.Y += point.Y;
            }

            centroid.X = (1 / (double)points.Count) * centroid.X;
            centroid.Y = (1 / (double)points.Count) * centroid.Y;

            return centroid;
        }
    }
}
