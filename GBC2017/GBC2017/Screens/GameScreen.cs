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
using FlatRedBall.Gui;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using FlatRedBall.Math;
using GBC2017.Entities.Structures;
using GBC2017.Factories;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GBC2017.Screens
{
	public partial class GameScreen
	{
	    private enum GameMode
	    {
	        Paused,
	        Normal,
	        Building,
	        Inspecting
	    };

	    private GameMode CurrentGameMode = GameMode.Normal;

	    private PositionedObjectList<SolarPanels> allSolarPanels;

		void CustomInitialize()
		{
		    FlatRedBallServices.IsWindowsCursorVisible = true;
		    FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;

            SetupCamera();
            PositionTiledMap();
		    InitializeFactories();
		    SetBuildButtonControls();
		}

	    private void SetBuildButtonControls()
	    {
            BuildBarInstance.SolarButtonClick += BuildBarInstanceOnSolarButtonClick;
	    }

	    private void BuildBarInstanceOnSolarButtonClick(IWindow window)
	    {
	        CurrentGameMode = GameMode.Building;
            var newPanel = SolarPanelsFactory.CreateNew();
            newPanel.MoveToLayer(EntityLayer);
	    }

	    private void InitializeFactories()
	    {
	        allSolarPanels = new PositionedObjectList<SolarPanels>();

            SolarPanelsFactory.Initialize(allSolarPanels, ContentManagerName);
	    }

	    private void SetupCamera()
	    {
            Camera.Main.OrthogonalWidth = 2560;
            Camera.Main.OrthogonalHeight = 1440;
        }

	    void PositionTiledMap()
	    {
            //This centers the map in the middle of the screen
	        justgrass.Position.X = -justgrass.Width / 2;

            //This positions the map between the info bar and build button bar
	        justgrass.Position.Y = justgrass.Height / 1.85f;
	    }

		void CustomActivity(bool firstTimeCalled)
		{
		    HandleTouchActivity();

		}

	    private void HandleTouchActivity()
	    {
            //User just clicked/touched somewhere, and nothing is currently selected
	        if (GuiManager.Cursor.PrimaryClick && GuiManager.Cursor.ObjectGrabbed == null)
	        {
	            foreach (var panel in allSolarPanels)
	            {
	                if (GuiManager.Cursor.IsOn3D(panel.SpriteInstance))
	                {
	                    GuiManager.Cursor.ObjectGrabbed = panel;
	                    break;
	                }

	            }
	        }
            else if (GuiManager.Cursor.PrimaryDown && GuiManager.Cursor.ObjectGrabbed != null)
	        {
	            GuiManager.Cursor.UpdateObjectGrabbedPosition();
            }
        }

	    void CustomDestroy()
		{


		}

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	}
}
