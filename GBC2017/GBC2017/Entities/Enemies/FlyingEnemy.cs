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
using GBC2017.Entities.BaseEntities;
using GBC2017.Factories;

namespace GBC2017.Entities.Enemies
{
	public partial class FlyingEnemy
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    rangedAttackSound = Flying_Shoot.CreateInstance();
		    rangedChargeSound = Flying_Powerup.CreateInstance();
		    Altitude = 300f;
		    _spriteRelativeY -= 30;

		}

	    protected override BaseEnemyProjectile CreateProjectile()
	    {
	        var newProjectile = FlyingEnemyProjectileFactory.CreateNew(LayerProvidedByContainer);
	        newProjectile.Altitude = Altitude-15f;
	        newProjectile.AltitudeVelocity = 10f;
	        return newProjectile;
	    }

	    public void AddSpritesToLayers(FlatRedBall.Graphics.Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

	        if (HasLightSource)
	        {
	            //LayerProvidedByContainer.Remove(LightSpriteInstance);
	            //SpriteManager.AddToLayer(LightSpriteInstance, darknessLayer);
	        }
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
