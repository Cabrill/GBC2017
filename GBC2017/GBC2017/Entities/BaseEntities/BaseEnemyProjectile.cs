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
	    public float MaxRange { private get; set; }
	    private bool _hitTheGround;

        private Vector3? _startingPosition;
	    private float _startingElevation;
	    private float _startingShadowWidth;
	    private float _startingShadowHeight;
	    private float _startingShadowAlpha;
	    protected SoundEffectInstance HitGroundSound;
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
	    }

	    private void CustomActivity()
	    {
	        if (ShouldBeDestroyed)
	        {
	            if (HitGroundSound.State == SoundState.Stopped)
	            {
	                Destroy();
                }
	        }
	        else
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
	                Visible = false;
	                ShouldBeDestroyed = true;
	                PlayHitGroundSound();
	            }
	        }
        }

        public void PlayHitTargetSound()
	    {
	        try
	        {
	            //if (!TargetHitSound.IsDisposed) TargetHitSound.Play();
	        }
	        catch (Exception)
	        {
	            //We may have hit the limit on number of sounds playable, but don't want to crash - just omit the sound
	        }
	    }

        private void PlayHitGroundSound()
	    {
            HitGroundSound.Play();
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

        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
