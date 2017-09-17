using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class BuildBarRuntime
    {
        partial void CustomInitialize()
        {
            
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
