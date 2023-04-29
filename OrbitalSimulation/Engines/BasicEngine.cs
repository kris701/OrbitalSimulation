﻿using OrbitalSimulation.Helpers;
using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Engines
{
    public class BasicEngine : IPhysicsEngine
    {
        public int _currentID = 0;
        public HashSet<OrbiterObject> Objects { get; set; } = new HashSet<OrbiterObject>();

        private readonly double GravitationalConstant = 6.674 * Math.Pow(10, -11);

        public void AddNewObject(OrbiterObject obj)
        {
            obj.ID = _currentID++;
            Objects.Add(obj);
        }

        public Point GetOrbitalVector(OrbiterObject source, OrbiterObject orbiter)
        {
            Point orbitalVelocity = new Point();
            double angle = Math.Atan2((orbiter.Location.Y - source.Location.Y), (orbiter.Location.X - source.Location.X));
            double distance = PointHelper.Distance(source.Location, orbiter.Location);
            double force = Math.Sqrt((GravitationalConstant * (source.KgMass * orbiter.KgMass)) / distance);

            var cosx = Math.Cos(angle);
            var siny = Math.Sin(angle);

            orbitalVelocity.X = cosx * force;
            orbitalVelocity.Y = siny * force;

            return orbitalVelocity;
        }

        public double GetOrbitalPeriod(OrbiterObject source)
        {
            return (2 * Math.PI * Math.Pow(source.Radius, 3/2)) / Math.Sqrt(GravitationalConstant * source.KgMass);
        }

        public OrbiterObject? GetNearestObject(OrbiterObject to)
        {
            if (Objects.Count == 0)
                return null;
            if (Objects.Count == 1)
                return to;

            OrbiterObject? nearest = null;
            double nearestDistance = double.MaxValue;
            foreach(var obj in Objects)
            {
                if (obj != to)
                {
                    double dist = PointHelper.Distance(to.Location, obj.Location);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearest = obj;
                    }
                }
            }
            return nearest;
        }

        public UpdateResult Update(double tickMultiplier)
        {
            UpdateResult returnCode = UpdateResult.NothingChanged;
            if (Objects.Count == 0)
                return returnCode;

            if (tickMultiplier <= 1)
            {
                returnCode = UpdateLocations(returnCode, tickMultiplier);
                return CollisionCheck(returnCode);
            }
            else
            {
                for (int i = 0; i < (int)tickMultiplier; i++)
                {
                    returnCode = UpdateLocations(returnCode, 1);
                    returnCode = CollisionCheck(returnCode);
                }
            }

            return returnCode;
        }

        private UpdateResult UpdateLocations(UpdateResult returnCode, double tickMultiplier)
        {
            foreach (var obj in Objects)
            {
                if (obj.IsStationary)
                    continue;
                var newVelocity = CalculateNextLocation(obj);
                obj.Location = new Point(obj.Location.X + newVelocity.X * tickMultiplier, obj.Location.Y + newVelocity.Y * tickMultiplier);
                obj.VelocityVector = newVelocity;

                if (returnCode < UpdateResult.ObjectsUpdated)
                    returnCode = UpdateResult.ObjectsUpdated;
            }
            return returnCode;
        }

        private UpdateResult CollisionCheck(UpdateResult returnCode)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                HashSet<OrbiterObject> newCollidedObjects = GetCollisionSet(Objects.ElementAt(i), Objects);
                if (newCollidedObjects.Count > 0)
                {
                    var newObject = GetNewObjectFromSetOfObjects(newCollidedObjects);
                    foreach (var obj in newCollidedObjects)
                        Objects.RemoveWhere(x => x.GetHashCode() == obj.GetHashCode());
                    Objects.Add(newObject);

                    if (returnCode < UpdateResult.ObjectsAdded)
                        returnCode = UpdateResult.ObjectsAdded;

                    i = 0;
                }
            }
            return returnCode;
        }

        public Point CalculateNextLocation(OrbiterObject obj)
        {
            var newVelocity = new Point(obj.VelocityVector.X, obj.VelocityVector.Y);
            if (!obj.IsNoclip)
            {
                foreach (var objb in Objects)
                {
                    if (objb != obj)
                    {
                        if (!objb.IsNoclip)
                        {
                            var force = GetGravitationalConstantForce(obj, objb);
                            newVelocity.X += force.X;
                            newVelocity.Y += force.Y;

                            if (objb.HasAtmosphere)
                            {
                                var drag = GetAtmosphericDrag(obj, objb);
                                newVelocity.X += drag.X;
                                newVelocity.Y += drag.Y;
                            }
                        }
                    }
                }
            }
            return newVelocity;
        }

        private OrbiterObject GetNewObjectFromSetOfObjects(HashSet<OrbiterObject> objects)
        {
            var newObject = new OrbiterObject()
            {
                IsStationary = IsAnyStationary(objects),
                ID = _currentID++
            };

            // Total mass
            foreach (var obj in objects)
                newObject.KgMass += obj.KgMass;

            // (Weighted) Combined velocity of all the objects
            foreach (var obj in objects)
            {
                newObject.VelocityVector = new Point(
                    newObject.VelocityVector.X + obj.VelocityVector.X * (obj.KgMass / newObject.KgMass),
                    newObject.VelocityVector.Y + obj.VelocityVector.Y* (obj.KgMass / newObject.KgMass));
            }

            // (Weighted) Centroid position of all the objects.
            // https://en.wikipedia.org/wiki/Centroid#By_geometric_decomposition
            Point newLocation = new Point();
            foreach (var obj in objects)
            {
                newLocation.X += obj.Location.X * obj.KgMass;
                newLocation.Y += obj.Location.Y * obj.KgMass;
            }
            newObject.Location = new Point(
                newLocation.X / newObject.KgMass,
                newLocation.Y / newObject.KgMass);

            // Combined area for finding the new radius
            double combinedArea = 0;
            foreach (var obj in objects)
                combinedArea += CircleHelper.GetAreaOfRadius(obj.Radius);
            newObject.Radius = CircleHelper.GetRadiusFromArea(combinedArea);

            if (IsAnyAtmospheric(objects))
            {
                newObject.HasAtmosphere = true;

                newObject.AtmTopLevel = newObject.Radius;
                foreach (var obj in objects)
                    if (obj.HasAtmosphere)
                        newObject.AtmTopLevel += obj.AtmTopLevel - obj.Radius;

                foreach (var obj in objects)
                    if (obj.HasAtmosphere)
                        newObject.AtmSeaLevelDensity += obj.AtmSeaLevelDensity;
            }

            return newObject;
        }

        private HashSet<OrbiterObject> GetCollisionSet(OrbiterObject self, HashSet<OrbiterObject> objects)
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

        private bool IsAnyAtmospheric(HashSet<OrbiterObject> objects)
        {
            foreach (var obj in objects)
                if (obj.HasAtmosphere)
                    return true;
            return false;
        }

        private Point GetGravitationalConstantForce(OrbiterObject obja, OrbiterObject objb)
        {
            Point accelerationVector = new Point();

            double distance = PointHelper.Distance(obja.Location, objb.Location);
            double constant = ((GravitationalConstant * obja.KgMass * objb.KgMass) / Math.Pow(distance,2));
            double force = constant / obja.KgMass;
            double angle = Math.Atan2((objb.Location.Y - obja.Location.Y), (objb.Location.X - obja.Location.X));

            var cosx = Math.Cos(angle);
            var siny = Math.Sin(angle);

            accelerationVector.X = cosx * force;
            accelerationVector.Y = siny * force;

            return accelerationVector;
        }

        private Point GetAtmosphericDrag(OrbiterObject obja, OrbiterObject objb)
        {
            Point drag = new Point();

            var distance = PointHelper.Distance(obja.Location, objb.Location);
            if (distance > objb.AtmTopLevel)
                return drag;
            var densityAtAltitude = objb.GetDensityAtAltitude(distance);

            var velocity = GetLengthOfVector(obja.VelocityVector);
            var area = CircleHelper.GetAreaOfRadius(obja.Radius);
            var dragCoefficiency = 0.47;
            var dragForce = -((double)1 / (double)2) * ((densityAtAltitude * dragCoefficiency * area) / obja.KgMass) * velocity;

            var angle = Math.Atan(obja.VelocityVector.Y / obja.VelocityVector.X);

            drag.X = (Math.Cos(angle) * dragForce);
            drag.Y = (Math.Sin(angle) * dragForce);

            return drag;
        }

        public double GetLengthOfVector(Point vector) => Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));

        public List<Point> PredictPath(OrbiterObject obj, int maxPathPoints, double maxPathLength)
        {
            List<Point> returnPoints = new List<Point>();
            OrbiterObject tempObject = new OrbiterObject(obj);
            double currentLength = 0;
            int current = 0;
            while (currentLength < maxPathLength && returnPoints.Count < maxPathPoints)
            {
                var newVelocity = CalculateNextLocation(tempObject);
                tempObject.Location = new Point(tempObject.Location.X + newVelocity.X, tempObject.Location.Y + newVelocity.Y);
                tempObject.VelocityVector = newVelocity;
                currentLength += GetLengthOfVector(newVelocity);
                if (current++ % 100 == 0)
                    returnPoints.Add(new Point(tempObject.Location.X, tempObject.Location.Y));
            }

            return returnPoints;
        }
    }
}
