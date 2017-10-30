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

namespace GBC2017.Entities.Enemies
{
	public partial class SmallSlime
	{
	    private const float maxJumpVelocity = 200f;
	    private const float _lightPulseDuration = 2;
	    private const float _lightPulseAmount = 0.5f;
	    private float _currentLightPulse = 0;
	    private int _pulseMod = 1;

	    private SoundEffectInstance _jumpStart;
	    private SoundEffectInstance _jumpLand;


        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
	    {
	        Altitude = 0;
	        GravityDrag = -400f;
	        CurrentJumpState = Jump.NotJumping;

	        meleeAttackSound = Slime_Hit.CreateInstance();
	        _jumpStart = Slime_Jump.CreateInstance();
	        _jumpLand = Slime_Land.CreateInstance();

	        meleeAttackSound.Pitch = SpriteInstance.TextureScale;
            _jumpStart.Pitch = SpriteInstance.TextureScale;
	        _jumpLand.Pitch = SpriteInstance.TextureScale;

	        var randomChoice = FlatRedBallServices.Random.Next(0, 3);
	        if (randomChoice == 0)
	        {
	            SpriteInstance.Red = LightSprite.Red = 1;
	            SpriteInstance.Green = LightSprite.Green = 0;
	            SpriteInstance.Blue = LightSprite.Blue = 0;
	        }
	        else if (randomChoice == 1)
	        {
	            SpriteInstance.Red = LightSprite.Red = 0;
	            SpriteInstance.Green = LightSprite.Green = 1;
	            SpriteInstance.Blue = LightSprite.Blue = 0;
	        }
	        else
	        {
	            SpriteInstance.Red = LightSprite.Red = 0;
	            SpriteInstance.Green = LightSprite.Green = 0;
	            SpriteInstance.Blue = LightSprite.Blue = 1;
	        }
        }

	    public void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

	        if (HasLightSource)
	        {
	            //LayerProvidedByContainer.Remove(LightSpriteInstance);
	            //SpriteManager.AddToLayer(LightSpriteInstance, darknessLayer);
	        }
	    }

	    private void CustomActivity()
	    {
	        _currentLightPulse += TimeManager.SecondDifference * _pulseMod;
	        if (_currentLightPulse >= _lightPulseDuration || _currentLightPulse <= 0)
	        {
	            _pulseMod *= -1;
	        }
	        LightSprite.TextureScale *= _lightPulseAmount + (_currentLightPulse / _lightPulseDuration) * _lightPulseAmount;
	        LightSprite.RelativeY = SpriteInstance.RelativeY - 20 * SpriteInstance.TextureScale;
        }

	    protected override void NavigateToTargetStructure()
	    {
	        if (CurrentJumpState == Jump.InAir && Altitude == 0)
	        {
	            CurrentJumpState = Jump.Landing;
	            Velocity = Vector3.Zero;
	            try
	            {
	                _jumpLand.Play();
	            }
	            catch (Exception) { };
            }
	        else if (CurrentJumpState == Jump.NotJumping || (CurrentJumpState == Jump.Landing && SpriteInstance.JustCycled))
	        {
	            if (_targetStructureForNavigation != null)
	            {
	                CurrentJumpState = Jump.PreparingJump;
	            }
	            else
	            {
	                CurrentJumpState = Jump.NotJumping;
	                CurrentActionState = Action.Standing;
	            }
	        }
	        else if (CurrentJumpState == Jump.PreparingJump && SpriteInstance.JustCycled)
	        {
	            if (_targetStructureForNavigation != null)
	            {
	                var angle = (float)Math.Atan2(Y - _targetStructureForNavigation.Position.Y,
	                    X - _targetStructureForNavigation.Position.X);
	                var direction = new Vector3(
	                    (float)-Math.Cos(angle),
	                    (float)-Math.Sin(angle), 0);
	                direction.Normalize();

	                var neededAltitudeVelocity = CalculateJumpAltitudeVelocity();

	                AltitudeVelocity = Math.Min(maxJumpVelocity, neededAltitudeVelocity);
	                Altitude = AltitudeVelocity * TimeManager.SecondDifference;

	                Velocity = direction * Speed * _currentScale;

	                CurrentJumpState = Jump.InAir;

	                CurrentDirectionState =
	                    (Velocity.X > 0 ? Direction.MovingRight : Direction.MovingLeft);
	                SpriteInstance.IgnoreAnimationChainTextureFlip = true;

	                try
	                {
	                    _jumpStart.Play();
	                }
	                catch (Exception){};


	            }
	            else
	            {
	                CurrentJumpState = Jump.Landing;
                }
	        }
	    }

        private void CustomDestroy()
	    {
	        if (_jumpStart != null && !_jumpStart.IsDisposed)
	        {
	            _jumpStart.Stop(true);
	        }
	        if (_jumpLand != null && !_jumpLand.IsDisposed)
	        {
	            _jumpLand.Stop(true);
	        }
        }

	    private static void CustomLoadStaticContent(string contentManagerName)
	    {


	    }
    }
}
