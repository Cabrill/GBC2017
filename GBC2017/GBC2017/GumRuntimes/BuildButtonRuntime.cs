using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures.EnergyProducers;
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
            StructureSprite.TextureLeft = (int)structure.SpriteInstance.LeftTexturePixel;
            StructureSprite.TextureTop = (int)structure.SpriteInstance.TopTexturePixel;
            StructureSprite.Width = textureWidth / textureHeight * 50;

            var structureAsWindTurbine = structure as WindTurbine;

            if (structureAsWindTurbine != null)
            {
                StructureSprite2.SourceFile = structureAsWindTurbine.TurbineSprite.Texture;
                textureHeight = structureAsWindTurbine.TurbineSprite.BottomTexturePixel - structureAsWindTurbine.TurbineSprite.TopTexturePixel;
                textureWidth = structureAsWindTurbine.TurbineSprite.RightTexturePixel - structureAsWindTurbine.TurbineSprite.LeftTexturePixel;
                StructureSprite2.TextureHeight = (int)textureHeight;
                StructureSprite2.TextureWidth = (int)textureWidth;
                StructureSprite2.TextureLeft = (int)structureAsWindTurbine.TurbineSprite.LeftTexturePixel;
                StructureSprite2.TextureTop = (int)structureAsWindTurbine.TurbineSprite.TopTexturePixel;

                StructureSprite2.Visible = true;
            }

            StructureName.Text = structure.DisplayName;

            var energyChange = (double)structure.EnergyRequiredPerSecond;
            var mineralChange = 0.0;

            var baseEnergyProducer = structure as BaseEnergyProducer;
            var baseCombatStructure = structure as BaseCombatStructure;
            var baseMineralsProducer = structure as BaseMineralsProducer;

            if (baseEnergyProducer != null)
            {
                energyChange = baseEnergyProducer.EffectiveEnergyProducedPerSecond;
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
            energyChange = Math.Round(energyChange);
            var energyChangeText = energyChange.ToString("0.##");
            EnergyChangeText.Text = energyChangeText;
            CurrentEnergyChangeDigitsState = GetDigitState(energyChangeText.Length);

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

        private static EnergyChangeDigits GetDigitState(int length)
        {
            switch (length)
            {
                case 1: return EnergyChangeDigits.SingleDigit;
                case 2: return EnergyChangeDigits.DoubleDigits;
                case 3: return EnergyChangeDigits.TripleDigits;
                case 4: return EnergyChangeDigits.QuadDigits;
                default: return EnergyChangeDigits.SingleDigit;
            }
        }
    }
}
