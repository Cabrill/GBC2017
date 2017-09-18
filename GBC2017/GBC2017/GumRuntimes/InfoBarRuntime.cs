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
        public void Update()
        {
            UpdateEnergyDisplay(EnergyManager.EnergyIncrease, EnergyManager.EnergyDecrease,
                EnergyManager.StoredEnergy, EnergyManager.MaxStorage);

            UpdateMineralsDisplay(MineralsManager.MineralsIncrease, MineralsManager.MineralsDecrease,
                MineralsManager.StoredMinerals, MineralsManager.MaxStorage);

            ConditionsDisplayInstance.Update();
        }

        private void UpdateEnergyDisplay(double energyIncrease, double energyDecrease, double currentStorage, double maxStorage)
        {
            EnergyDisplayInstance.UpdateDisplay(energyIncrease, energyDecrease, currentStorage, maxStorage);
        }

        private void UpdateMineralsDisplay(double mineralsIncrease, double mineralsDecrease, double storedMinerals, double maxStorage)
        {
            MineralsDisplayInstance.UpdateDisplay(mineralsIncrease, mineralsDecrease, storedMinerals, maxStorage);
        }
    }
}
