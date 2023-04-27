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
        public HashSet<OrbiterObject> Objects { get; set; }

        public void AddNewObject(OrbiterObject obj);

        public double GetOrbitalPeriod(OrbiterObject source);
        public Point GetOrbitalVector(OrbiterObject source, OrbiterObject orbiter);
        public OrbiterObject? GetNearestObject(OrbiterObject to);
        public UpdateResult Update(double tickMultiplier);
        public Point CalculateNextLocation(OrbiterObject obj);
        public List<Point> PredictPath(OrbiterObject obj, int maxPathPoints, double maxPathLength);
    }
}
