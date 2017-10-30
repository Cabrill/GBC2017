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

namespace GBC2017.Entities.Structures.Utility
{
	public partial class CarbonTree
	{
	    private double _lastMineralGenerationTime;
	    public bool ShouldGenerateMinerals { get; set; }

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            _lastMineralGenerationTime = TimeManager.CurrentTime;


        }

		private void CustomActivity()
		{
		    if (IsTurnedOn && !SpriteInstance.Animate) SpriteInstance.Animate = true;
		    else if (!IsTurnedOn && SpriteInstance.Animate) SpriteInstance.Animate = false;

		    if (TimeManager.SecondsSince(_lastMineralGenerationTime) >= 1)
		    {
		        _lastMineralGenerationTime = TimeManager.CurrentTime;
		        ShouldGenerateMinerals = true;
            }
        }

	    public new void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

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
