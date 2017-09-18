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
	    private bool IsHurt => CurrentActionState == Action.Hurt;
	    private int _currentNumberOfPotentialTargets;


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
		        MeleeAttackRadiusCircleInstance.Visible = IsMeleeAttacker;
		        RangedAttackRadiusCircleInstance.Visible = IsRangedAttacker;
		    }
		    else
#endif
		    {
		        CircleInstance.Visible = false;
		        MeleeAttackRadiusCircleInstance.Visible = false;
		        RangedAttackRadiusCircleInstance.Visible = false;
            }

            SpriteInstance.RelativeY = SpriteInstance.Height / 2;

		    _lastRangeAttackTime = TimeManager.CurrentTime;
		    _lastMeleeAttackTime = TimeManager.CurrentTime;

            HealthRemaining = MaximumHealth;

		    _healthBar = CreateResourceBar(ResourceBarRuntime.BarType.Health);		    
		}

	    public static void Initialize(AxisAlignedRectangle playArea, PositionedObjectList<BaseStructure> potentialTargetList)
	    {
	        _playArea = playArea;
	        _potentialTargetList = potentialTargetList;
	    }

		private void CustomActivity()
		{
		    _currentNumberOfPotentialTargets = _potentialTargetList.Count;

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

		    if (!IsDead && !IsAttacking && !IsHurt && (_currentAttackTarget == null || !TargetIsInAttackRange(_currentAttackTarget)))
		    {
		        NavigationActivity();
		    }

		    UpdateHealthBar();
        }

	    private void UpdateHealthBar()
	    {
	        if (HealthRemaining < MaximumHealth)
	        {
	            _healthBar.UpdateBar(HealthRemaining, MaximumHealth, false);
	            _healthBar.X = (X - Camera.Main.X) * CameraZoomManager.GumCoordOffset;
	            _healthBar.Y = (Y + SpriteInstance.Height - Camera.Main.Y) * CameraZoomManager.GumCoordOffset;
	            _healthBar.Width = SpriteInstance.Width * CameraZoomManager.GumCoordOffset;
	            _healthBar.Height = SpriteInstance.Width / 5 * CameraZoomManager.GumCoordOffset;
	            _healthBar.Visible = true;
	        }
	        else
	        {
	            _healthBar.Visible = false;
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
