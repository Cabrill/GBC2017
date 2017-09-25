using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GBC2017.GameClasses
{
    public static class GameFormulas
    {
        private static readonly float[] HourlyUsageModifiers = {
            1.08695652173913f,
            1f,
            1f,
            1.04347826086957f,
            1.04347826086957f,
            1.08695652173913f,
            1.08695652173913f,
            1.52173913043478f,
            2f,
            2f,
            1.73913043478261f,
            1.60869565217391f,
            1.56521739130435f,
            1.65217391304348f,
            1.43478260869565f,
            1.95652173913044f,
            1.56521739130435f,
            2.04347826086957f,
            2.39130434782609f,
            1.73913043478261f,
            1.52173913043478f,
            1.52173913043478f,
            1.43478260869565f,
            1.08695652173913f,
        };

        private static float _hourlyUsageGraphSum = 0f;
        private static float HourlyUsageGraphSum 
        {
            get
            {
                if (_hourlyUsageGraphSum < 1f) _hourlyUsageGraphSum = HourlyUsageModifiers.Sum();
                return _hourlyUsageGraphSum;
            }
        }



        /// <summary>
        /// Takes the desired hour, and the minimum hourly value for the day, then returns the typical energy consumption for the given hour
        /// </summary>
        /// <param name="hour">The hour of energy use you need</param>
        /// <param name="minValue">The minimum hourly energy usage for the entire day</param>
        /// <returns></returns>
        public static float HourlyEnergyUsageFromCurveAndMinValue(int hour, float minValue = 2.3f)
        {
            if (hour >= 0 && hour <= 23)
            {
                return HourlyUsageModifiers[hour] * minValue;
            }

            //The only way we get here is if the hour value wasn't in 0-23 format.
            //We want to throw an exception if we're in debug mode to catch the problem
            //But we never want a production app to crash, so we return a safe value
#if DEBUG
                throw new InvalidDataException("Hour should be between 0 and 23");
#endif
            return minValue;
        }

        /// <summary>
        /// Takes the desired hour and the average hourly value for the day, then returns the typical energy consumption for the given hour
        /// </summary>
        /// <param name="hour">The hour of energy use you need</param>
        /// <param name="avgValue">The average hourly energy usage for the entire day</param>
        /// <returns></returns>
        public static float HourlyEnergyUsageFromCurveAndAvgValue(int hour, float avgValue = 3.4625f)
        {
            var minValue = (avgValue * HourlyUsageModifiers.Length) / HourlyUsageGraphSum;

            return HourlyEnergyUsageFromCurveAndMinValue(hour, minValue);
        }
    }
}