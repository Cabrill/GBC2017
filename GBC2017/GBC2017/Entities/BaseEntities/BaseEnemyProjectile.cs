using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseEnemyProjectile
	{
	    private static float _maximumY;
	    protected float _currentScale;
	    private float _startingSpriteScale;
	    private float _startingLightScale;
	    private float _startingCircleRadius;

        public float Altitude { get; set; }
        public float AltitudeVelocity { get; set; }
	    public float GravityDrag { get; set; } = -100f;
	    private bool _hitTheGround;

        private Vector3? _startingPosition;
	    private float _startingShadowWidth;
	    private float _startingShadowHeight;
	    protected float _startingShadowAlpha;
	    protected SoundEffectInstance HitGroundSound;
	    protected SoundEffectInstance HitTargetSound;
        private bool _AddedToLayers = false;
	    public bool ShouldBeDestroyed;

	    public static void Initialize(float maximumY)
	    {
	        _maximumY = maximumY;
        }

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

	        _startingShadowWidth = LightOrShadowSprite.Width;
	        _startingShadowHeight = LightOrShadowSprite.Height;
	        _startingShadowAlpha = LightOrShadowSprite.Alpha;
            _hitTheGround = false;
	        _startingPosition = null;
	        ShouldBeDestroyed = false;
	        Visible = true;
	        CurrentState = VariableState.Flying;

	        CalculateScale();
	        UpdateScale();
	    }

	    private void CalculateScale()
	    {
	        _currentScale = 0.3f + (0.4f * (1 - Y / _maximumY));
	    }

        protected virtual void UpdateScale()
	    {
	        SpriteInstance.TextureScale = _startingSpriteScale * _currentScale;
	        LightOrShadowSprite.TextureScale = _startingLightScale * _currentScale;
	        CircleInstance.Radius = _startingCircleRadius * _currentScale;
	    }

        private void CustomActivity()
	    {
            if (CurrentState == VariableState.Impact && SpriteInstance.JustCycled)
	        {
	            Visible = false;
                SpriteInstance.Animate = false;
	            ShouldBeDestroyed = true;
	        }
            else if (CurrentState != VariableState.Impact)
            {
                CalculateScale();
                UpdateScale();
            }

	        if (ShouldBeDestroyed)
	        {
	            if ((HitGroundSound == null || HitGroundSound.State == SoundState.Stopped) &&
	                (HitTargetSound == null || HitTargetSound.State == SoundState.Stopped))
	            {
	                Destroy();
	            }
	        }
	        else
	        {
	            UpdateAnimation();
                if (CurrentState != VariableState.Impact)
	            {
	                var distanceAtWhichToGrow = HasLightSource ? 200 : 400;
	                var pctLightShadow = MathHelper.Clamp(1 - (SpriteInstance.RelativeY / (distanceAtWhichToGrow * _currentScale)), 0, 1);

	                LightOrShadowSprite.Width = _startingShadowWidth * pctLightShadow * _currentScale;
	                LightOrShadowSprite.Height = _startingShadowHeight * pctLightShadow * _currentScale;
	                LightOrShadowSprite.Alpha = _startingShadowAlpha * pctLightShadow;

	                _hitTheGround = Altitude <= 0;

	                if (_hitTheGround)
	                {
	                    HandleImpact();
	                    PlayHitGroundSound();
	                }
	            }
	        }
	    }

	    public void HandleImpact()
	    {
            CurrentState = VariableState.Impact;
	        Velocity = Vector3.Zero;
            CustomHandleImpact();
	    }

	    protected virtual void CustomHandleImpact()
	    {
	        
	    }

	    private void UpdateAnimation()
	    {
	        if (CurrentState == VariableState.Flying)
	        {
	            AltitudeVelocity += GravityDrag * TimeManager.SecondDifference;
	            Altitude += AltitudeVelocity * TimeManager.SecondDifference;
	        }

	        if (!SpriteInstance.Animate || SpriteInstance.CurrentChain.Count == 1)
	        {
	            SpriteInstance.RelativeY = Altitude + SpriteInstance.CurrentChain[0].RelativeY;
	        }
	        else
	        {
                SpriteInstance.UpdateToCurrentAnimationFrame();
                
	            if (SpriteInstance.UseAnimationRelativePosition && SpriteInstance.RelativePosition != Vector3.Zero)
	            {
	                SpriteInstance.RelativeX *= SpriteInstance.FlipHorizontal ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale;
	                SpriteInstance.RelativeY *= SpriteInstance.FlipVertical ? -SpriteInstance.TextureScale : SpriteInstance.TextureScale;
	            }
	            SpriteInstance.RelativeY += Altitude * _currentScale;
	        }
        }

        public void PlayHitTargetSound()
	    {
	        try
	        {
	            HitTargetSound.Play();
	        }catch (Exception){}
	    }

        private void PlayHitGroundSound()
	    {
	        try
	        {
	            HitGroundSound.Play();
            }
	        catch (Exception) { }
	    }

	    public void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        if (LayerProvidedByContainer != null)
            {

	            LayerProvidedByContainer.Remove(LightOrShadowSprite);
                LayerProvidedByContainer.Remove(CircleInstance);
	        }

	        if (_AddedToLayers)
	        {
	            darknessLayer.Remove(LightOrShadowSprite);
                hudLayer.Remove(CircleInstance);
                if (HasLightSource) darknessLayer.Remove(SpriteInstance);
            }

	        SpriteManager.AddToLayer(LightOrShadowSprite, darknessLayer);
	        ShapeManager.AddToLayer(CircleInstance, hudLayer);
            if (HasLightSource) SpriteManager.AddToLayer(SpriteInstance, darknessLayer);

	        _AddedToLayers = true;
        }

        private void CustomDestroy()
		{
		    if (HitGroundSound != null && !HitGroundSound.IsDisposed)
		    {
		        HitGroundSound.Stop(true);
                HitGroundSound.Dispose();
		    }
		    if (HitTargetSound != null && !HitTargetSound.IsDisposed)
		    {
		        HitTargetSound.Stop(true);
		        HitTargetSound.Dispose();
		    }
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
