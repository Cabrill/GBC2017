using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Math;
using GBC2017.Entities;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities.Structures;
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

        private static Home _home;

        public static void Initialize(PositionedObjectList<BaseStructure> allStructures)
        {
            _allStructures =  allStructures;
            _home = _allStructures.OfType<Home>().FirstOrDefault();
        }

        public static void Update(bool isPregame)
        {
            if (isPregame && _home != null)
            {
                if (_energyBuildDebt > 0)
                {
                    if (_energyBuildDebt <= StoredEnergy)
                    {
                        DrainBatteriesEqually(_energyBuildDebt);
                        _energyBuildDebt = 0;
                    }
                }

                StoredEnergy = BatteryList.Sum(b => b.BatteryLevel) + _home.BatteryLevel;
                MaxStorage = BatteryList.Sum(b => b.InternalBatteryMaxStorage) + _home.InternalBatteryMaxStorage;
            }
            else if (TimeManager.SecondsSince(_lastUpdateTime) >= SecondsBetweenUpdates)
            {
                if (_energyBuildDebt > 0)
                {
                    if (_energyBuildDebt <= StoredEnergy)
                    {
                        DrainBatteriesEqually(_energyBuildDebt);
                        _energyBuildDebt = 0;
                    }
                }

                var energyGenerators = _allStructures.Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false && s is BaseEnergyProducer).Cast<BaseEnergyProducer>();
                var energyGeneratorArray = energyGenerators as BaseEnergyProducer[] ?? energyGenerators.ToArray();
                EnergyIncrease = energyGeneratorArray.Sum(eg => eg.EffectiveEnergyProducedPerSecond);

                var energyRequesters = _allStructures.Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false && !(s is Home)).Except(energyGeneratorArray);
                var energyRequesterArray = energyRequesters as BaseStructure[] ?? energyRequesters.ToArray();
                
                var availableIncrease = EnergyIncrease - _energyBuildDebt;
                _energyBuildDebt = 0;

                var energyRequestSum = energyRequesterArray.Sum(eu => eu.EnergyRequestAmount);

                var energyInStorage = BatteryList.Sum(b => b.BatteryLevel) + _home.BatteryLevel;

                var sufficientEnergyFromIncreaseAlone = availableIncrease >= energyRequestSum;

                if (sufficientEnergyFromIncreaseAlone)
                {
                    foreach (var energyRequester in energyRequesterArray)
                    {
                        energyRequester.ReceiveEnergy(energyRequester.EnergyRequestAmount);
                    }

                    var energySurplus = availableIncrease - energyRequestSum;
                    if (energySurplus > 0)
                    {
                        ChargeBatteriesEqually(energySurplus);
                    }
                    EnergyDecrease = energyRequestSum;
                }
                else //Insufficient energy for all requests, charge non-batteries first, then evalute remainder
                {
                    var nonBatteryRequesters = energyRequesterArray.Where(er => !(er is Home)).Except(BatteryList);
                    var nonBatteryRequesterList = nonBatteryRequesters as BaseStructure[] ?? nonBatteryRequesters.ToArray();
                    var nonBatteryRequestSum = nonBatteryRequesterList.Sum(nbr => nbr.EnergyRequestAmount);
                    EnergyDecrease = nonBatteryRequestSum;

                    var thereIsSufficientEnergyForNonBatteries = availableIncrease + energyInStorage >= nonBatteryRequestSum;

                    var energyDistributed = 0.0;

                    if (thereIsSufficientEnergyForNonBatteries)
                    {
                        foreach (var energyRequester in nonBatteryRequesterList)
                        {
                            var amountToDistribute = energyRequester.EnergyRequestAmount;
                            energyRequester.ReceiveEnergy(amountToDistribute);
                            energyDistributed += amountToDistribute;
                        }
                    }
                    else //Insufficient energy for even the non-batteries, fill the smallest demands first
                    {
                        var energyAvailable = availableIncrease + StoredEnergy;
                        var energyRequestsInAscendingOrder = nonBatteryRequesterList.OrderBy(nbr => nbr.EnergyRequestAmount).ToArray();
                        var numberOfNonBatteryRequests = energyRequestsInAscendingOrder.Count();

                        for (var i = 0; i < numberOfNonBatteryRequests; i++)
                        {
                            var energyRequester = energyRequestsInAscendingOrder[i];
                            var amountToDistribute = Math.Min(energyAvailable / (numberOfNonBatteryRequests-i), energyRequester.EnergyRequestAmount);
                            energyRequester.ReceiveEnergy(amountToDistribute);
                            energyAvailable -= amountToDistribute;
                            energyDistributed += amountToDistribute;
                        }
                    }

                    var energyBalance = availableIncrease - energyDistributed;

                    if (energyBalance < 0)
                    {
                        DrainBatteriesEqually(energyBalance);
                    }
                    else if (energyBalance > 0)
                    {
                        ChargeBatteriesEqually(energyBalance);
                    }
                }

                StoredEnergy = BatteryList.Sum(b => b.BatteryLevel) + _home.BatteryLevel;
                MaxStorage = BatteryList.Sum(b => b.InternalBatteryMaxStorage) + _home.InternalBatteryMaxStorage;

                _lastUpdateTime = TimeManager.CurrentTime;
            }
        }

        private static void ChargeBatteriesEqually(double chargeSum)
        {
            var chargeAvailable = chargeSum;
            var orderedBatteries = BatteryList.Where(b => b.BatteryLevel < MaxStorage).OrderByDescending(b => b.BatteryLevel).ToArray();
            var numberOfBatteries = orderedBatteries.Length;

            for (var i = 0; i < numberOfBatteries; i++)
            {
                var battery = orderedBatteries[i];
                var amountToCharge = chargeSum / (numberOfBatteries - i);
                amountToCharge = Math.Min(battery.EnergyRequestAmount, amountToCharge);

                battery.ReceiveEnergy(amountToCharge);

                chargeAvailable -= amountToCharge;
            }

            if (chargeAvailable > 0)
            {
                _home.ReceiveEnergy(Math.Min(_home.EnergyRequestAmount, chargeAvailable));
            }

#if DEBUG
            if (Math.Abs(chargeAvailable) < 0) throw new InvalidDataException("If this isn't <0 then we somehow spent too much!");
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
                amountToDrain = Math.Min(battery.BatteryLevel, amountToDrain);

                battery.DrainEnergy(amountToDrain);

                energyDrainSum -= amountToDrain;
            }

            if (energyDrainSum > 0)
            {
                var amountToDrain = Math.Min(_home.BatteryLevel, energyDrainSum);
                _home.DrainEnergy(amountToDrain);
                energyDrainSum -= amountToDrain;
            }

#if DEBUG
            if (Math.Abs(energyDrainSum) > 1f) throw new InvalidDataException("If this isn't 0~ then we somehow didn't charge enough!");
#endif
        }

        public static bool CanAfford(double amount)
        {
#if DEBUG
            if (DebugVariables.IgnoreStructureBuildCost) return true;
#endif
            return StoredEnergy + EnergyIncrease - _energyBuildDebt >= amount;
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
            return false;
        }
    }
}
