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
	    protected BaseEnemy targetEnemy;
	    protected SoundEffectInstance attackSound;
	    protected float _aimRotation;
	    protected float _shotAltitude = 1f;
	    private float? _startingRangeRadius;

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
	        _startingRangeRadius = RangedRadius;
	    }

	    public static void Initialize(PositionedObjectList<BaseEnemy> potentialTargets)
	    {
	        _potentialTargetList = potentialTargets;
	    }

	    protected override void UpdateScale()
	    {
            base.UpdateScale();
	        if (_startingRangeRadius.HasValue)
	        {
	            RangeCircleInstance.Radius = _startingRangeRadius.Value * _currentScale;
	        }
	    }

        private void CustomActivity()
		{
            if (IsBeingPlaced == false && IsTurnedOn)
            {
#if DEBUG
                if (DebugVariables.TurretsAimAtMouse) RotateToAimMouse();
#endif

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
		            SetAnimationFromAimRotation();

                    if (BatteryLevel >= EnergyCostToFire)
		            {
		                PerformFiringActivity();
		            }
		        }
		    }
		}

#if DEBUG
        /// <summary>
        /// Determines where the enemy will be, so it can shoot at it
        /// </summary>
        private void RotateToAimMouse()
	    {
	        //Gather information about the target
	        var targetPositionX = FlatRedBall.Gui.GuiManager.Cursor.WorldXAt(1);
	        var targetPositionY = FlatRedBall.Gui.GuiManager.Cursor.WorldYAt(1);

	        var targetPosition = new Vector3(targetPositionX, targetPositionY, 1);

	        var aimLocation = targetPosition;

	        var angle = (float)Math.Atan2(Position.Y - aimLocation.Y, Position.X - aimLocation.X);

	        _aimRotation = angle;

	        SetAnimationFromAimRotation();
	        PerformFiringActivity();
	    }
#endif

        /// <summary>
        /// Determines where the enemy will be, so it can shoot at it
        /// </summary>
        protected virtual void RotateToAim()
        {
            var startPosition = GetProjectilePositioning();

            //Gather information about the target
            var targetPosition = targetEnemy.CircleInstance.Position;

	        var targetVector = targetEnemy.Velocity;
	        var targetDistance = Vector3.Distance(startPosition, targetPosition);

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
	            targetDistance = Vector3.Distance(startPosition, aimLocation);
	            timeToTravel = targetDistance / ProjectileSpeed;
	            aimAheadDistance = targetVector * timeToTravel;
	            aimLocation = targetPosition + aimAheadDistance;
	        }

	        var angle = (float)Math.Atan2(startPosition.Y - aimLocation.Y, startPosition.X - aimLocation.X);

	        _aimRotation = angle;
	    }

        protected virtual void SetAnimationFromAimRotation()
	    {
	        var isolatedAim = _aimRotation % (2 * Math.PI);//A full circle is is 2*Pi

	        if (isolatedAim < 0) isolatedAim = (2 * Math.PI) - Math.Abs(isolatedAim);

	        var quadSize = (2 * Math.PI) / 4;//Four quads in a circle
	        var aimQuad = (int)(isolatedAim / quadSize);
	        var quadRemainder = isolatedAim % quadSize;

            //var quadProgress = (int)Math.Floor(quadRemainder / (quadrantSegments/5));

            var quadPercent = quadRemainder / quadSize;
	        int quadProgress = 0;

	        if (aimQuad == 0 || aimQuad == 2)//bottom-left and top-right
	        {
	            if (quadPercent > 0.7f) quadProgress = 4;
                else if (quadPercent > .4f) quadProgress = 3;
	            else if (quadPercent > .25f) quadProgress = 2;
                else if (quadPercent > .12f) quadProgress = 1;
            }
	        else//bottom-right and top-left
	        {
	            if (quadPercent > 0.97f) quadProgress = 4;
	            else if (quadPercent > .9f) quadProgress = 3;
	            else if (quadPercent > .7f) quadProgress = 2;
	            else if (quadPercent > .35f) quadProgress = 1;
            }

	        if (targetEnemy == null || !targetEnemy.IsFlying)
	        {
	            SpriteInstance.CurrentChainName = "Turn";
	        }
	        else
	        {
	            SpriteInstance.CurrentChainName = "UpTurn";
            }

	        SpriteInstance.CurrentFrameIndex = (aimQuad * 5) + quadProgress;


	        UpdateAnimation();
        }

	    protected virtual Vector3 GetProjectilePositioning(float? angle = null)
	    {
	        if (!angle.HasValue) angle = _aimRotation;

	        var direction = new Vector3(
	            (float)-Math.Cos(angle.Value),
	            (float)-Math.Sin(angle.Value), 0);
	        direction.Normalize();
	        return new Vector3(Position.X + AxisAlignedRectangleInstance.Width / 2 * direction.X, Position.Y + AxisAlignedRectangleInstance.Height * direction.Y, 0);
        }

	    private void ChooseTarget()
	    {
	        BaseEnemy newTarget = null;

	        foreach (BaseEnemy pt in _potentialTargetList)
	        {
	            if (pt.IsDead) continue;
	            if (!pt.CircleInstance.CollideAgainst(RangeCircleInstance)) continue;
	            newTarget = pt;
	            break;
	        }

	        if (newTarget != null)
	        {
	            targetEnemy = newTarget;
	        }
	    }

	    private void PerformFiringActivity()
	    {
	        if (TimeManager.SecondsSince(LastFiredTime) > SecondsBetweenFiring)
            {
	            var newProjectile = CreateNewProjectile();
                newProjectile.DamageInflicted = AttackDamage;
                newProjectile.Speed = ProjectileSpeed;
                newProjectile.Position = Position;

                var direction = new Vector3(
                    (float)-Math.Cos(_aimRotation),
                    (float)-Math.Sin(_aimRotation), 0);
                direction.Normalize();
                newProjectile.Position = GetProjectilePositioning();

                newProjectile.AltitudeVelocity = CalculateAltitudeVelocity(newProjectile);

                newProjectile.Velocity = direction * newProjectile.Speed;

                newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity);

                PlayFireSound();

                LastFiredTime = TimeManager.CurrentTime;
                BatteryLevel -= EnergyCostToFire;
                _energyReceivedCurrentUpdate += EnergyCostToFire;
            }
        }

	    private float CalculateAltitudeVelocity(BasePlayerProjectile projectile)
	    {
	        if (targetEnemy == null) return 0f;

	        var targetPosition = targetEnemy.Position;
	        var targetDistance = Vector3.Distance(projectile.Position, targetPosition);

	        var timeToTravel = targetDistance / ProjectileSpeed;

	        var altitudeDifference = targetEnemy.Altitude + (targetEnemy.SpriteInstance.Height/2) - projectile.Altitude;
	        var altitudeVelocity = (altitudeDifference / timeToTravel) - ((projectile.GravityDrag * timeToTravel) / 2);

            return altitudeVelocity;
	    }

	    private void PlayFireSound()
	    {
	        try
	        {
	            attackSound.Play();
	        }catch (Exception){}
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
	        if (attackSound != null && !attackSound.IsDisposed)
	        {
	            attackSound.Stop(true);
	            attackSound.Dispose();
	        }
	    }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}

