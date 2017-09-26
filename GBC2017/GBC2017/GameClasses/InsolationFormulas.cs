using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GBC2017.GameClasses
{
    /// <summary>
    /// Utility class to determine insolation for specific cities during specific days
    /// All data courtesy of:  https://www.gaisma.com
    /// </summary>
    public class InsolationFormulas
    {
        #region Enums

        /// <summary>
        /// All available cities for which data is available
        /// </summary>
        public enum City
        {
            Helsinki,
            Brisbane
        }
        #endregion

        #region Singleton

        public static InsolationFormulas Instance { get; } = new InsolationFormulas();

        #endregion

        #region City Data

        /// <summary>
        /// A dictionary with a key of City enum, and value of Lat/Lon tuple
        /// </summary>
        private Dictionary<City, Tuple<double, double>> CityLatAndLonTuples = new Dictionary<City, Tuple<double, double>>()
        {
            {City.Helsinki, Tuple.Create(60.1699, 24.9384) }, //60.1699° N, 24.9384° E
            {City.Brisbane, Tuple.Create(27.4698, 153.0251)}  //27.4698° S, 153.0251° E
        };

        /// <summary>
        /// The monthly insolation rates for Helsinki, with [0]=January
        /// </summary>
        private readonly float[] HelsinkiMonthInsolation = {
            0.28f,
            0.99f,
            2.23f,
            3.83f,
            5.14f,
            5.37f,
            5.17f,
            3.98f,
            2.47f,
            1.12f,
            0.46f,
            0.16f,
        };

        /// <summary>
        /// The monthly insolation rates for Brisbane, with [0]=January
        /// </summary>
        private readonly float[] BrisbaneMonthInsolation = {
            6.64f,
            5.79f,
            5.27f,
            4.18f,
            3.40f,
            3.23f,
            3.47f,
            4.29f,
            5.47f,
            5.97f,
            6.56f,
            6.80f,
        };

        private Dictionary<City, float[]> CityMonthlyInsolationArrays;


        #endregion

        #region Constructor
        private InsolationFormulas()
        {
            CityMonthlyInsolationArrays.Add(City.Helsinki, HelsinkiMonthInsolation);
            CityMonthlyInsolationArrays.Add(City.Brisbane, BrisbaneMonthInsolation);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// The hourly insolation for a specific city on a specific date
        /// </summary>
        /// <param name="city">The City name as an enum</param>
        /// <param name="date">The date used in calculating hourly insolation</param>
        /// <returns>The hourly insolation of the city for the date in terms of kWh/m²</returns>
        public float GetHourlyInsolationForCityAndDate(City city, DateTime date)
        {
            var dailyyInsolation = DailyInsolationForCityAndDate(city, date);
            var latLon = CityLatAndLonTuples[city];

            var riseTime = new DateTime();
            var setTime = new DateTime();
            var isSunrise = false;
            var isSunset = false;

            SunTimes.Instance.CalculateSunRiseSetTimes(latLon.Item1, latLon.Item2, date, ref riseTime,
                ref setTime, ref isSunrise, ref isSunset);


            var dayTimeSpan = setTime - riseTime;

            if (dayTimeSpan.TotalHours > 0)
            {
                return dailyyInsolation / (float) dayTimeSpan.TotalHours;
            }
            else
            {
                return 0f;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Determins the daily insolation for a specific city and a specific day
        /// </summary>
        /// <param name="city">The City name as an enum</param>
        /// <param name="date">The date used in calculating daily insolation</param>
        /// <returns>The insolation for the city on the given date in terms of kWh/m²/day</returns>
        private float DailyInsolationForCityAndDate(City city, DateTime date)
        {
            var currentMonth = date.Month;
            var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
            var day = date.Day;
            var daysInMonth = DateTime.DaysInMonth(date.Year, currentMonth);

            var dayAsPercentOfMonth = day / (float)daysInMonth;

            //Minus one from months because arrays are always zero-indexed
            var currentMonthInsolation = CityMonthlyInsolationArrays[city][currentMonth - 1];
            var nextMonthInsolation = CityMonthlyInsolationArrays[city][nextMonth - 1];

            return MathHelper.Lerp(currentMonthInsolation, nextMonthInsolation, dayAsPercentOfMonth);
        }
        #endregion
    }
}
