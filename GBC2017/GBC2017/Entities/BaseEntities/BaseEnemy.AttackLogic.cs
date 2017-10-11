using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.BaseEntities
{
    public partial class BaseEnemy
    {
        #region Propertie and fields

        private bool IsAttacking => CurrentActionState == Action.StartMeleeAttack ||
                                    CurrentActionState == Action.FinishMeleeAttack ||
                                    CurrentActionState == Action.StartRangedAttack ||
                                    CurrentActionState == Action.FinishRangedAttack ||
                                    (CurrentActionState == Action.RangedAim && _currentAttackTarget != null && TargetIsInAttackRange(_currentAttackTarget));


        private BaseStructure _currentAttackTarget;
        private int _lastNumberOfAvailableTargets;

        private double _lastRangeAttackTime;
        private double _lastMeleeAttackTime;

        private bool AttackIsAvailable => IsRangedAttacker
            ? TimeManager.SecondsSince(_lastRangeAttackTime) > SecondsBetweenRangedAttack
            : TimeManager.SecondsSince(_lastMeleeAttackTime) > SecondsBetweenMeleeAttack;
        #endregion

        #region Melee & Ranged Logic

        private void ChooseStructureForAttack()
        {
            var shouldAttackNavigationTarget = _targetStructureForNavigation != null &&
                                               _targetStructureForNavigation.IsDestroyed == false &&
                                               TargetIsInAttackRange(_targetStructureForNavigation);
            if (shouldAttackNavigationTarget)
            {
                _currentAttackTarget =  _targetStructureForNavigation;
            }
            else if (_potentialTargetList != null && _potentialTargetList.Count > 0)
            {
                _currentAttackTarget =
                    _potentialTargetList.Where(pt =>
                            pt.CurrentState == BaseStructure.VariableState.Built &&
                            pt.IsDestroyed == false &&
                            TargetIsInAttackRange(pt))
                        .OrderBy(pt => Vector3.Distance(Position, pt.Position)
                        ).FirstOrDefault();
            }
        }

        private void SharedAttackActivity()
        {
            if (_lastNumberOfAvailableTargets != _currentNumberOfPotentialTargets || (_currentAttackTarget != null && (_currentAttackTarget.IsDestroyed || !TargetIsInAttackRange(_currentAttackTarget))))
            {
                _currentAttackTarget = null;
            }

            if (_currentAttackTarget == null)
            {
                ChooseStructureForAttack();
            }

            var canAttack = !IsHurt &&
                            _currentAttackTarget != null &&
                            AttackIsAvailable &&
                            TargetIsInAttackRange(_currentAttackTarget);

            if (canAttack)
            {
                PerformAttackOnStructure();
            }
        }

        private void PerformAttackOnStructure()
        {
            if (_currentAttackTarget == null) return;

            StopMovement();

            CurrentDirectionState = (Position.X < _currentAttackTarget.Position.X ?
                Direction.MovingRight :
                Direction.MovingLeft);

            if (IsRangedAttacker && CurrentActionState != Action.StartRangedAttack)
            {
                CurrentActionState = Action.StartRangedAttack;
                this.Call(PlayRangedChargeSound).After(0.3f);
            }
            else if (IsMeleeAttacker && CurrentActionState != Action.StartMeleeAttack && CurrentActionState != Action.FinishMeleeAttack)
            {
                CurrentActionState = Action.StartMeleeAttack;
            }
        }

        private bool TargetIsInAttackRange(BaseStructure structure)
        {
            if (IsMeleeAttacker)
            {
                return MeleeAttackRadiusCircleInstance.CollideAgainst(structure.AxisAlignedRectangleInstance);
            }
            else if (IsRangedAttacker)
            {
                return RangedAttackRadiusCircleInstance.CollideAgainst(structure.AxisAlignedRectangleInstance);
            }

            return false;
        }

        private void PlayRangedChargeSound()
        {
            try
            {
                rangedChargeSound.Play();
            }
            catch (Exception){}

        }

        #endregion

        #region Melee logic

        private void MeleeAttackActivity()
        {
            if (!IsJumper || Altitude == 0)
            {
                if (IsHurt && SpriteInstance.JustCycled)
                {
                    CurrentActionState = Action.Standing;
                }
                else if (CurrentActionState == Action.StartMeleeAttack && SpriteInstance.JustCycled)
                {
                    if (_currentAttackTarget != null) DealMeleeDamage();
                    CurrentActionState = Action.FinishMeleeAttack;
                }
                else if (CurrentActionState == Action.FinishMeleeAttack && SpriteInstance.JustCycled)
                {
                    CurrentActionState = Action.Standing;
                }
                else
                {
                    SharedAttackActivity();
                }
            }
        }

        private void DealMeleeDamage()
        {
            _lastMeleeAttackTime = TimeManager.CurrentTime;
            PlayAttackSound();
            _currentAttackTarget?.TakeMeleeDamage(this);
            CurrentActionState = Action.Standing;
        }

        private void PlayAttackSound()
        {
            try
            {
                if (IsMeleeAttacker)
                {
                    meleeAttackSound?.Play();
                }
                else if (IsRangedAttacker)
                {
                    rangedAttackSound?.Play();
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Ranged logic

        private void RangedAttackActivity()
        {
            if (IsHurt && SpriteInstance.JustCycled)
            {
                if (_currentAttackTarget == null || _currentAttackTarget.IsDestroyed)
                {
                    CurrentActionState = Action.RangedAim;
                }
                else
                {
                    CurrentActionState = Action.Standing;
                }
            }
            else if (CurrentActionState == Action.StartRangedAttack && SpriteInstance.JustCycled)
            {
                if (_currentAttackTarget != null) FireProjectile();
                CurrentActionState = Action.FinishRangedAttack;
            }
            else if (CurrentActionState == Action.FinishRangedAttack && SpriteInstance.JustCycled)
            {
                if (_currentAttackTarget == null || _currentAttackTarget.IsDestroyed)
                {
                    CurrentActionState = Action.Standing;
                }
                else
                {
                    CurrentActionState = Action.RangedAim;
                }
            }
            else
            {
                SharedAttackActivity();
            }
        }

        private Vector3 GetProjectilePositioning(float angle)
        {
            var direction = new Vector3(
                (float)-Math.Cos(angle),
                (float)-Math.Sin(angle), 0);
            direction.Normalize();
            return new Vector3(Position.X + 55f * direction.X, Position.Y + 30f + 25f * direction.Y, 0);
        }

        private void FireProjectile()
        {
            var newProjectile = CreateProjectile();
            newProjectile.Position.Y += CircleInstance.Radius * 1.5f;
            newProjectile.DamageInflicted = RangedAttackDamage;
            newProjectile.Speed = ProjectileSpeed;

            var angle = (float)Math.Atan2(Position.Y - _currentAttackTarget.Position.Y, Position.X - _currentAttackTarget.Position.X);
            var direction = new Vector3(
                (float)-Math.Cos(angle),
                (float)-Math.Sin(angle), 0);
            direction.Normalize();

            newProjectile.Position = GetProjectilePositioning(angle);
            newProjectile.Velocity = direction * newProjectile.Speed;
            newProjectile.RotationZ = (float)Math.Atan2(-newProjectile.XVelocity, newProjectile.YVelocity) - MathHelper.ToRadians(90);

            PlayAttackSound();

            _lastRangeAttackTime = TimeManager.CurrentTime;
        }

        #endregion
    }
}
