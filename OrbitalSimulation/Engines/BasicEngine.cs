using OrbitalSimulation.Helpers;
using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Engines
{
    public class BasicEngine : IPhysicsEngine
    {
        private readonly double GravitationalConstant = 6.674 * Math.Pow(10, -11);
        private Random _rnd = new Random();

        public double GetHorisontalOrbitalSpeed(OrbiterObject source, OrbiterObject orbiter)
        {
            var distance = PointHelper.Distance(source.Location, orbiter.Location);
            return Math.Sqrt((GravitationalConstant * (source.KgMass * orbiter.KgMass)) / distance);
        }

        public double GetOrbitalPeriod(OrbiterObject source)
        {
            return (2 * Math.PI * Math.Pow(source.Radius, 3/2)) / Math.Sqrt(GravitationalConstant * source.KgMass);
        }

        public bool Update(List<OrbiterObject> objects)
        {
            bool requireUIRefresh = false;

            // Calculate Movement
            foreach (var obja in objects)
            {
                if (obja.IsStationary)
                    continue;

                var newVelocity = new Point(obja.VelocityVector.X, obja.VelocityVector.Y);
                if (!obja.IsNoclip)
                {
                    foreach (var objb in objects)
                    {
                        if (objb != obja)
                        {
                            if (!objb.IsNoclip)
                            {
                                var force = GetGravitationalConstantForce(obja, objb);
                                newVelocity.X += force.X;
                                newVelocity.Y += force.Y;
                            }
                        }
                    }
                }
                obja.Location = new Point(obja.Location.X + newVelocity.X, obja.Location.Y + newVelocity.Y);
                obja.VelocityVector = newVelocity;
            }

            // Calculate Collisions
            for (int i = 0; i < objects.Count; i++)
            {
                HashSet<OrbiterObject> newCollidedObjects = GetCollisionSet(objects[i], objects);
                if (newCollidedObjects.Count > 0)
                {
                    var newObject = GetNewObjectFromSetOfObjects(newCollidedObjects);
                    foreach (var obj in newCollidedObjects)
                        objects.Remove(obj);
                    objects.Add(newObject);

                    requireUIRefresh = true;

                    i = 0;
                }
            }

            return requireUIRefresh;
        }

        private OrbiterObject GetNewObjectFromSetOfObjects(HashSet<OrbiterObject> objects)
        {
            // Total mass
            double combinedMass = 0;
            foreach (var obj in objects)
                combinedMass += obj.KgMass;

            // (Weighted) Combined velocity of all the objects
            Point combinedVelocity = new Point();
            foreach (var obj in objects)
            {
                combinedVelocity.X += obj.VelocityVector.X * (obj.KgMass / combinedMass);
                combinedVelocity.Y += obj.VelocityVector.Y * (obj.KgMass / combinedMass);
            }

            // (Weighted) Centroid position of all the objects.
            // https://en.wikipedia.org/wiki/Centroid#By_geometric_decomposition
            Point newLocation = new Point();
            foreach (var obj in objects)
            {
                newLocation.X += obj.Location.X * obj.KgMass;
                newLocation.Y += obj.Location.Y * obj.KgMass;
            }
            newLocation.X = newLocation.X / combinedMass;
            newLocation.Y = newLocation.Y / combinedMass;

            // Combined area for finding the new radius
            double combinedArea = 0;
            foreach (var obj in objects)
                combinedArea += CircleHelper.GetAreaOfRadius(obj.Radius);
            double newRadius = CircleHelper.GetRadiusFromArea(combinedArea);

            return new OrbiterObject(
                IsAnyStationary(objects),
                newLocation,
                combinedVelocity,
                combinedMass,
                newRadius);
        }

        public double GetVelocityEnergy(Point velocity) => Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2));

        public double GetPseudoDoubleWithinRange(double lowerBound, double upperBound)
        {
            var rDouble = _rnd.NextDouble();
            var rRangeDouble = rDouble * (upperBound - lowerBound) + lowerBound;
            return rRangeDouble;
        }

        private HashSet<OrbiterObject> GetCollisionSet(OrbiterObject self, List<OrbiterObject> objects)
        {
            HashSet<OrbiterObject> collidedObjects = new HashSet<OrbiterObject>();
            bool isFreeFromCollisions = true;
            foreach (var obj in objects) 
            {
                if (obj != self) 
                {
                    if (PointHelper.Distance(obj.Location, self.Location) <= (obj.Radius + self.Radius))
                    {
                        if (!obj.IsNoclip && !self.IsNoclip)
                        {
                            collidedObjects.Add(obj);
                            collidedObjects.Add(self);
                        }
                        else if (self.IsNoclip)
                            isFreeFromCollisions = false;
                    }
                }
            }
            if (isFreeFromCollisions)
                self.IsNoclip = false;
            return collidedObjects;
        }

        private bool IsAnyStationary(HashSet<OrbiterObject> objects)
        {
            foreach (var obj in objects)
                if (obj.IsStationary)
                    return true;
            return false;
        }

        private Point GetGravitationalConstantForce(OrbiterObject obja, OrbiterObject objb)
        {
            double distance = PointHelper.Distance(obja.Location, objb.Location);
            double constant = ((GravitationalConstant * obja.KgMass * objb.KgMass) / Math.Pow(distance,2));
            double acceleration = constant / obja.KgMass;
            double angle = Math.Atan2((objb.Location.Y - obja.Location.Y), (objb.Location.X - obja.Location.X));
            Point force = new Point();

            var cosx = Math.Cos(angle);
            var siny = Math.Sin(angle);

            force.X = cosx * acceleration;
            force.Y = siny * acceleration;

            return force;
        }
    }
}
