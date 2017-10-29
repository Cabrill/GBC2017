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

        private float mostRecentSun;
        private float mostRecentWind;
        private float mostRecentWater;

        private const int SecondsInAnHour = 60 * 60;
        private const float HalfHourlyForecastWidth = 12.5f;
        private int lastHour;

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
            lastHour = int.MinValue;
        }

        public void UpdateFirstItem(List<float> hourlySun = null, List<float> hourlyWind = null,
            List<float> hourlyWater = null)
        {
            mostRecentSun = hourlySun[hourlySun.Count - 1];
            mostRecentWind = hourlyWind[hourlyWind.Count - 1];
            mostRecentWater = hourlyWater[hourlyWater.Count - 1];
        }

        public void Update(DateTime gameTime)
        {
            if (lastHour == int.MinValue) lastHour = gameTime.Hour;

            var firstHour = HourlyForecastContainer.Children[0] as HourlyForecastRuntime;

            var secondsPastTheHour = (SecondsInAnHour - (gameTime.Minute * 60) - gameTime.Second) / 2;
            var percentOfHour = (float)decimal.Divide(secondsPastTheHour, SecondsInAnHour);

            if (lastHour != gameTime.Hour)
            {
                firstHour.SetValues(mostRecentSun, mostRecentWind, mostRecentWater);
                firstHour.X = 0;
                HourlyForecastContainer.Children.Remove(firstHour);
                HourlyForecastContainer.Children.Insert(HourlyForecastCount, firstHour);

                firstHour = HourlyForecastContainer.Children[0] as HourlyForecastRuntime;
            }

            lastHour = gameTime.Hour;

            var newOffset = HalfHourlyForecastWidth * ((percentOfHour * 2) - 1) - 7.1f;

            firstHour.X = newOffset;

            TimeText.Text = DateTimeToString(gameTime);
        }

        private string DateTimeToString(DateTime time)
        {
            var month = time.ToString("MMMM");
            var day = time.ToString("dd");
            var hour = time.Hour.ToString();
            var minute = "";

            if (time.Minute < 15) minute = "00";
            else if (time.Minute < 30) minute = "15";
            else if (time.Minute < 45) minute = "30";
            else minute = "45";

            return $"{month} | {day} | {hour}:{minute}";
        }
    }
}
