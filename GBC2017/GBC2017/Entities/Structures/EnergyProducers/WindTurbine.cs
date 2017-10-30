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
using FlatRedBall.Math;
using GBC2017.GameClasses;
using GBC2017.StaticManagers;

namespace GBC2017.Entities.Structures.EnergyProducers
{
    public partial class WindTurbine
    {
        private float _lastWindEffectiveness;
        private bool _hasSetTurbineSprite;
        private float? _startingTurbineScale;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();
            TurbineSprite.RelativeY = SpriteInstance.Height * 0.95f;
            _startingTurbineScale = TurbineSprite.TextureScale;
        }

        protected override void UpdateScale()
        {
            base.UpdateScale();
            if (_startingTurbineScale.HasValue)
            {
                TurbineSprite.TextureScale = _startingTurbineScale.Value * _currentScale;
                TurbineSprite.RelativeY = SpriteInstance.Height * 0.95f;
            }
        }

        private void CustomActivity()
        {
            if (IsTurnedOn)
            {
                EffectiveEnergyProducedPerSecond = BaseEnergyPerSecond();

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
                else if (_lastWindEffectiveness != WindManager.windSpeed)
                {
                    _lastWindEffectiveness = WindManager.windSpeed;
                }
                TurbineSprite.RelativeRotationZVelocity = -WindManager.windSpeed;
            }
            else
            {
                EffectiveEnergyProducedPerSecond = 0;
                TurbineSprite.RelativeRotationZVelocity = 0;
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

        private double BaseEnergyPerSecond()
        {
            var powerCoefficient = 0.05f;
            var area = Math.Pow(BladeRadiusInMeters,2) * Math.PI;
            double kiloWattReal = powerCoefficient * 0.5f * AirDensity * Math.Pow(WindManager.windSpeed, 3) * area;
            double wattInGame = kiloWattReal * 1000 / GameFormulas.RealSecondsPerGameHour;
            return wattInGame;
        }

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
