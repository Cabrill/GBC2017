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
        private static List<float> _nineHourForecast;
        public static List<float> NineHourForecast => _nineHourForecast;

        public static float SunlightEffectiveness { get; private set; } = 1f;
        public static bool SunIsUp { get; private set; }
        public static bool MoonIsUp { get; private set; }

        private static HorizonBoxRuntime _horizon;
        private static DateTime _lastUpdate;

        public static void Initialize(HorizonBoxRuntime horizonBox, DateTime gameDateTime)
        {
            _horizon = horizonBox;
            SunlightEffectiveness = 1f;
            SunIsUp = true;
            MoonIsUp = false;

            _nineHourForecast = new List<float>(9);
            for (var i = 0; i < 9; i++)
            {
                _nineHourForecast.Add(_horizon.ForecastFor(gameDateTime.AddHours(i)));
            }
            _lastUpdate = gameDateTime;
        }

        public static void UpdateConditions(DateTime gameDateTime)
        {
            SunIsUp = _horizon.SunAboveHorizon;
            MoonIsUp = _horizon.MoonAboveHorizon;

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
                SunlightEffectiveness = _horizon.SunAboveHorizon
                    ? GetSunEffectiveness()
                    : GetMoonEffectiveness();
            }

            if ((gameDateTime - _lastUpdate).Hours >= 1)
            {
                _lastUpdate = gameDateTime;
                _nineHourForecast.RemoveAt(0);
                _nineHourForecast.Add(_horizon.ForecastFor(gameDateTime.AddHours(_nineHourForecast.Count-1)));
            }
        }

        private static float GetSunEffectiveness()
        {
            return  _horizon.SunPercentageAboveHorizon;
        }

        private static float GetMoonEffectiveness()
        {
            //Moonlight is 1/345th as effective as sunlight for powering solar panels
            //https://www.quora.com/Can-moon-light-produce-electricity-from-solar-panels-at-night-Can-moon-light-generate-the-electron-hole-pair-in-a-solar-cell
            return _horizon.MoonPercentageAboveHorizon/345;
        }
    }
}
