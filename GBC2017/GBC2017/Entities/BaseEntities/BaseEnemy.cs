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
using FlatRedBall.Utilities;
using GBC2017.GumRuntimes;
using Gum.Converters;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using RenderingLibrary.Graphics;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseEnemy
	{
	    private static AxisAlignedRectangle _playArea;
	    private static PositionedObjectList<BaseStructure> _potentialTargetList;

        public bool IsDead => _healthRemaining <= 0;
        

	    private Vector3 storedVelocity;
        private BaseStructure targetStructure;
	    private double LastRangeAttackTime;
	    private double LastMeleeAttackTime;
	    private float _healthRemaining;
	    protected SoundEffectInstance rangedChargeSound;
	    protected SoundEffectInstance rangedAttackSound;

	    private ResourceBarRuntime _healthBar;

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
            
		    _healthRemaining = MaximumHealth;

		    MeleeRadius = CircleInstance.Radius * 1.1f;

		    LastRangeAttackTime = TimeManager.CurrentTime;
		    LastMeleeAttackTime = TimeManager.CurrentTime;

		    _healthBar = CreateResourceBar(ResourceBarRuntime.BarType.Health);
        }

	    public static void Initialize(AxisAlignedRectangle playArea, PositionedObjectList<BaseStructure> potentialTargetList)
	    {
	        _playArea = playArea;
	        _potentialTargetList = potentialTargetList;
	    }

		private void CustomActivity()
		{
		    if (IsDead)
		    {
		        PerformDeath();
		    }
            else if (CurrentActionState == Action.Hurt && SpriteInstance.JustCycled)
		    {
                ResumeMovement();
            }
            else if (CurrentActionState == Action.StartRangedAttack && SpriteInstance.JustCycled)
		    {
                FireProjectile();
		    }
            else if (CurrentActionState == Action.FinishRangedAttack && SpriteInstance.JustCycled)
		    {
		        if (targetStructure == null || targetStructure.IsDestroyed)
		        {
		            ResumeMovement();
		        }
		        else
		        {
		            CurrentActionState = Action.RangedAim;
		        }
		    }
            else if (CurrentActionState == Action.RangedAim && (targetStructure == null || targetStructure.IsDestroyed))
		    {
		        ResumeMovement();
		    }
            else if (CurrentActionState != Action.StartRangedAttack && 
                CurrentActionState != Action.FinishRangedAttack && 
                CurrentActionState != Action.Hurt &&
                TimeManager.SecondsSince(LastRangeAttackTime) > SecondsBetweenRangedAttack)
		    {
		        PerformRangedAttackOnTarget();
            }

		    if (_healthRemaining < MaximumHealth)
		    {
		        _healthBar.UpdateBar(_healthRemaining, MaximumHealth, false);
		        _healthBar.X = X;
		        _healthBar.Y = Y + SpriteInstance.Height/2f;
		        _healthBar.Visible = true;
		    }
		    else
		    {
		        _healthBar.Visible = false;
		    }
        }

	    private void StopMovement()
	    {
	        if (Velocity != Vector3.Zero)
	        {
	            storedVelocity = Velocity;
	        }
	        Velocity = Vector3.Zero;
        }

	    private void ResumeMovement()
	    {
	        Velocity = storedVelocity;
	        CurrentActionState = Action.Running;
	        CurrentDirectionState =
	            (Velocity.X > 0 ? Direction.MovingRight : Direction.MovingLeft);
	        SpriteInstance.IgnoreAnimationChainTextureFlip = true;
	    }

	    private void FireProjectile()
	    {
	        var newProjectile = CreateProjectile();
	        newProjectile.Position = Position;
	        newProjectile.Position.Y -= CircleInstance.Radius * 0.75f;
	        newProjectile.DamageInflicted = RangedAttackDamage;
	        newProjectile.Speed = ProjectileSpeed;
	        newProjectile.MaxRange = RangedRadius * 1.5f;

	        var angle = (float)Math.Atan2(newProjectile.Position.Y - targetStructure.Position.Y, newProjectile.Position.X - targetStructure.Position.X);
	        var direction = new Vector3(
	            (float)-Math.Cos(angle),
	            (float)-Math.Sin(angle), 0);
	        direction.Normalize();

            newProjectile.Velocity = direction * newProjectile.Speed;
	        newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity)-MathHelper.ToRadians(90);

            rangedAttackSound.Play();

            LastRangeAttackTime = TimeManager.CurrentTime;

            CurrentActionState = Action.FinishRangedAttack;
        }

        private void PerformRangedAttackOnTarget()
	    {
	        if (targetStructure != null && (targetStructure.IsDestroyed || Vector3.Distance(Position, targetStructure.Position) > RangedRadius))
	        {
	            targetStructure = null;
	        }

	        if (targetStructure == null)
	        {
	            ChooseStructureTarget();
	        }

	        if (targetStructure != null)
	        {
	            StopMovement();

	            CurrentDirectionState =(Position.X < targetStructure.Position.X ? 
                    Direction.MovingRight : 
                    Direction.MovingLeft);

	            CurrentActionState = Action.StartRangedAttack;
	            this.Call(rangedChargeSound.Play).After(0.3f);
	        }
	    }

	    private void ChooseStructureTarget()
	    {
	        if (_potentialTargetList != null && _potentialTargetList.Count > 0)
	        {
	            targetStructure =
	                _potentialTargetList.Where(pt => 
                        pt.CurrentState == BaseStructure.VariableState.Built && 
                        pt.IsDestroyed == false && 
                        Vector3.Distance(Position, pt.Position) <= RangedRadius)
                    .OrderBy(pt => Vector3.Distance(Position, pt.Position)
                    ).FirstOrDefault();
	        }
	    }

	    public void GetHitBy(BasePlayerProjectile projectile)
	    {
	        _healthRemaining -= projectile.DamageInflicted;
            projectile.PlayHitTargetSound();

	        if (_healthRemaining <= 0)
	        {
	            PerformDeath();
	        }
	        else
	        {
	            if (CurrentActionState == Action.StartRangedAttack)
	            {
                    Instructions.Clear();
	                if (rangedChargeSound.State == SoundState.Playing) rangedChargeSound.Stop();
	            }

	            StopMovement();
                CurrentActionState = Action.Hurt;
	        }
	    }

	    private void PerformDeath()
	    {
	        StopMovement();

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
	        X = _playArea.Left;
	        Y = FlatRedBallServices.Random.Between(_playArea.Bottom + CircleInstance.Radius, _playArea.Top - CircleInstance.Radius);
        }

	    public void PlaceOnRightSide()
	    {
	        CurrentDirectionState = Direction.MovingLeft;
	        CurrentActionState = Action.Running;
	        XVelocity = -this.Speed;
	        X = _playArea.Right;
	        Y = FlatRedBallServices.Random.Between(_playArea.Bottom + CircleInstance.Radius, _playArea.Top - CircleInstance.Radius);
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
            rangedAttackSound.Dispose();
            rangedChargeSound.Dispose();
		    _healthBar.Destroy();

        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	    private ResourceBarRuntime CreateResourceBar(ResourceBarRuntime.BarType barType)
	    {
	        var newBar = new ResourceBarRuntime
	        {
	            XUnits = GeneralUnitType.PixelsFromMiddle,
	            YUnits = GeneralUnitType.PixelsFromMiddleInverted,
	            XOrigin = HorizontalAlignment.Center,
	            YOrigin = VerticalAlignment.Top,
	            WidthUnits = DimensionUnitType.Absolute,
	            HeightUnits = DimensionUnitType.Absolute,
	            Width = SpriteInstance.Width,
	            Height = SpriteInstance.Width / 5,
	            CurrentBarTypeState = barType,
	            Visible = false
	        };
	        newBar.AddToManagers();
	        return newBar;
	    }


    }
}
