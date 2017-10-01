using GBC2017.GameClasses.Interfaces;

namespace GBC2017.GameClasses.Cities
{
    public class Helsinki : ICity
    {
        public static Helsinki Instance { get; } = new Helsinki();

        public string CityName => "Helsinki";

        public float Latitude => 60.1699f;

        public float Longitude => 24.9384f;

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
}
}
