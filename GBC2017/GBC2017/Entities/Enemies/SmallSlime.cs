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

namespace GBC2017.Entities.Enemies
{
	public partial class SmallSlime
	{
	    private const float maxJumpVelocity = 200f;

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
	    }

	    public void AddSpritesToLayers(FlatRedBall.Graphics.Layer darknessLayer, Layer hudLayer)
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


	    }

	    protected override void NavigateToTargetStructure()
	    {
	        if (CurrentJumpState == Jump.InAir && Altitude == 0)
	        {
	            CurrentJumpState = Jump.Landing;
	            Velocity = Vector3.Zero;
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
	            }
	            else
	            {
	                CurrentActionState = Action.Standing;
	            }
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
