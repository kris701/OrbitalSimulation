﻿using OrbitalSimulation.Models;
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
        public static OrbitalBody GetPresetBodyFromID(BuilderOptions id)
        {
            switch (id)
            {
                case BuilderOptions.Earth: return GetEarth();
                case BuilderOptions.Moon: return GetMoon();
                case BuilderOptions.Sun: return GetSun();
                case BuilderOptions.ISS: return GetISS();
            }
            throw new Exception("Builder option not found!");
        }

        public static OrbitalBody GetSun()
        {
            return new OrbitalBody(
                true,
                new Point(),
                new Point(),
                1.988 * Math.Pow(10, 30),
                696340000);
        }

        public static OrbitalBody GetEarth()
        {
            var body = new OrbitalBody()
            {
                KgMass = 5.972 * Math.Pow(10, 24),
                Radius = 6371000,
                HasAtmosphere = true,
                AtmSeaLevelDensity = 1.2250,
                AtmTopLevel = 6371000 + 100000,
            };
            return body;
        }

        public static OrbitalBody GetMoon()
        {
            var body = new OrbitalBody()
            {
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
                KgMass = 450000,
                Radius = 109,
                HasAtmosphere = false
            };
            return body;
        }
    }
}
