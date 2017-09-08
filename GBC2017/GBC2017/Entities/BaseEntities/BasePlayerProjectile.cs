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
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BasePlayerProjectile
	{
	    public bool ShouldBeDestroyed { get; private set; }
	    public float MaxRange { private get; set; }
	    private bool _hitTheGround;

        private Vector3? _startingPosition;
	    private float _startingElevation;
	    private float _startingShadowWidth;
	    private float _startingShadowHeight;
	    private float _startingShadowAlpha;

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
		    ShouldBeDestroyed = false;
		    _hitTheGround = false;
		    _startingPosition = null;
        }

		private void CustomActivity()
		{
		    if (!_startingPosition.HasValue)
		    {
		        _startingPosition = Position;
		        _startingElevation = ShadowSprite.RelativeY;
		        _startingShadowWidth = ShadowSprite.Width;
		        _startingShadowHeight = ShadowSprite.Height;
		        _startingShadowAlpha = ShadowSprite.Alpha;
		    }

		    var pctDistanceTraveled = (Position - _startingPosition.Value).Length() / MaxRange;
		    var negativeModifier = 1 - (pctDistanceTraveled / 2);
		    var shadowElevation = _startingElevation * negativeModifier;

		    ShadowSprite.RelativeY = CircleInstance.RelativeY = shadowElevation;
		    SpriteInstance.RelativeY = _startingElevation * pctDistanceTraveled / 2;
		    ShadowSprite.Width = _startingShadowWidth * (2 + pctDistanceTraveled);
		    ShadowSprite.Height = _startingShadowHeight * (2 + pctDistanceTraveled);
		    ShadowSprite.Alpha = _startingShadowAlpha * (0.75f + pctDistanceTraveled);

		    _hitTheGround = pctDistanceTraveled >= 1;

		    ShouldBeDestroyed = _hitTheGround;
        }

		private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
