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
            var netEnergy = energyIncrease - energyDecrease;

            var energyIncreaseText = $"+ {energyIncrease}";
            var energyDecreaseText = $"- {energyDecrease}";
            var energyNetText = $"{netEnergy}";

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
