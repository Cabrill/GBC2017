using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.Entities.BaseEntities;
using GBC2017.Factories;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.Structures.Combat
{
	public partial class Cannon
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            attackSound = Cannon_Shoot.CreateInstance();
            _shotAltitude = 100f;
        }

		private void CustomActivity()
		{


		}

	    /// <summary>
	    /// Determines where the enemy will be, so it can shoot at it
	    /// </summary>
	    protected override void RotateToAim()
	    {
	        var startPosition = GetProjectilePositioning();

	        //Gather information about the target
	        var aimLocation = targetEnemy.CircleInstance.Position;

	        var angle = (float)Math.Atan2(startPosition.Y - aimLocation.Y, startPosition.X - aimLocation.X);

	        _aimRotation = angle;
	    }

        protected override BasePlayerProjectile CreateNewProjectile()
	    {
	        var newProjectile = CannonProjectileFactory.CreateNew(LayerProvidedByContainer);
	        _shotAltitude = 100f;
	        if (SpriteInstance.CurrentChainName == "UpTurn")
	        {
	            _shotAltitude = 150f;
	        }
	        newProjectile.Altitude = _shotAltitude;
            return newProjectile;
	    }

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
