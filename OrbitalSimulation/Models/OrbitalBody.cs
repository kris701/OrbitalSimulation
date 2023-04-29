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
        public string Image { get; set; } = "";

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

        public OrbitalBody(HashSet<OrbitalBody> bodies, int newID = -1)
        {
            IsStationary = IsAnyStationary(bodies);
            ID = newID;

            // Largest object keeps its image
            string largestImage = "";
            double largestMass = 0;
            foreach(var obj in bodies)
            {
                if (obj.KgMass > largestMass)
                {
                    largestMass = obj.KgMass;
                    largestImage = obj.Image;
                }
            }
            Image = largestImage;

            // Total mass
            foreach (var obj in bodies)
                KgMass += obj.KgMass;

            // (Weighted) Combined velocity of all the objects
            foreach (var obj in bodies)
            {
                VelocityVector = new Point(
                    VelocityVector.X + obj.VelocityVector.X * (obj.KgMass / KgMass),
                    VelocityVector.Y + obj.VelocityVector.Y * (obj.KgMass / KgMass));
            }

            // (Weighted) Centroid position of all the objects.
            // https://en.wikipedia.org/wiki/Centroid#By_geometric_decomposition
            Point newLocation = new Point();
            foreach (var obj in bodies)
            {
                newLocation.X += obj.Location.X * obj.KgMass;
                newLocation.Y += obj.Location.Y * obj.KgMass;
            }
            Location = new Point(
                newLocation.X / KgMass,
                newLocation.Y / KgMass);

            // Combined area for finding the new radius
            double combinedArea = 0;
            foreach (var obj in bodies)
                combinedArea += CircleHelper.GetAreaOfRadius(obj.Radius);
            Radius = CircleHelper.GetRadiusFromArea(combinedArea);

            // Combine atmosphere if any
            if (IsAnyAtmospheric(bodies))
            {
                HasAtmosphere = true;

                AtmTopLevel = Radius;
                foreach (var obj in bodies)
                    if (obj.HasAtmosphere)
                        AtmTopLevel += obj.AtmTopLevel - obj.Radius;

                foreach (var obj in bodies)
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

        public double GetVelocity() => Math.Sqrt(Math.Pow(VelocityVector.X, 2) + Math.Pow(VelocityVector.Y, 2));

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

        public double GetDensityAtAltitude(double distanceFromCenter)
        {
            return AtmSeaLevelDensity * Math.Pow((1 - ((distanceFromCenter - Radius) / (AtmTopLevel - Radius))), 2);
        }
    }
}
