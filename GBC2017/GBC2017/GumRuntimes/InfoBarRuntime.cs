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
    }
}
