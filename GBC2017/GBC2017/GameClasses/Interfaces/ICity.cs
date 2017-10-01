using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GameClasses.Interfaces
{
    public interface ICity
    {
        string CityName { get; }
        float Latitude { get; }
        float Longitude { get; }

        float[] MonthlyInsolation { get; }
    }
}
