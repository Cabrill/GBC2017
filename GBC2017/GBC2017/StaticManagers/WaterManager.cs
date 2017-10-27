using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.GameClasses.Interfaces;

namespace GBC2017.StaticManagers
{
    class WaterManager
    {
        private static List<float> _nineHourForecast;

        public static List<float> NineHourForecast => _nineHourForecast;

        public static void Initialize(ICity city)
        {
            WaterVelocity = city.WaterVelocity;

            _nineHourForecast = new List<float>(9);
            for (var i = 0; i < 9; i++)
            {
                _nineHourForecast.Add(WaterVelocity);
            }
        }

        public static float WaterVelocity { get; private set; } //in m/s
    }
}
