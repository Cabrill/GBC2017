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
	public partial class LaserTurretProjectile
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            if (HitGroundSound == null || HitGroundSound.IsDisposed) HitGroundSound = GlobalContent.Laser_Ground.CreateInstance();
            if (HitTargetSound == null || HitTargetSound.IsDisposed) HitTargetSound = GlobalContent.Laser_Hit.CreateInstance();
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

	    protected override void CustomHandleImpact()
	    {
	        var duration = SpriteInstance.AnimationChains["Impact"].TotalLength;
	        LightOrShadowSprite.Tween("Alpha", 0, duration, InterpolationType.Exponential, Easing.Out);
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
