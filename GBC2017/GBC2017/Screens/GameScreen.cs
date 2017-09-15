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
using GBC2017.StaticManagers;
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

	    private DateTime gameTimeOfDay;

	    private double lastEnemyWave;
	    private int numberOfLastWave = 0;

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
            PositionTiledMap();
		    SetInfoBarControls();
            SetBuildButtonControls();
		    InitializeBaseEntities();
		    gameTimeOfDay = new DateTime(2017, 1, 1, 12,0,0);
		    lastEnemyWave = TimeManager.CurrentTime;
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
		    var temp = Gum.Wireframe.GraphicalUiElement.CanvasWidth;

            UpdateGameModeActivity();

		    HandleTouchActivity();
		    SelectedItemActivity();

            if (!IsPaused)
            {
                TemporaryDebugWaveSpawning();

                SunlightManager.UpdateConditions(gameTimeOfDay);
                UpdateGameTime();
                
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

	    private void TemporaryDebugWaveSpawning()
	    {
	        if (TimeManager.SecondsSince(lastEnemyWave) > 20)
	        {
	            lastEnemyWave = TimeManager.CurrentTime;

                numberOfLastWave++;
	            var useLeftSide = numberOfLastWave > 3 && numberOfLastWave % 3 == 0;

                for (var i = 0; i < numberOfLastWave; i++)
                {
                    if (numberOfLastWave > 3 && i % 3 == 0)
                    {
                        useLeftSide = !useLeftSide;
                    }

                    if (useLeftSide)
                    {
                        var newAlien = BasicAlienFactory.CreateNew(EntityLayer);
                        newAlien.PlaceOnLeftSide();
                    }
                    else
                    {
                        var newAlien = BasicAlienFactory.CreateNew(EntityLayer);
                        newAlien.PlaceOnRightSide();
                    }
                }
	        }
	    }

	    private void UpdateGameTime()
	    {
	        var addSeconds = TimeManager.SecondDifference * 480;
	        gameTimeOfDay = gameTimeOfDay.AddSeconds(addSeconds);

	        HorizonBoxInstance.Update(gameTimeOfDay);
        }

	    private void SelectedItemActivity()
	    {
	        if (selectedObject == null)
	        {
	            EnemyInfoInstance.Hide();
	            StructureInfoInstance.Hide();
	            return;
	        }

	        if (selectedObject is BaseStructure)
	        {
                EnemyInfoInstance.Hide();
                StructureInfoInstance.Show((BaseStructure) selectedObject);
	        }
	        if (selectedObject is BaseEnemy)
	        {
                StructureInfoInstance.Hide();
                EnemyInfoInstance.Show((BaseEnemy) selectedObject);
	        }
	    }

	    private void UpdateGameModeActivity()
	    {
            if (!AllStructuresList.Any(s => s is Home)) RestartScreen(false);

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

	    bool _pinching = false;
	    float _pinchInitialDistance;

        private void HandleTouchActivity()
	    {


            while (TouchPanel.IsGestureAvailable)
	        {
	            GestureSample gesture = TouchPanel.ReadGesture();

	            if (gesture.GestureType == GestureType.Pinch)
	            {
	                // current positions
	                Vector2 a = gesture.Position;
	                Vector2 b = gesture.Position2;
	                float dist = Vector2.Distance(a, b);

	                // prior positions
	                Vector2 aOld = gesture.Position - gesture.Delta;
	                Vector2 bOld = gesture.Position2 - gesture.Delta2;
	                float distOld = Vector2.Distance(aOld, bOld);

	                if (!_pinching)
	                {
	                    // start of pinch, record original distance
	                    _pinching = true;
	                    _pinchInitialDistance = distOld;
	                }

	                // work out zoom amount based on pinch distance...
	                float scale = (distOld - dist) * 0.05f;
//	                ZoomBy(scale);
	            }
	            else if (gesture.GestureType == GestureType.PinchComplete)
	            {
	                // end of pinch
	                _pinching = false;
	            }
	        }


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
	                        StructureInfoInstance.Show(structure);
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
	                            EnemyInfoInstance.Show(enemy);
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
