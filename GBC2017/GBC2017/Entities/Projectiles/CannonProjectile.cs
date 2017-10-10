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
		    if (HitGroundSound == null || HitGroundSound.IsDisposed) HitGroundSound = GlobalContent.Cannon_Hit.CreateInstance();
		    HitTargetSound = HitGroundSound;
		    circleRadius = CircleInstance.Radius;
		}

	    protected override void HandleImpact()
	    {
            
	        RotationX = 0;
	        RotationY = 0;
	        RotationZ = 0;
            SpriteInstance.TextureScale = 2f;
	        
	        LightOrShadowSprite.Tween(HandleTweenerUpdate, 0, 2f, SpriteInstance.AnimationChains["Impact"].TotalLength,
	            InterpolationType.Elastic, Easing.Out);
	    }

	    private void HandleTweenerUpdate(float newPosition)
	    {
	        LightOrShadowSprite.TextureScale = newPosition;
	        CircleInstance.Radius = circleRadius * newPosition*2f;
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
