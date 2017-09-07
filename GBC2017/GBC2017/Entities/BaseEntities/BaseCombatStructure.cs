using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using GBC2017.Factories;
using GBC2017.Screens;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseCombatStructure
	{
	    public static PositionedObjectList<BaseEnemy> PotentialTargetList { private get; set; }
        private static Random random;
	    private BaseEnemy targetEnemy;
        

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    random = new Random((int)TimeManager.SystemCurrentTime);
		    RangeCircleInstance.Visible = true;
		    AfterIsBeingPlacedSet += (not, used) => { RangeCircleInstance.Visible = false; };
		}

		private void CustomActivity()
		{
		    if (IsBeingPlaced == false)
		    {
		        if (targetEnemy != null && !RangeCircleInstance.CollideAgainst(targetEnemy.CircleInstance))
		        {
		            targetEnemy = null;
		        }

		        if (targetEnemy == null && PotentialTargetList.Count > 0)
		        {
		            ChooseTarget();
		        }

		        if (targetEnemy != null)
		        {
		            RotateToAim();
		            PerformFiringActivity();
		        }
		    }
		}

        /// <summary>
        /// Determines where the enemy will be, so it can shoot at it
        /// </summary>
	    private void RotateToAim()
	    {
            //Gather information about the target
	        var targetPosition = targetEnemy.Position;
	        var targetVector = targetEnemy.Velocity;
	        var targetDistance = Position - targetPosition;

            //Calculate how long the bullet would take to reach them
	        var timeToTravel = targetDistance.Length() / ProjectileSpeed;

            //Calculate how far they would travel in that time
	        var aimAheadDistance = targetVector * timeToTravel;

            //Aim where they'll be in the time the bullet takes to travel
	        var aimLocation = aimAheadDistance + targetPosition;

            //Recalculate time with the new aiming location
	        targetDistance = Position - aimLocation;
            timeToTravel = targetDistance.Length() / ProjectileSpeed;
	        aimAheadDistance = targetVector * timeToTravel;
	        aimLocation = targetPosition + aimAheadDistance;

	        var angle = (float)Math.Atan2(Position.Y - aimLocation.Y, Position.X - aimLocation.X);
	        
	        RotationZ = angle;
	    }

	    private void ChooseTarget()
	    {
	        var localPotentialTarget =
	            PotentialTargetList.FirstOrDefault(pt => pt.CircleInstance.CollideAgainst(RangeCircleInstance));

	        if (localPotentialTarget != null)
	        {
	            targetEnemy = localPotentialTarget;
	        }
	    }

	    private void PerformFiringActivity()
	    {
	        if (TimeManager.SecondsSince(LastFiredTime) > SecondsBetweenFiring)
            {
	            var newProjectile = CreateProjectile();

                newProjectile.Position = Position;

                var direction = new Vector3(
                    (float)-Math.Cos(RotationZ),
                    (float)-Math.Sin(RotationZ), 0);
                direction.Normalize();

                newProjectile.Position.X += 22.5f * direction.X;
                newProjectile.Position.Y += 12.5f * direction.Y;

                newProjectile.Velocity = direction * newProjectile.Speed;

                newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity);

                LastFiredTime = TimeManager.CurrentTime;
            }
        }

        /// <summary>
        /// Allows the child combat structure to generate a projectile of its own type
        /// </summary>
        /// <returns>The projectile to be fired by the </returns>
	    protected virtual BasePlayerProjectile CreateProjectile()
	    {
	        return new BasePlayerProjectile();
	    }

	    private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
