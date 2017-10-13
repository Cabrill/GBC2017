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
	    private static float _maximumY;
	    protected float _currentScale;


        public event Action<BaseEnemy> OnDeath;
	    public float Altitude { get; protected set; }
	    protected float AltitudeVelocity { get; set; }
	    protected float GravityDrag { get; set; } = -100f;

        private static AxisAlignedRectangle _leftSpawnArea;
	    private static AxisAlignedRectangle _rightSpawnArea;

        private static PositionedObjectList<BaseStructure> _potentialTargetList;
	    private int _currentNumberOfPotentialTargets;
	    private int _lastNumberOfPotentialTargets;

        public float HealthRemaining { get; private set; }
        public bool IsDead => HealthRemaining <= 0;
	    private bool IsHurt => CurrentActionState == Action.Hurt;

        private int _lastFrameIndex;
	    private string _lastFrameChain;

	    private Vector3? _startingPosition;
	    private float _startingShadowWidth;
	    private float _startingShadowHeight;
	    private float _startingShadowAlpha;
	    private float _startingRangedRadius;
	    private float _startingMeleeRadius;
	    private float _startingSpriteScale;
	    private float _startingLightScale;
	    private float _startingCircleRadius;

        protected float _spriteRelativeY;

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
		    _startingSpriteScale = SpriteInstance.TextureScale;
		    _startingLightScale = LightSprite.TextureScale;
		    _startingCircleRadius = CircleInstance.Radius;
		    _startingShadowWidth = ShadowSprite.Width;
		    _startingShadowHeight = ShadowSprite.Height;
		    _startingShadowAlpha = ShadowSprite.Alpha;
		    _startingRangedRadius = RangedAttackRadius;
	        _startingMeleeRadius = MeleeAttackRadius;

            HealthRemaining = MaximumHealth;
		    _healthBarWidth = SpriteInstance.Width;
            _healthBar = CreateResourceBar(ResourceBarRuntime.BarType.Health);
		    _spriteRelativeY = GetSpriteRelativeY();

            CalculateScale();
            UpdateScale();
            UpdateAnimation();

		    Altitude = 0f;
		    AltitudeVelocity = 0f;
		    GravityDrag = 0f;
		}

	    public static void Initialize(AxisAlignedRectangle left, AxisAlignedRectangle right, PositionedObjectList<BaseStructure> potentialTargetList, float maximumY)
	    {
	        _leftSpawnArea = left;
	        _rightSpawnArea = right;
	        _potentialTargetList = potentialTargetList;
	        _maximumY = maximumY;
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

            CalculateScale();
            UpdateScale();
		    UpdateAnimation();
            UpdateHealthBar();

		    _lastNumberOfPotentialTargets = _currentNumberOfPotentialTargets;
		}

	    private void CalculateScale()
	    {
	        _currentScale = 0.4f + (0.3f * (1 - Y / _maximumY));
	    }

        protected virtual void UpdateScale()
	    {
	        SpriteInstance.TextureScale = _startingSpriteScale * _currentScale;
	        CircleInstance.Radius = _startingCircleRadius * _currentScale;
	        
            if (HasLightSource) LightSprite.TextureScale = _startingLightScale * _currentScale;
            if (IsRangedAttacker) RangedAttackRadiusCircleInstance.Radius = _startingRangedRadius * _currentScale;
	        if (IsMeleeAttacker) MeleeAttackRadiusCircleInstance.Radius = _startingMeleeRadius * _currentScale;
	    }

        private void UpdateAnimation()
	    {
	        if (Altitude > 0 && (!IsFlying || IsDead))
	        {
	            AltitudeVelocity += GravityDrag * TimeManager.SecondDifference;
	        }
	        else
	        {
	            AltitudeVelocity = 0;
	        }
	        Altitude = Math.Max(0, Altitude + AltitudeVelocity * TimeManager.SecondDifference);

	        if (!SpriteInstance.Animate || SpriteInstance.CurrentChain.Count == 1)
	        {
	            SpriteInstance.RelativeX = SpriteInstance.CurrentChain[0].RelativeX * (SpriteInstance.FlipHorizontal ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale);
                SpriteInstance.RelativeY = (Altitude+ _spriteRelativeY) * _currentScale  + SpriteInstance.CurrentChain[0].RelativeY * SpriteInstance.TextureScale;
	        }
	        else
	        {
                SpriteInstance.UpdateToCurrentAnimationFrame();
                
	            if (SpriteInstance.UseAnimationRelativePosition && SpriteInstance.RelativePosition != Vector3.Zero)
	            {
	                SpriteInstance.RelativeX *= (SpriteInstance.FlipHorizontal ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale);
	                SpriteInstance.RelativeY *= (SpriteInstance.FlipVertical ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale);
	            }
	            SpriteInstance.RelativeY += (Altitude + _spriteRelativeY) * _currentScale;
	        }

	        var pctLightShadow = MathHelper.Clamp(1 - (SpriteInstance.RelativeY / (800*_currentScale)), 0, 1);

	        ShadowSprite.Width = _startingShadowWidth * pctLightShadow * _currentScale;
	        ShadowSprite.Height = _startingShadowHeight * pctLightShadow * _currentScale;
	        ShadowSprite.Alpha = _startingShadowAlpha * pctLightShadow;

	        if (HasLightSource)
	        {
	            LightSprite.TextureScale = _startingLightScale * _currentScale;
	            LightSprite.RelativeY = SpriteInstance.RelativeY;
	        }
        }

	    private void UpdateHealthBar()
	    {
	        if (HealthRemaining < MaximumHealth)
	        {
	            _healthBar.UpdateBar(HealthRemaining, MaximumHealth, false);
	            _healthBar.X = (X - Camera.Main.X) * CameraZoomManager.GumCoordOffset;
	            _healthBar.Y = (Y + (_spriteRelativeY + Altitude) * _currentScale - Camera.Main.Y) * CameraZoomManager.GumCoordOffset;
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

	            Velocity = Vector3.Zero;
                CurrentActionState = Action.Hurt;
	        }
	    }

	    private void PerformDeath()
	    {
	        Velocity = Vector3.Zero;

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
                LayerProvidedByContainer.Remove(LightSprite);
            }

            if (HasLightSource) FlatRedBall.SpriteManager.AddToLayer(LightSprite, darknessLayer);


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
