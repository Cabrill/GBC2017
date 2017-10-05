﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Graphics;
using GBC2017.GameClasses.BaseClasses;
using GBC2017.GameClasses.Cities;
using GBC2017.GameClasses.Interfaces;

namespace GBC2017.GameClasses.Levels
{
    class HelsinkiLevel : BaseLevel
    {
        public override ICity City => Helsinki.Instance;
        public override DateTime StartTime => new DateTime(2017, 5, 1);
        public override DateTime EndTime => new DateTime(2017, 5, 4);
        public override float AvgDailyEnergyUsage => 5;

        public HelsinkiLevel(Layer layerForEnemies) : base(layerForEnemies)
        {
            
        }

    }
}
