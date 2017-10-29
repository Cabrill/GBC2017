using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.ResourceManagers;

namespace GBC2017.GumRuntimes
{
    public partial class NewResourceBarRuntime
    {
        public void UpdateBar(double current, double total)
        {
            if (TextInstance.Visible)
            {
                if (total > 0)
                {
                    TextInstance.Text = $"{EnergyManager.FormatEnergyAmount(current)}/{EnergyManager.FormatEnergyAmount(total)}";
                }
                else
                {
                    TextInstance.Text = $"{EnergyManager.FormatEnergyAmount(current)}";
                }
            }

            if (total > 0)
            {
                BarFillPercent = (float)(current / total) * 100f;
            }
            else
            {
                BarFillPercent = 100;
            }
        }
    }
}
