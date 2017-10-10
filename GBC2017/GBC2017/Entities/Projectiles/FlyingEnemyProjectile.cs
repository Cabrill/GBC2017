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
		    if (HitGroundSound == null || HitGroundSound.IsDisposed) HitGroundSound = GlobalContent.Flying_Hit_Ground.CreateInstance();
		    if (HitTargetSound == null || HitTargetSound.IsDisposed) HitTargetSound = GlobalContent.Flying_Hit_Target.CreateInstance();
        }

	    private void CustomActivity()
	    {


	    }

	    protected override void HandleImpact()
	    {
	        SpriteInstance.TextureScale = 2f;
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
