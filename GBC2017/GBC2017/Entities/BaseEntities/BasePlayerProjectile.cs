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
	    public float MaxRange { private get; set; }
	    private bool _hitTheGround;

        private Vector3? _startingPosition;
	    private float _startingElevation;
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
		            _startingElevation = Altitude;
		            _startingShadowWidth = LightOrShadowSprite.Width;
		            _startingShadowHeight = LightOrShadowSprite.Height;
		            _startingShadowAlpha = LightOrShadowSprite.Alpha;
		        }

		        var pctDistanceTraveled = (Position - _startingPosition.Value).Length() / MaxRange;
		        var negativeModifier = 1 - pctDistanceTraveled;

		        if (HasLightSource)
		        {
		            SpriteInstance.RelativeY = _startingElevation * negativeModifier;

		            LightOrShadowSprite.Width = _startingShadowWidth * 0.25f + pctDistanceTraveled / 2;
		            LightOrShadowSprite.Height = _startingShadowHeight * 0.25f + pctDistanceTraveled / 2;
		            LightOrShadowSprite.Alpha = _startingShadowAlpha * (0.5f + pctDistanceTraveled);
		        }
		        else
		        {
		            SpriteInstance.RelativeY = _startingElevation * negativeModifier;

		            LightOrShadowSprite.Width = _startingShadowWidth * (2 + pctDistanceTraveled);
		            LightOrShadowSprite.Height = _startingShadowHeight * (2 + pctDistanceTraveled);
		            LightOrShadowSprite.Alpha = _startingShadowAlpha * (0.75f + pctDistanceTraveled);
		        }

		        _hitTheGround = pctDistanceTraveled >= 1;

		        if (_hitTheGround)
		        {
		            PlayHitGroundSound();
		            CurrentState = VariableState.Impact;
		        }
		    }
		}

	    protected virtual void HandleImpact()
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
