using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using Microsoft.Xna.Framework;

namespace GBC2017.Entities.BaseEntities
{
    public partial class BaseEnemy
    {
        #region Properties and fields

        private BaseStructure _targetStructureForNavigation;
        private float _lastDistanceToNavigationTarget;

        #endregion

        #region Placement methods
        public void PlaceOnLeftSide()
        {
            X = _playArea.Left;
            Y = FlatRedBallServices.Random.Between(_playArea.Bottom + CircleInstance.Radius, _playArea.Top - CircleInstance.Radius);

            NavigateToTargetStructure();
        }

        public void PlaceOnRightSide()
        {
            X = _playArea.Right;
            Y = FlatRedBallServices.Random.Between(_playArea.Bottom + CircleInstance.Radius, _playArea.Top - CircleInstance.Radius);

            NavigateToTargetStructure();
        }

        #endregion

        #region Navigation methods

        private void NavigationActivity()
        {
            var currentDistanceToTargetNavigation = float.MaxValue;

            if (_targetStructureForNavigation != null && !_targetStructureForNavigation.IsDestroyed)
            {
                currentDistanceToTargetNavigation =
                    Vector3.Distance(Position, _targetStructureForNavigation.Position);
            }

            var shouldUpdateNavigation = (CurrentActionState == Action.Running && currentDistanceToTargetNavigation > _lastDistanceToNavigationTarget) ||
                                          _targetStructureForNavigation == null || 
                                          _targetStructureForNavigation.IsDestroyed || 
                                          !TargetIsInAttackRange(_targetStructureForNavigation);

            if (shouldUpdateNavigation)
            {

                if (_lastNumberOfAvailableTargets != _currentNumberOfPotentialTargets || _targetStructureForNavigation == null || _targetStructureForNavigation.IsDestroyed)
                {
                    ChooseStructureForNavigation();
                }

                if (_targetStructureForNavigation != null)
                {
                    shouldUpdateNavigation = currentDistanceToTargetNavigation > _lastDistanceToNavigationTarget ||
                                             Velocity.Equals(Vector3.Zero);
                    if (shouldUpdateNavigation)
                    {
                        NavigateToTargetStructure();
                    }
                }
            }
            _lastDistanceToNavigationTarget = currentDistanceToTargetNavigation;
        }

        private void ChooseStructureForNavigation()
        {
            if (_potentialTargetList != null && _potentialTargetList.Count > 0)
            {
                _targetStructureForNavigation =
                    _potentialTargetList.Where(pt =>
                            pt.CurrentState == BaseStructure.VariableState.Built &&
                            pt.IsDestroyed == false)
                        .OrderBy(pt => Vector3.Distance(Position, pt.Position)
                        ).FirstOrDefault();
            }
        }

        private void NavigateToTargetStructure()
        {
            if (_targetStructureForNavigation == null || _targetStructureForNavigation.IsDestroyed)
            {
                ChooseStructureForNavigation();
            }
            if (_targetStructureForNavigation != null)
            {
                _lastDistanceToNavigationTarget = Vector3.Distance(Position, _targetStructureForNavigation.Position);

                var angle = (float)Math.Atan2(Y - _targetStructureForNavigation.Position.Y,
                    X - _targetStructureForNavigation.Position.X);
                var direction = new Vector3(
                    (float)-Math.Cos(angle),
                    (float)-Math.Sin(angle), 0);
                direction.Normalize();

                Velocity = direction * Speed;

                CurrentActionState = Action.Running;
                CurrentDirectionState =
                    (Velocity.X > 0 ? Direction.MovingRight : Direction.MovingLeft);
                SpriteInstance.IgnoreAnimationChainTextureFlip = true;
            }
            else
            {
                CurrentActionState = Action.Standing;
            }
        }

        private void StopMovement()
        {
            Velocity = Vector3.Zero;
        }

        #endregion
    }
}
