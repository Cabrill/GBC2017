using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlatRedBall.Graphics;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Enemies;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Factories;

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
        private const float KiloWattToWatt = 1000;
        #endregion

        #region Public properties

        public const int RealSecondsPerGameHour = 8;
        public float MinimumEnergyCostForAnEnemy;
        private float EnergyCostOfBasicAlien;
        private float EnergyCostOfFlyingEnemy;
        private float EnergyCostOfSlimeAlien;
        private float EnergyCostOfMeleeAlien;
        private float EnergyCostOfSmallSlime;

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
            basicTower.Destroy();

            //Instantiate an instance of the base enemy, and read it's stats
            var basicEnemy = new BasicAlien();
            BaseEnemyMovementSpeed = basicEnemy.Speed;

            EnergyCostOfBasicAlien = EnergyRatingForEnemy(basicEnemy);
            basicEnemy.Destroy();

            var slime = new SlimeAlien();
            EnergyCostOfSlimeAlien = EnergyRatingForEnemy(slime);
            slime.Destroy();

            var flying = new FlyingEnemy();
            EnergyCostOfFlyingEnemy = EnergyRatingForEnemy(flying);
            flying.Destroy();

            var melee = new MeleeAlien();
            EnergyCostOfMeleeAlien = EnergyRatingForEnemy(melee);
            melee.Destroy();

            var smallslime = new SmallSlime();
            EnergyCostOfSmallSlime = EnergyRatingForEnemy(smallslime);
            smallslime.Destroy();

            MinimumEnergyCostForAnEnemy = Math.Min(EnergyCostOfBasicAlien, float.MaxValue);
            MinimumEnergyCostForAnEnemy = Math.Min(EnergyCostOfSlimeAlien, MinimumEnergyCostForAnEnemy);
            MinimumEnergyCostForAnEnemy = Math.Min(EnergyCostOfFlyingEnemy, MinimumEnergyCostForAnEnemy);
            MinimumEnergyCostForAnEnemy = Math.Min(EnergyCostOfMeleeAlien, MinimumEnergyCostForAnEnemy);
            MinimumEnergyCostForAnEnemy = Math.Min(EnergyCostOfSmallSlime, MinimumEnergyCostForAnEnemy);
        }
        #endregion

        #region Energy usage methods

        /// <summary>
        /// Takes the desired hour, and the minimum hourly value for the day, then returns the typical energy consumption for the given hour
        /// </summary>
        /// <param name="hour">The hour of energy use you need</param>
        /// <param name="minValue">The minimum hourly energy usage in WattHours for the entire day</param>
        /// <returns></returns>
        public float HourlyEnergyUsageFromCurveAndMinValue(int hour, float minValue = 2.3f)
        {
            if (hour >= 0 && hour <= 23)
            {
                return HourlyUsageModifiers[hour] * minValue * KiloWattToWatt; ;
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
        /// <param name="avgValue">The average hourly energy usage in WattHours for the entire day</param>
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
        /// <returns>The estimated energy in watts expended to kill this enemy and/or recover from damage they deal</returns>
        public float EnergyRatingForEnemy(BaseEnemy enemy)
        {
            //Psuedo-code for calculating energy usage
            //Life = HP * movementspeed
            //DPS = Damage * AttacksPerSecond * Range
            //energyusage = Life + DPS

            //Adding two free hits here, since the turret will fire at least twice more after an enemy dies
            var hp = enemy.MaximumHealth * BaseEnergyPerHitPoint + (BaseEnergyPerHitPoint*2);
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

        public BaseEnemy StrongestAffordableEnemy(ref float energyAmount, bool includeBoss, Layer layerToPutEnemyOn)
        {
            var energyToSpend = 0f;
            energyToSpend = energyToSpend < EnergyCostOfBasicAlien && EnergyCostOfBasicAlien < energyAmount ? EnergyCostOfBasicAlien : energyToSpend;
            energyToSpend = energyToSpend < EnergyCostOfFlyingEnemy && EnergyCostOfFlyingEnemy < energyAmount ? EnergyCostOfFlyingEnemy : energyToSpend;
            energyToSpend = energyToSpend < EnergyCostOfMeleeAlien && EnergyCostOfMeleeAlien < energyAmount ? EnergyCostOfMeleeAlien : energyToSpend;
            energyToSpend = energyToSpend < EnergyCostOfSmallSlime && EnergyCostOfSmallSlime < energyAmount ? EnergyCostOfSmallSlime : energyToSpend;
            energyToSpend = includeBoss && energyToSpend < EnergyCostOfSlimeAlien && EnergyCostOfSlimeAlien < energyAmount ? EnergyCostOfSlimeAlien : energyToSpend;

            if (energyToSpend == 0)
            {
                return null;
            }

            if (includeBoss && energyToSpend == EnergyCostOfSlimeAlien)
            {
                var boss = SlimeAlienFactory.CreateNew(layerToPutEnemyOn);
                boss.MaximumHealth = energyAmount / BaseEnergyPerHitPoint;
                boss.HealthRemaining = boss.MaximumHealth;
                energyAmount = 0;
                return boss;
            }

            energyAmount -= energyToSpend;

            if (energyToSpend == EnergyCostOfBasicAlien) return BasicAlienFactory.CreateNew(layerToPutEnemyOn);
            if (energyToSpend == EnergyCostOfFlyingEnemy) return FlyingEnemyFactory.CreateNew(layerToPutEnemyOn);
            if (energyToSpend == EnergyCostOfMeleeAlien) return MeleeAlienFactory.CreateNew(layerToPutEnemyOn);
            if (energyToSpend == EnergyCostOfSmallSlime) return SmallSlimeFactory.CreateNew(layerToPutEnemyOn);
            //if (energyToSpend == EnergyCostOfSlimeAlien) return SlimeAlienFactory.CreateNew(layerToPutEnemyOn);

            return null;
        }
        #endregion
    }
}