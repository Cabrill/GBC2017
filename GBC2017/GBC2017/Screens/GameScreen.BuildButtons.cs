using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using GBC2017.GameClasses.Interfaces;
using GBC2017.GumRuntimes;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;

namespace GBC2017.Screens
{
    public partial class GameScreen
    {
        private void HandleBuildingButton()
        {
            var buildButton = GuiManager.Cursor.WindowPushed as IBuildButton;

            if (buildButton != null && buildButton.IsEnabled && GuiManager.Cursor.WindowOver != GuiManager.Cursor.WindowPushed)
            {
                BuildBarInstance.CurrentBuildMenuState = BuildBarRuntime.BuildMenu.None;
                BuildBarInstance.UpdateSelection();

                var existingBuilding = AllStructuresList.FirstOrDefault(s => s.IsBeingPlaced);

                if (existingBuilding != null)
                {
                    if (existingBuilding.GetType() == buildButton.BuildingType)
                    {
                        existingBuilding.Position = new Vector3(GuiManager.Cursor.WorldXAt(existingBuilding.Z), GuiManager.Cursor.WorldYAt(existingBuilding.Z), existingBuilding.Z);
                        GuiManager.Cursor.ObjectGrabbed = existingBuilding;
                        return;
                    }

                    existingBuilding.Destroy();
                }

                var newBuilding = buildButton.BuildingFactory.CreateNew(WorldLayer) as BaseStructure;
                newBuilding.Position = new Vector3(GuiManager.Cursor.WorldXAt(newBuilding.Z), GuiManager.Cursor.WorldYAt(newBuilding.Z), newBuilding.Z);
                GuiManager.Cursor.ObjectGrabbed = newBuilding;
            }
        }

        private void SetBuildButtonControls()
        {
            //Build category buttons
            BuildBarInstance.EnergyStructureButtonClick += BuildBarInstanceOnEnergyStructureButtonClick;
            BuildBarInstance.CombatStructureButtonClick += BuildBarInstanceOnCombatStructureButtonClick;
            BuildBarInstance.UtilityStructureButtonClick += BuildBarInstanceOnUtilityStructureButtonClick;

            SetBuildCosts();
        }
        
        private void SetBuildCosts()
        {
            BaseStructure structure = new SolarPanels();
            BuildBarInstance.UpdateButton(structure, SolarPanelsFactory.Self);
            structure.Destroy();

            structure = new WindTurbine();
            BuildBarInstance.UpdateButton(structure, WindTurbineFactory.Self);
            structure.Destroy();

            structure = new HydroGenerator();
            BuildBarInstance.UpdateButton(structure, HydroGeneratorFactory.Self);
            structure.Destroy();

            structure = new Battery();
            BuildBarInstance.UpdateButton(structure, BatteryFactory.Self);
            structure.Destroy();

            structure = new CarbonTree();
            BuildBarInstance.UpdateButton(structure, CarbonTreeFactory.Self);
            structure.Destroy();

            structure = new LaserTurret();
            BuildBarInstance.UpdateButton(structure, LaserTurretFactory.Self);
            structure.Destroy();

            structure = new Cannon();
            BuildBarInstance.UpdateButton(structure, CannonFactory.Self);
            structure.Destroy();

            structure = new TallLaser();
            BuildBarInstance.UpdateButton(structure, TallLaserFactory.Self);
            structure.Destroy();

            structure = new ShieldGenerator();
            BuildBarInstance.UpdateButton(structure, ShieldGeneratorFactory.Self);
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

        #region Helper Methods

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
        }

        private void CallHydroDestroyed()
        {
            BuildBarInstance.SetHydroIsEnabled(true);
        }
        #endregion
    }
}
