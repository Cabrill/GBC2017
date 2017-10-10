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
using FlatRedBall.Gum;
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
	    public float Altitude { get; protected set; }

        private static AxisAlignedRectangle _leftSpawnArea;
	    private static AxisAlignedRectangle _rightSpawnArea;
        private static PositionedObjectList<BaseStructure> _potentialTargetList;

	    public float HealthRemaining { get; private set; }
        public bool IsDead => HealthRemaining <= 0;
	    private bool IsHurt => CurrentActionState == Action.Hurt;
	    private int _currentNumberOfPotentialTargets;
	    private float _relativeYOffset;
	    private int _lastFrameIndex;
	    private string _lastFrameChain;

	    private Vector3? _startingPosition;
	    private float _startingShadowWidth;
	    private float _startingShadowHeight;
	    private float _startingShadowAlpha;
	    private float _spriteRelativeY;

        protected SoundEffectInstance rangedChargeSound;
	    protected SoundEffectInstance rangedAttackSound;
	    protected SoundEffectInstance meleeAttackSound;

	    private ResourceBarRuntime _healthBar;
	    private float _healthBarWidth;

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

		    _lastRangeAttackTime = TimeManager.CurrentTime;
		    _lastMeleeAttackTime = TimeManager.CurrentTime;

            HealthRemaining = MaximumHealth;
		    _healthBarWidth = SpriteInstance.Width;
            _healthBar = CreateResourceBar(ResourceBarRuntime.BarType.Health);
		    _relativeYOffset = SpriteInstance.Height / 2;
		    SpriteInstance.RelativeY += _relativeYOffset;
		    _lastFrameIndex = -1;
		    _lastFrameChain = "";
		    _spriteRelativeY = GetSpriteRelativeY();
        }

	    public static void Initialize(AxisAlignedRectangle left, AxisAlignedRectangle right, PositionedObjectList<BaseStructure> potentialTargetList)
	    {
	        _leftSpawnArea = left;
	        _rightSpawnArea = right;
	        _potentialTargetList = potentialTargetList;
	    }

		private void CustomActivity()
		{
		    _currentNumberOfPotentialTargets = _potentialTargetList.Count;

		    if (IsDead)
		    {
		        PerformDeath();
		    }
		    else if (IsHurt && SpriteInstance.JustCycled)
		    {
		        CurrentActionState = Action.Standing;
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

		    UpdateAnimation();
            UpdateHealthBar();
        }

	    private void UpdateAnimation()
	    {
	        if (!_startingPosition.HasValue)
	        {
	            _startingPosition = Position;
	            _startingShadowWidth = ShadowSprite.Width;
	            _startingShadowHeight = ShadowSprite.Height;
	            _startingShadowAlpha = ShadowSprite.Alpha;
	        }

            if (SpriteInstance.CurrentFrameIndex != _lastFrameIndex || SpriteInstance.CurrentChainName != _lastFrameChain)
	        {
	            _lastFrameIndex = SpriteInstance.CurrentFrameIndex;
	            _lastFrameChain = SpriteInstance.CurrentChainName;

	            if (SpriteInstance.UseAnimationRelativePosition && SpriteInstance.RelativePosition != Vector3.Zero)
	            {
	                SpriteInstance.RelativeX *= SpriteInstance.FlipHorizontal ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale;
	                SpriteInstance.RelativeY *= SpriteInstance.FlipVertical ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale;
	            }
	            SpriteInstance.RelativeY += SpriteInstance.Height/2;

	            SpriteInstance.RelativeY += Altitude;
            }

	        var pctLightShadow = MathHelper.Clamp(1 - (SpriteInstance.RelativeY / 800), 0, 1);

	        ShadowSprite.Width = _startingShadowWidth * pctLightShadow;
	        ShadowSprite.Height = _startingShadowHeight * pctLightShadow;
	        ShadowSprite.Alpha = _startingShadowAlpha * pctLightShadow;
        }

	    private void UpdateHealthBar()
	    {
	        if (HealthRemaining < MaximumHealth)
	        {
	            _healthBar.UpdateBar(HealthRemaining, MaximumHealth, false);
	            _healthBar.X = (X - Camera.Main.X) * CameraZoomManager.GumCoordOffset;
	            _healthBar.Y = (Y + _spriteRelativeY + Altitude - Camera.Main.Y) * CameraZoomManager.GumCoordOffset;
	            _healthBar.Width = _healthBarWidth * CameraZoomManager.GumCoordOffset;
	            _healthBar.Height = _healthBarWidth / 5 * CameraZoomManager.GumCoordOffset;
	            _healthBar.Visible = true;
	        }
	        else
	        {
	            _healthBar.Visible = false;
	        }
        }

	    private float GetSpriteRelativeY()
	    {
	        if (SpriteInstance.CurrentChain == null || SpriteInstance.CurrentChain.Count == 1)
	        {
	            return SpriteInstance.Height / 2;
	        }
	        else
	        {
	            var maxHeight = 0f;
	            foreach (var frame in SpriteInstance.CurrentChain)
	            {
	                maxHeight = Math.Max(maxHeight, (frame.BottomCoordinate - frame.TopCoordinate) * frame.Texture.Height * SpriteInstance.TextureScale);
	            }
	            return maxHeight / 2;
	        }
	    }

        public void GetHitBy(BasePlayerProjectile projectile)
	    {
	        HealthRemaining -= projectile.DamageInflicted;
            projectile?.PlayHitTargetSound();

	        if (HealthRemaining <= 0)
	        {
	            PerformDeath();
	        }
	        else
	        {
	            if (CurrentActionState == Action.StartRangedAttack)
	            {
                    Instructions.Clear();
	                if (rangedChargeSound != null && !rangedChargeSound.IsDisposed && rangedChargeSound.State == SoundState.Playing) rangedChargeSound.Stop();
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
            else if (IsFlying && Altitude > 0)
            {
                Altitude += TimeManager.SecondDifference * -300f;
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
		    if (rangedAttackSound != null && !rangedAttackSound.IsDisposed)
		    {
                rangedAttackSound.Stop(true);
                rangedAttackSound.Dispose();
		    }

		    if (rangedChargeSound != null && !rangedChargeSound.IsDisposed) 
		    {
		        rangedChargeSound.Stop(true);
		        rangedChargeSound.Dispose();
            }
		    _healthBar?.Destroy();
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	    protected void AddSpritesToLayers(FlatRedBall.Graphics.Layer darknessLayer, FlatRedBall.Graphics.Layer hudLayer)
	    {
	        if (LayerProvidedByContainer != null)
	        {
	            LayerProvidedByContainer.Remove(CircleInstance);
            }
	        
	        ShapeManager.AddToLayer(CircleInstance, hudLayer);

	        var frbLayer = GumIdb.AllGumLayersOnFrbLayer(hudLayer).FirstOrDefault();

	        frbLayer?.Remove(_healthBar);
	        _healthBar.MoveToLayer(frbLayer);
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
