using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Gui;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Factories;
using GBC2017.GumRuntimes;

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
            
            //Combat build buttons
            BuildBarInstance.LaserTurretButtonClick += BuildBarInstanceOnLaserTurretButtonClick;

            //Utility build buttons
        }

        #region Build category buttons

        private void BuildBarInstanceOnEnergyStructureButtonClick(IWindow window)
        {
            BuildBarInstance.CurrentBuildMenuState = BuildBarInstance.CurrentBuildMenuState == BuildBarRuntime.BuildMenu.BuildEnergy ? BuildBarRuntime.BuildMenu.None : BuildBarRuntime.BuildMenu.BuildEnergy;
        }

        private void BuildBarInstanceOnCombatStructureButtonClick(IWindow window)
        {
            BuildBarInstance.CurrentBuildMenuState = BuildBarInstance.CurrentBuildMenuState == BuildBarRuntime.BuildMenu.BuildCombat ? BuildBarRuntime.BuildMenu.None : BuildBarRuntime.BuildMenu.BuildCombat;
        }

        private void BuildBarInstanceOnUtilityStructureButtonClick(IWindow window)
        {
            BuildBarInstance.CurrentBuildMenuState = BuildBarInstance.CurrentBuildMenuState == BuildBarRuntime.BuildMenu.BuildUtility ? BuildBarRuntime.BuildMenu.None : BuildBarRuntime.BuildMenu.BuildUtility;
        }

        #endregion

        #region Energy structure buttons
        private void BuildBarInstanceOnSolarButtonClick(IWindow window)
        {
            CurrentGameMode = GameMode.Building;

            if (ShouldCreateNewBuildRequest<SolarPanels>())
            {
                var newPanel = SolarPanelsFactory.CreateNew(EntityLayer);
                FindValidLocationFor(newPanel);

                //TODO:  message for player if no valid location found
            }
        }
        #endregion

        #region Combat structure buttons

        private void BuildBarInstanceOnLaserTurretButtonClick(IWindow window)
        {
            CurrentGameMode = GameMode.Building;

            if (ShouldCreateNewBuildRequest<LaserTurret>())
            {
                var newTurret = LaserTurretFactory.CreateNew(EntityLayer);
                FindValidLocationFor(newTurret);

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

            int searchIncrement = (int)structure.AxisAlignedRectangleInstance.Width;

            var xMod = 1;
            var yMod = 1;

            var maxXCounter = 0;
            var maxYCounter = 0;

            var xCounter = 0;
            var yCounter = 0;

            var effectiveX = 0;
            var effectiveY = 0;

            //Search the play area, but stop once we gone outside of it
            while (PlayAreaRectangle.IsPointOnOrInside(effectiveX, effectiveY))
            {
                //Set the X/Y values of the structure's collision rectangle
                structure.AxisAlignedRectangleInstance.Position.X = effectiveX;
                structure.AxisAlignedRectangleInstance.Position.Y = effectiveY;

                //If no structures block this location, and no enemies block it, then it's valid
                if (!baseStructures.Any(
                    os => os.AxisAlignedRectangleInstance.CollideAgainst(structure.AxisAlignedRectangleInstance)))
                {
                    if (!AllEnemiesList.Any(e => e.CircleInstance.CollideAgainst(structure.AxisAlignedRectangleInstance)))
                    {
                        structure.Position = structure.AxisAlignedRectangleInstance.Position;
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
                    xMod *= -1;
                    yMod *= -1;

                    xCounter = 0;
                    yCounter = 0;

                    maxXCounter++;

                    //X has more search space than Y, so we have to test if there's still available space to search
                    var proposedY = (maxYCounter+1) * searchIncrement;
                    var proposedNegativeY = -(maxYCounter+1) * searchIncrement;

                    //We've reached the edge of the height of the play area, so don't increment
                    if (!(PlayAreaRectangle.IsPointOnOrInside(0, proposedY) == false ||
                        PlayAreaRectangle.IsPointOnOrInside(0, proposedNegativeY) == false))
                    {
                        maxYCounter++;
                    }
                }

                effectiveX = xCounter * searchIncrement * xMod;
                effectiveY = yCounter * searchIncrement * yMod;
            }

            structure.Destroy();
            return false;
        }

        #endregion
    }
}
