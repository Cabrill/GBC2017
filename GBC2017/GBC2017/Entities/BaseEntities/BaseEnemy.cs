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
using GBC2017.ResourceManagers;
using GBC2017.StaticManagers;
using Gum.Converters;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using RenderingLibrary.Graphics;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseEnemy
	{
	    public event Action<BaseEnemy> OnDeath;
        private static AxisAlignedRectangle _playArea;
	    private static PositionedObjectList<BaseStructure> _potentialTargetList;

	    public float HealthRemaining { get; private set; }
        public bool IsDead => HealthRemaining <= 0;
        

        private BaseStructure _targetStructureToAttack;
	    private BaseStructure _targetStructureForNavigation;
	    private float _distanceToTargetStructureForNavigation;

	    private double _lastRangeAttackTime;
	    private double _lastMeleeAttackTime;
	    
	    protected SoundEffectInstance rangedChargeSound;
	    protected SoundEffectInstance rangedAttackSound;
	    protected SoundEffect meleeAttackSound;

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
            
		    HealthRemaining = MaximumHealth;

		    MeleeRadius = CircleInstance.Radius * 1.1f;

		    _lastRangeAttackTime = TimeManager.CurrentTime;
		    _lastMeleeAttackTime = TimeManager.CurrentTime;

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
            else if (IsRangedAttacker)
		    {
		        RangedAttackActivity();
		    }
            else if (IsMeleeAttacker)
		    {
		        MeleeAttackActivity();
		    }
            else if (CurrentActionState == Action.Running)
		    {
		        var currentDistanceToTargetNavigation =
		            Vector3.Distance(Position, _targetStructureForNavigation.Position);

                //This means we overshot it.  Correct course!
		        if (currentDistanceToTargetNavigation > _distanceToTargetStructureForNavigation)
		        {
		            ResumeMovement();
		        }
		        _distanceToTargetStructureForNavigation = currentDistanceToTargetNavigation;

		    }

            if (HealthRemaining < MaximumHealth)
		    {
		        _healthBar.UpdateBar(HealthRemaining, MaximumHealth, false);
		        _healthBar.X = X * CameraZoomManager.GumCoordOffset;
                _healthBar.Y = (Y + SpriteInstance.Height/2f) * CameraZoomManager.GumCoordOffset;
                _healthBar.Visible = true;
		    }
		    else
		    {
		        _healthBar.Visible = false;
		    }
        }

	    private void MeleeAttackActivity()
	    {
	        if (CurrentActionState == Action.Hurt && SpriteInstance.JustCycled && (_targetStructureToAttack == null || _targetStructureToAttack.IsDestroyed))
	        {
	            ResumeMovement();
	        }
	        else if (CurrentActionState == Action.MeleeAttack && SpriteInstance.JustCycled)
	        {
	            DealMeleeDamage();
	        }
	        else if (CurrentActionState != Action.MeleeAttack &&
	                 CurrentActionState != Action.Hurt &&
	                 TimeManager.SecondsSince(_lastMeleeAttackTime) > SecondsBetweenMeleeAttack)
	        {
	            PerformAttackOnStructure();
	        }
        }

	    private void DealMeleeDamage()
	    {
	        _lastMeleeAttackTime = TimeManager.CurrentTime;
            meleeAttackSound?.Play();
	        _targetStructureToAttack.TakeMeleeDamage(this);
            CurrentActionState = Action.Standing;
	    }

	    private void RangedAttackActivity()
	    {
	        if (CurrentActionState == Action.Hurt && SpriteInstance.JustCycled)
	        {
	            if (_targetStructureToAttack == null || _targetStructureToAttack.IsDestroyed)
	            {
	                CurrentActionState = Action.RangedAim;
                }
	            else
	            {
	                ResumeMovement();
                }
	        }
            else if (CurrentActionState == Action.StartRangedAttack && SpriteInstance.JustCycled)
	        {
	            FireProjectile();
	        }
	        else if (CurrentActionState == Action.FinishRangedAttack && SpriteInstance.JustCycled)
	        {
	            if (_targetStructureToAttack == null || _targetStructureToAttack.IsDestroyed)
	            {
	                ResumeMovement();
	            }
	            else
	            {
	                CurrentActionState = Action.RangedAim;
	            }
	        }
	        else if (CurrentActionState == Action.RangedAim && (_targetStructureToAttack == null || _targetStructureToAttack.IsDestroyed))
	        {
	            ResumeMovement();
	        }
	        else if (CurrentActionState != Action.StartRangedAttack &&
	                 CurrentActionState != Action.FinishRangedAttack &&
	                 CurrentActionState != Action.Hurt &&
	                 TimeManager.SecondsSince(_lastRangeAttackTime) > SecondsBetweenRangedAttack)
	        {
	            PerformAttackOnStructure();
	        }
        }

	    private void StopMovement()
	    {
	        Velocity = Vector3.Zero;
        }

	    private void ResumeMovement()
	    {
	        if (_targetStructureForNavigation == null || _targetStructureForNavigation.IsDestroyed)
	        {
	            ChooseStructureForNavigation();
	        }
	        if (_targetStructureForNavigation != null)
	        {
	            _distanceToTargetStructureForNavigation = Vector3.Distance(Position, _targetStructureForNavigation.Position);

                var angle = (float) Math.Atan2(Y - _targetStructureForNavigation.Position.Y,
	                X - _targetStructureForNavigation.Position.X);
	            var direction = new Vector3(
	                (float)-Math.Cos(angle),
	                (float)-Math.Sin(angle), 0);
	            direction.Normalize();

	            Velocity = direction * Speed;

                CurrentActionState = Action.Running;
	            CurrentDirectionState =
	                (Velocity.X > 0 ? Direction.MovingRight : Direction.MovingLeft);
	            SpriteInstance.IgnoreAnimationChainTextureFlip = true;
            }
	        else
	        {
	            CurrentActionState = Action.Standing;
	        }
	    }

	    private void FireProjectile()
	    {
	        var newProjectile = CreateProjectile();
	        newProjectile.Position = Position;
	        newProjectile.Position.Y -= CircleInstance.Radius * 0.75f;
	        newProjectile.DamageInflicted = RangedAttackDamage;
	        newProjectile.Speed = ProjectileSpeed;
	        newProjectile.MaxRange = RangedRadius * 1.5f;

	        var angle = (float)Math.Atan2(newProjectile.Position.Y - _targetStructureToAttack.Position.Y, newProjectile.Position.X - _targetStructureToAttack.Position.X);
	        var direction = new Vector3(
	            (float)-Math.Cos(angle),
	            (float)-Math.Sin(angle), 0);
	        direction.Normalize();

            newProjectile.Velocity = direction * newProjectile.Speed;
	        newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity)-MathHelper.ToRadians(90);

            rangedAttackSound.Play();

            _lastRangeAttackTime = TimeManager.CurrentTime;

            CurrentActionState = Action.FinishRangedAttack;
        }

        private void PerformAttackOnStructure()
	    {
	        if (_targetStructureToAttack != null && (_targetStructureToAttack.IsDestroyed || Vector3.Distance(Position, _targetStructureToAttack.Position) > RangedRadius))
	        {
	            _targetStructureToAttack = null;
	        }

	        if (_targetStructureToAttack == null)
	        {
	            ChooseStructureForAttack();
	        }

	        if (_targetStructureToAttack != null)
	        {
	            StopMovement();

	            CurrentDirectionState =(Position.X < _targetStructureToAttack.Position.X ? 
                    Direction.MovingRight : 
                    Direction.MovingLeft);

	            if (IsRangedAttacker)
	            {
	                CurrentActionState = Action.StartRangedAttack;
	                this.Call(rangedChargeSound.Play).After(0.3f);
                }
                else if (IsMeleeAttacker)
	            {
	                CurrentActionState = Action.MeleeAttack;
	            }
	        }
	    }

	    private void ChooseStructureForNavigation()
	    {
	        if (_potentialTargetList != null && _potentialTargetList.Count > 0)
	        {
	            _targetStructureForNavigation =
	                _potentialTargetList.Where(pt =>
	                        pt.CurrentState == BaseStructure.VariableState.Built &&
	                        pt.IsDestroyed == false)
	                    .OrderBy(pt => Vector3.Distance(Position, pt.Position)
	                    ).FirstOrDefault();
	        }
        }

	    private void ChooseStructureForAttack()
	    {
	        if (_potentialTargetList != null && _potentialTargetList.Count > 0)
	        {
	            _targetStructureToAttack =
	                _potentialTargetList.Where(pt => 
                        pt.CurrentState == BaseStructure.VariableState.Built && 
                        pt.IsDestroyed == false && 
                        (IsRangedAttacker && Vector3.Distance(Position, pt.Position) <= RangedRadius || IsMeleeAttacker && Vector3.Distance(Position, pt.Position) <= MeleeRadius))
                    .OrderBy(pt => Vector3.Distance(Position, pt.Position)
                    ).FirstOrDefault();
	        }
	    }

	    public void GetHitBy(BasePlayerProjectile projectile)
	    {
	        HealthRemaining -= projectile.DamageInflicted;
            projectile.PlayHitTargetSound();

	        if (HealthRemaining <= 0)
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
                OnDeath?.Invoke(this);
                Destroy();
            }
	    }

	    public void PlaceOnLeftSide()
	    {
	        X = _playArea.Left;
	        Y = FlatRedBallServices.Random.Between(_playArea.Bottom + CircleInstance.Radius, _playArea.Top - CircleInstance.Radius);

	        ResumeMovement();
	    }

	    public void PlaceOnRightSide()
	    {
	        X = _playArea.Right;
	        Y = FlatRedBallServices.Random.Between(_playArea.Bottom + CircleInstance.Radius, _playArea.Top - CircleInstance.Radius);

	        ResumeMovement();
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
            rangedAttackSound?.Dispose();
            rangedChargeSound?.Dispose();
		    _healthBar?.Destroy();
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
