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
using GBC2017.Entities;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
using GBC2017.Factories;
using GBC2017.ResourceManagers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using RenderingLibrary;
using Camera = FlatRedBall.Camera;

namespace GBC2017.Screens
{
	public partial class GameScreen
	{
        #region Properties and Fields
        private enum GameMode
	    {
	        Normal,
	        Building,
	        Inspecting
	    };

	    private GameMode CurrentGameMode = GameMode.Normal;
	    private PositionedObject selectedObject;
        #endregion

        #region Initialization
        void CustomInitialize()
		{
            #if WINDOWS || DESKTOP_GL
            FlatRedBallServices.IsWindowsCursorVisible = true;
            #endif

            FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;

		    var createAHome = HomeFactory.CreateNew(EntityLayer);

            FindValidLocationFor(createAHome);
		    createAHome.CurrentState = BaseStructure.VariableState.Built;
		    createAHome.IsBeingPlaced = false;

            SetCollisionVisibility();
            SetupCamera();
            PositionTiledMap();
		    SetInfoBarControls();
            SetBuildButtonControls();
		    InitializeBaseEntities();
		}

	    private void InitializeBaseEntities()
	    {
	        BaseCombatStructure.Initialize(AllEnemiesList);
	        BaseEnemy.Initialize(PlayAreaRectangle, AllStructuresList);
            EnergyManager.Initialize(AllStructuresList);
            MineralsManager.Initialize(AllStructuresList);
	    }

	    private void SetCollisionVisibility()
	    {
            #if DEBUG
	        if (DebugVariables.ShowDebugShapes)
	        {
	            PlayAreaRectangle.Visible = true;
	        }
	        else
            #endif
	        {
	            PlayAreaRectangle.Visible = false;
	        }
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
	        justgrass.Position.Y = justgrass.Height/3;

            //Set the play area to match the tilemap size/position
	        PlayAreaRectangle.Y = justgrass.Position.Y- justgrass.Height/2.65f;
	        PlayAreaRectangle.Height = justgrass.Height/1.65f;
	        PlayAreaRectangle.Width = justgrass.Width;
	    }
#endregion

#region Activity
        void CustomActivity(bool firstTimeCalled)
		{
            #if DEBUG
		    HandleDebugInput();
            #endif

		    UpdateGameModeActivity();

		    HandleTouchActivity();
		    SelectedItemActivity();

            if (!IsPaused)
		    {
                BuildingStatusActivity();
                EnemyStatusActivity();
		        PlayerProjectileActivity();
		        EnemyProjectileActivity();

		        EnergyManager.Update();
		        MineralsManager.Update();

                InfoBarInstance.UpdateEnergyDisplay(EnergyManager.EnergyIncrease, EnergyManager.EnergyDecrease,
		            EnergyManager.StoredEnergy, EnergyManager.MaxStorage);

		        InfoBarInstance.UpdateMineralsDisplay(MineralsManager.MineralsIncrease, MineralsManager.MineralsDecrease,
		            MineralsManager.StoredMinerals, MineralsManager.MaxStorage);
		    }
		}

	    private void SelectedItemActivity()
	    {
	        if (selectedObject == null)
	        {
	            EnemyInfoInstance.Visible = false;
	            StructureInfoInstance.Visible = false;
	            return;
	        }

	        if (selectedObject is BaseStructure)
	        {
                EnemyInfoInstance.Visible = false;
                ShowStructureInfo((BaseStructure) selectedObject);
	        }
	        if (selectedObject is BaseEnemy)
	        {
                StructureInfoInstance.Visible = false;
                ShowEnemyInfo((BaseEnemy) selectedObject);
	        }
	    }

	    private void UpdateGameModeActivity()
	    {
	        if (AllStructuresList.Any(s => s.IsBeingPlaced))
	        {
	            CurrentGameMode = GameMode.Building;
	        }
            else if (selectedObject != null)
            { 
	            CurrentGameMode = GameMode.Inspecting;
            }
	        else
	        {
	            CurrentGameMode = GameMode.Normal;
	        }
	    }

	    private void EnemyStatusActivity()
	    {
	        for (var i = AllEnemiesList.Count; i > 0; i--)
	        {
	            var enemy = AllEnemiesList[i-1];
	            if (!PlayAreaRectangle.CollideAgainst(enemy.CircleInstance))
	            {
	                enemy.Destroy();
	            }
	        }
	    }

	    private void EnemyProjectileActivity()
	    {
	        for (var i = EnemyProjectileList.Count; i > 0; i--)
	        {
	            var projectile = EnemyProjectileList[i - 1];

	            if (!PlayAreaRectangle.IsPointOnOrInside(projectile.X, projectile.Y))
	            {
	                projectile.Destroy();
	            }
	            else
	            {
	                for (var e = AllStructuresList.Count; e > 0; e--)
	                {
	                    var structure = AllStructuresList[e - 1];

                        if (structure.CurrentState != BaseStructure.VariableState.Built || !projectile.CircleInstance.CollideAgainst(structure.AxisAlignedRectangleInstance)) continue;

	                    structure.GetHitBy(projectile);
	                    projectile.Destroy();
	                }
	            }
	        }
        }

	    private void PlayerProjectileActivity()
	    {
	        for (var i = PlayerProjectileList.Count; i > 0; i--)
	        {
	            var projectile = PlayerProjectileList[i-1];

	            if (!PlayAreaRectangle.IsPointOnOrInside(projectile.X, projectile.Y))
	            {
	                projectile.Destroy();
	            }
                else
                {
                    for (var e = AllEnemiesList.Count; e > 0; e--)
                    {
                        var enemy = AllEnemiesList[e - 1];
                        if (!projectile.CircleInstance.CollideAgainst(enemy.CircleInstance)) continue;

                        enemy.GetHitBy(projectile);
                        projectile.Destroy();
                    }
                }
	        }
	    }

	    private void BuildingStatusActivity()
	    {
	        if (CurrentGameMode != GameMode.Building) return;

	        var newStructure = AllStructuresList.FirstOrDefault(s => s.IsBeingPlaced);

	        if (newStructure == null)
	        {
	            CurrentGameMode = GameMode.Normal;
	        }
	        else
	        {
	            var newLocationIsValid = AllStructuresList.All(otherStructure => otherStructure.IsBeingPlaced || 
	                                                                             !otherStructure.AxisAlignedRectangleInstance.CollideAgainst(newStructure.AxisAlignedRectangleInstance));

	            if (newLocationIsValid)
	            {
	                if (AllEnemiesList.Any(enemy => enemy.CircleInstance.CollideAgainst(newStructure.AxisAlignedRectangleInstance)))
	                {
	                    newLocationIsValid = false;
	                }
	            }
	            if (newStructure.IsValidLocation != newLocationIsValid)
	            {
	                newStructure.IsValidLocation = newLocationIsValid;
	            }
	        }
	    }

        #if DEBUG
        private void HandleDebugInput()
	    {
	        if (InputManager.Keyboard.KeyPushed(Keys.X))
	        {
	            var newAlien = BasicAlienFactory.CreateNew(EntityLayer);
                newAlien.PlaceOnRightSide();
	        }

	        if (InputManager.Keyboard.KeyPushed(Keys.Z))
	        {
	            var newAlien = BasicAlienFactory.CreateNew(EntityLayer);
                newAlien.PlaceOnLeftSide();
	        }

        }
        #endif
        private void HandleTouchActivity()
	    {
            //User just clicked/touched somewhere, and nothing is currently selected
	        if ((GuiManager.Cursor.PrimaryClick || GuiManager.Cursor.PrimaryDown) &&
	            GuiManager.Cursor.ObjectGrabbed == null)
	        {
	            if (CurrentGameMode == GameMode.Building)
	            {
	                var structureBeingBuilt = AllStructuresList.FirstOrDefault(s => s.IsBeingPlaced);

	                if (structureBeingBuilt != null && GuiManager.Cursor.IsOn3D(structureBeingBuilt.SpriteInstance))
	                {
	                    GuiManager.Cursor.ObjectGrabbed = structureBeingBuilt;
	                }
	            }
	            else //Not building, user is possibly selecting an object
	            {
                    //Remove the current selection if the user clicks off of it
	                if (selectedObject != null && !(selectedObject as IClickable).HasCursorOver(GuiManager.Cursor))
	                {
                        
                        selectedObject = null;
	                }

	                foreach (var structure in AllStructuresList)
	                {
	                    if (GuiManager.Cursor.IsOn3D(structure.SpriteInstance))
	                    {
	                        GuiManager.Cursor.ObjectGrabbed = structure;
	                        selectedObject = structure;
	                        CurrentGameMode = GameMode.Inspecting;
	                        ShowStructureInfo(structure);
	                        break;
	                    }
	                }

                    //Didn't select a structure, check for enemies
	                if (selectedObject == null)
	                {
	                    foreach (var enemy in AllEnemiesList)
	                    {
	                        if (GuiManager.Cursor.IsOn3D(enemy.SpriteInstance))
	                        {
	                            GuiManager.Cursor.ObjectGrabbed = enemy;
	                            selectedObject = enemy;
	                            CurrentGameMode = GameMode.Inspecting;
	                            ShowEnemyInfo(enemy);
	                            break;
	                        }
	                    }
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
	            }
	        }
	        else if (!GuiManager.Cursor.PrimaryDown)
	        {
	            GuiManager.Cursor.ObjectGrabbed = null;
	        }
	    }

	    private void ShowStructureInfo(BaseStructure structure)
	    {
	        StructureInfoInstance.Visible = true;
	        StructureInfoInstance.X = structure.X;
	        StructureInfoInstance.Y = structure.Y + structure.SpriteInstance.Height / 2;
	        StructureInfoInstance.OnClose = () => selectedObject = null;
	    }

	    private void ShowEnemyInfo(BaseEnemy enemy)
	    {
	        EnemyInfoInstance.Visible = true;
	        EnemyInfoInstance.X = enemy.X;
	        EnemyInfoInstance.Y = enemy.Y + enemy.SpriteInstance.Height / 2;
	        EnemyInfoInstance.OnClose = () => selectedObject = null;
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
