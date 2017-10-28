using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Graphics;
using GBC2017.Entities.BaseEntities;
using GBC2017.GameClasses.BaseClasses;
using GBC2017.GameClasses.Cities;
using GBC2017.GameClasses.Interfaces;
using GBC2017.GumRuntimes;

namespace GBC2017.GameClasses.Levels
{
    class HelsinkiLevel : BaseLevel
    {
        public override ICity City => Helsinki.Instance;
        public override DateTime StartTime => new DateTime(2017, 6, 21, 5, 0, 0);
        public override DateTime EndTime => new DateTime(2017, 6, 24, 5, 0, 0);
        public override float AvgDailyEnergyUsage => 3.45f;

        public HelsinkiLevel(FlatRedBall.Math.PositionedObjectList<BaseEnemy> enemyList, Layer layerForEnemies, List<AlienShipRuntime> alienShips) : base(enemyList, layerForEnemies, alienShips)
        {
            
        }

    }
}
