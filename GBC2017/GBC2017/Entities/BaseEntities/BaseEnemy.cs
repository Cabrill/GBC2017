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

		    HealthRemaining = MaximumHealth;
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
	            storedVelocity = Velocity;
                Velocity = Vector3.Zero;
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

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }


	}
}
