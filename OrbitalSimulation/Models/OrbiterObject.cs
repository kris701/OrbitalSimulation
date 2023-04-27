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
        public bool IsStationary { get; set; } = false;
        public Point Location { get; set; } = new Point();
        public Point VelocityVector { get; set; } = new Point();
        public double KgMass { get; set; } = 0;
        public double Radius { get; set; } = 0;
        public bool IsNoclip { get; set; } = false;

        public OrbiterObject()
        {

        }

        public OrbiterObject(OrbiterObject other)
        {
            ID = other.ID;
            IsStationary = other.IsStationary;
            Location = other.Location;
            VelocityVector = other.VelocityVector;
            KgMass = other.KgMass;
            Radius = other.Radius;
            IsNoclip = other.IsNoclip;
        }

        public OrbiterObject(bool isStationary, Point location, Point velocityVector, double kgMass, double radius, bool isNoclip = false, int iD = -1)
        {
            IsStationary = isStationary;
            Location = location;
            VelocityVector = velocityVector;
            KgMass = kgMass;
            Radius = radius;
            IsNoclip = isNoclip;
            ID = iD;
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
