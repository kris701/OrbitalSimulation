using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalSimulation.Helpers
{
    public static class CircleHelper
    {
        public static double GetAreaOfRadius(double radius) => Math.PI * Math.Pow(radius, 2);
        public static double GetRadiusFromArea(double area) => Math.Sqrt(area / Math.PI);
    }
}
