using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Graphics;
using GBC2017.Factories;
using GBC2017.GameClasses.Interfaces;

namespace GBC2017.GameClasses.BaseClasses
{
    public abstract class BaseLevel
    {
        public abstract ICity City { get; }
        public abstract DateTime StartTime { get; }
        public abstract DateTime EndTime { get; }
        public abstract float AvgDailyEnergyUsage { get; }

        private DateTime _lastEnemyWave;
        private Layer _layerForEnemies;
        private int _wavesSent;
        private int _wavesToEaseIntoDifficulty = 24;

        protected BaseLevel(Layer layerForEnemies)
        {
            this._layerForEnemies = layerForEnemies;
            _wavesSent = 0;
        }

        /// <summary>
        /// Decides whether the player has achieved victory.  If there is an end time this method will return true
        /// if the player has passed the end time.  For other conditions, the derived Level should override this.
        /// </summary>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        public bool HasReachedVictory(DateTime currentDateTime)
        {
            if (EndTime != DateTime.MaxValue)
            {
                return currentDateTime >= EndTime;
            }
            return false;
        }

        /// <summary>
        /// Decides whether the player has been defeated.  All levels end when the house has been destroyed, but
        /// if there are additional defeat criteria then the derived Level should override this.
        /// </summary>
        /// <param name="currentDateTime">The current game time</param>
        /// <returns></returns>
        public bool HasReachedDefeat(DateTime currentDateTime)
        {
            return false;
        }

        /// <summary>
        /// Creates enemies equal to the energy amount
        /// </summary>
        /// <param name="energyAmount">Amount of energy to spend in creating enemies</param>
        public void CreateEnemiesFromEnergy(float energyAmount)
        {
            var minimumCostOfAnEnemy = GameFormulas.Instance.MinimumEnergyCostForAnEnemy;
            var energyAvailable = energyAmount;

            while (energyAvailable >= minimumCostOfAnEnemy)
            {
                var newEnemy = BasicAlienFactory.CreateNew(_layerForEnemies);
                newEnemy.PlaceOnRightSide();
                energyAvailable -= GameFormulas.Instance.EnergyRatingForEnemy(newEnemy);
            }
        }

        public void Update(DateTime currentDateTime)
        {
            if (currentDateTime > _lastEnemyWave && currentDateTime.Hour != _lastEnemyWave.Hour)
            {
                var wavesModifier = 1f;
                if (_wavesSent < _wavesToEaseIntoDifficulty)
                {
                    wavesModifier = (float)_wavesSent / _wavesToEaseIntoDifficulty;
                }

                var energyToSpend = wavesModifier *
                    GameFormulas.Instance.HourlyEnergyUsageFromCurveAndAvgValue(currentDateTime.Hour,
                        AvgDailyEnergyUsage);

                CreateEnemiesFromEnergy(energyToSpend);

                _lastEnemyWave = currentDateTime;
                _wavesSent++;
            }
        }
    }
}
