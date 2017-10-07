using System;
using System.Collections.Generic;
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

namespace GBC2017.ResourceManagers
{
    public static class MineralsManager
    {
        private const double SecondsBetweenUpdates = 1;

        public static double MineralsIncrease { get; private set; }
        public static double MineralsDecrease { get; private set; }
        public static double StoredMinerals { get; private set; }
        public static double MaxStorage { get; private set; }

        private static double _lastUpdateTime;
        private static double _mineralsDebt;

        private static PositionedObjectList<BaseStructure> _allStructures;
        private static Home _home;

        public static void Initialize(PositionedObjectList<BaseStructure> allStructures)
        {
            _allStructures = allStructures;
            _home = _allStructures.OfType<Home>().FirstOrDefault();
        }

        public static void Update(bool isPregame)
        {
            if (isPregame)
            {
                MaxStorage = _home?.MaxMineralsStorage ?? 0;
                StoredMinerals = _home?.CurrentMinerals ?? 0;
            }
            else if (TimeManager.SecondsSince(_lastUpdateTime) >= SecondsBetweenUpdates)
            {
                MaxStorage = _home?.MaxMineralsStorage ?? 0;
                MineralsDecrease = _mineralsDebt;

                if (_mineralsDebt > 0)
                {
                    var paySuccess = _home?.SubtractMinerals(_mineralsDebt);
                    if (paySuccess.HasValue && paySuccess.Value) _mineralsDebt = 0;
                }

                var mineralsGenerator = _allStructures.Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false &&  s.HasSufficientEnergy && s is BaseMineralsProducer).Cast<BaseMineralsProducer>();
                var mineralsGeneratorArray = mineralsGenerator as BaseMineralsProducer[] ?? mineralsGenerator.ToArray();
                MineralsIncrease = mineralsGeneratorArray.Sum(eg => eg.MineralsProducedPerSecond);
                DepositMinerals(MineralsIncrease);

                //var mineralsRequesters = _allStructures.Where(s => s.IsBeingPlaced == false && s.IsDestroyed == false).Except(mineralsGeneratorArray);
                //var mineralsRequesterArray = mineralsRequesters as BaseStructure[] ?? mineralsRequesters.ToArray();

                StoredMinerals = _home?.CurrentMinerals ?? 0;

                _lastUpdateTime = TimeManager.CurrentTime;
            }
        }


        public static bool CanAfford(double amount)
        {
#if DEBUG
            if (DebugVariables.IgnoreStructureBuildCost) return true;
#endif
            return StoredMinerals + MineralsIncrease - _mineralsDebt >= amount;
        }

        public static bool DebitMinerals(double debitAmount)
        {
#if DEBUG
            if (DebugVariables.IgnoreStructureBuildCost) return true;
#endif
            if (StoredMinerals  >= debitAmount)
            {
                _home.SubtractMinerals(debitAmount);
                return true;
            }
            else if (_mineralsDebt + debitAmount <= MineralsIncrease)
            {
                _mineralsDebt += debitAmount;
                return true;
            }
            return false;
        }

        public static void DepositMinerals(double depositAmount)
        {
            _home.AddMinerals(depositAmount);
        }
    }
}
