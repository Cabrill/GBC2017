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
            StructureHealth = $" {structure.HealthRemaining.ToString("0")} / {structure.MaximumHealth.ToString("0")}";

            if (structure.HasInternalBattery)
            {
                StructureEnergy = $"{structure.BatteryLevel.ToString("0")} / {structure.InternalBatteryMaxStorage.ToString("0")}";
                CurrentHasBatteryState = HasBattery.True;
            }
            else
            {
                CurrentHasBatteryState = HasBattery.False;
            }

            CurrentResourceUsageState = ResourceUsage.True;
            if (structure is Home)
            {
                var home = structure as Home;
                StructureMinerals = $"{home.MaxMineralsStorage}";
                CurrentStoresMineralsState = StoresMinerals.True;
            }
            else
            {
                CurrentStoresMineralsState = StoresMinerals.False;

            }

            var netEnergy = 0.0;
            var netMinerals = 0.0;

            var structureAsEnergyProducer = structure as BaseEnergyProducer;
            var structureAsMineralsProducer = structure as BaseMineralsProducer;

            if (structureAsEnergyProducer != null)
            {
                netEnergy += structureAsEnergyProducer.EnergyProducedPerSecond;
            }
            netEnergy -= structure.EnergyUsedLastSecond;

            if (structureAsMineralsProducer != null)
            {
                netMinerals += structureAsMineralsProducer.MineralsProducedPerSecond;
            }
            netMinerals -= structure.MineralsUsedLastSecond;

            SetEnergyUsage(netEnergy);
            SetMineralsUsage(netMinerals);
        }

        public void Hide()
        {
            Visible = false;
        }

        private void SetEnergyUsage(double energyUsage)
        {
            StructureEnergyChange = energyUsage.ToString("0.0");
            if (energyUsage > 0)
            {
                CurrentEnergyUsageState = EnergyUsage.Positive;
            }
            else if (energyUsage < 0)
            {
                CurrentEnergyUsageState = EnergyUsage.Negative;
            }
            else
            {
                CurrentEnergyUsageState = EnergyUsage.Balanced;
            }
        }

        private void SetMineralsUsage(double mineralsUsage)
        {
            StructureMineralChange = mineralsUsage.ToString("0.0");
            if (mineralsUsage > 0)
            {
                CurrentMineralsUsageState = MineralsUsage.Positive;
            }
            else if (mineralsUsage < 0)
            {
                CurrentMineralsUsageState = MineralsUsage.Negative;
            }
            else
            {
                CurrentMineralsUsageState = MineralsUsage.Balanced;
            }
        }
    }
}
