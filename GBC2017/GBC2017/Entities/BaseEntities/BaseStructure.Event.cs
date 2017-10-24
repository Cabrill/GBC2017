using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities;
using GBC2017.Entities.Enemies;
using GBC2017.Entities.GraphicalElements;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Screens;
namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseStructure
	{
        void OnAfterIsBeingPlacedSet (object sender, EventArgs e)
        {
            CheckmarkInstance.Visible = IsBeingPlaced;
            XCancelInstance.Visible = IsBeingPlaced;

            if (_hudLayer != null)
            { 
                _hudLayer.Remove(SpriteInstance);
                 SpriteManager.AddToLayer(SpriteInstance, LayerProvidedByContainer);

                var structureAsWindTurbine = this as WindTurbine;
                if (structureAsWindTurbine != null)
                {
                    _hudLayer.Remove(structureAsWindTurbine.TurbineSprite);
                    SpriteManager.AddToLayer(structureAsWindTurbine.TurbineSprite, LayerProvidedByContainer);
                }

                var structureAsHydroGenerator = this as HydroGenerator;
                if (structureAsHydroGenerator != null)
                {
                    _hudLayer.Remove(structureAsHydroGenerator.LargeWheelSprite);
                    _hudLayer.Remove(structureAsHydroGenerator.SmallWheelSprite);
                    _hudLayer.Remove(structureAsHydroGenerator.InnerCircleSprite);

                    SpriteManager.AddToLayer(structureAsHydroGenerator.LargeWheelSprite, LayerProvidedByContainer);
                    SpriteManager.AddToLayer(structureAsHydroGenerator.SmallWheelSprite, LayerProvidedByContainer);
                    SpriteManager.AddToLayer(structureAsHydroGenerator.InnerCircleSprite, LayerProvidedByContainer);
                }
            }
        }

        void OnAfterIsValidLocationSet (object sender, EventArgs e)
        {
            if (CurrentState != VariableState.CantAfford && IsValidLocation)
            {
                CurrentState = VariableState.ValidLocation;
                CheckmarkInstance.CurrentState = Checkmark.VariableState.Enabled;
            }
            else
            {
                CurrentState = VariableState.InvalidLocation;
                CheckmarkInstance.CurrentState = Checkmark.VariableState.Disabled;
            }
        }

        void OnAfterIsTurnedOnSet (object sender, EventArgs e)
        {
            if (IsTurnedOn)
            {
                LightSpriteInstance.Red = 255;
                LightSpriteInstance.Green = 50;
                LightSpriteInstance.Blue = 50;
            }
            else
            {
                LightSpriteInstance.Red = 255;
                LightSpriteInstance.Green = 255;
                LightSpriteInstance.Blue = 255;
            }
        }
	}
}
