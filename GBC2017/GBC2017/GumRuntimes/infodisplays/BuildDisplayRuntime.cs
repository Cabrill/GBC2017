using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures.Utility;
using GBC2017.ResourceManagers;
using GBC2017.StaticManagers;
using Gum.DataTypes;

namespace GBC2017.GumRuntimes
{
    public partial class BuildDisplayRuntime
    {
        public void Update(BaseStructure structure)
        {
            Visible = true;

            StructureName = structure.DisplayName;
            MineralCost = structure.MineralsBuildCost.ToString("0");
            StructureHealth = structure.HealthRemaining.ToString("0");
            DescriptionText = structure.StructureDescription;

            HealthBar.BarFillPercent = (structure.MaximumHealth/100) * 100f;
            

            StructureSprite.Texture = structure.SpriteInstance.Texture;
            var textureHeight = structure.SpriteInstance.BottomTexturePixel - structure.SpriteInstance.TopTexturePixel;
            var textureWidth = structure.SpriteInstance.RightTexturePixel - structure.SpriteInstance.LeftTexturePixel;

            StructureSprite.TextureHeight = (int)textureHeight;
            StructureSprite.TextureWidth = (int)textureWidth;
            StructureSprite.TextureLeft = (int)structure.SpriteInstance.LeftTexturePixel;
            StructureSprite.TextureTop = (int)structure.SpriteInstance.TopTexturePixel;

            if (textureWidth > textureHeight)
            {
                StructureSprite.WidthUnits = DimensionUnitType.Percentage;
                StructureSprite.HeightUnits = DimensionUnitType.PercentageOfOtherDimension;
                StructureSprite.Width = 100;
                StructureSprite.Height = textureHeight / textureWidth * 100;
            }
            else
            {
                var maxWidth = SpriteContainer.Width;

                var proposedHeight = 100f;
                var proposedWidth = textureWidth / textureHeight * 100;
                
                if (proposedWidth > maxWidth)
                {
                    proposedHeight = (maxWidth / proposedWidth) * 100f;
                }

                StructureSprite.HeightUnits = DimensionUnitType.Percentage;
                StructureSprite.WidthUnits = DimensionUnitType.PercentageOfOtherDimension;
                StructureSprite.Height = proposedHeight;
                StructureSprite.Width = proposedWidth;
            }

            if (structure is SolarPanels)
            {
                EnergyBar.BarFillPercent = 10;
                CurrentEnergyUsageState = EnergyUsage.Positive;
                StructureEnergyChange = "";
            }
            else if (structure is WindTurbine)
            {
                EnergyBar.BarFillPercent = 60;
                CurrentEnergyUsageState = EnergyUsage.Positive;
                StructureEnergyChange = "";
            }
            else if (structure is HydroGenerator)
            {
                EnergyBar.BarFillPercent = 100;
                CurrentEnergyUsageState = EnergyUsage.Positive;
                StructureEnergyChange = "";
            }
            else if (structure is Battery)
            {
                EnergyBar.BarFillPercent = 0;
                CurrentEnergyUsageState = EnergyUsage.None;
                StructureEnergyChange = "";
            }
            else if (structure is CarbonTree tree)
            {
                EnergyBar.BarFillPercent = 0;
                CurrentEnergyUsageState = EnergyUsage.Negative;
                StructureEnergyChange = tree.EnergyRequiredPerSecond.ToString("#");
            }
            else if (structure is ShieldGenerator shield)
            {
                EnergyBar.BarFillPercent = 0;
                CurrentEnergyUsageState = EnergyUsage.Negative;
                StructureEnergyChange = shield.EnergyRequiredPerSecond.ToString("#");
            }
            else if (structure is BaseCombatStructure combat)
            {
                EnergyBar.BarFillPercent = 0;
                CurrentEnergyUsageState = EnergyUsage.Negative;
                StructureEnergyChange = (combat.EnergyCostToFire / combat.SecondsBetweenFiring).ToString("#");
            }
            else
            {
                EnergyBar.BarFillPercent = 0;
                CurrentEnergyUsageState = EnergyUsage.None;
            }
        }
    }
}
