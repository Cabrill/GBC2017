using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;

namespace GBC2017.GumRuntimes
{
    public partial class StructureInfoRuntime
    {
        public void Show(BaseStructure structure)
        {
            Visible = true;
            X = structure.X;
            Y = structure.Y + structure.SpriteInstance.Height / 2;
            StructureName = structure.DisplayName;
            StructureHealth = $" {structure.HealthRemaining} / {structure.MaximumHealth}";

            if (structure.HasInternalBattery)
            {
                StructureEnergy = $"{structure.BatteryLevel} / {structure.InternalBatteryMaxStorage}";

            }
            else
            {
                
            }

            if (structure is Home)
            {
                var home = structure as Home;
                StructureMinerals = $"{home.MaxMineralsStorage}";
            }
        }

        public void Hide()
        {
            Visible = false;
        }
    }
}
