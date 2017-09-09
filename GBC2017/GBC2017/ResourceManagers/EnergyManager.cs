using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Math;
using GBC2017.Entities;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures.Utility;
using RenderingLibrary;

namespace GBC2017.ResourceManagers
{
    public static class EnergyManager
    {
        private const double SecondsBetweenUpdates = 1;

        public static double EnergyIncrease { get; private set; }
        public static double EnergyDecrease { get; private set; }
        public static double StoredEnergy { get; private set; }
        public static double MaxStorage { get; private set; }

        private static double _lastUpdateTime;
        private static double _energyBuildDebt;

        private static PositionedObjectList<BaseStructure> _allStructures;
        private static IEnumerable<Battery> BatteryList => _allStructures.OfType<Battery>()
            .Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false);

        public static void Initialize(PositionedObjectList<BaseStructure> allStructures)
        {
            _allStructures =  allStructures;
        }

        public static void Update()
        {
            if (TimeManager.SecondsSince(_lastUpdateTime) >= SecondsBetweenUpdates)
            {
                var energyGenerators = _allStructures.Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false && s is BaseEnergyProducer).Cast<BaseEnergyProducer>();
                var energyGeneratorArray = energyGenerators as BaseEnergyProducer[] ?? energyGenerators.ToArray();
                EnergyIncrease = energyGeneratorArray.Sum(eg => eg.EnergyProducedPerSecond);

                var energyRequesters = _allStructures.Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false).Except(energyGeneratorArray);
                var energyRequesterArray = energyRequesters as BaseStructure[] ?? energyRequesters.ToArray();

                
                var availableEnergy = EnergyIncrease - _energyBuildDebt;
                _energyBuildDebt = 0;

                var energyRequestSum = energyRequesterArray.Sum(eu => eu.EnergyRequestAmount);

                var energyInStorage = StoredEnergy;

                var sufficientEnergyFromIncreaseAlone = availableEnergy >= energyRequestSum;

                if (sufficientEnergyFromIncreaseAlone)
                {
                    foreach (var energyRequester in energyRequesterArray)
                    {
                        energyRequester.ReceiveEnergy(energyRequester.EnergyRequestAmount);
                    }

                    var energySurplus = availableEnergy - energyRequestSum;
                    if (energySurplus > 0)
                    {
                        ChargeBatteriesEqually(energySurplus);
                    }
                    EnergyDecrease = energyRequestSum;
                }
                else //Insufficient energy for all requests, charge non-batteries first, then evalute remainder
                {
                    var nonBatteryRequesters = energyRequesterArray.Except(BatteryList);
                    var nonBatteryRequesterList = nonBatteryRequesters as BaseStructure[] ?? nonBatteryRequesters.ToArray();
                    var nonBatteryRequestSum = nonBatteryRequesterList.Sum(nbr => nbr.EnergyRequestAmount);
                    EnergyDecrease = nonBatteryRequestSum;

                    var thereIsSufficientEnergyForNonBatteries = availableEnergy + energyInStorage >= nonBatteryRequestSum;

                    var energyDistributed = 0.0;

                    if (thereIsSufficientEnergyForNonBatteries)
                    {
                        foreach (var energyRequester in nonBatteryRequesterList)
                        {
                            energyRequester.ReceiveEnergy(energyRequester.EnergyRequestAmount);
                            energyDistributed += energyRequester.EnergyRequestAmount;
                        }
                    }
                    else //Insufficient energy for even the non-batteries, fill the smallest demands first
                    {
                        var energyAvailable = availableEnergy + StoredEnergy;
                        var energyRequestsInAscendingOrder = nonBatteryRequesterList.OrderBy(nbr => nbr.EnergyRequestAmount);
                        var numberOfNonBatteryRequests = energyRequestsInAscendingOrder.Count();

                        foreach (var energyRequester in energyRequestsInAscendingOrder)
                        {
                            if (energyAvailable <= 0) break;

                            var amountToDistribute = Math.Min(energyAvailable / numberOfNonBatteryRequests, energyRequester.EnergyRequestAmount);
                            energyRequester.ReceiveEnergy(amountToDistribute);
                            energyAvailable -= amountToDistribute;
                            energyDistributed += amountToDistribute;
                        }
                    }

                    var energyBalance = availableEnergy - energyDistributed;

                    if (energyBalance < 0)
                    {
                        DrainBatteriesEqually(energyBalance);
                    }
                    else if (energyBalance > 0)
                    {
                        ChargeBatteriesEqually(energyBalance);
                    }
                }

                StoredEnergy = BatteryList.Sum(b => b.BatteryLevel);
                MaxStorage = BatteryList.Sum(b => b.InternalBatteryMaxStorage);

                _lastUpdateTime = TimeManager.CurrentTime;
            }
        }

        private static void ChargeBatteriesEqually(double chargeSum)
        {
            var orderedBatteries = BatteryList.Where(b => b.BatteryLevel > 0).OrderByDescending(b => b.BatteryLevel).ToArray();
            var numberOfBatteries = orderedBatteries.Length;

            for (var i = 0; i < numberOfBatteries; i++)
            {
                var battery = orderedBatteries[i];
                var amountToCharge = chargeSum / (numberOfBatteries - i);
                amountToCharge = Math.Min(battery.EnergyRequestAmount, amountToCharge);

                battery.DrainEnergy(amountToCharge);

                chargeSum -= amountToCharge;
            }

#if DEBUG
            //If this isn't 0+ then we somehow spent too much!
            Debug.Assert(Math.Abs(chargeSum) > -1);
#endif
        }

        /// <summary>
        /// Call to drain from all batteries equally.  Value passed must not be greater than the charge of all batteries!
        /// </summary>
        /// <param name="drainSum">The sum to be drained from all batteries</param>
        private static void DrainBatteriesEqually(double drainSum)
        {
            var orderedBatteries = BatteryList.Where(b => b.BatteryLevel > 0).OrderBy(b => b.BatteryLevel).ToArray();
            var numberOfBatteries = orderedBatteries.Length;
            var energyDrainSum = Math.Abs(drainSum);

            for (var i = 0; i < numberOfBatteries; i++)
            {
                var battery = orderedBatteries[i];
                var amountToDrain = energyDrainSum / (numberOfBatteries - i);
                amountToDrain = Math.Max(battery.BatteryLevel, amountToDrain);

                battery.DrainEnergy(amountToDrain);

                energyDrainSum -= amountToDrain;
            }

            #if DEBUG
            //If this isn't 0~ then we somehow didn't charge enough!
            Debug.Assert(Math.Abs(energyDrainSum) < 1);
            #endif
        }

        public static bool DebitEnergyForBuildRequest(double energyBuildCost)
        {
#if DEBUG
            if (DebugVariables.IgnoreStructureBuildCost) return true;
#endif
            if (StoredEnergy >= energyBuildCost)
            {
                DrainBatteriesEqually(energyBuildCost);
                return true;
            }
            else if (_energyBuildDebt + energyBuildCost <= EnergyIncrease)
            {
                _energyBuildDebt += energyBuildCost;
                return true;
            }
        }
    }
}
