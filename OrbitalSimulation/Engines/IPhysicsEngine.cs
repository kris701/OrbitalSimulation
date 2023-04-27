﻿using OrbitalSimulation.Models;
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
        public HashSet<OrbiterObject> Objects { get; set; }

        public void AddNewObject(OrbiterObject obj);

        public double GetOrbitalPeriod(OrbiterObject source);
        public Point GetOrbitalVector(OrbiterObject source, OrbiterObject orbiter);
        public OrbiterObject? GetNearestObject(OrbiterObject to);
        public bool Update();
    }
}
