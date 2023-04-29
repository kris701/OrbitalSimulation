using OrbitalSimulation.Helpers;
using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static OrbitalSimulation.Engines.IPhysicsEngine;

namespace OrbitalSimulation.Engines
{
    public class BasicEngine : IPhysicsEngine
    {
        public event CollisioEventHandler? CollisionOccured;
        public event BodyDeletedHandler? BodyDeleted;
        public event BodyAddedHandler? BodyAdded;

        public HashSet<OrbitalBody> Bodies { get; set; } = new HashSet<OrbitalBody>();

        private int _currentID = 0;

        private readonly double GravitationalConstant = 6.674 * Math.Pow(10, -11);

        public void AddNewBody(OrbitalBody body)
        {
            body.ID = _currentID++;
            Bodies.Add(body);
        }

        public Point GetOrbitalVector(OrbitalBody satelliteBody, OrbitalBody anchorBody)
        {
            Point orbitalVelocity = new Point();
            double angle = Math.Atan2((satelliteBody.Location.Y - anchorBody.Location.Y), (satelliteBody.Location.X - anchorBody.Location.X));
            double distance = PointHelper.Distance(anchorBody.Location, satelliteBody.Location);
            double force = Math.Sqrt((GravitationalConstant * (anchorBody.KgMass * satelliteBody.KgMass)) / distance);

            var cosx = Math.Cos(angle);
            var siny = Math.Sin(angle);

            orbitalVelocity.X = cosx * force;
            orbitalVelocity.Y = siny * force;

            return orbitalVelocity;
        }

        public OrbitalBody? GetNearestBody(OrbitalBody to)
        {
            if (Bodies.Count == 0)
                return null;
            if (Bodies.Count == 1)
                return to;

            OrbitalBody? nearest = null;
            double nearestDistance = double.MaxValue;
            foreach(var obj in Bodies)
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

        public void Update(double tickMultiplier)
        {
            if (Bodies.Count == 0)
                return;

            if (tickMultiplier <= 1)
            {
                UpdateLocations(tickMultiplier);
                CollisionCheck();
            }
            else
            {
                for (int i = 0; i < (int)tickMultiplier; i++)
                {
                    UpdateLocations(1);
                    CollisionCheck();
                }
            }
        }

        private void UpdateLocations(double tickMultiplier)
        {
            foreach (var obj in Bodies)
            {
                if (obj.IsStationary)
                    continue;
                var newVelocity = CalculateNextLocation(obj);
                obj.Location = new Point(obj.Location.X + newVelocity.X * tickMultiplier, obj.Location.Y + newVelocity.Y * tickMultiplier);
                obj.VelocityVector = newVelocity;
            }
        }

        private void CollisionCheck()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                HashSet<OrbitalBody> newCollidedObjects = GetCollisionSet(Bodies.ElementAt(i), Bodies);
                if (newCollidedObjects.Count > 0)
                {
                    if (CollisionOccured != null)
                        CollisionOccured.Invoke(newCollidedObjects);

                    var newObject = new OrbitalBody(newCollidedObjects, _currentID++);
                    foreach (var obj in newCollidedObjects)
                    {
                        Bodies.RemoveWhere(x => x.GetHashCode() == obj.GetHashCode());
                        if (BodyDeleted != null)
                            BodyDeleted.Invoke(obj);
                    }
                    Bodies.Add(newObject);
                    if (BodyAdded != null)
                        BodyAdded.Invoke(newObject);

                    i = 0;
                }
            }
        }

        public Point CalculateNextLocation(OrbitalBody body)
        {
            var newVelocity = new Point(body.VelocityVector.X, body.VelocityVector.Y);
            if (!body.IsNoclip)
            {
                foreach (var otherBody in Bodies)
                {
                    if (otherBody != body)
                    {
                        if (!otherBody.IsNoclip)
                        {
                            var force = GetGravitationalConstantForce(body, otherBody);
                            newVelocity.X += force.X;
                            newVelocity.Y += force.Y;

                            if (otherBody.HasAtmosphere)
                            {
                                var drag = GetAtmosphericDrag(body, otherBody);
                                newVelocity.X += drag.X;
                                newVelocity.Y += drag.Y;
                            }
                        }
                    }
                }
            }
            return newVelocity;
        }

        private HashSet<OrbitalBody> GetCollisionSet(OrbitalBody body, HashSet<OrbitalBody> bodies)
        {
            HashSet<OrbitalBody> collidedBodies = new HashSet<OrbitalBody>();
            bool isFreeFromCollisions = true;
            foreach (var otherBody in bodies) 
            {
                if (otherBody != body) 
                {
                    if (PointHelper.Distance(otherBody.Location, body.Location) <= (otherBody.Radius + body.Radius))
                    {
                        if (!otherBody.IsNoclip && !body.IsNoclip)
                        {
                            collidedBodies.Add(otherBody);
                            collidedBodies.Add(body);
                        }
                        else if (body.IsNoclip)
                            isFreeFromCollisions = false;
                    }
                }
            }
            if (isFreeFromCollisions)
                body.IsNoclip = false;
            return collidedBodies;
        }

        private Point GetGravitationalConstantForce(OrbitalBody body, OrbitalBody anchorBody)
        {
            Point accelerationVector = new Point();

            double distance = PointHelper.Distance(body.Location, anchorBody.Location);
            double constant = ((GravitationalConstant * body.KgMass * anchorBody.KgMass) / Math.Pow(distance,2));
            double force = constant / body.KgMass;
            double angle = Math.Atan2((anchorBody.Location.Y - body.Location.Y), (anchorBody.Location.X - body.Location.X));

            var cosx = Math.Cos(angle);
            var siny = Math.Sin(angle);

            accelerationVector.X = cosx * force;
            accelerationVector.Y = siny * force;

            return accelerationVector;
        }

        // https://farside.ph.utexas.edu/teaching/celestial/Celestial/node94.html
        private Point GetAtmosphericDrag(OrbitalBody dragBody, OrbitalBody anchorBody)
        {
            Point drag = new Point();

            var distance = PointHelper.Distance(dragBody.Location, anchorBody.Location);
            if (distance > anchorBody.AtmTopLevel)
                return drag;
            var densityAtAltitude = anchorBody.GetDensityAtAltitude(distance);

            var velocity = dragBody.GetVelocity();
            var area = CircleHelper.GetAreaOfRadius(dragBody.Radius);
            var dragCoefficiency = 0.47;
            var dragForce = -((double)1 / (double)2) * ((densityAtAltitude * dragCoefficiency * area) / dragBody.KgMass) * velocity;

            var angle = Math.Atan(dragBody.VelocityVector.Y / dragBody.VelocityVector.X);

            drag.X = (Math.Cos(angle) * dragForce);
            drag.Y = (Math.Sin(angle) * dragForce);

            return drag;
        }

        public double GetLengthOfVector(Point vector) => Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));

        public List<Point> PredictPath(OrbitalBody body, int maxPathPoints, double maxPathLength)
        {
            List<Point> returnPoints = new List<Point>();
            OrbitalBody tempObject = new OrbitalBody(body);
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
