using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class InfoBarRuntime
    {
        public void UpdateEnergyDisplay(double energyIncrease, double energyDecrease, double currentStorage, double maxStorage)
        {
            EnergyDisplayInstance.UpdateDisplay(energyIncrease, energyDecrease, currentStorage, maxStorage);
        }

        public void UpdateMineralsDisplay(double mineralsIncrease, double mineralsDecrease, double storedMinerals, double maxStorage)
        {
            MineralsDisplayInstance.UpdateDisplay(mineralsIncrease, mineralsDecrease, storedMinerals, maxStorage);
        }
    }
}
