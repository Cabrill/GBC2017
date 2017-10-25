using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Graphics.Texture;
using FlatRedBall.Gui;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures.Utility;
using GBC2017.Performance;

namespace GBC2017.GumRuntimes
{
    public partial class BuildBarRuntime
    {
        public void UpdateButton(BaseStructure structure, IEntityFactory factory)
        {
            BuildButtonRuntime button = null;
            switch (structure)
            {
                case SolarPanels _:
                    button = SolarButton;
                    break;
                case WindTurbine _:
                    button = WindButton;
                    break;
                case HydroGenerator _:
                    button = HydroButton;
                    break;
                case LaserTurret _:
                    button = LaserTurretButton;
                    break;
                case Cannon _:
                    button = CannonButton;
                    break;
                case TallLaser _:
                    button = TallLaserButton;
                    break;
                case Battery _:
                    button = BatteryButton;
                    break;
                case CarbonTree _:
                    button = CarbonTreeButton;
                    break;
                case ShieldGenerator _:
                    button = ShieldGeneratorButton;
                    break;
            }
            button?.UpdateFromStructure(structure, factory);
        }


        public void SetHydroIsEnabled(bool isEnabled)
        {
            HydroButton.Enabled = isEnabled;
            HydroButton.CurrentEnabledStatusState = isEnabled ? BuildButtonRuntime.EnabledStatus.Enabled : BuildButtonRuntime.EnabledStatus.Disabled;
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
