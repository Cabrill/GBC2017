using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Gui;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.ResourceManagers;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;

namespace GBC2017.GumRuntimes
{
    public partial class StructureInfoRuntime
    {
        private BaseStructure structureShown;

        public void CustomInitialize()
        {
            SwitchOnOffInstanceClick += OnSwitchOnOffInstanceClick;
            RepairButtonInstanceClick += OnRepairButtonInstanceClick;
            DestroyButtonInstanceClick += OnDestroyButtonInstanceClick;
        }

        private void OnDestroyButtonInstanceClick(IWindow window)
        {
            structureShown.DestroyByRequest();
            structureShown = null;
            Visible = false;
        }

        private void OnRepairButtonInstanceClick(IWindow window1)
        {
            if (RepairButtonInstance.CurrentEnabledStatusState == RepairButtonRuntime.EnabledStatus.Enabled)
            {
                var energyRepairCost = structureShown.GetEnergyRepairCost();
                var mineralRepairCost = structureShown.GetMineralRepairCost();

                var canAfford = EnergyManager.CanAfford(energyRepairCost) &&
                                MineralsManager.CanAfford(mineralRepairCost);

                if (canAfford)
                {
                    EnergyManager.DebitEnergyForBuildRequest(energyRepairCost);
                    MineralsManager.DebitMinerals(mineralRepairCost);
                    structureShown.Repair();
                }
            }
        }

        private void OnSwitchOnOffInstanceClick(IWindow window1)
        {
            structureShown.IsTurnedOn = !(structureShown.IsTurnedOn);
        }

        public void Show(BaseStructure structure)
        {
            if (structure == structureShown)
            {
                Visible = true;
                Update();
            }
            else
            {
                SetDisplayFor(structure);
            }
        }

        private void SetDisplayFor(BaseStructure structure)
        {
            Visible = true;
            structureShown = structure;

            var minMaxX = (CameraZoomManager.OriginalOrthogonalWidth - GetAbsoluteWidth()) / 2;
            var minMaxY = (CameraZoomManager.OriginalOrthogonalHeight - GetAbsoluteHeight()) / 2;

            var newX = (structureShown.X - Camera.Main.X) * CameraZoomManager.GumCoordOffset;
            var newY = (structureShown.Y - Camera.Main.Y + structureShown.SpriteInstance.Height / 2) *
                       CameraZoomManager.GumCoordOffset + GetAbsoluteHeight() / 2;

            X = MathHelper.Clamp(newX, -minMaxX, minMaxX);
            Y = MathHelper.Clamp(newY, -minMaxY, minMaxY);

            StructureName = structureShown.DisplayName;

            if (structure.HasInternalBattery)
            {
                StructureEnergy =
                    $"{structure.BatteryLevel.ToString("0")} / {structure.InternalBatteryMaxStorage.ToString("0")}";
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
                DestroyButtonInstanceEnabledStatusState = DestroyButtonRuntime.EnabledStatus.Disabled;
                SwitchOnOffInstance.Visible = false;
            }
            else
            {
                CurrentStoresMineralsState = StoresMinerals.False;
                DestroyButtonInstance.CurrentEnabledStatusState = DestroyButtonRuntime.EnabledStatus.Enabled;
                SwitchOnOffInstance.Visible = true;
            }
        }

        public void Update()
        {

            StructureHealth = $" {structureShown.HealthRemaining.ToString("0")} / {structureShown.MaximumHealth.ToString("0")}";

            if (CurrentHasBatteryState == HasBattery.True)
            {
                StructureEnergy = $"{structureShown.BatteryLevel.ToString("0")} / {structureShown.InternalBatteryMaxStorage.ToString("0")}";
            }

            if (!(structureShown is Home))
            {
                SwitchOnOffInstance.CurrentOnOffStateState = structureShown.IsTurnedOn
                    ? SwitchOnOffRuntime.OnOffState.On
                    : SwitchOnOffRuntime.OnOffState.Off;
            }


            if (structureShown.HealthRemaining >= structureShown.MaximumHealth)
            {
                RepairButtonInstance.CurrentEnabledStatusState = RepairButtonRuntime.EnabledStatus.Disabled;
            }
            else
            {
                var energyRepairCost = structureShown.GetEnergyRepairCost();
                var mineralRepairCost = structureShown.GetMineralRepairCost();

                var canAfford = EnergyManager.CanAfford(energyRepairCost) &&
                                MineralsManager.CanAfford(mineralRepairCost);
                RepairButtonInstance.CurrentEnabledStatusState = canAfford
                    ? RepairButtonRuntime.EnabledStatus.Enabled
                    : RepairButtonRuntime.EnabledStatus.Unaffordable;
            }


            var netEnergy = 0.0;
            var netMinerals = 0.0;

            var structureAsEnergyProducer = structureShown as BaseEnergyProducer;
            var structureAsMineralsProducer = structureShown as BaseMineralsProducer;

            if (structureAsEnergyProducer != null)
            {
                netEnergy += structureAsEnergyProducer.EffectiveEnergyProducedPerSecond;
            }
            netEnergy -= structureShown.EnergyReceivedLastSecond;

            if (structureAsMineralsProducer != null)
            {
                netMinerals += structureAsMineralsProducer.MineralsProducedPerSecond;
            }
            netMinerals -= structureShown.MineralsReceivedLastSecond;

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
