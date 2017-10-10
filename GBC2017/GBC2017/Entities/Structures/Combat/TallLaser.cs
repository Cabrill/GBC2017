using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.Entities.BaseEntities;
using GBC2017.Factories;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.Structures.Combat
{
	public partial class TallLaser
	{
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            attackSound = GlobalContent.TallLaser_Shoot.CreateInstance();
        }

	    protected override void SetAnimationFromAimRotation()
	    {
	        if (SpriteInstance.CurrentFrameIndex != _lastFrameIndex)
	        {
                SpriteInstance.UpdateToCurrentAnimationFrame();
	            _lastFrameIndex = SpriteInstance.CurrentFrameIndex;
	            _lastFrameChain = SpriteInstance.CurrentChainName;

	            if (SpriteInstance.UseAnimationRelativePosition && SpriteInstance.RelativePosition != Vector3.Zero)
	            {
	                SpriteInstance.RelativeX *= SpriteInstance.TextureScale;
	                SpriteInstance.RelativeY *= SpriteInstance.TextureScale;
	            }
	            SpriteInstance.RelativeY += _spriteRelativeY;
	        }
	    }

	    protected override BasePlayerProjectile CreateNewProjectile()
	    {
	        var newProjectile = TallLaserProjectileFactory.CreateNew(LayerProvidedByContainer);
	        return newProjectile;
	    }

        private void CustomActivity()
		{


		}

		private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
