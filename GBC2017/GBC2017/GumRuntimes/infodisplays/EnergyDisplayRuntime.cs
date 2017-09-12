using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class EnergyDisplayRuntime
    {
        public void UpdateDisplay(double energyIncrease, double energyDecrease, double storedEnergy, double maxStorage)
        {
            UpdateChanges(energyIncrease, energyDecrease);
            EnergyBar.UpdateBar(storedEnergy, maxStorage);
        }

        private void UpdateChanges(double energyIncrease, double energyDecrease)
        {
            var netEnergy = energyIncrease - energyDecrease;

            var energyIncreaseText = $"+ {Math.Round(energyIncrease)}";
            var energyDecreaseText = $"- {Math.Round(energyDecrease)}";
            var energyNetText = $"{Math.Round(netEnergy)}";

            EnergyIncreaseText.Text = energyIncreaseText;
            EnergyDecreaseText.Text = energyDecreaseText;
            EnergyNetText.Text = energyNetText;

            if (netEnergy > 0)
            {
                CurrentEnergyBalanceState = EnergyBalance.Positive;
            }
            else if (netEnergy < 0)
            {
                CurrentEnergyBalanceState = EnergyBalance.Negative;
            }
            else
            {
                CurrentEnergyBalanceState = EnergyBalance.Balanced;
            }
        }
    }
}
