using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.GameClasses.Interfaces;
using Microsoft.Xna.Framework;

namespace GBC2017.GameClasses
{
    /// <summary>
    /// Utility class to determine insolation for specific cities during specific days
    /// All data courtesy of:  https://www.gaisma.com
    /// </summary>
    public class InsolationFormulas
    {
        #region Public properties

        public float CurrentHourlyInsolation
        {
            get
            {
                if (CurrentCity == null || CurrentDateTime == DateTime.MinValue)
                {
                    return 0f;
                }
                else
                {
                    if (CurrentDateTime == priorDateTime)
                    {
                        return priorInsolationLevel;
                    }

                    var newInsolationLevel = GetHourlyInsolationForCityAndDate(CurrentCity, CurrentDateTime);
                    priorInsolationLevel = newInsolationLevel;
                    priorDateTime = CurrentDateTime;
                    return newInsolationLevel;
                }
            }
        }
        #endregion

        #region Private properties

        private DateTime priorDateTime;
        private float priorInsolationLevel;

        private ICity CurrentCity;
        private DateTime CurrentDateTime;

        #endregion

        #region Singleton

        public static InsolationFormulas Instance { get; } = new InsolationFormulas();
        
        #endregion

        #region Constructor
        private InsolationFormulas()
        {
            CurrentDateTime = DateTime.MinValue;
            priorDateTime = DateTime.MinValue;
        }
        #endregion

        #region Public methods

        public void SetCityAndDate(ICity city, DateTime date)
        {
            CurrentCity = city;
            CurrentDateTime = date;
        }

        public void UpdateDateTime(DateTime date)
        {
            CurrentDateTime = date;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// The hourly insolation for a specific city on a specific date
        /// </summary>
        /// <param name="city">The City name as an enum</param>
        /// <param name="date">The date used in calculating hourly insolation</param>
        /// <returns>The hourly insolation of the city for the date in terms of kWh/m²</returns>
        private float GetHourlyInsolationForCityAndDate(ICity city, DateTime date)
        {
            var dailyyInsolation = DailyInsolationForCityAndDate(city, date);

            var riseTime = new DateTime();
            var setTime = new DateTime();
            var isSunrise = false;
            var isSunset = false;

            SunTimes.Instance.CalculateSunRiseSetTimes(city.Latitude, city.Longitude, date, ref riseTime,
                ref setTime, ref isSunrise, ref isSunset);

            var dayTimeSpan = setTime - riseTime;

            if (dayTimeSpan.TotalHours > 0)
            {
                return dailyyInsolation / (float)dayTimeSpan.TotalHours;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Determins the daily insolation for a specific city and a specific day
        /// </summary>
        /// <param name="city">The City name as an enum</param>
        /// <param name="date">The date used in calculating daily insolation</param>
        /// <returns>The insolation for the city on the given date in terms of kWh/m²/day</returns>
        private float DailyInsolationForCityAndDate(ICity city, DateTime date)
        {
            var currentMonth = date.Month;
            var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
            var day = date.Day;
            var daysInMonth = DateTime.DaysInMonth(date.Year, currentMonth);

            var dayAsPercentOfMonth = day / (float)daysInMonth;

            //Minus one from months because arrays are always zero-indexed
            var currentMonthInsolation = city.MonthlyInsolation[currentMonth - 1];
            var nextMonthInsolation = city.MonthlyInsolation[nextMonth - 1];

            return MathHelper.Lerp(currentMonthInsolation, nextMonthInsolation, dayAsPercentOfMonth);
        }
        #endregion
    }
}
