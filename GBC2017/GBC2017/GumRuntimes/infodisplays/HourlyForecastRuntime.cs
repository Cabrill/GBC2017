using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class HourlyForecastRuntime
    {
        public void SetValues(float sun, float wind, float water)
        {
            SunPercent = (0.1f + sun*0.9f) * 100;
            WindPercent = (0.1f + 0.9f*(wind/25)) * 100;
            WaterPercent = (0.1f + 0.9f*(water/5)) * 100;
        }
    }
}
