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
using GBC2017.Entities.Projectiles;
using GBC2017.Factories;

namespace GBC2017.Entities.Enemies
{
	public partial class BasicAlien
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            rangedAttackSound = Alien_Shoot.CreateInstance();
            rangedChargeSound = Alien_Powerup.CreateInstance();
        }

		private void CustomActivity()
		{


		}

	    protected override BaseEnemyProjectile CreateProjectile()
	    {
	        var newProjectile = RangedEnemyProjectileFactory.CreateNew(LayerProvidedByContainer);
	        newProjectile.Altitude = 30f;
	        newProjectile.AltitudeVelocity = 1f;
	        newProjectile.GravityDrag = -25f;
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

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
