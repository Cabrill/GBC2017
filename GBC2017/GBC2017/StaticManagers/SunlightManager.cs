using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.StaticManagers
{
    /// <summary>
    /// This class tracks how effective sunlight is in the current conditions
    /// </summary>
    public static class SunlightManager
    {
        public static float SunlightEffectiveness { get; private set; } = 1f;

        public static void UpdateConditions(DateTime timeOfDay)
        {
            if (timeOfDay.Hour >= 18)
            {
                const int duskStartInMinutes = 22 * 60;
                const int duskLastsInMinutes = 4 * 60;

                var skyOpacity = Math.Max(0.1f, (duskStartInMinutes - timeOfDay.TimeOfDay.TotalMinutes) / duskLastsInMinutes);

                SunlightEffectiveness = (float)skyOpacity;
            }
            else if (timeOfDay.Hour < 6)
            {
                const int dawnStartInMinutes = 4 * 60;
                const int dawnLastsInMinutes = 4 * 60;

                var skyOpacity = Math.Max(0.1f, (timeOfDay.TimeOfDay.TotalMinutes - dawnStartInMinutes) / dawnLastsInMinutes);

                SunlightEffectiveness = (float)skyOpacity;
            }
            else
            {
                const int fullSunTimeInMinutes = 14 * 60;
                const float fullSunWindowInMinutes = 12 * 60;
                var minutesFromFullSun = (float)Math.Abs(timeOfDay.TimeOfDay.TotalMinutes - fullSunTimeInMinutes);

                var percentFullSun = 1 - Math.Min(1f, minutesFromFullSun / fullSunWindowInMinutes);

                SunlightEffectiveness = (float)percentFullSun;
            }
        }
    }
}
