using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class MineralsDisplayRuntime
    {
        public void UpdateDisplay(double mineralsIncrease, double mineralsDecrease, double storedMinerals, double maxStorage)
        {
            UpdateChanges(mineralsIncrease, mineralsDecrease);
            MineralsBar.UpdateBar(storedMinerals, maxStorage);
        }

        private void UpdateChanges(double energyIncrease, double energyDecrease)
        {
            var netEnergy = energyIncrease - energyDecrease;

            var energyIncreaseText = $"+ {Math.Round(energyIncrease)}";
            var energyDecreaseText = $"- {Math.Round(energyDecrease)}";
            var energyNetText = $"{Math.Round(netEnergy)}";

            MineralsIncreaseText.Text = energyIncreaseText;
            MineralsDecreaseText.Text = energyDecreaseText;
            MineralsNetText.Text = energyNetText;

            if (netEnergy > 0)
            {
                CurrentMineralsBalanceState = MineralsBalance.Positive;;
            }
            else if (netEnergy < 0)
            {
                CurrentMineralsBalanceState = MineralsBalance.Negative;
            }
            else
            {
                CurrentMineralsBalanceState = MineralsBalance.Balanced;
            }
        }
    }
}
