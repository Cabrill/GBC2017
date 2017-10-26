using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Audio;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Gui;
using FlatRedBall.Gum;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using FlatRedBall.Math;
using GBC2017.Entities;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Projectiles;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures.Utility;
using GBC2017.Factories;
using GBC2017.GameClasses;
using GBC2017.GameClasses.BaseClasses;
using GBC2017.GameClasses.Cities;
using GBC2017.GameClasses.Levels;
using GBC2017.GumRuntimes;
using GBC2017.Performance;
using GBC2017.ResourceManagers;
using GBC2017.StaticManagers;
using GBC2017.SunMoonCalculations;
using Gum.Converters;
using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using RenderingLibrary;
using RenderingLibrary.Graphics;
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

        private BaseLevel CurrentLevel;
        private DateTime currentLevelDateTime;

        private GameMode CurrentGameMode = GameMode.Normal;
        private bool GameHasStarted;
        private PositionedObject selectedObject;

        private List<ResourceIncreaseNotificationRuntime> resourceIncreaseNotificationList;


        #endregion

        #region Initialization

        void CustomInitialize()
        {
#if WINDOWS || DESKTOP_GL
            FlatRedBallServices.IsWindowsCursorVisible = true;
#endif

            FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;

            resourceIncreaseNotificationList = new List<ResourceIncreaseNotificationRuntime>();

            //TODO:  Set these values by loading a level
            CurrentLevel = new HelsinkiLevel(AllEnemiesList, WorldLayer, new List<AlienShipRuntime>() {AlienShipInstance1, AlienShipInstance2});
            currentLevelDateTime = CurrentLevel.StartTime;
            InsolationFormulas.Instance.SetCityAndDate(CurrentLevel.City, currentLevelDateTime);

            LoadTiledMap();

            InitializeFactories();
            InitializeBaseEntities();

            CameraZoomManager.Initialize();
            AdjustLayerOrthoValues();
            InitializeShaders();
            SetCollisionVisibility();

            SetInfoBarControls();
            SetBuildButtonControls();

            var createAHome = HomeFactory.CreateNew(WorldLayer);

            GetHomeLocationFromTileMap(createAHome);

            InitializeManagers();


            StartButtonInstance.Click += OnStartButtonInstanceClick;
            GameHasStarted = false;
            HorizonBoxInstance.Update(currentLevelDateTime, CurrentLevel.City);
            WindManager.Initialize(CurrentLevel.City, currentLevelDateTime);
            WaterManager.Initialize(CurrentLevel.City);

            CreateNotificationPool();
        }
        
        private void InitializeShaders()
        {
            WorldLayer.RenderTarget = WorldRenderTarget;
            LightLayer.RenderTarget = DarknessRenderTarget;
            BackgroundLayer.RenderTarget = BackgroundRenderTarget;

            ShaderRendererInstance.WorldTexture = WorldRenderTarget;
            ShaderRendererInstance.DarknessTexture = DarknessRenderTarget;
            ShaderRendererInstance.BackgroundTexture = BackgroundRenderTarget;
            //ShaderRendererInstance.Effect = darknessshader;
            ShaderRendererInstance.Viewer = Camera.Main;

            ShaderRendererInstance.InitializeRenderVariables();
        }

        private void AdjustLayerOrthoValues()
        {
            WorldLayer.LayerCameraSettings.OrthogonalWidth = Camera.Main.OrthogonalWidth;
            WorldLayer.LayerCameraSettings.OrthogonalHeight = Camera.Main.OrthogonalHeight;

            LightLayer.LayerCameraSettings.OrthogonalWidth = Camera.Main.OrthogonalWidth;
            LightLayer.LayerCameraSettings.OrthogonalHeight = Camera.Main.OrthogonalHeight;

            ShaderOutputLayer.LayerCameraSettings.OrthogonalWidth = Camera.Main.OrthogonalWidth;
            ShaderOutputLayer.LayerCameraSettings.OrthogonalHeight = Camera.Main.OrthogonalHeight;

            InfoLayer.LayerCameraSettings.OrthogonalWidth = Camera.Main.OrthogonalWidth;
            InfoLayer.LayerCameraSettings.OrthogonalHeight = Camera.Main.OrthogonalHeight;
        }

        private void InitializeManagers()
        {
            EnergyManager.Initialize(AllStructuresList);
            MineralsManager.Initialize(AllStructuresList);
            SunlightManager.Initialize(HorizonBoxInstance);
        }

        private void InitializeBaseEntities()
        {
            var maxY = Camera.Main.OrthogonalHeight * 0.8f - Camera.Main.OrthogonalHeight / 2;
            BaseStructure.Initialize(maxY);
            BaseEnemyProjectile.Initialize(maxY);
            BasePlayerProjectile.Initialize(maxY);
            BaseCombatStructure.Initialize(AllEnemiesList);
            BaseEnemy.Initialize(AlienSpawnLeftRectangle, AlienSpawnRightRectangle, AllStructuresList, maxY);
        }

        private void InitializeFactories()
        {
            HomeFactory.EntitySpawned +=
                home => home.AddSpritesToLayers(LightLayer, InfoLayer);

            LaserTurretProjectileFactory.EntitySpawned +=
                projectile => projectile.AddSpritesToLayers(LightLayer, InfoLayer);

            CannonProjectileFactory.EntitySpawned +=
                projectile => projectile.AddSpritesToLayers(LightLayer, InfoLayer);

            TallLaserProjectileFactory.EntitySpawned +=
                projectile => projectile.AddSpritesToLayers(LightLayer, InfoLayer);

            RangedEnemyProjectileFactory.EntitySpawned +=
                projectile => projectile.AddSpritesToLayers(LightLayer, InfoLayer);

            FlyingEnemyProjectileFactory.EntitySpawned +=
                projectile => projectile.AddSpritesToLayers(LightLayer, InfoLayer);

            LaserTurretFactory.EntitySpawned +=
                turrent => turrent.AddSpritesToLayers(LightLayer, InfoLayer);

            CannonFactory.EntitySpawned +=
                cannon => cannon.AddSpritesToLayers(LightLayer, InfoLayer);

            TallLaserFactory.EntitySpawned +=
                laser => laser.AddSpritesToLayers(LightLayer, InfoLayer);

            ShieldGeneratorFactory.EntitySpawned +=
                shieldgenerator => shieldgenerator.AddSpritesToLayers(LightLayer, InfoLayer);

            SolarPanelsFactory.EntitySpawned +=
                solarpanel => solarpanel.AddSpritesToLayers(LightLayer, InfoLayer);

            WindTurbineFactory.EntitySpawned +=
                windturbine => windturbine.AddSpritesToLayers(LightLayer, InfoLayer);

            HydroGeneratorFactory.EntitySpawned +=
                hydro => hydro.AddSpritesToLayers(LightLayer, InfoLayer);

            BatteryFactory.EntitySpawned +=
                battery => battery.AddSpritesToLayers(LightLayer, InfoLayer);

            CarbonTreeFactory.EntitySpawned +=
                carbontree => carbontree.AddSpritesToLayers(LightLayer, InfoLayer);

            BasicAlienFactory.EntitySpawned +=
                alien =>
                {
                    alien.AddSpritesToLayers(LightLayer, InfoLayer);
                    alien.OnDeath += CreateResourceNotification;
                };

            MeleeAlienFactory.EntitySpawned +=
                alien =>
                {
                    alien.AddSpritesToLayers(LightLayer, InfoLayer);
                    alien.OnDeath += CreateResourceNotification;
                };

            FlyingEnemyFactory.EntitySpawned +=
                alien =>
                {
                    alien.AddSpritesToLayers(LightLayer, InfoLayer);
                    alien.OnDeath += CreateResourceNotification;
                };

            SlimeAlienFactory.EntitySpawned +=
                alien =>
                {
                    alien.AddSpritesToLayers(LightLayer, InfoLayer);
                    alien.OnDeath += CreateResourceNotification;
                };

            SmallSlimeFactory.EntitySpawned +=
                alien =>
                {
                    alien.AddSpritesToLayers(LightLayer, InfoLayer);
                    alien.OnDeath += CreateResourceNotification;
                };
        }

        private void SetCollisionVisibility()
        {
#if DEBUG
            if (DebugVariables.ShowDebugShapes)
            {
                PlayAreaPolygon.Visible = true;
            }
            else
#endif
            {
                PlayAreaPolygon.Visible = false;
            }
        }

        void LoadTiledMap()
        {
            HelsinkiMap.AddToManagers(WorldLayer);
            HelsinkiMap.Z = -10;
            HelsinkiMap.MapLayers[2].RelativeZ = 14;

            PlayAreaPolygon = HelsinkiMap.ShapeCollections[0].Polygons[0];

            AlienSpawnLeftRectangle = HelsinkiMap.ShapeCollections[0].AxisAlignedRectangles[0];
            AlienSpawnRightRectangle = HelsinkiMap.ShapeCollections[0].AxisAlignedRectangles[1];

            PlayAreaPolygon.AttachTo(HelsinkiMap, true);
            AlienSpawnLeftRectangle.AttachTo(HelsinkiMap, true);
            AlienSpawnRightRectangle.AttachTo(HelsinkiMap, true);

            //This centers the map in the middle of the screen
            HelsinkiMap.Position.X = -HelsinkiMap.Width / 2;

            //This positions the map between the info bar and build button bar
            HelsinkiMap.Position.Y = HelsinkiMap.Height / 2;

            HelsinkiMap.RemoveFromManagersOneWay();
            HelsinkiMap.AddToManagers(WorldLayer);

            ShapeManager.AddPolygon(PlayAreaPolygon);
            ShapeManager.AddToLayer(PlayAreaPolygon, HUDLayer);


            //AlienSpawnLeftRectangle.Position = new Vector3(AlienSpawnLeftRectangle.X - HelsinkiMap.Width / 2, AlienSpawnLeftRectangle.Y + HelsinkiMap.Height / 2, AlienSpawnLeftRectangle.Z);
            ShapeManager.AddToLayer(AlienSpawnLeftRectangle, HUDLayer);


            //AlienSpawnRightRectangle.Position = new Vector3(AlienSpawnRightRectangle.X - HelsinkiMap.Width / 2, AlienSpawnRightRectangle.Y + HelsinkiMap.Height / 2, AlienSpawnRightRectangle.Z);
            ShapeManager.AddToLayer(AlienSpawnRightRectangle, HUDLayer);
        }

        #endregion

        #region Activity

        void CustomActivity(bool firstTimeCalled)
        {

#if DEBUG
            HandleDebugInput();
            ShowDebugInfo();
#endif
            UpdateMusic();
            UpdateGameModeActivity();

            HandleTouchActivity();
            SelectedItemActivity();
            BuildingStatusActivity();

            
            var gameplayOccuring = !IsPaused && GameHasStarted;
            if (gameplayOccuring)
            {
                UpdateGameTime();

                CurrentLevel.Update(currentLevelDateTime);

                InsolationFormulas.Instance.UpdateDateTime(currentLevelDateTime);

                HorizonBoxInstance.Update(currentLevelDateTime, CurrentLevel.City);

                WindManager.Update(currentLevelDateTime);
                SunlightManager.UpdateConditions();

                EnemyStatusActivity();
                PlayerProjectileActivity();
                EnemyProjectileActivity();
            }

            EnergyManager.Update(!GameHasStarted);
            MineralsManager.Update(!GameHasStarted);
            InfoBarInstance.Update();
        }

        private void ShowDebugInfo()
        {
            if (DebugVariables.ShowPerformanceStats)
            {
                string allUpdatedInfo = FlatRedBall.Debugging.Debugger.GetAutomaticallyUpdatedObjectInformation();
                string entityBreakdown = FlatRedBall.Debugging.Debugger.GetAutomaticallyUpdatedEntityInformation();
                string shapeBreakdown =
                    FlatRedBall.Debugging.Debugger.GetAutomaticallyUpdatedBreakdownFromList(FlatRedBall.Math.Geometry
                        .ShapeManager.AutomaticallyUpdatedShapes);
                string combinedInfo = $"{allUpdatedInfo}\n{shapeBreakdown}\n{entityBreakdown}";

                FlatRedBall.Debugging.Debugger.TextCorner = FlatRedBall.Debugging.Debugger.Corner.TopLeft;
                FlatRedBall.Debugging.Debugger.Write(combinedInfo);

                FlatRedBall.Debugging.Debugger.TextCorner = FlatRedBall.Debugging.Debugger.Corner.BottomLeft;
                FlatRedBall.Debugging.Debugger.TextRed = 0f;
                FlatRedBall.Debugging.Debugger.TextGreen = 0f;
                FlatRedBall.Debugging.Debugger.TextBlue = 0f;

                FlatRedBall.Graphics.Renderer.RecordRenderBreaks = true;
                var renderBreaks = FlatRedBall.Graphics.Renderer.LastFrameRenderBreakList != null
                    ? FlatRedBall.Graphics.Renderer.LastFrameRenderBreakList.Count
                    : 0;
                FlatRedBall.Debugging.Debugger.Write(renderBreaks);
            }
            FlatRedBall.Debugging.Debugger.Write(GuiManager.Cursor.WindowOver);
        }

        private void UpdateGameTime()
        {
            var addSeconds = TimeManager.SecondDifference * 60 * GameFormulas.RealSecondsPerGameHour;
            currentLevelDateTime = currentLevelDateTime.AddSeconds(addSeconds);
        }

        private void SelectedItemActivity()
        {
            if (selectedObject == null)
            {
                EnemyInfoInstance.Hide();
                StructureInfoInstance.Hide();
                return;
            }

            var objectAsStructure = selectedObject as BaseStructure;
            var objectAsEnemy = selectedObject as BaseEnemy;

            if (objectAsStructure != null)
            {
                EnemyInfoInstance.Hide();
                if (!objectAsStructure.IsDestroyed)
                {
                    StructureInfoInstance.Show((BaseStructure) selectedObject);
                }
                else
                {
                    StructureInfoInstance.Hide();
                }

            }
            else if (objectAsEnemy != null)
            {
                StructureInfoInstance.Hide();
                if (!objectAsEnemy.IsDead)
                {
                    EnemyInfoInstance.Show(objectAsEnemy);
                }
                else
                {
                    EnemyInfoInstance.Hide();
                }
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

            if (GameHasStarted)
            {
                if (CurrentLevel.HasReachedDefeat(currentLevelDateTime) || (!AllStructuresList.Any(s => s is Home)))
                {
                    LevelFailed();
                }
                else if (CurrentLevel.HasReachedVictory(currentLevelDateTime))
                {
                    LevelVictory();
                }
            }
        }

        private void LevelFailed()
        {
            AudioManager.StopSong();
            try
            {
                defeat_sound.Play();
            }
            catch (Exception){};
            ShowGameEndDisplay(playerWon: false);
        }

        private void LevelVictory()
        {
            AudioManager.StopSong();
            try
            {
                victory_sound.Play();
            }
            catch (Exception) { };
            ShowGameEndDisplay(playerWon: true);
        }

        private void ShowGameEndDisplay(bool playerWon)
        {
            BuildBarInstance.Visible = false;
            GameHasStarted = false;
            EndGameDisplayInstance.RetryButtonClick += RestartLevel;
            EndGameDisplayInstance.CurrentVictoryStatusState = playerWon ? EndGameDisplayRuntime.VictoryStatus.Win : EndGameDisplayRuntime.VictoryStatus.Lose;
            EndGameDisplayInstance.Visible = true;
        }

        private void RestartLevel(IWindow window)
        {
            CameraZoomManager.Reset();
            InfoBarInstance.Reset();
            RestartScreen(false);
        }
    

    private void EnemyStatusActivity()
	    {
	        for (var i = AllEnemiesList.Count; i > 0; i--)
	        {
	            var enemy = AllEnemiesList[i-1];
	            if (enemy.IsDead) continue;

	            if (!PlayAreaPolygon.CollideAgainst(enemy.CircleInstance))
	            {
	                enemy.Destroy();
	                enemy = null;
	            }
	            else
	            {
                    //Colide enemies against others
	                for (var j = i- 1; j > 0; j--)
	                {
	                    var otherEnemy = AllEnemiesList[j - 1];

	                    if (otherEnemy.IsDead) continue;

	                    if (enemy.IsFlying == otherEnemy.IsFlying || (enemy.IsJumper && otherEnemy.IsJumper && enemy.Altitude > 0 && otherEnemy.IsJumper && otherEnemy.Altitude > 0))
	                    {
	                        enemy.CircleInstance.CollideAgainstBounce(otherEnemy.CircleInstance, enemy.SpriteInstance.Width, otherEnemy.SpriteInstance.Width,
	                            elasticity: 0.1f);
	                    }
	                }

                    //Collide enemies against buildings
	                if (enemy.IsFlying) continue;

	                for (var j = AllStructuresList.Count(); j > 0; j--)
	                {
	                    var structure = AllStructuresList[j - 1];
	                    if (structure.IsBeingPlaced || structure.IsDestroyed) continue;

	                    enemy.CircleInstance.CollideAgainstBounce(structure.AxisAlignedRectangleInstance, thisMass: 0f, otherMass: 1f,
	                        elasticity: 0.1f);
	                }
	            }
	        }
	    }

	    private void EnemyProjectileActivity()
	    {
	        for (var i = EnemyProjectileList.Count; i > 0; i--)
	        {
	            var projectile = EnemyProjectileList[i - 1];
	            if (projectile.ShouldBeDestroyed ||
	                projectile.CurrentState != BaseEnemyProjectile.VariableState.Flying) continue;

	           for (var e = AllStructuresList.Count; e > 0; e--)
	            {
	                var structure = AllStructuresList[e - 1];
	                if (structure.IsBeingPlaced || structure.IsDestroyed) continue;

	                if (structure is ShieldGenerator)
	                {
	                    var shieldGenerator = structure as ShieldGenerator;

	                    if (shieldGenerator.ShieldIsUp && shieldGenerator.PolygonShieldInstance
	                            .CollideAgainst(projectile.CircleInstance))
	                    {
	                        shieldGenerator.HitShieldWith(projectile);
	                        projectile.HandleImpact();
	                        continue;
	                    }
	                }

	                if (projectile.CircleInstance.CollideAgainst(structure.AxisAlignedRectangleInstance))
	                {
	                    structure.GetHitBy(projectile);
	                    projectile.HandleImpact();
                    }

	            }
	        }
        }

	    private void PlayerProjectileActivity()
	    {
	        for (var i = PlayerProjectileList.Count; i > 0; i--)
	        {
	            var projectile = PlayerProjectileList[i-1];
	            if (projectile.ShouldBeDestroyed) continue;

	            if (projectile.CurrentState != BasePlayerProjectile.VariableState.Flying &&
	                !(projectile is CannonProjectile)) continue;

	            if (projectile.CurrentState == BasePlayerProjectile.VariableState.Impact)
	            {
	                for (var e = AllEnemiesList.Count; e > 0; e--)
	                {
	                    var enemy = AllEnemiesList[e - 1];
	                    if (!projectile.CircleInstance.CollideAgainstBounce(enemy.CircleInstance, 2, 0f, 0f)) continue;

	                    enemy.GetHitBy(projectile);
	                }
                }
	            else
	            {
	                for (var e = AllEnemiesList.Count; e > 0; e--)
	                {
	                    var enemy = AllEnemiesList[e - 1];

	                    if (enemy.IsDead || !projectile.CircleInstance.CollideAgainst(enemy.CircleInstance)) continue;

	                    enemy.GetHitBy(projectile);
	                    projectile.HandleImpact();
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
	            var newLocationIsValid = PlayAreaPolygon.IsPointInside(ref newStructure.Position) && AllStructuresList.All(otherStructure => otherStructure.IsBeingPlaced || 
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
	        //FlatRedBall.Debugging.Debugger.Write(FlatRedBall.Gui.GuiManager.Cursor.WindowOver);

            if (InputManager.Keyboard.KeyPushed(Keys.X))
	        {
	            var newAlien = SmallSlimeFactory.CreateNew(WorldLayer);
                newAlien.PlaceOnRightSide();
	        }

	        if (InputManager.Keyboard.KeyPushed(Keys.Z))
	        {
	            var newAlien = BasicAlienFactory.CreateNew(WorldLayer);
                newAlien.PlaceOnRightSide();
	        }

	        if (InputManager.Keyboard.KeyPushed(Keys.C))
	        {
	            var newAlien = SlimeAlienFactory.CreateNew(WorldLayer);
	            newAlien.PlaceOnRightSide();
	        }

	        if (InputManager.Keyboard.KeyPushed(Keys.V))
	        {
	            var newAlien = FlyingEnemyFactory.CreateNew(WorldLayer);
	            newAlien.PlaceOnRightSide();
	        }

	        if (InputManager.Keyboard.KeyPushed(Keys.B))
	        {
	            var newAlien = MeleeAlienFactory.CreateNew(WorldLayer);
	            newAlien.PlaceOnRightSide();
	        }

        }
        #endif

        private void HandleTouchActivity()
	    {
	        foreach (var gesture in InputManager.TouchScreen.LastFrameGestures)
	        {
	            if (gesture.GestureType == GestureType.Pinch)
	            {
	                CameraZoomManager.PerformZoom(gesture, InputManager.TouchScreen.PinchStarted);
	                Camera.Main.ForceUpdateDependencies();
                    //Update the HorizonBox since the CameraZoomManager doesn't have a reference to it.
                    HorizonBoxInstance.ReactToCameraChange();
	                AdjustLayerOrthoValues();
	            }
                else if (GuiManager.Cursor.ObjectGrabbed == null && !InputManager.TouchScreen.IsPinching && (gesture.GestureType & (GestureType.FreeDrag | GestureType.HorizontalDrag |
                                                 GestureType.VerticalDrag)) > 0)
	            {
	                const int cameraMoveSpeed = 1;
	                var a = gesture.Position;

	                // prior positions
	                var aOld = gesture.Position - gesture.Delta;

	                var newX = Camera.Main.X - ((a.X - aOld.X) * cameraMoveSpeed / CameraZoomManager.GumCoordOffset);
	                var newY = Camera.Main.Y + ((a.Y - aOld.Y) * cameraMoveSpeed /  CameraZoomManager.GumCoordOffset);

	                var effectiveScreenLimitX = (CameraZoomManager.OriginalOrthogonalWidth  - Camera.Main.OrthogonalWidth) / 2;
	                var effectiveScreenLimitY = (CameraZoomManager.OriginalOrthogonalHeight - Camera.Main.OrthogonalHeight) / 2;

                    newX = MathHelper.Clamp(newX, -effectiveScreenLimitX, effectiveScreenLimitX);
	                newY = MathHelper.Clamp(newY, -effectiveScreenLimitY, effectiveScreenLimitY);

	                Camera.Main.X = newX;
	                Camera.Main.Y = newY;

                    //Update the HorizonBox since the CameraZoomManager doesn't have a reference to it.
	                Camera.Main.ForceUpdateDependencies();
                    HorizonBoxInstance.ReactToCameraChange();
                }
            }

	        //User clicked a building button
	        if (GuiManager.Cursor.PrimaryDown && GuiManager.Cursor.WindowPushed != null)
	        {
	            HandleBuildingButton();
	        }


            //User just clicked/touched somewhere, and nothing is currently selected
            if ((GuiManager.Cursor.PrimaryClick || GuiManager.Cursor.PrimaryDown) &&
	            GuiManager.Cursor.ObjectGrabbed == null && GuiManager.Cursor.WindowOver == null)
	        {
	            if (CurrentGameMode == GameMode.Building)
	            {
	                var structureBeingBuilt = AllStructuresList.FirstOrDefault(s => s.IsBeingPlaced);

	                if (structureBeingBuilt != null && GuiManager.Cursor.IsOn3D(structureBeingBuilt.SpriteInstance))
	                {
                        if (!(structureBeingBuilt is HydroGenerator)) GuiManager.Cursor.ObjectGrabbed = structureBeingBuilt;
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
                var shouldAllowDrag = objectAsStructure != null && objectAsStructure.IsBeingPlaced;

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

        #region Other Methods
	    private void CreateResourceNotification(BaseEnemy enemy)
	    {
	        MineralsManager.DepositMinerals(enemy.MineralsRewardedWhenKilled);

	        var notification = resourceIncreaseNotificationList.FirstOrDefault(n => n.Visible == false);

	        if (notification != null)
	        {
	            notification.X = (enemy.X - Camera.Main.X) * CameraZoomManager.GumCoordOffset;
	            notification.Y = (enemy.Y - Camera.Main.Y) * CameraZoomManager.GumCoordOffset;
	            notification.AmountOfIncrease = $"+{enemy.MineralsRewardedWhenKilled}";
                notification.Play();
	        }
        }

        private void CreateNotificationPool()
        {
            for (int i = 0; i < 10; i++)
            {
                var notification = new ResourceIncreaseNotificationRuntime();
                notification.AddToManagers();
                notification.MoveToFrbLayer(HUDLayer,GameScreenGum);
                notification.Visible = false;
                resourceIncreaseNotificationList.Add(notification);
            }
        }
        #endregion

        #region Destroy
        void CustomDestroy()
		{
		    if (resourceIncreaseNotificationList != null)
		    {
		        foreach (var notification in resourceIncreaseNotificationList)
		        {
		            notification.Destroy();
		        }
		    }
            ShapeManager.Remove(PlayAreaPolygon);
		    ShapeManager.Remove(AlienSpawnLeftRectangle);
		    ShapeManager.Remove(AlienSpawnRightRectangle);
		}
#endregion

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        
    }
}
