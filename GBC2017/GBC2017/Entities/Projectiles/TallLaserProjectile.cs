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
	public partial class TallLaserProjectile
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    if (HitGroundSound == null || HitGroundSound.IsDisposed) HitGroundSound = GlobalContent.TallLaser_Hit.CreateInstance();
		    if (HitTargetSound == null || HitTargetSound.IsDisposed) HitTargetSound = GlobalContent.TallLaser_Hit.CreateInstance();
        }

        private void CustomActivity()
		{


		}

	    protected override void CustomHandleImpact()
        {
	        RotationZ = FlatRedBallServices.Random.Between(-4, 4);
	        LightOrShadowSprite.TextureScale = 2f;
	        LightOrShadowSprite.Tween(HandleTweenerUpdate, 2f, 0f, SpriteInstance.AnimationChains["Impact"].TotalLength, InterpolationType.Exponential, Easing.Out).Start();
	    }

	    private void HandleTweenerUpdate(float newPosition)
	    {
	        LightOrShadowSprite.TextureScale = newPosition;
	    }


        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
