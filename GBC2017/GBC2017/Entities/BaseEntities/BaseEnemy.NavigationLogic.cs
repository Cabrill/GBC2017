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

        protected BaseStructure _targetStructureForNavigation;
        private float _lastDistanceToNavigationTarget;
        protected float _currentDistanceToNavigationTarget;

        #endregion

        #region Placement methods
        public void PlaceOnLeftSide()
        {
            X = _leftSpawnArea.Left;
            Y = FlatRedBallServices.Random.Between(_leftSpawnArea.Bottom + CircleInstance.Radius, _leftSpawnArea.Top - CircleInstance.Radius);
            CurrentActionState = Action.Running;
            CurrentDirectionState = Direction.MovingRight;
        }

        public void PlaceOnRightSide()
        {
            X = _rightSpawnArea.Right;
            Y = FlatRedBallServices.Random.Between(_rightSpawnArea.Bottom + CircleInstance.Radius, _rightSpawnArea.Top - CircleInstance.Radius);

            CurrentActionState = Action.Running;
            CurrentDirectionState = Direction.MovingLeft;
        }

        #endregion

        #region Navigation methods

        private void NavigationActivity()
        {
            _currentDistanceToNavigationTarget = float.MaxValue;

            if (_targetStructureForNavigation != null && !_targetStructureForNavigation.IsDestroyed)
            {
                _currentDistanceToNavigationTarget =
                    Vector3.Distance(Position, _targetStructureForNavigation.Position);
            }

            var shouldUpdateNavigation = IsJumper || 
                (CurrentActionState == Action.Running && _currentDistanceToNavigationTarget > _lastDistanceToNavigationTarget) ||                           
                _targetStructureForNavigation == null || 
                _targetStructureForNavigation.IsDestroyed || 
                !TargetIsInAttackRange(_targetStructureForNavigation);

            if (shouldUpdateNavigation)
            {

                if (_lastNumberOfPotentialTargets != _currentNumberOfPotentialTargets || _targetStructureForNavigation == null || _targetStructureForNavigation.IsDestroyed)
                {
                    ChooseStructureForNavigation();
                }

                if (_targetStructureForNavigation != null)
                {
                    shouldUpdateNavigation = IsJumper || _currentDistanceToNavigationTarget > _lastDistanceToNavigationTarget || Velocity.Equals(Vector3.Zero);
                    if (shouldUpdateNavigation)
                    {
                        NavigateToTargetStructure();
                        _currentDistanceToNavigationTarget = Vector3.Distance(Position, _targetStructureForNavigation.Position);
                    }
                }
            }
            _lastDistanceToNavigationTarget = _currentDistanceToNavigationTarget;
        }

        private void ChooseStructureForNavigation()
        {
            if (_potentialTargetList == null || _potentialTargetList.Count <= 0) return;

            var minDistance = float.MaxValue;
            BaseStructure potentialTarget = null;

            foreach (var target in _potentialTargetList)
            {
                if (target.CurrentState != BaseStructure.VariableState.Built || target.IsDestroyed) continue;

                var distanceToTarget = Vector3.Distance(Position, target.Position);

                if (distanceToTarget >= minDistance) continue;
                    
                minDistance = distanceToTarget;
                potentialTarget = target;
            }

            _targetStructureForNavigation = potentialTarget;
        }

        protected virtual void NavigateToTargetStructure()
        {
            if (_targetStructureForNavigation != null)
            {
                var angle = (float)Math.Atan2(Y - _targetStructureForNavigation.Position.Y,
                    X - _targetStructureForNavigation.Position.X);
                var direction = new Vector3(
                    (float)-Math.Cos(angle),
                    (float)-Math.Sin(angle), 0);
                direction.Normalize();

                Velocity = direction * Speed * _currentScale;

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

        protected float CalculateJumpAltitudeVelocity()
        {
            if (_targetStructureForNavigation == null) return 0f;

            var targetPosition = _targetStructureForNavigation.AxisAlignedRectangleInstance.Position;
            var targetDistance = Vector3.Distance(CircleInstance.Position, targetPosition);
            var timeToTravel = targetDistance / Speed;

            var altitudeVelocity = (0.5f * (GravityDrag * (timeToTravel * timeToTravel))) /
                                   -timeToTravel;

            return altitudeVelocity;
        }
        #endregion
    }
}
