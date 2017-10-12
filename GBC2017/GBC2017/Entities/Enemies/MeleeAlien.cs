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

namespace GBC2017.Entities.Enemies
{
	public partial class MeleeAlien
	{
	    private const float _lightPulseDuration = 1;
	    private const float _lightPulseAmount = 0.3f;
	    private float _currentLightPulse = 0;
	    private int _pulseMod = 1;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            meleeAttackSound = GlobalContent.alien_melee.CreateInstance();


        }

		private void CustomActivity()
		{
		    _currentLightPulse += TimeManager.SecondDifference * _pulseMod;
		    if (_currentLightPulse >= _lightPulseDuration || _currentLightPulse <= 0)
		    {
		        _pulseMod *= -1;
		    }
		    LightSprite.TextureScale *= _lightPulseAmount + (_currentLightPulse / _lightPulseDuration) * _lightPulseAmount;
		    LightSprite.RelativeY = SpriteInstance.RelativeY - 20 * SpriteInstance.TextureScale;

        }

	    public void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

            LayerProvidedByContainer.Remove(SpriteInstance);
            SpriteManager.AddToLayer(SpriteInstance, darknessLayer);

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
