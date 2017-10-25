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
        public static void Initialize(ICity city)
        {
            WaterVelocity = city.WaterVelocity;
        }

        public static float WaterVelocity { get; private set; } //in m/s
    }
}
