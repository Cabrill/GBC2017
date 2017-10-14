using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Gui;
using FlatRedBall.Math.Geometry;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures.Utility;
using GBC2017.Factories;
using GBC2017.GumRuntimes;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;

namespace GBC2017.Screens
{
    public partial class GameScreen
    {
        private void SetBuildButtonControls()
        {
            //Build category buttons
            BuildBarInstance.EnergyStructureButtonClick += BuildBarInstanceOnEnergyStructureButtonClick;
            BuildBarInstance.CombatStructureButtonClick += BuildBarInstanceOnCombatStructureButtonClick;
            BuildBarInstance.UtilityStructureButtonClick += BuildBarInstanceOnUtilityStructureButtonClick;
            
            //Energy build buttons
            BuildBarInstance.SolarButtonClick += BuildBarInstanceOnSolarButtonClick;
            BuildBarInstance.WindButtonClick += BuildBarInstanceOnWindButtonClick;
            BuildBarInstance.HydroButtonClick += BuildBarInstanceOnHydroButtonClick;
            
            //Combat build buttons
            BuildBarInstance.LaserTurretButtonClick += BuildBarInstanceOnLaserTurretButtonClick;
            BuildBarInstance.CannonButtonClick += BuildBarInstanceOnCannonButtonClick;
            BuildBarInstance.TallLaserButtonClick += BuildBarInstanceOnTallLaserButtonClick;

            //Utility build buttons
            BuildBarInstance.BatteryButtonClick += BuildBarInstanceOnBatteryButtonClick;
            BuildBarInstance.CarbonTreeButtonClick += BuildBarInstanceOnCarbonTreeButtonClick;
            BuildBarInstance.ShieldGeneratorButtonClick += BuildBarInstanceOnShieldGeneratorButtonClick;

            SetBuildCosts();
        }

        private void SetBuildCosts()
        {
            BaseStructure structure = new SolarPanels();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new WindTurbine();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new HydroGenerator();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new Battery();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new CarbonTree();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new LaserTurret();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new Cannon();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new TallLaser();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();

            structure = new ShieldGenerator();
            BuildBarInstance.UpdateButton(structure);
            structure.Destroy();
        }

        #region Build category buttons

        private void BuildBarInstanceOnEnergyStructureButtonClick(IWindow window)
        {
            BuildBarInstance.CurrentBuildMenuState = BuildBarInstance.CurrentBuildMenuState == BuildBarRuntime.BuildMenu.BuildEnergy ? BuildBarRuntime.BuildMenu.None : BuildBarRuntime.BuildMenu.BuildEnergy;
            BuildBarInstance.UpdateSelection();
        }

        private void BuildBarInstanceOnCombatStructureButtonClick(IWindow window)
        {
            BuildBarInstance.CurrentBuildMenuState = BuildBarInstance.CurrentBuildMenuState == BuildBarRuntime.BuildMenu.BuildCombat ? BuildBarRuntime.BuildMenu.None : BuildBarRuntime.BuildMenu.BuildCombat;
            BuildBarInstance.UpdateSelection();
        }

        private void BuildBarInstanceOnUtilityStructureButtonClick(IWindow window)
        {
            BuildBarInstance.CurrentBuildMenuState = BuildBarInstance.CurrentBuildMenuState == BuildBarRuntime.BuildMenu.BuildUtility ? BuildBarRuntime.BuildMenu.None : BuildBarRuntime.BuildMenu.BuildUtility;
            BuildBarInstance.UpdateSelection();
        }

        #endregion

        #region Energy structure buttons
        private void BuildBarInstanceOnSolarButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<SolarPanels>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newPanel = SolarPanelsFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newPanel);

                //TODO:  message for player if no valid location found
            }
        }

        private void BuildBarInstanceOnWindButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<WindTurbine>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newTurbine = WindTurbineFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newTurbine);

                //TODO:  message for player if no valid location found
            }
        }

        private void BuildBarInstanceOnHydroButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<HydroGenerator>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newHydro = HydroGeneratorFactory.CreateNew(WorldLayer);
                GetHydroGeneratorLocationFromTileMap(newHydro);

                //TODO:  message for player if no valid location found
            }
        }
        #endregion

        #region Combat structure buttons

        private void BuildBarInstanceOnLaserTurretButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<LaserTurret>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newTurret = LaserTurretFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newTurret);

                //TODO:  message for player if no valid location found
            }
        }

        private void BuildBarInstanceOnTallLaserButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<LaserTurret>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newTurret = TallLaserFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newTurret);

                //TODO:  message for player if no valid location found
            }
        }

        private void BuildBarInstanceOnCannonButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<LaserTurret>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newTurret = CannonFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newTurret);

                //TODO:  message for player if no valid location found
            }
        }

        #endregion

        #region Utility structure buttons
        private void BuildBarInstanceOnBatteryButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<Battery>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newBattery = BatteryFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newBattery);

                //TODO:  message for player if no valid location found
            }
        }


        private void BuildBarInstanceOnCarbonTreeButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<CarbonTree>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newCarbonTree = CarbonTreeFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newCarbonTree);

                //TODO:  message for player if no valid location found
            }
        }

        private void BuildBarInstanceOnShieldGeneratorButtonClick(IWindow window)
        {
            if (ShouldCreateNewBuildRequest<ShieldGenerator>())
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var newShieldGenerator = ShieldGeneratorFactory.CreateNew(WorldLayer);
                FindValidLocationFor(newShieldGenerator);

                //TODO:  message for player if no valid location found
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks to see if the user is already building something, and if it's a different type than what is being requested
        /// we cancel that request.  If it's the same type as being requested, we keep the existing build request.
        /// </summary>
        /// <typeparam name="TStructure">Structure type of new request</typeparam>
        /// <returns>If the caller should create a new build command</returns>
        private bool ShouldCreateNewBuildRequest<TStructure>()
        {
            var existingBuildCommand = AllStructuresList.FirstOrDefault(s => s.IsBeingPlaced);

            if (existingBuildCommand == null)
            {
                return true;
            }
            else if (existingBuildCommand is TStructure)
            {
                return false;
            }
            else
            {
                existingBuildCommand.Destroy();
                return true;
            }
        }

        private void GetHomeLocationFromTileMap(Home home)
        {
            var shapeCollections = HelsinkiMap.ShapeCollections.Where(sc => sc.Name == "BuildingSpawnPoints").FirstOrDefault();
            var locationRectangle = shapeCollections?.AxisAlignedRectangles.Where(aar => aar.Name == "HomeSpawnPoint").FirstOrDefault();
            var locationPosition = locationRectangle.Position + HelsinkiMap.Position;

            home.Position = locationPosition;
            home.IsValidLocation = true;
            home.CurrentState = BaseStructure.VariableState.Built;
            home.IsBeingPlaced = false;
        }

        private void GetHydroGeneratorLocationFromTileMap(HydroGenerator hydro)
        {
            var shapeCollections = HelsinkiMap.ShapeCollections.Where(sc => sc.Name == "BuildingSpawnPoints").FirstOrDefault();
            var locationRectangle = shapeCollections?.AxisAlignedRectangles.Where(aar => aar.Name == "HydroSpawnPoint").FirstOrDefault();
            var locationPosition = locationRectangle.Position + HelsinkiMap.Position;

            hydro.Position = locationPosition;
            hydro.IsValidLocation = true;
            hydro.OnBuild = CallHydroBuilt;
            hydro.OnDestroy = CallHydroDestroyed;
        }

        private void CallHydroBuilt()
        {
            BuildBarInstance.SetHydroIsEnabled(false);
            BuildBarInstance.HydroButtonClick -= BuildBarInstanceOnHydroButtonClick;
        }

        private void CallHydroDestroyed()
        {
            BuildBarInstance.SetHydroIsEnabled(true);
            BuildBarInstance.HydroButtonClick += BuildBarInstanceOnHydroButtonClick;
        }

        /// <summary>
        /// Starts in the center, and circles outward looking for a valid location for the building
        /// </summary>
        /// <param name="structure">The structure to be placed</param>
        /// <returns>Whether a valid location was found</returns>
        private bool FindValidLocationFor(BaseStructure structure)
        {
            var otherStructures = AllStructuresList.Where(s => s.IsBeingPlaced == false);
            var baseStructures = otherStructures as BaseStructure[] ?? otherStructures.ToArray();

            //Can't conflict if there are no other buildings
            if (baseStructures.Any() == false)
            {
                structure.IsValidLocation = true;
                return true;
            }

            int searchIncrement = (int)Math.Min(structure.AxisAlignedRectangleInstance.Width, structure.AxisAlignedRectangleInstance.Height);

            var xMod = 1;
            var yMod = 1;

            var maxXCounter = 0;
            var maxYCounter = 0;

            var xCounter = 0;
            var yCounter = 0;

            var baseX = Camera.Main.X;
            var baseY = Camera.Main.Y;

            var effectiveX = baseX;
            var effectiveY = baseY;

            //Search the play area, but stop once we gone outside of it
            while (PlayAreaPolygon.IsPointInside(effectiveX, effectiveY))
            {
                //Set the X/Y values of the structure's collision rectangle
                structure.Position = new Vector3(effectiveX, effectiveY, structure.Z);
                structure.AxisAlignedRectangleInstance.ForceUpdateDependencies();

                //If no structures block this location, and no enemies block it, then it's valid
                if (!baseStructures.Any(
                    os => os.CollideAgainst(structure)))
                {
                    if (!AllEnemiesList.Any(e => e.CollideAgainst(structure)))
                    {
                        structure.IsValidLocation = true;
                        return true;
                    }
                }

                //Try the next highest X value
                if (xCounter < maxXCounter)
                {
                    xCounter++;
                }
                //Try the next highest Y value
                else if (yCounter < maxYCounter)
                {
                    xCounter = 0;
                    yCounter++;
                }
                //Tried all the +x/+y values, try -x/+y
                else if (xMod == 1 && yMod == 1)
                {
                    xMod *= -1;
                    xCounter = 0;
                    yCounter = 0;
                }
                //Tried all the -x/+y values, try +x/-y
                else if (xMod == -1 && yMod == 1)
                {
                    xMod *= -1;
                    yMod *= -1;
                    xCounter = 0;
                    yCounter = 0;
                }
                //Tried all the +x/-y values, try -x/-y
                else if (xMod == 1 && yMod == -1)
                {
                    xMod *= -1;
                    xCounter = 0;
                    yCounter = 0;
                }
                //Tried all the -x/-y values, move up to higher values of +x/+y
                else if (xMod == -1 && yMod == -1)
                {
                    var incrementMade = false;
                    xMod *= -1;
                    yMod *= -1;

                    xCounter = 0;
                    yCounter = 0;

                    var proposedMaxX = baseX + (maxXCounter + 1) * searchIncrement;

                    //We've reached the edge of the height of the play area, so don't increment
                    if (PlayAreaPolygon.IsPointInside(proposedMaxX, 0) || PlayAreaPolygon.IsPointInside(-proposedMaxX, 0))
                    {
                        maxXCounter++;
                        incrementMade = true;
                    }

                    var proposedMaxY = baseY + (maxYCounter+1) * searchIncrement;

                    //We've reached the edge of the height of the play area, so don't increment
                    if (PlayAreaPolygon.IsPointInside(0, proposedMaxY) || PlayAreaPolygon.IsPointInside(0, -proposedMaxY))
                    {
                        maxYCounter++;
                        incrementMade = true;
                    }
                    if (!incrementMade)
                    {
                        break;
                    }
                }

                //Restrain to the play rectangle, while still searching everywhere
                var proposedEffectiveX = baseX + xCounter * searchIncrement * xMod;
                var proposedEffectiveY = baseY + yCounter * searchIncrement * yMod;

                if (PlayAreaPolygon.IsPointInside(proposedEffectiveX, proposedEffectiveY))
                {
                    effectiveX = proposedEffectiveX;
                    effectiveY = proposedEffectiveY;
                }
                else if (PlayAreaPolygon.IsPointInside(proposedEffectiveX, effectiveY))
                {
                    effectiveX = proposedEffectiveX;
                }
                else if (PlayAreaPolygon.IsPointInside(effectiveX, proposedEffectiveY))
                {
                    effectiveY = proposedEffectiveY;
                }
                else
                {
                    break;
                }
            }

            structure.IsValidLocation = true;
            structure.Position = new Vector3(Camera.Main.X, Camera.Main.Y, structure.Z);
            return true;;
        }

        #endregion
    }
}
