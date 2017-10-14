using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities;
using GBC2017.Entities.Enemies;
using GBC2017.Entities.GraphicalElements;
using GBC2017.Entities.Projectiles;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.Utility;
using GBC2017.Screens;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.BaseEntities
{
    public partial class BaseEnemyProjectile
    {
        void OnAfterSpeedSet (object sender, EventArgs e)
        {
            if (CurrentState == VariableState.Impact)
            {
                Velocity = Vector3.Zero;
                SpriteInstance.RelativeRotationZ = 0;
                HandleImpact();
            }
        }

    }
}
