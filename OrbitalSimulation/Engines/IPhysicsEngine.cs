using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalSimulation.Engines
{
    public interface IPhysicsEngine
    {
        public delegate void BodyDeletedHandler(OrbitalBody body);
        public delegate void BodyAddedHandler(OrbitalBody body);
        public delegate void CollisioEventHandler(HashSet<OrbitalBody> collidedBodies);

        public event BodyDeletedHandler? BodyDeleted;
        public event BodyAddedHandler? BodyAdded;
        public event CollisioEventHandler? CollisionOccured;

        public HashSet<OrbitalBody> Bodies { get; set; }

        public void AddNewBody(OrbitalBody body);

        public Point GetOrbitalVector(OrbitalBody satelliteBody, OrbitalBody anchorBody);
        public OrbitalBody? GetNearestBody(OrbitalBody to);
        public void Update(double tickMultiplier);
        public Point CalculateNextLocation(OrbitalBody body);
        public List<Point> PredictPath(OrbitalBody body, int maxPathPoints, double maxPathLength);
    }
}
