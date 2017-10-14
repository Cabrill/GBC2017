using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Graphics;
using GBC2017.Entities.BaseEntities;
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

        protected float EnergyToSpend;

        public abstract float WaterFlowRate { get; }

        private DateTime _lastEnemyWave;
        private Layer _layerForEnemies;
        private int _wavesSent;
        private int _wavesToEaseIntoDifficulty = 48;
        protected FlatRedBall.Math.PositionedObjectList<BaseEnemy> _enemyList;

        protected BaseLevel(FlatRedBall.Math.PositionedObjectList<BaseEnemy> enemyList, Layer layerForEnemies)
        {
            this._layerForEnemies = layerForEnemies;
            _wavesSent = 0;
            EnergyToSpend = 0;
            _enemyList = enemyList;
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
                return currentDateTime >= EndTime && _enemyList.Count == 0;
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
        private void CreateEnemiesFromEnergy()
        {
            if (!(EnergyToSpend >= GameFormulas.Instance.MinimumEnergyCostForAnEnemy)) return;

            var newEnemy = GameFormulas.Instance.StrongestAffordableEnemy(ref EnergyToSpend, _layerForEnemies);
            newEnemy?.PlaceOnRightSide();
        }

        public void Update(DateTime currentDateTime)
        {
            if (currentDateTime > _lastEnemyWave && currentDateTime.Hour != _lastEnemyWave.Hour)
            {
                var energyToSpend = 0f;

                //Don't increment energy after time limit is reached, just spend what's already available
                if (currentDateTime <= EndTime)
                {

                    var wavesModifier = 1f;
                    if (_wavesSent < _wavesToEaseIntoDifficulty)
                    {
                        wavesModifier = (float) _wavesSent / _wavesToEaseIntoDifficulty;
                    }

                    energyToSpend = wavesModifier *
                                    GameFormulas.Instance.HourlyEnergyUsageFromCurveAndAvgValue(currentDateTime.Hour,
                                        AvgDailyEnergyUsage);
                }

                EnergyToSpend += energyToSpend;

                _lastEnemyWave = currentDateTime;
                _wavesSent++;
            }

            CreateEnemiesFromEnergy();
        }
    }
}
