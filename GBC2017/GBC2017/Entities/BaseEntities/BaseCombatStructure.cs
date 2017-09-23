using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using GBC2017.Factories;
using GBC2017.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseCombatStructure
	{
	    private static PositionedObjectList<BaseEnemy> _potentialTargetList;
	    private BaseEnemy targetEnemy;
	    protected SoundEffect attackSound;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
		    RangeCircleInstance.Visible = true;
		    LastFiredTime = TimeManager.CurrentTime;

            AfterIsBeingPlacedSet += (not, used) => { RangeCircleInstance.Visible = false; };
		}

	    public static void Initialize(PositionedObjectList<BaseEnemy> potentialTargets)
	    {
	        _potentialTargetList = potentialTargets;
	    }

		private void CustomActivity()
		{
            if (IsBeingPlaced == false)
		    {
		        if (targetEnemy != null &&  (targetEnemy.IsDead || !RangeCircleInstance.CollideAgainst(targetEnemy.CircleInstance)))
		        {
		            targetEnemy = null;
		        }

		        if (targetEnemy == null && _potentialTargetList.Count > 0)
		        {
		            ChooseTarget();
		        }

		        if (targetEnemy != null)
		        {
		            RotateToAim();

		            if (BatteryLevel >= EnergyCostToFire)
		            {
		                PerformFiringActivity();
		            }
		        }
		    }
		}

        /// <summary>
        /// Determines where the enemy will be, so it can shoot at it
        /// </summary>
	    private void RotateToAim()
	    {
            //Gather information about the target
	        var targetPosition = targetEnemy.CircleInstance.Position;

	        var targetVector = targetEnemy.Velocity;
	        var targetDistance = Vector3.Distance(Position, targetPosition);

            //Calculate how long the bullet would take to reach them
            var timeToTravel = targetDistance / ProjectileSpeed;

	        var aimLocation = targetPosition;

            //If they're moving, we'll aim ahead
            if (targetVector != Vector3.Zero)
	        {
	            //Calculate how far they would travel in the time 
	            var aimAheadDistance = targetVector * timeToTravel;

	            //Aim where they'll be in the time the bullet takes to travel
	            aimLocation = aimAheadDistance + targetPosition;

	            //Recalculate time with the new aiming location
	            targetDistance = Vector3.Distance(Position, aimLocation);
	            timeToTravel = targetDistance / ProjectileSpeed;
	            aimAheadDistance = targetVector * timeToTravel;
	            aimLocation = targetPosition + aimAheadDistance;
	        }

	        var angle = (float)Math.Atan2(Position.Y - aimLocation.Y, Position.X - aimLocation.X);
	        //var projectileOffset = GetProjectilePositioning(angle);
            //angle = (float)Math.Atan2(Position.Y-projectileOffset.Y - aimLocation.Y, Position.X-projectileOffset.X - aimLocation.X);


            RotationZ = angle;
	    }

	    private Vector3 GetProjectilePositioning(float? angle = null)
	    {
	        if (!angle.HasValue) angle = RotationZ;

	        var direction = new Vector3(
	            (float)-Math.Cos(angle.Value),
	            (float)-Math.Sin(angle.Value), 0);
	        direction.Normalize();

            return new Vector3(22.5f, 12.5f,0) * direction;
	    }

	    private void ChooseTarget()
	    {
	        var localPotentialTarget =
	            _potentialTargetList.FirstOrDefault(pt => pt.CircleInstance.CollideAgainst(RangeCircleInstance));

	        if (localPotentialTarget != null)
	        {
	            targetEnemy = localPotentialTarget;
	        }
	    }

	    private void PerformFiringActivity()
	    {
	        if (TimeManager.SecondsSince(LastFiredTime) > SecondsBetweenFiring)
            {
	            var newProjectile = CreateNewProjectile();
                newProjectile.DamageInflicted = AttackDamage;
                newProjectile.Speed = ProjectileSpeed;
                newProjectile.MaxRange = RangedRadius * 1.5f;
                newProjectile.Position = Position;

                var direction = new Vector3(
                    (float)-Math.Cos(RotationZ),
                    (float)-Math.Sin(RotationZ), 0);
                direction.Normalize();
                newProjectile.Position += GetProjectilePositioning();

                newProjectile.Velocity = direction * newProjectile.Speed;

                newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity);

                attackSound.Play();

                LastFiredTime = TimeManager.CurrentTime;
                BatteryLevel -= EnergyCostToFire;
                _energyReceivedThisSecond += EnergyCostToFire;
            }
        }

	    public new void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

	        LayerProvidedByContainer.Remove(RangeCircleInstance);
	        ShapeManager.AddToLayer(RangeCircleInstance, hudLayer);
	    }

        /// <summary>
        /// Allows the child combat structure to generate a projectile of its own type
        /// </summary>
        /// <returns>The projectile to be fired by the </returns>
	    protected virtual BasePlayerProjectile CreateNewProjectile()
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
