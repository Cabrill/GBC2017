using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Glue.StateInterpolation;
using GBC2017.GameClasses.BaseClasses;
using GBC2017.GameClasses.Interfaces;
using Microsoft.Xna.Framework;
using StateInterpolationPlugin;

namespace GBC2017.StaticManagers
{
    public static class WindManager
    {
        private static Random random;
        private static ICity city;

        public static float windSpeed{ get; private set; } = 1f; //in m/s

        private const int _secondsBetweenUpdates = 8;
        private static double _timeOfLastUpdate;

        public static void Initialize(ICity gameCity, DateTime gameDateTime)
        {
            random = new Random((int)gameDateTime.Ticks);
            city = gameCity;

            windSpeed = DailyWindSpeedForCityAndDate(city, gameDateTime);
            _timeOfLastUpdate = TimeManager.CurrentTime;
        }

        public static void Update(DateTime gameDateTime)
        {
            if (TimeManager.SecondsSince(_timeOfLastUpdate) >= _secondsBetweenUpdates)
            {
                _timeOfLastUpdate = TimeManager.CurrentTime;

                var oldWindSpeed = windSpeed;

                var averageWindSpeedForCityAndDate = DailyWindSpeedForCityAndDate(city, gameDateTime);

                var probOfIncrease = ProbOfHighWindSpeedForCityAndDate(city, gameDateTime);
                var probOfDecrease = probOfIncrease * 1.1f;

                if (oldWindSpeed > averageWindSpeedForCityAndDate)
                {
                    probOfIncrease /= 2;
                    probOfDecrease = (1 - probOfIncrease) / 2;
                }
                else if (oldWindSpeed < averageWindSpeedForCityAndDate)
                {
                    probOfIncrease *= 2;
                    probOfDecrease = (1 - probOfIncrease) / 2;
                }

                var randomChanceOfWindChange = random.NextDouble();

                var incOrDecMod = 0;

                if (probOfIncrease >= randomChanceOfWindChange)
                {
                    incOrDecMod = 1;
                } else if (probOfDecrease >= randomChanceOfWindChange)
                {
                    incOrDecMod = -1;
                }
                else
                {
                    return;
                }

                var msPerScale = 3.5f * random.NextDouble();
                float newWindSpeed = (float)Math.Max(0f,oldWindSpeed + (msPerScale * incOrDecMod));

                var shiftTime = FlatRedBallServices.Random.Between(1, _secondsBetweenUpdates);

                var windSpeedTweener = new Tweener();
                windSpeedTweener = new Tweener(0, 1, shiftTime, InterpolationType.Bounce, Easing.InOut);
                windSpeedTweener.PositionChanged += (a) =>
                {
                    windSpeed = ((1-a) * oldWindSpeed) + (a * newWindSpeed);
                };
                windSpeedTweener.Start();
                TweenerManager.Self.Add(windSpeedTweener);
            }
        }


        private static float DailyWindSpeedForCityAndDate(ICity city, DateTime date)
        {
            var currentMonth = date.Month;
            var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
            var day = date.Day;
            var daysInMonth = DateTime.DaysInMonth(date.Year, currentMonth);

            var dayAsPercentOfMonth = day / (float)daysInMonth;

            //Minus one from months because arrays are always zero-indexed
            var currentMonthWindSpeed = city.MonthlyAverageWindSpeedInMs[currentMonth - 1];
            var nextMonthWindSpeed= city.MonthlyAverageWindSpeedInMs[nextMonth - 1];

            return MathHelper.Lerp(currentMonthWindSpeed, nextMonthWindSpeed, dayAsPercentOfMonth);
        }

        private static float ProbOfHighWindSpeedForCityAndDate(ICity city, DateTime date)
        {
            var currentMonth = date.Month;
            var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
            var day = date.Day;
            var daysInMonth = DateTime.DaysInMonth(date.Year, currentMonth);

            var dayAsPercentOfMonth = day / (float)daysInMonth;

            //Minus one from months because arrays are always zero-indexed
            var currentMonthWindSpeed = city.WindProbabilityGreaterThanBeaufort4[currentMonth - 1];
            var nextMonthWindSpeed = city.WindProbabilityGreaterThanBeaufort4[nextMonth - 1];

            return MathHelper.Lerp(currentMonthWindSpeed, nextMonthWindSpeed, dayAsPercentOfMonth);
        }
    }
}
