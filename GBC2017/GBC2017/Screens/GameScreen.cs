using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using Microsoft.Xna.Framework.Graphics;


namespace GBC2017.Screens
{
	public partial class GameScreen
	{

		void CustomInitialize()
		{
		    SetupCamera();
            PositionTiledMap();

		}

	    private void SetupCamera()
	    {
	        FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;
	        ;
            Camera.Main.OrthogonalWidth = 2560;
            Camera.Main.OrthogonalHeight = 1440;

        }

	    void PositionTiledMap()
	    {
	        justgrass.Position.X = -justgrass.Width / 2;
	        justgrass.Position.Y = justgrass.Height / 2;
	    }

		void CustomActivity(bool firstTimeCalled)
		{


		}

		void CustomDestroy()
		{


		}

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	}
}
