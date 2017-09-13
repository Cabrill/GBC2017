using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;

namespace GBC2017.GumRuntimes
{
    public partial class EnemyInfoRuntime
    {
        public void Show(BaseEnemy enemy)
        {
            Visible = true;
            X = enemy.X;
            Y = enemy.Y + enemy.SpriteInstance.Height / 2;
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
