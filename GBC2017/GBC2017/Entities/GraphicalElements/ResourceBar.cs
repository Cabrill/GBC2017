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

namespace GBC2017.Entities.GraphicalElements
{
	public partial class ResourceBar
	{
	    public float Height => FrameSprite.Height;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
		{


		}

	    public void SetWidth(float newWidth)
	    {
	        BackgroundSprite.Width = newWidth;
	        FrameSprite.Width = newWidth+6;
	        BarSprite.Width = newWidth;

	        var newHeight = newWidth / 5;
	        BackgroundSprite.Height = newHeight;
	        FrameSprite.Height = newHeight + 6;
	        BarSprite.Height = newHeight;
	    }

	    public void SetRelativeY(float relativeY)
	    {
	        BackgroundSprite.RelativeY = relativeY + Height;
	        FrameSprite.RelativeY = relativeY + Height;
	        BarSprite.RelativeY = relativeY + Height;
        }

	    public void Update(float newPct)
	    {
	        if (!FrameSprite.Visible) Show();

	        BarSprite.Width = Math.Max(0.001f,BackgroundSprite.Width * newPct);
	        BarSprite.RelativeX = (BarSprite.Width - BackgroundSprite.Width)/2;
	    }

	    public void Show()
	    {
	        BackgroundSprite.Visible = true;
	        FrameSprite.Visible = true;
	        BarSprite.Visible = true;
        }

	    public void Hide()
	    {
            if (!FrameSprite.Visible) return;
            
            BackgroundSprite.Visible = false;
	        FrameSprite.Visible = false;
	        BarSprite.Visible = false;
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
