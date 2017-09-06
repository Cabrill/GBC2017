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
using FlatRedBall.Math.Statistics;
using GBC2017.Factories;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.Structures
{
	public partial class LaserTurret
	{
	    private Random random;
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
            random = new Random();

		}

		private void CustomActivity()
		{
		    if (!IsBeingPlaced) RotationZ += RandomExtensions.NextFloat(random, 0.1f);
		}
        
        public override void FireWeapon()
	    {
	        var newProjectile = LaserTurretProjectileFactory.CreateNew(LayerProvidedByContainer);
	        newProjectile.Position = Position;

	        var direction = new Vector3((float)-Math.Cos(RotationZ),
	            (float)-Math.Sin(RotationZ), 0);
	        direction.Normalize();

            newProjectile.Position.X += 22.5f * direction.X;
	        newProjectile.Position.Y += 12.5f * direction.Y;

	        newProjectile.Velocity = direction * newProjectile.Speed;
            
	        newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity);
	    }

    private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
