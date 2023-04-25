using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalSimulation.Engines
{
    public interface IPhysicsEngine
    {
        public double GetOrbitalPeriod(OrbiterObject source);
        public double GetHorisontalOrbitalSpeed(OrbiterObject source, OrbiterObject orbiter);
        public bool Update(List<OrbiterObject> objects);
    }
}
