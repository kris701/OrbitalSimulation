using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Models
{
    public class OrbiterObject : IEquatable<OrbiterObject?>
    {
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as OrbiterObject);
        }

        public bool Equals(OrbiterObject? other)
        {
            return other is not null &&
                   IsStationary == other.IsStationary &&
                   EqualityComparer<Point>.Default.Equals(Location, other.Location) &&
                   EqualityComparer<Point>.Default.Equals(VelocityVector, other.VelocityVector) &&
                   KgMass == other.KgMass &&
                   Radius == other.Radius;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsStationary, Location, VelocityVector, KgMass, Radius);
        }
    }
}
