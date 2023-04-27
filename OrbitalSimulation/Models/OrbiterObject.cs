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
        public int ID { get; set; } = -1;
        public bool IsStationary { get; set; }
        public Point Location { get; set; }
        public Point VelocityVector { get; set; }
        public double KgMass { get; set; }
        public double Radius { get; set; }
        public bool IsNoclip { get; set; }

        public OrbiterObject(bool isStationary, Point location, Point velocityVector, double kgMass, double radius, bool isNoclip = false)
        {
            IsStationary = isStationary;
            Location = location;
            VelocityVector = velocityVector;
            KgMass = kgMass;
            Radius = radius;
            IsNoclip = isNoclip;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as OrbiterObject);
        }

        public bool Equals(OrbiterObject? other)
        {
            return other is not null &&
                   ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID);
        }

        public static bool operator ==(OrbiterObject obj1, OrbiterObject obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(obj1, null))
                return false;
            if (ReferenceEquals(obj2, null))
                return false;
            return obj1.Equals(obj2);
        }
        public static bool operator !=(OrbiterObject obj1, OrbiterObject obj2) => !(obj1 == obj2);
    }
}
