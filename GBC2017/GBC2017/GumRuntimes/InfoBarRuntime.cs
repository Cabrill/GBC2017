using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.ResourceManagers;

namespace GBC2017.GumRuntimes
{
    public partial class InfoBarRuntime
    {
        private DateTime _lastForecastUpdate;

        public void Initialize(DateTime gameTime, DateTime endTime, List<float> hourlySunForecast, List<float> hourlyWindForecast, List<float> hourlyWaterForecast)
        {
            ConditionsForecastInstance.Initialize(endTime, hourlySunForecast, hourlyWindForecast, hourlyWaterForecast);
            _lastForecastUpdate = gameTime;
        }

        public void Update(DateTime gameTime, List<float> hourlySunForecast, List<float> hourlyWindForecast, List<float> hourlyWaterForecast)
        {
            UpdateEnergyDisplay(EnergyManager.EnergyIncrease, EnergyManager.EnergyDecrease,
                EnergyManager.StoredEnergy, EnergyManager.MaxStorage);

            UpdateMineralsDisplay(MineralsManager.StoredMinerals);

            if ((gameTime - _lastForecastUpdate).Hours >= 1)
            {
                ConditionsForecastInstance.UpdateFirstItem(hourlySunForecast, hourlyWindForecast, hourlyWaterForecast);
                _lastForecastUpdate = gameTime;
            }

            ConditionsForecastInstance.Update(gameTime);
        }

        private void UpdateEnergyDisplay(double energyIncrease, double energyDecrease, double currentStorage, double maxStorage)
        {
            EnergyDisplayInstance.UpdateDisplay(energyIncrease, energyDecrease, currentStorage, maxStorage);
        }

        private void UpdateMineralsDisplay(double storedMinerals)
        {
            MineralsDisplayInstance.UpdateDisplay(storedMinerals);
        }

        public void Reset()
        {
            EnergyDisplayInstance.UpdateDisplay(0, 0, 1000, 1000);
        }
    }
}
