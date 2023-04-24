using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Models
{
    public class OrbiterObject
    {
        public bool IsCollided { get; set; }
        public bool IsStationary { get; set; }
        public Point Location { get; set; }
        public Point VelocityVector { get; set; }
        public double KgMass { get; set; }
        public double Radius { get; set; }

        public OrbiterObject(bool isStationary, Point location, Point velocityVector, double kgMass, double radius)
        {
            IsStationary = isStationary;
            Location = location;
            VelocityVector = velocityVector;
            KgMass = kgMass;
            Radius = radius;
        }
    }
}
