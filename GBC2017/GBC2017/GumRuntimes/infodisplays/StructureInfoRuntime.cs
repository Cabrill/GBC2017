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
using GBC2017.Entities.Structures.Utility;
using GBC2017.ResourceManagers;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;

namespace GBC2017.GumRuntimes
{
    public partial class StructureInfoRuntime
    {
        private BaseStructure structureShown;

        partial void CustomInitialize()
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
            if (structureShown.CurrentOnOffState == BaseStructure.OnOff.On)
            {
                structureShown.CurrentOnOffState = BaseStructure.OnOff.Off;
            }
            else
            {
                structureShown.CurrentOnOffState = BaseStructure.OnOff.On;
            }
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
                CurrentHasBatteryState = HasBattery.True;
            }
            else
            {
                CurrentHasBatteryState = HasBattery.False;
            }

            CurrentResourceUsageState = ResourceUsage.True;
            if (structure is Home)
            {
                DestroyButtonInstance.CurrentEnabledStatusState = DestroyButtonRuntime.EnabledStatus.Disabled;
                SwitchOnOffInstance.Visible = false;
            }
            else
            {
                DestroyButtonInstance.CurrentEnabledStatusState = DestroyButtonRuntime.EnabledStatus.Enabled;
                SwitchOnOffInstance.Visible = (!(structure is Battery));
            }
        }

        private void Update()
        {

            StructureHealth = $" {structureShown.HealthRemaining.ToString("0")}";
            HealthBar.BarFillPercent = (structureShown.HealthRemaining / structureShown.MaximumHealth) * 100f;

            if (CurrentHasBatteryState == HasBattery.True)
            {
                EnergyBar.BarFillPercent =
                    (float) (structureShown.BatteryLevel / structureShown.InternalBatteryMaxStorage) * 100f;
            }
            else
            {
                EnergyBar.BarFillPercent = 0;
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
                var mineralRepairCost = structureShown.GetMineralRepairCost();
                var canAfford = MineralsManager.CanAfford(mineralRepairCost);

                RepairButtonInstance.CurrentEnabledStatusState = canAfford
                    ? RepairButtonRuntime.EnabledStatus.Enabled
                    : RepairButtonRuntime.EnabledStatus.Unaffordable;
            }

            var netEnergy = 0.0;

            var structureAsEnergyProducer = structureShown as BaseEnergyProducer;

            if (structureAsEnergyProducer != null)
            {
                netEnergy += structureAsEnergyProducer.EffectiveEnergyProducedPerSecond;
            }
            netEnergy += structureShown.EnergyReceivedLastSecond;
            
            SetEnergyUsage(netEnergy);
        }

        public void Hide()
        {
            Visible = false;
        }

        private void SetEnergyUsage(double energyUsage)
        {
            var energyChange = EnergyManager.FormatEnergyAmount(energyUsage);

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
            StructureEnergyChange = energyChange;
        }
    }
}
