using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Glue.StateInterpolation;
using FlatRedBall.Graphics;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using StateInterpolationPlugin;

namespace GBC2017.Entities.Projectiles
{
	public partial class FlyingEnemyProjectile
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    if (HitGroundSound == null || HitGroundSound.IsDisposed) HitGroundSound = Flying_Hit_Ground.CreateInstance();
		    if (HitTargetSound == null || HitTargetSound.IsDisposed) HitTargetSound = Flying_Hit_Target.CreateInstance();
        }

	    private void CustomActivity()
	    {


	    }

	    protected override void CustomHandleImpact()
        {
	        SpriteInstance.TextureScale = 2f * _currentScale;
	        var duration = SpriteInstance.AnimationChains["Impact"].TotalLength / 2;

	        LightOrShadowSprite.Tween(HandleLightGrowUpdate, 0, 4f, duration,
	            InterpolationType.Elastic, Easing.Out);

	        this.Call(() => FadeOut(0.25f)).After(duration);
	    }

	    private void HandleLightGrowUpdate(float newPosition)
	    {
	        LightOrShadowSprite.TextureScale = newPosition * _currentScale;
	    }

	    private void FadeOut(float duration)
	    {
	        LightOrShadowSprite.Tween("Alpha", 0, duration, InterpolationType.Exponential, Easing.Out);
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
