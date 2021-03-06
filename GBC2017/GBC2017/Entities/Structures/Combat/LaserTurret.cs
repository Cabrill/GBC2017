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
using FlatRedBall.Math.Statistics;
using GBC2017.Entities.BaseEntities;
using GBC2017.Factories;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.Structures.Combat
{
	public partial class LaserTurret
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            attackSound = Laser_Shoot.CreateInstance();
            _shotAltitude = 100f;
        }

		private void CustomActivity()
		{
		    
        }

	    protected override BasePlayerProjectile CreateNewProjectile()
	    {
	        var newProjectile = LaserTurretProjectileFactory.CreateNew(LayerProvidedByContainer);
	        _shotAltitude = 75f;
	        if (SpriteInstance.CurrentChainName == "UpTurn")
	        {
	            _shotAltitude = 125f;
	        }
	        newProjectile.Altitude = _shotAltitude;

            return newProjectile;
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
