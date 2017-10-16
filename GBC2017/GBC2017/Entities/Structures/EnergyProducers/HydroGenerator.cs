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
using GBC2017.StaticManagers;

namespace GBC2017.Entities.Structures.EnergyProducers
{
	public partial class HydroGenerator
	{
	    private bool _hasSetSpriteColors;
	    private SpriteList _waterDrops;

	    private float? _startingLargeWheelScale;
	    private float _startingSmallWheelScale;
	    private float _startingInnerWheelScale;

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
                emitter.SecondFrequency = 0.05f;
            }


            _startingLargeWheelScale = LargeWheelSprite.TextureScale;
            _startingSmallWheelScale = SmallWheelSprite.TextureScale;
            _startingInnerWheelScale = InnerCircleSprite.TextureScale;
        }

	    protected override void UpdateScale()
	    {
	        base.UpdateScale();
	        if (_startingLargeWheelScale.HasValue)
	        {
	            LargeWheelSprite.TextureScale = _startingLargeWheelScale.Value * _currentScale;
	            SmallWheelSprite.TextureScale = _startingSmallWheelScale * _currentScale;
	            InnerCircleSprite.TextureScale = _startingInnerWheelScale * _currentScale;
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
		        if (IsTurnedOn)
		        {
		            EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();

		            LargeWheelSprite.RelativeRotationZVelocity = (float) -1;
		            SmallWheelSprite.RelativeRotationZVelocity = (float) 1;


		            UpdateEmitters();
		        }
		        else
		        {
		            EffectiveEnergyProducedPerSecond = 0;
                    LargeWheelSprite.RelativeRotationZVelocity = 0;
		            SmallWheelSprite.RelativeRotationZVelocity = 0;
                }
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

	        var minimumY = LargeWheelSprite.Y - LargeWheelSprite.Height/3;

	        for (var i = _waterDrops.Count; i > 0; i--)
	        {
	            var waterdrop = _waterDrops[i - 1];
	            if (waterdrop.Y < minimumY)
	            {
	                _waterDrops.RemoveAt(i-1);
                    SpriteManager.RemoveSprite(waterdrop);
	            }
	        }
        }

	    private double BaseEnergyPerSecond()
	    {
            var energyGeneratedPerSecInRealLife = efficiency * waterDensity * bladeArea * Math.Pow(WaterManager.waterVelocity, 3) / 2;
            var energyGeneratedPerSecInGame = energyGeneratedPerSecInRealLife * 24 * 3600 / 300;
	        return energyGeneratedPerSecInGame;
	    }


        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
