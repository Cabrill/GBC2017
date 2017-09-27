using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures.Utility;

namespace GBC2017.GumRuntimes
{
    public partial class BuildButtonRuntime
    {
        public void UpdateFromStructure(BaseStructure structure)
        {
            EnergyCost = structure.EnergyBuildCost.ToString();
            MineralsCost = structure.MineralsBuildCost.ToString();
            StructureSprite.SourceFile = structure.SpriteInstance.Texture;

            var textureHeight = structure.SpriteInstance.BottomTexturePixel - structure.SpriteInstance.TopTexturePixel;
            var textureWidth = structure.SpriteInstance.RightTexturePixel - structure.SpriteInstance.LeftTexturePixel;

            StructureSprite.TextureHeight = (int)textureHeight;
            StructureSprite.TextureWidth = (int)textureWidth;
            StructureSprite.TextureLeft = (int)structure.SpriteInstance.LeftTextureCoordinate;
            StructureSprite.TextureTop = (int)structure.SpriteInstance.TopTextureCoordinate;
            StructureSprite.Width = textureWidth / textureHeight * 50;

            StructureName.Text = structure.DisplayName;

            var energyChange = (double)structure.EnergyRequiredPerSecond;
            var mineralChange = 0.0;

            var baseEnergyProducer = structure as BaseEnergyProducer;
            var baseCombatStructure = structure as BaseCombatStructure;
            var baseMineralsProducer = structure as BaseMineralsProducer;

            if (baseEnergyProducer != null)
            {
                energyChange = baseEnergyProducer.BaseEnergyProducedPerSecond;
                CurrentEnergyChangeState = energyChange > 0 ? EnergyChange.Increase : EnergyChange.NoChange;
            }
            else if (baseCombatStructure != null)
            {
                energyChange = baseCombatStructure.EnergyCostToFire /
                                          baseCombatStructure.SecondsBetweenFiring;
                CurrentEnergyChangeState = energyChange > 0 ? EnergyChange.Decrease : EnergyChange.NoChange;
            }
            else
            {
                energyChange = structure.EnergyRequiredPerSecond;
                CurrentEnergyChangeState = energyChange > 0 ? EnergyChange.Decrease : EnergyChange.NoChange;
            }
            EnergyChangeText.Text = energyChange.ToString();
            CurrentEnergyChangeDigitsState = energyChange >= 10 ? EnergyChangeDigits.DoubleDigits : EnergyChangeDigits.SingleDigit;

            if (baseMineralsProducer != null)
            {
                mineralChange = baseMineralsProducer.MineralsProducedPerSecond;
                CurrentMineralChangeState = MineralChange.Increase;
            }
            else
            {
                CurrentMineralChangeState = mineralChange > 0 ? MineralChange.Decrease : MineralChange.NoChange;
            }
            MineralsChangeText.Text = mineralChange.ToString();
        }
    }
}
