using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;

namespace GBC2017.Entities.Structures.EnergyProducers
{
	public partial class HydroGenerator
	{
	    private bool _hasSetSpriteColors;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {

            EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();
        }

		private void CustomActivity()
		{
		    if (IsBeingPlaced)
		    {
		        LargeWheelSprite.ColorOperation = SpriteInstance.ColorOperation;
		        LargeWheelSprite.Red = SpriteInstance.Red;
		        LargeWheelSprite.Green = SpriteInstance.Green;
		        LargeWheelSprite.Blue = SpriteInstance.Blue;

		        SmallWheelSprite.ColorOperation = SpriteInstance.ColorOperation;
		        SmallWheelSprite.Red = SpriteInstance.Red;
		        SmallWheelSprite.Green = SpriteInstance.Green;
		        SmallWheelSprite.Blue = SpriteInstance.Blue;

		        InnerCircleSprite.ColorOperation = SpriteInstance.ColorOperation;
		        InnerCircleSprite.Red = SpriteInstance.Red;
		        InnerCircleSprite.Green = SpriteInstance.Green;
		        InnerCircleSprite.Blue = SpriteInstance.Blue;
            }
		    else if (!_hasSetSpriteColors)
		    {
		        LargeWheelSprite.ColorOperation = SpriteInstance.ColorOperation;
		        LargeWheelSprite.Red = SpriteInstance.Red;
		        LargeWheelSprite.Green = SpriteInstance.Green;
		        LargeWheelSprite.Blue = SpriteInstance.Blue;

		        SmallWheelSprite.ColorOperation = SpriteInstance.ColorOperation;
		        SmallWheelSprite.Red = SpriteInstance.Red;
		        SmallWheelSprite.Green = SpriteInstance.Green;
		        SmallWheelSprite.Blue = SpriteInstance.Blue;

		        InnerCircleSprite.ColorOperation = SpriteInstance.ColorOperation;
		        InnerCircleSprite.Red = SpriteInstance.Red;
		        InnerCircleSprite.Green = SpriteInstance.Green;
		        InnerCircleSprite.Blue = SpriteInstance.Blue;

                _hasSetSpriteColors = true;
		    }
		    else
		    {
		        LargeWheelSprite.RelativeRotationZVelocity = -0.5f;
		        SmallWheelSprite.RelativeRotationZVelocity = 0.5f;
		        EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();
            }

		}

	    private double BaseEnergyPerSecond()
	    {
	        return 1;
	    }


        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
