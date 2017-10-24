using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GameClasses.Interfaces
{
    public interface ICity
    {
        int UTCOffset { get; }
        string CityName { get; }
        float Latitude { get; }
        float Longitude { get; }

        float WaterVelocity { get; }

        float[] MonthlyInsolation { get; }

        float[] MonthlyAverageWindSpeedInMs { get; }
        float[] WindProbabilityGreaterThanBeaufort4 { get; }
    }
}
