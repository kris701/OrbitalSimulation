using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrbitalSimulation.Presets
{
    public enum BuilderOptions
    {
        None,
        // Planetary
        Earth, Moon, Sun,
        // Other
        ISS
    }

    public static class PresetBuilder
    {
        public static OrbiterObject GetObjectFromID(BuilderOptions option)
        {
            switch (option)
            {
                case BuilderOptions.Earth: return GetEarth();
                case BuilderOptions.Moon: return GetMoon();
                case BuilderOptions.Sun: return GetSun();
                case BuilderOptions.ISS: return GetISS();
            }
            throw new Exception("Builder option not found!");
        }

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
            var body = new OrbiterObject()
            {
                KgMass = 5.972 * Math.Pow(10, 24),
                Radius = 6371000,
                HasAtmosphere = true,
                AtmSeaLevelDensity = 1.2250,
                AtmTopLevel = 6371000 + 100000,
            };
            return body;
        }

        public static OrbiterObject GetMoon()
        {
            var body = new OrbiterObject()
            {
                KgMass = 7.34767309 * Math.Pow(10, 22),
                Radius = 1737400,
                HasAtmosphere = false
            };
            return body;
        }

        public static OrbiterObject GetISS()
        {
            var body = new OrbiterObject()
            {
                KgMass = 450000,
                Radius = 109,
                HasAtmosphere = false
            };
            return body;
        }
    }
}
