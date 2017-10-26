using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class MineralsDisplayRuntime
    {
        public void UpdateDisplay(double storedMinerals)
        {
            MineralsBar.UpdateBar(storedMinerals, 0);
        }
    }
}
