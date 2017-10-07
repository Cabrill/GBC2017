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
	    private SpriteList _waterDrops;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            SpriteInstance.RelativeY = SpriteInstance.Height / 3;
            EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();

            _waterDrops = new SpriteList();

            foreach (var emitter in EmitterListFile)
            {
                emitter.LayerToEmitOn = LayerProvidedByContainer;
            }
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

		        foreach (var emitter in EmitterListFile)
		        {
		            emitter.Position = Position;
		        }

                _hasSetSpriteColors = true;
		    }
		    else
		    {
		        LargeWheelSprite.RelativeRotationZVelocity = -0.5f;
		        SmallWheelSprite.RelativeRotationZVelocity = 0.5f;
		        EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();

		        UpdateEmitters();
		    }
        }

	    private void UpdateEmitters()
	    {
	        const double wheelSegment = (Math.PI * 2) / 6;

	        foreach (var emitter in EmitterListFile)
	        {
	            var randomNum = (int)FlatRedBallServices.Random.Between(0, 6);

	            emitter.Position.X = Position.X + (float)Math.Cos(LargeWheelSprite.RelativeRotationZ + randomNum*wheelSegment) * LargeWheelSprite.Height *
	                                 0.4f;
	            emitter.Position.Y = Position.Y + (float)Math.Sin(LargeWheelSprite.RelativeRotationZ + randomNum*wheelSegment) * LargeWheelSprite.Height *
	                                 0.4f;
	            emitter.TimedEmit(_waterDrops);
	        }


	        var waterDropDisappearsAtY = LargeWheelSprite.Position.Y - LargeWheelSprite.Height * FlatRedBallServices.Random.Between(0.4f, 0.6f);
            var waterDropMaxDisappearAtY = LargeWheelSprite.Position.Y - LargeWheelSprite.Height * 0.6f;

	        bool dropDestroyed = false;

            for (int i = _waterDrops.Count; i > 0; i--)
	        {
	            var waterDrop = _waterDrops[i - 1];
	            if (!dropDestroyed && waterDrop.Y < waterDropDisappearsAtY)
	            {
	                dropDestroyed = true;
                    _waterDrops.RemoveAt(i-1);
                    SpriteManager.RemoveSprite(waterDrop);
                }
                else if (waterDrop.Y < waterDropMaxDisappearAtY)
	            {
	                _waterDrops.RemoveAt(i - 1);
	                SpriteManager.RemoveSprite(waterDrop);
                }
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
