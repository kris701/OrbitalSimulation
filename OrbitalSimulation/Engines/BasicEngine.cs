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

        public void Update(List<OrbiterObject> objects)
        {
            foreach(var obja in objects)
            {
                if (obja.IsStationary || obja.IsCollided)
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

                obja.IsCollided = HaveCollided(obja, objects);
            }
        }

        private bool HaveCollided(OrbiterObject self, List<OrbiterObject> objects)
        {
            foreach (var obj in objects)
                if (obj != self)
                    if (!obj.IsCollided)
                        if (Distance(obj.Location, self.Location) <= (obj.Radius + self.Radius))
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
