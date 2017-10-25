using GBC2017.GameClasses.Interfaces;

namespace GBC2017.GameClasses.Cities
{
    public class Helsinki : ICity
    {
        public static Helsinki Instance { get; } = new Helsinki();

        public int UTCOffset => 3;

        public string CityName => "Helsinki";

        public float Latitude => 60.1699f;

        public float Longitude => 24.9384f;

        public float WaterVelocity { get; } = 2.5f;

        public float[] MonthlyInsolation { get; }= {
            0.28f,
            0.99f,
            2.23f,
            3.83f,
            5.14f,
            5.37f,
            5.17f,
            3.98f,
            2.47f,
            1.12f,
            0.46f,
            0.16f
        };

        public float[] MonthlyAverageWindSpeedInMs { get; } =
        {
            5.14444f,
            4.63f,
            4.63f,
            4.63f,
            4.63f,
            5.14444f,
            4.63f,
            5.14444f,
            4.11556f,
            5.14444f,
            4.63f,
            5.14444f,
            4.63f
        };

        public float[] WindProbabilityGreaterThanBeaufort4 { get; } =
        {
            0.43f,
            0.34f,
            0.31f,
            0.30f,
            0.22f,
            0.33f,
            0.26f,
            0.40f,
            0.25f,
            0.41f,
            0.41f,
            0.36f
        };
    }
}
