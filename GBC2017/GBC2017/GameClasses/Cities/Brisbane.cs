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

        public string CityName => "Brisbane";

        public float Latitude => -27.4698f;

        public float Longitude => 153.0251f;

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
    }
}
