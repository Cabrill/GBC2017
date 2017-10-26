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

            UpdateMineralsDisplay(MineralsManager.StoredMinerals);

            //ConditionsDisplayInstance.Update();
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
