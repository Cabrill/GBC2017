using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Graphics.Texture;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures.Utility;

namespace GBC2017.GumRuntimes
{
    public partial class BuildBarRuntime
    {
        public void UpdateButton(BaseStructure structure)
        {
            BuildButtonRuntime button = null;

            if (structure is SolarPanels) button = SolarButton;
            else if (structure is WindTurbine) button = WindButton;
            else if (structure is LaserTurret) button = LaserTurretButton;
            else if (structure is Battery) button = BatteryButton;
            else if (structure is CarbonTree) button = CarbonTreeButton;
            else if (structure is ShieldGenerator) button = ShieldGeneratorButton;
            
            button?.UpdateFromStructure(structure);
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
