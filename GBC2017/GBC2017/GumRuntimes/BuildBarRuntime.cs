using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class BuildBarRuntime
    {
        public void UpdateSolarButton(double energy, double minerals)
        {
            SolarButton.EnergyCost = energy.ToString();
            SolarButton.MineralsCost = minerals.ToString();
        }

        public void UpdateWindButton(double energy, double minerals)
        {
            WindButton.EnergyCost = energy.ToString();
            WindButton.MineralsCost = minerals.ToString();
        }

        public void UpdateLaserButton(double energy, double minerals)
        {
            LaserTurretButton.EnergyCost = energy.ToString();
            LaserTurretButton.MineralsCost = minerals.ToString();
        }

        public void UpdateBatteryButton(double energy, double minerals)
        {
            BatteryButton.EnergyCost = energy.ToString();
            BatteryButton.MineralsCost = minerals.ToString();
        }

        public void UpdateCarbonTreeButton(double energy, double minerals)
        {
            CarbonTreeButton.EnergyCost = energy.ToString();
            CarbonTreeButton.MineralsCost = minerals.ToString();
        }

        public void UpdateShieldGeneratorButton(double energy, double minerals)
        {
            ShieldGeneratorButton.EnergyCost = energy.ToString();
            ShieldGeneratorButton.MineralsCost = minerals.ToString();
        }

        public void UpdateSelection()
        {
            UnhighlightAll();

            switch (CurrentBuildMenuState)
            {
                case BuildMenu.BuildEnergy: EnergyStructureButton.CurrentHighlightedState = CategoryButtonRuntime.Highlighted.True;
                    break;
                case BuildMenu.BuildCombat: CombatStructureButton.CurrentHighlightedState = CategoryButtonRuntime.Highlighted.True;
                    break;
                case BuildMenu.BuildUtility: UtilityStructureButton.CurrentHighlightedState = CategoryButtonRuntime.Highlighted.True;
                    break;
            }
        }

        private void UnhighlightAll()
        {
            EnergyStructureButton.CurrentHighlightedState = CategoryButtonRuntime.Highlighted.False;
            CombatStructureButton.CurrentHighlightedState = CategoryButtonRuntime.Highlighted.False;
            UtilityStructureButton.CurrentHighlightedState = CategoryButtonRuntime.Highlighted.False;
        }
    }
}
