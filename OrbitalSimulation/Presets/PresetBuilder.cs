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
        Earth, Moon, Mars, Venus, Sun,
        // Other
        ISS, Cat
    }

    public static class PresetBuilder
    {
        public static OrbitalBody GetPresetBodyFromID(BuilderOptions id)
        {
            switch (id)
            {
                case BuilderOptions.Earth: return GetEarth();
                case BuilderOptions.Moon: return GetMoon();
                case BuilderOptions.Sun: return GetSun();
                case BuilderOptions.Mars: return GetMars();
                case BuilderOptions.Venus: return GetVenus();
                case BuilderOptions.ISS: return GetISS();
                case BuilderOptions.Cat: return GetCat();
            }
            throw new Exception("Builder option not found!");
        }

        public static OrbitalBody GetSun()
        {
            var body = new OrbitalBody()
            {
                Image = "sun.png",
                KgMass = 1.988 * Math.Pow(10, 30),
                Radius = 696340000
            };
            return body;
        }

        public static OrbitalBody GetEarth()
        {
            var body = new OrbitalBody()
            {
                Image = "earth.png",
                KgMass = 5.972 * Math.Pow(10, 24),
                Radius = 6371000,
                HasAtmosphere = true,
                AtmSeaLevelDensity = 1.2250,
                AtmTopLevel = 6371000 + 100000,
            };
            return body;
        }

        public static OrbitalBody GetMars()
        {
            var body = new OrbitalBody()
            {
                Image = "mars.png",
                KgMass = 6.4171 * Math.Pow(10, 23),
                Radius = 3389100,
                HasAtmosphere = true,
                AtmSeaLevelDensity = 0.02,
                AtmTopLevel = 3389100 + 130000,
            };
            return body;
        }

        public static OrbitalBody GetVenus()
        {
            var body = new OrbitalBody()
            {
                Image = "venus.png",
                KgMass = 4.8675 * Math.Pow(10, 24),
                Radius = 6051800,
                HasAtmosphere = true,
                AtmSeaLevelDensity = 65,
                AtmTopLevel = 6051800 + 250000,
            };
            return body;
        }

        public static OrbitalBody GetMoon()
        {
            var body = new OrbitalBody()
            {
                Image = "moon.png",
                KgMass = 7.34767309 * Math.Pow(10, 22),
                Radius = 1737400,
                HasAtmosphere = false
            };
            return body;
        }

        public static OrbitalBody GetISS()
        {
            var body = new OrbitalBody()
            {
                Image = "iss.png",
                KgMass = 450000,
                Radius = 109,
                HasAtmosphere = false
            };
            return body;
        }

        public static OrbitalBody GetCat()
        {
            var body = new OrbitalBody()
            {
                Image = "cat.png",
                KgMass = 5,
                Radius = 1,
                HasAtmosphere = false
            };
            return body;
        }
    }
}
