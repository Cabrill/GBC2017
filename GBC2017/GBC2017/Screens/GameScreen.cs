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
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
using GBC2017.Factories;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GBC2017.Screens
{
	public partial class GameScreen
	{
        #region Properties and Fields
        private enum GameMode
	    {
	        Paused,
	        Normal,
	        Building,
	        Inspecting
	    };

	    private GameMode CurrentGameMode = GameMode.Normal;
        #endregion

        #region Initialization
        void CustomInitialize()
		{
		    FlatRedBallServices.IsWindowsCursorVisible = true;
		    FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;

            SetupCamera();
            PositionTiledMap();
		    SetBuildButtonControls();
		}

	    private void SetBuildButtonControls()
	    {
            BuildBarInstance.SolarButtonClick += BuildBarInstanceOnSolarButtonClick;
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

            //Set the play area to match the tilemap size/position
	        PlayAreaRectangle.Y = justgrass.Position.Y - justgrass.Height/2;
	        PlayAreaRectangle.Height = justgrass.Height;
	        PlayAreaRectangle.Width = justgrass.Width;
	    }
        #endregion

        #region Button Click Events
	    private void BuildBarInstanceOnSolarButtonClick(IWindow window)
	    {
	        CurrentGameMode = GameMode.Building;
	        var newPanel = SolarPanelsFactory.CreateNew();
	        newPanel.MoveToLayer(EntityLayer);
	    }
        #endregion

        #region Activity
        void CustomActivity(bool firstTimeCalled)
		{
		    HandleTouchActivity();

		}

	    private void HandleTouchActivity()
	    {
            //User just clicked/touched somewhere, and nothing is currently selected
	        if ((GuiManager.Cursor.PrimaryClick || GuiManager.Cursor.PrimaryDown) && GuiManager.Cursor.ObjectGrabbed == null)
	        {
	            foreach (var structure in AllStructuresList)
	            {
	                if (GuiManager.Cursor.IsOn3D(structure.SpriteInstance))
	                {
	                    GuiManager.Cursor.ObjectGrabbed = structure;
	                    break;
	                }

	            }
	        }
            else if (GuiManager.Cursor.PrimaryDown && GuiManager.Cursor.ObjectGrabbed != null)
	        {
	            var objectAsStructure = GuiManager.Cursor.ObjectGrabbed as BaseStructure;
	            var shouldAllowDrag = objectAsStructure != null && objectAsStructure.IsBeingPlaced &&
	                                  PlayAreaRectangle.IsMouseOver(GuiManager.Cursor, EntityLayer);

                if (shouldAllowDrag)
	            {
	                GuiManager.Cursor.UpdateObjectGrabbedPosition();
	                var newLocationIsValid = true;
                    foreach (var structure in AllStructuresList)
	                {
	                    if (structure.IsBeingPlaced == false && structure.AxisAlignedRectangleInstance.CollideAgainst(objectAsStructure.AxisAlignedRectangleInstance))
	                    {
	                        newLocationIsValid = false;
	                        break;
	                    }
	                }
	                objectAsStructure.IsInValidLocation = newLocationIsValid;
                }
	        }
            else if (!GuiManager.Cursor.PrimaryDown)
	        {
	            GuiManager.Cursor.ObjectGrabbed = null;
	        }
        }

        #endregion

        #region Destroy
        void CustomDestroy()
		{


		}
        #endregion

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	}
}
