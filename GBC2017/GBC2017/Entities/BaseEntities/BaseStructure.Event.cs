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
	}
}
