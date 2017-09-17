using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.StaticManagers;

namespace GBC2017.GumRuntimes
{
    public partial class EnemyInfoRuntime
    {
        public void Show(BaseEnemy enemy)
        {
            Visible = true;
            X = enemy.X * CameraZoomManager.GumCoordOffset;
            Y = (enemy.Y + enemy.SpriteInstance.Height / 2) * CameraZoomManager.GumCoordOffset;
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
