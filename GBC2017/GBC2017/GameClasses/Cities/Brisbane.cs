using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.GameClasses.Interfaces;

namespace GBC2017.GameClasses.Cities
{
    public class Brisbane : ICity
    {
        public static Brisbane Instance { get; } = new Brisbane();

        public int UTCOffset => 10;

        public string CityName => "Brisbane";

        public float Latitude => -27.4698f;

        public float Longitude => 153.0251f;

        public float WaterVelocity { get; } = 1.5f;

        public float[] MonthlyInsolation { get; } = {
            6.64f,
            5.79f,
            5.27f,
            4.18f,
            3.40f,
            3.23f,
            3.47f,
            4.29f,
            5.47f,
            5.97f,
            6.56f,
            6.80f,
        };

        public float[] MonthlyAverageWindSpeedInMs { get; } =
        {
            5.14444f,
            5.14444f,
            4.63f,
            4.63f,
            4.11556f,
            4.11556f,
            4.11556f,
            4.63f,
            5.14444f,
            5.65889f,
            5.65889f,
            5.65889f
        };

        public float[] WindProbabilityGreaterThanBeaufort4 { get; } =
        {
            0.41f,
            0.35f,
            0.30f,
            0.19f,
            0.15f,
            0.19f,
            0.18f,
            0.24f,
            0.38f,
            0.46f,
            0.47f,
            0.48f
        };
    }
}
