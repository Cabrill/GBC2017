using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.StaticManagers;

namespace GBC2017.Entities.Structures.EnergyProducers
{
	public partial class WindTurbine
	{
	    private float _lastWindEffectiveness;
	    private bool _hasSetTurbineSprite;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    EffectiveEnergyProducedPerSecond = 25f *
		                                       WindManager.WindEffectiveness;
		    TurbineSprite.RelativeY = SpriteInstance.Height * 0.95f;
		}

		private void CustomActivity()
		{
		    EffectiveEnergyProducedPerSecond = 25f *
		                                       WindManager.WindEffectiveness;

		    if (IsBeingPlaced)
		    {
		        TurbineSprite.ColorOperation = SpriteInstance.ColorOperation;
		        TurbineSprite.Red = SpriteInstance.Red;
		        TurbineSprite.Green = SpriteInstance.Green;
		        TurbineSprite.Blue = SpriteInstance.Blue;
            }
            else if (!_hasSetTurbineSprite)
		    {
		        TurbineSprite.ColorOperation = SpriteInstance.ColorOperation;
		        TurbineSprite.Red = SpriteInstance.Red;
		        TurbineSprite.Green = SpriteInstance.Green;
		        TurbineSprite.Blue = SpriteInstance.Blue;

		        _hasSetTurbineSprite = true;
		    }
		    else if (_lastWindEffectiveness != WindManager.WindEffectiveness)
		    {
		        _lastWindEffectiveness = WindManager.WindEffectiveness;
		        TurbineSprite.RelativeRotationZVelocity = -WindManager.WindEffectiveness;
		    }
		}

	    public new void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

	        LayerProvidedByContainer.Remove(TurbineSprite);
	        SpriteManager.AddToLayer(TurbineSprite, hudLayer);

            if (HasLightSource)
	        {
	            //LayerProvidedByContainer.Remove(LightSpriteInstance);
	            //SpriteManager.AddToLayer(LightSpriteInstance, darknessLayer);
	        }
	    }

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
