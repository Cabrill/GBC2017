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
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseEnemy
	{
        public static float LeftSideSpawnX { get; set; }
        public static float RightSideSpawnX { get; set; }

	    private Vector3 storedVelocity;
	    private Type preferredStructureToAttack;
        private BaseStructure targetStructure;
	    private double LastRangeAttackTime;
	    private double LastMeleeAttackTime;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{
#if DEBUG
		    if (DebugVariables.ShowDebugShapes)
		    {
		        CircleInstance.Visible = true;
		    }
		    else
#endif
		    {
		        CircleInstance.Visible = false;
		    }

		    preferredStructureToAttack = Type.GetType(PreferredStructureType);
            HealthRemaining = MaximumHealth;
		    MeleeRadius = CircleInstance.Radius * 1.1f;
		    LastRangeAttackTime = TimeManager.CurrentTime;
		    LastMeleeAttackTime = TimeManager.CurrentTime;

        }

		private void CustomActivity()
		{
		    if (HealthRemaining <= 0)
		    {
		        PerformDeath();
		    }
            else if (CurrentActionState == Action.Hurt && SpriteInstance.JustCycled)
		    {
		        CurrentActionState = Action.Running;
		        Velocity = storedVelocity;
		    }
            else if (CurrentActionState == Action.StartRangedAttack && SpriteInstance.JustCycled)
		    {
		        var newProjectile = CreateProjectile();
		        newProjectile.Position = Position;
		        newProjectile.Position.Y -= CircleInstance.Radius * 0.75f;
		        newProjectile.XVelocity = (CurrentDirectionState == Direction.MovingLeft ? -ProjectileSpeed : ProjectileSpeed);
		        newProjectile.DamageInflicted = RangedAttackDamage;
		        newProjectile.Speed = ProjectileSpeed;
		        newProjectile.MaxRange = RangedRadius * 1.5f;

                CurrentActionState = Action.FinishRangedAttack;
		    }
            else if (CurrentActionState == Action.FinishRangedAttack && SpriteInstance.JustCycled)
		    {
		        Velocity = storedVelocity;
                CurrentActionState = Action.Running;
            }
            else if (CurrentActionState != Action.StartRangedAttack && 
                CurrentActionState != Action.FinishRangedAttack && 
                TimeManager.SecondsSince(LastRangeAttackTime) > SecondsBetweenRangedAttack)
		    {
		        if (targetStructure == null)
		        {
		            ChooseStructureTarget();
		            PerformRangedAttackOnTarget();
		        }
		    }
        }

	    private void PerformRangedAttackOnTarget()
	    {
	        CurrentActionState = Action.StartRangedAttack;
	        storedVelocity = Velocity;
            Velocity = Vector3.Zero;

	        LastRangeAttackTime = TimeManager.CurrentTime;
	    }

	    private void ChooseStructureTarget()
	    {
	        
	    }

	    public void GetHitBy(BasePlayerProjectile projectile)
	    {
	        HealthRemaining -= projectile.DamageInflicted;

	        if (HealthRemaining <= 0)
	        {
	            PerformDeath();
	        }
	        else
	        {
	            CurrentActionState = Action.Hurt;
	            if (Velocity != Vector3.Zero)
	            {
	                storedVelocity = Velocity;
	                Velocity = Vector3.Zero;
	            }
	        }
	    }

	    private void PerformDeath()
	    {
            Velocity = Vector3.Zero;

	        if (CurrentActionState != Action.Dying)
	        {
	            CurrentActionState = Action.Dying;
	        }
	        else if (SpriteInstance.JustCycled)
	        {
	            Destroy();
	        }
	    }

	    public void PlaceOnLeftSide()
	    {
	        CurrentDirectionState = Direction.MovingRight;
	        CurrentActionState = Action.Running;
	        XVelocity = this.Speed;
	        X = LeftSideSpawnX;
	    }

	    public void PlaceOnRightSide()
	    {
	        CurrentDirectionState = Direction.MovingLeft;
	        CurrentActionState = Action.Running;
	        XVelocity = -this.Speed;
	        X = RightSideSpawnX;
	    }

	    /// <summary>
	    /// Allows the child combat structure to generate a projectile of its own type
	    /// </summary>
	    /// <returns>The projectile to be fired by the </returns>
	    protected virtual BaseEnemyProjectile CreateProjectile()
	    {
	        return new BaseEnemyProjectile();
	    }

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }


	}
}
