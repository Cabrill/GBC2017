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
	public partial class BasePlayerProjectile
	{
	    private int groundHitCounter = 0;
	    public float Altitude { get; set; }
	    public float AltitudeVelocity { get; set; }
	    public float GravityDrag { get; set; } = -100f;
        private bool _hitTheGround;

        private Vector3? _startingPosition;
	    private float _startingShadowWidth;
	    private float _startingShadowHeight;
	    private float _startingShadowAlpha;
	    protected SoundEffectInstance HitGroundSound;
	    protected SoundEffectInstance HitTargetSound;
	    private bool _spritedAddedToLayers = false;
	    public bool ShouldBeDestroyed;

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

            //These have to be set here, because the object is pooled (reused)
            _hitTheGround = false;
		    _startingPosition = null;
		    ShouldBeDestroyed = false;
		    Visible = true;
		    LightOrShadowSprite.Position = Position;
            CurrentState = VariableState.Flying;
		}

		private void CustomActivity()
		{
		    UpdateAnimation();

            if (CurrentState == VariableState.Impact && SpriteInstance.JustCycled)
		    {
		        Visible = false;
		        SpriteInstance.Animate = false;
		        ShouldBeDestroyed = true;
		    }

            if (ShouldBeDestroyed)
		    {
		        if (HitGroundSound.State == SoundState.Stopped && HitTargetSound.State == SoundState.Stopped)
		        {
		            Destroy();
		        }
		    }
		    else if (CurrentState != VariableState.Impact)
            {
                if (!_startingPosition.HasValue)
		        {
		            _startingPosition = Position;
		            _startingShadowWidth = LightOrShadowSprite.Width;
		            _startingShadowHeight = LightOrShadowSprite.Height;
		            _startingShadowAlpha = LightOrShadowSprite.Alpha;
		        }

		        var distanceAtWhichToGrow = HasLightSource ? 200 : 400;
		        var pctLightShadow = MathHelper.Clamp(1 - (SpriteInstance.RelativeY / distanceAtWhichToGrow), 0, 1);

                LightOrShadowSprite.Width = _startingShadowWidth * pctLightShadow;
		        LightOrShadowSprite.Height = _startingShadowHeight * pctLightShadow;
		        LightOrShadowSprite.Alpha = _startingShadowAlpha * pctLightShadow;

                _hitTheGround = Altitude <= 0;

		        if (_hitTheGround)
		        {
		            PlayHitGroundSound();
		            CurrentState = VariableState.Impact;
		        }
		    }
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
	            SpriteInstance.RelativeY += Altitude;
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

        private void PlayHitGroundSound()
	    {
	        try
	        {
	            HitGroundSound.Play();
            }
	        catch (Exception){}
        }

	    public void PlayHitTargetSound()
	    {
	        try
	        {
	            HitTargetSound.Play();
	        }
	        catch (Exception) { }
        }


	    public void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        if (!_spritedAddedToLayers)
	        {
	            if (HasLightSource)
	            {
	                LayerProvidedByContainer.Remove(LightOrShadowSprite);
	                SpriteManager.AddToLayer(LightOrShadowSprite, darknessLayer);
	            }

	            LayerProvidedByContainer.Remove(CircleInstance);
	            ShapeManager.AddToLayer(CircleInstance, hudLayer);

	            _spritedAddedToLayers = true;
	        }
	    }

        private void CustomDestroy()
		{
		    if (HitGroundSound != null && !HitGroundSound.IsDisposed)
		    {
                if (HitGroundSound.State != SoundState.Stopped) HitGroundSound.Stop(true);
		        HitGroundSound.Dispose();
		    }

		    if (HitTargetSound != null && !HitTargetSound.IsDisposed)
		    {
		        if (HitTargetSound.State != SoundState.Stopped) HitTargetSound.Stop(true);
                HitTargetSound.Dispose();
		    }
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
