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

        public double GetHorisontalOrbitalSpeed(OrbiterObject source, OrbiterObject orbiter)
        {
            var distance = Distance(source.Location, orbiter.Location);
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
                foreach (var objb in objects)
                {
                    if (objb != obja)
                    {
                        var force = GetGravitationalConstantForce(obja, objb);
                        newVelocity.X += force.X;
                        newVelocity.Y += force.Y;
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

        private double GetAreaOfRadius(double radius) => Math.PI * Math.Pow(radius, 2);
        private double GetRadiusFromArea(double area) => Math.Sqrt(area / Math.PI);

        private OrbiterObject GetNewObjectFromSetOfObjects(HashSet<OrbiterObject> objects)
        {
            Point combinedVelocity = new Point();
            foreach (var obj in objects)
            {
                combinedVelocity.X += obj.VelocityVector.X;
                combinedVelocity.Y += obj.VelocityVector.Y;
            }

            Point newLocation = CalculateCentroid(objects);

            double combinedArea = 0;
            foreach (var obj in objects)
                combinedArea += GetAreaOfRadius(obj.Radius);
            double newRadius = GetRadiusFromArea(combinedArea);

            double combinedMass = 0;
            foreach (var obj in objects)
                combinedMass += obj.KgMass;

            return new OrbiterObject(
                IsAnyStationary(objects),
                newLocation,
                combinedVelocity,
                combinedMass,
                newRadius);
        }

        private Point CalculateCentroid(HashSet<OrbiterObject> objects)
        {
            Point currentPoint = new Point();

            foreach(var obj in objects)
            {
                currentPoint.X += obj.Location.X;
                currentPoint.Y += obj.Location.Y;
            }

            currentPoint.X = (1 / (double)objects.Count) * currentPoint.X;
            currentPoint.Y = (1 / (double)objects.Count) * currentPoint.Y;

            return currentPoint;
        }

        private HashSet<OrbiterObject> GetCollisionSet(OrbiterObject self, List<OrbiterObject> objects)
        {
            HashSet<OrbiterObject> collidedObjects = new HashSet<OrbiterObject>();
            foreach (var obj in objects) {
                if (obj != self) {
                    if (Distance(obj.Location, self.Location) <= (obj.Radius + self.Radius))
                    {
                        collidedObjects.Add(obj);
                        collidedObjects.Add(self);
                    }
                }
            }
            return collidedObjects;
        }

        private bool IsAnyStationary(HashSet<OrbiterObject> objects)
        {
            foreach (var obj in objects)
                if (obj.IsStationary)
                    return true;
            return false;
        }

        private double Distance(Point a, Point b)
        {
            return Math.Sqrt(
                Math.Pow(a.X - b.X, 2) +
                Math.Pow(a.Y - b.Y, 2)
                );
        }

        private Point GetGravitationalConstantForce(OrbiterObject obja, OrbiterObject objb)
        {
            double distance = Distance(obja.Location, objb.Location);
            double constant = GravitationalConstant * ((obja.KgMass * objb.KgMass) / Math.Pow(distance,2));
            double angle = Math.Atan2((objb.Location.Y - obja.Location.Y), (objb.Location.X - obja.Location.X));
            Point force = new Point();

            var visualAngle = angle * (180 / Math.PI);

            var cosx = Math.Cos(angle);
            var siny = Math.Sin(angle);

            force.X = cosx * constant;
            force.Y = siny * constant;

            return force;
        }
    }
}
