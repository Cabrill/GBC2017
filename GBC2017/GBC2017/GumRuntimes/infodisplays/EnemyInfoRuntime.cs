using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using GBC2017.Entities.BaseEntities;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;

namespace GBC2017.GumRuntimes
{
    public partial class EnemyInfoRuntime
    {
        public void Show(BaseEnemy enemy)
        {
            Visible = true;

            var minMaxX = Camera.Main.OrthogonalWidth / 2 - Camera.Main.X-GetAbsoluteWidth()/2;
            var minMaxY = Camera.Main.OrthogonalHeight / 2 - Camera.Main.Y-GetAbsoluteHeight()/2;
            X = MathHelper.Clamp(enemy.X * CameraZoomManager.GumCoordOffset, -minMaxX, minMaxX);
            Y = MathHelper.Clamp((enemy.Y + enemy.SpriteInstance.Height / 2) * CameraZoomManager.GumCoordOffset, -minMaxY, minMaxY);

            EnemyName = enemy.DisplayName;
            EnemyHealth = $"{enemy.HealthRemaining} / {enemy.MaximumHealth}";
            EnemyMelee = enemy.MeleeAttackDamage.ToString();
            EnemyRanged = enemy.RangedAttackDamage.ToString();
        }

        public void Hide()
        {
            Visible = false;
        }
    }
}
