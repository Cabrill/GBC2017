using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures.Utility;
using GBC2017.GameClasses.Interfaces;
using GBC2017.Performance;
using Gum.DataTypes;

namespace GBC2017.GumRuntimes
{
    public partial class BuildButtonRuntime : IBuildButton
    {
        public IEntityFactory BuildingFactory { get; private set; }
        public Type BuildingType { get; private set; }
        public bool IsEnabled => Enabled;

        public void UpdateFromStructure(BaseStructure structure, IEntityFactory factory)
        {
            BuildingFactory = factory;
            BuildingType = structure.GetType();

            MineralsCost = structure.MineralsBuildCost.ToString();
            StructureSprite.SourceFile = structure.SpriteInstance.Texture;

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
                StructureSprite.Width = 80;
                StructureSprite.Height = textureHeight / textureWidth * 100;
            }
            else
            {
                StructureSprite.HeightUnits = DimensionUnitType.Percentage;
                StructureSprite.WidthUnits = DimensionUnitType.PercentageOfOtherDimension;
                StructureSprite.Height = 80;
                StructureSprite.Width = textureWidth / textureHeight * 100;
            }
        }
    }
}
