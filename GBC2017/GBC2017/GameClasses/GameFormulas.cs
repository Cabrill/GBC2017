using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Enemies;
using GBC2017.Entities.Structures.Combat;

namespace GBC2017.GameClasses
{
    public class GameFormulas
    {

        #region Singleton

        public static GameFormulas Instance { get; } = new GameFormulas();

        #endregion

        #region Energy Usage fields
        private readonly float[] HourlyUsageModifiers = {
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

        private float HourlyUsageGraphSum;
        #endregion

        #region Public properties

        public const int RealSecondsPerGameHour = 8;

        #endregion

        #region Enemy Valuation fields

        private float BaseEnergyPerHitPoint;
        private float BaseCombatStructureRange;
        private float BaseProjectileSpeed;
        private float BaseEnemyMovementSpeed;

        #endregion

        #region Constructor

        private GameFormulas()
        {
            //Calculate the sum of the HourlyUsageModifiers, to be used in deriving hourly min values from avg hourly value
            HourlyUsageGraphSum = HourlyUsageModifiers.Sum();

            //Instantiate an instance of the base attack structure, and read it's stats
            var basicTower = new LaserTurret();
            var baseDamage = basicTower.AttackDamage;
            var baseEnergyPerAttack = basicTower.EnergyCostToFire;

            BaseEnergyPerHitPoint = baseEnergyPerAttack / baseDamage;
            BaseCombatStructureRange = basicTower.RangedRadius;
            BaseProjectileSpeed = basicTower.ProjectileSpeed;

            //Instantiate an instance of the base enemy, and read it's stats
            var basicEnemy = new BasicAlien();
            BaseEnemyMovementSpeed = basicEnemy.Speed;

            //Destroy the objects we used for analysis
            basicEnemy.Destroy();
            basicTower.Destroy();
        }
        #endregion

        #region Energy usage methods

        /// <summary>
        /// Takes the desired hour, and the minimum hourly value for the day, then returns the typical energy consumption for the given hour
        /// </summary>
        /// <param name="hour">The hour of energy use you need</param>
        /// <param name="minValue">The minimum hourly energy usage for the entire day</param>
        /// <returns></returns>
        public float HourlyEnergyUsageFromCurveAndMinValue(int hour, float minValue = 2.3f)
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
        public float HourlyEnergyUsageFromCurveAndAvgValue(int hour, float avgValue = 3.4625f)
        {
            var minValue = (avgValue * HourlyUsageModifiers.Length) / HourlyUsageGraphSum;

            return HourlyEnergyUsageFromCurveAndMinValue(hour, minValue);
        }

        #endregion

        #region Enemy evaluation methods

        /// <summary>
        /// Attempts to calculate an estimated amount of energy expended by a player based on an enemy's attributes
        /// </summary>
        /// <param name="enemy">The enemy to be evaluated</param>
        /// <returns>The estimated energy expended to kill this enemy and/or recover from damage they deal</returns>
        public float EnergyRatingForEnemy(BaseEnemy enemy)
        {
            //Psuedo-code for calculating energy usage
            //Life = HP * movementspeed
            //DPS = Damage * AttacksPerSecond * Range
            //energyusage = Life + DPS

            var hp = enemy.MaximumHealth * BaseEnergyPerHitPoint;
            var movementModifier = Math.Max(1f, enemy.Speed / BaseEnemyMovementSpeed);
            var life = hp * movementModifier;

            //var damage = enemy.IsRangedAttacker ? enemy.RangedAttackDamage : enemy.MeleeAttackDamage;
            //var rangeModifier = enemy.IsRangedAttacker ? (enemy.RangedAttackRadius / BaseCombatStructureRange) : 1f;
            //var perSecondModifier = enemy.IsRangedAttacker
            //    ? enemy.SecondsBetweenRangedAttack
            //    : enemy.SecondsBetweenMeleeAttack;

            //var dps = damage * rangeModifier * perSecondModifier;

            return life;// + dps;
        }
        #endregion
    }
}