using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Presets
{
    public static class PresetBuilder
    {
        public static OrbiterObject GetSun()
        {
            return new OrbiterObject(
                true,
                new Point(),
                new Point(),
                1.988 * Math.Pow(10, 30),
                696340000);
        }

        public static OrbiterObject GetEarth()
        {
            return new OrbiterObject(
                true,
                new Point(),
                new Point(),
                5.972 * Math.Pow(10, 24),
                6371000);
        }

        public static OrbiterObject GetMoon()
        {
            return new OrbiterObject(
                true,
                new Point(),
                new Point(),
                7.34767309 * Math.Pow(10, 22),
                1737400);
        }
    }
}
