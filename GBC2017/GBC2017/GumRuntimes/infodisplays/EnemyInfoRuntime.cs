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

            var isEnemyOnScreen = (enemy.X >= Camera.Main.X - Camera.Main.OrthogonalWidth / 2 ||
                                   enemy.X <= Camera.Main.X + Camera.Main.OrthogonalWidth / 2) &&
                                  (enemy.Y >= Camera.Main.Y - Camera.Main.OrthogonalHeight / 2 ||
                                   enemy.Y <= Camera.Main.Y + Camera.Main.OrthogonalHeight / 2);

            Visible = isEnemyOnScreen;

            if (Visible)
            {
                var minMaxX = (Camera.Main.OrthogonalWidth + GetAbsoluteWidth()) / 2 + Camera.Main.X;
                var minMaxY = (Camera.Main.OrthogonalHeight + GetAbsoluteHeight()) / 2 + Camera.Main.Y;
                X = MathHelper.Clamp(enemy.X - Camera.Main.X / CameraZoomManager.GumCoordOffset, -minMaxX, minMaxX);
                Y = MathHelper.Clamp((enemy.Y - Camera.Main.Y + enemy.SpriteInstance.Height / 2) / CameraZoomManager.GumCoordOffset,
                    -minMaxY, minMaxY);

                EnemyName = enemy.DisplayName;
                EnemyHealth = $"{enemy.HealthRemaining} / {enemy.MaximumHealth}";
                EnemyMelee = enemy.MeleeAttackDamage.ToString();
                EnemyRanged = enemy.RangedAttackDamage.ToString();
            }
        }

        public void Hide()
        {
            Visible = false;
        }
    }
}
