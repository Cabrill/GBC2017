using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Glue.StateInterpolation;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using StateInterpolationPlugin;

namespace GBC2017.Entities.Projectiles
{
	public partial class CannonProjectile
	{
	    private float circleRadius;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    if (HitGroundSound == null || HitGroundSound.IsDisposed) HitGroundSound = Cannon_Hit.CreateInstance();
		    HitTargetSound = HitGroundSound;
		    circleRadius = CircleInstance.Radius;

		    GravityDrag = -25f;
		}

	    protected  override void CustomHandleImpact()
	    {
	        RotationX = 0;
	        RotationY = 0;
	        RotationZ = 0;
            SpriteInstance.TextureScale = 2f * _currentScale;
	        LightOrShadowSprite.Alpha = 1f;
            
            var duration = SpriteInstance.AnimationChains["Impact"].TotalLength / 2;
	        
            this.Tween(HandleLightGrowUpdate, 0, 4f, duration,
	            InterpolationType.Elastic, Easing.Out);

	        this.Call(FadeOut).After(duration);
	    }

	    private void HandleLightGrowUpdate(float newPosition)
	    {
	        SpriteInstance.TextureScale = 2f * newPosition * _currentScale;
	        SpriteInstance.Alpha = newPosition/6;
            LightOrShadowSprite.TextureScale = newPosition * _currentScale;
	        CircleInstance.Radius = circleRadius * newPosition * _currentScale;
	    }

	    private void HandleShrinkUpdate(float newPosition)
	    {
	        SpriteInstance.TextureScale = 2f * newPosition * _currentScale;
	        SpriteInstance.Alpha = newPosition/4;
            LightOrShadowSprite.TextureScale = newPosition * _currentScale;
	        LightOrShadowSprite.Alpha = newPosition * _currentScale;
            CircleInstance.Radius = circleRadius * newPosition * _currentScale;
        }

	    private void FadeOut()
	    {
	        var duration = SpriteInstance.AnimationChains["Impact"].TotalLength / 2;

	        this.Tween(HandleShrinkUpdate, 4, 0f, duration,
	            InterpolationType.Exponential, Easing.Out);

	    }

        private void CustomActivity()
		{


		}

		private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
