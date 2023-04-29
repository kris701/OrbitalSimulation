using OrbitalSimulation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Models
{
    public class OrbitalBody : IEquatable<OrbitalBody?>
    {
        public int ID { get; set; } = -1;
        public bool IsStationary { get; set; } = false;
        public Point Location { get; set; } = new Point();
        public Point VelocityVector { get; set; } = new Point();
        public double KgMass { get; set; } = 0;
        public double Radius { get; set; } = 0;
        public bool IsNoclip { get; set; } = false;
        public bool HasAtmosphere { get; set; } = false;
        public double AtmSeaLevelDensity { get; set; } = 0;
        public double AtmTopLevel { get; set; } = 0;

        public OrbitalBody()
        {

        }

        public OrbitalBody(OrbitalBody other)
        {
            ID = other.ID;
            IsStationary = other.IsStationary;
            Location = other.Location;
            VelocityVector = other.VelocityVector;
            KgMass = other.KgMass;
            Radius = other.Radius;
            IsNoclip = other.IsNoclip;
            HasAtmosphere = other.HasAtmosphere;
            AtmSeaLevelDensity = other.AtmSeaLevelDensity;
            AtmTopLevel = other.AtmTopLevel;
        }

        public OrbitalBody(bool isStationary, Point location, Point velocityVector, double kgMass, double radius, bool isNoclip = false, int iD = -1)
        {
            IsStationary = isStationary;
            Location = location;
            VelocityVector = velocityVector;
            KgMass = kgMass;
            Radius = radius;
            IsNoclip = isNoclip;
            ID = iD;
        }

        public OrbitalBody(HashSet<OrbitalBody> bodys, int newID = -1)
        {
            IsStationary = IsAnyStationary(bodys);
            ID = newID;

            // Total mass
            foreach (var obj in bodys)
                KgMass += obj.KgMass;

            // (Weighted) Combined velocity of all the objects
            foreach (var obj in bodys)
            {
                VelocityVector = new Point(
                    VelocityVector.X + obj.VelocityVector.X * (obj.KgMass / KgMass),
                    VelocityVector.Y + obj.VelocityVector.Y * (obj.KgMass / KgMass));
            }

            // (Weighted) Centroid position of all the objects.
            // https://en.wikipedia.org/wiki/Centroid#By_geometric_decomposition
            Point newLocation = new Point();
            foreach (var obj in bodys)
            {
                newLocation.X += obj.Location.X * obj.KgMass;
                newLocation.Y += obj.Location.Y * obj.KgMass;
            }
            Location = new Point(
                newLocation.X / KgMass,
                newLocation.Y / KgMass);

            // Combined area for finding the new radius
            double combinedArea = 0;
            foreach (var obj in bodys)
                combinedArea += CircleHelper.GetAreaOfRadius(obj.Radius);
            Radius = CircleHelper.GetRadiusFromArea(combinedArea);

            // Combine atmosphere if any
            if (IsAnyAtmospheric(bodys))
            {
                HasAtmosphere = true;

                AtmTopLevel = Radius;
                foreach (var obj in bodys)
                    if (obj.HasAtmosphere)
                        AtmTopLevel += obj.AtmTopLevel - obj.Radius;

                foreach (var obj in bodys)
                    if (obj.HasAtmosphere)
                        AtmSeaLevelDensity += obj.AtmSeaLevelDensity;
            }
        }

        private bool IsAnyStationary(HashSet<OrbitalBody> bodies)
        {
            foreach (var obj in bodies)
                if (obj.IsStationary)
                    return true;
            return false;
        }

        private bool IsAnyAtmospheric(HashSet<OrbitalBody> bodies)
        {
            foreach (var obj in bodies)
                if (obj.HasAtmosphere)
                    return true;
            return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as OrbitalBody);
        }

        public bool Equals(OrbitalBody? other)
        {
            return other is not null &&
                   ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID);
        }

        public static bool operator ==(OrbitalBody obj1, OrbitalBody obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(obj1, null))
                return false;
            if (ReferenceEquals(obj2, null))
                return false;
            return obj1.Equals(obj2);
        }
        public static bool operator !=(OrbitalBody obj1, OrbitalBody obj2) => !(obj1 == obj2);

        // de Pater and Lissauer 2010
        public double GetDensityAtAltitude(double distanceFromCenter)
        {
            return AtmSeaLevelDensity * Math.Exp((-(distanceFromCenter - Radius) / AtmTopLevel));
        }
    }
}
