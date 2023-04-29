using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalSimulation.Engines
{
    public enum UpdateResult { None, NothingChanged, ObjectsUpdated, ObjectsAdded }
    public interface IPhysicsEngine
    {
        public HashSet<OrbitalBody> Bodies { get; set; }

        public void AddNewBody(OrbitalBody body);

        public Point GetOrbitalVector(OrbitalBody satelliteBody, OrbitalBody anchorBody);
        public OrbitalBody? GetNearestBody(OrbitalBody to);
        public UpdateResult Update(double tickMultiplier);
        public Point CalculateNextLocation(OrbitalBody body);
        public List<Point> PredictPath(OrbitalBody body, int maxPathPoints, double maxPathLength);
    }
}
