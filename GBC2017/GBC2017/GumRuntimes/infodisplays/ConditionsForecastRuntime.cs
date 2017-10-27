using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Converters;
using Gum.DataTypes;

namespace GBC2017.GumRuntimes
{
    public partial class ConditionsForecastRuntime
    {
        public int HourlyForecastCount => HourlyForecastContainer.Children.Count;

        private const int SecondsInAnHour = 60 * 60;
        private const float HalfHourlyForecastWidth = 12.5f;

        public void Initialize(List<float> hourlySun, List<float> hourlyWind, List<float> hourlyWater)
        {
            (HourlyForecastContainer.Children[0] as HourlyForecastRuntime).SetValues(0, 0, 0);
            (HourlyForecastContainer.Children[0] as HourlyForecastRuntime).XUnits = GeneralUnitType.Percentage;

            for (var i = 1; i < HourlyForecastCount; i++)
            {
                if (HourlyForecastContainer.Children[i] is HourlyForecastRuntime forecast)
                {
                    forecast.SetValues(hourlySun[i-1], hourlyWind[i-1], hourlyWater[i-1]);
                    forecast.XUnits = GeneralUnitType.Percentage;
                }
            }
        }

        public void Update(DateTime gameTime, List<float> hourlySun = null, List<float> hourlyWind = null, List<float> hourlyWater = null)
        {
            if (hourlySun != null)
            {
                var firstItem = HourlyForecastContainer.Children[0] as HourlyForecastRuntime;
                firstItem.SetValues(hourlySun[hourlySun.Count-1], hourlyWind[hourlyWind.Count-1], hourlyWater[hourlyWater.Count-1]);
                firstItem.X = 0;
                HourlyForecastContainer.Children.Remove(firstItem);
                HourlyForecastContainer.Children.Insert(HourlyForecastCount, firstItem);
                UpdateLayout();
                UpdateLayout();
                UpdateLayout();
            }

            var firstHour = HourlyForecastContainer.Children[0] as HourlyForecastRuntime;

            var secondsPastTheHour = (SecondsInAnHour- (gameTime.Minute * 60) - gameTime.Second)/2;
            var percentOfHour = (float)decimal.Divide(secondsPastTheHour, SecondsInAnHour);

            var newOffset = HalfHourlyForecastWidth * ((percentOfHour * 2) - 1)-7.1f;

            firstHour.X = newOffset;
            
        }
    }
}
