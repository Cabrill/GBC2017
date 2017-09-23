using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.GumRuntimes;

namespace GBC2017.StaticManagers
{
    /// <summary>
    /// This class tracks how effective sunlight is in the current conditions
    /// </summary>
    public static class SunlightManager
    {
        public static float SunlightEffectiveness { get; private set; } = 1f;
        public static bool SunIsUp { get; private set; }
        public static bool MoonIsUp { get; private set; }

        private static HorizonBoxRuntime _horizon;
        private static float HorizonPositionY => _horizon.HorizonPositionY;


        public static void Initialize(HorizonBoxRuntime horizonBox)
        {
            _horizon = horizonBox;
            SunlightEffectiveness = 1f;
            SunIsUp = true;
            MoonIsUp = false;
        }

        public static void UpdateConditions(DateTime timeOfDay)
        {
            SunIsUp = _horizon.SunPositionY <= HorizonPositionY;
            MoonIsUp = _horizon.MoonPositionY <= HorizonPositionY;

            if (SunIsUp)
            {
                SunlightEffectiveness = GetSunEffectiveness();
            }
            else if (MoonIsUp)
            {
                SunlightEffectiveness = GetMoonEffectiveness();
            }
            else
            {
                SunlightEffectiveness = _horizon.SunPositionY <= _horizon.MoonPositionY
                    ? GetSunEffectiveness()
                    : GetMoonEffectiveness();
            }
        }

        private static float GetSunEffectiveness()
        {
            var sunlight = _horizon.SunPositionY-50f;
            return Math.Max(0,1 - (sunlight / 500));
        }

        private static float GetMoonEffectiveness()
        {
            //Moonlight is 1/345th as effective as sunlight for powering solar panels
            //https://www.quora.com/Can-moon-light-produce-electricity-from-solar-panels-at-night-Can-moon-light-generate-the-electron-hole-pair-in-a-solar-cell
            var moonlight = _horizon.MoonPositionY - 50f;
            return Math.Max(0,(1 - (moonlight /500)) / 345);
        }
    }
}
