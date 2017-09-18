using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Glue.StateInterpolation;
using StateInterpolationPlugin;

namespace GBC2017.StaticManagers
{
    public static class WindManager
    {
        public static float WindEffectiveness { get; private set; } = 1f;

        private const int _secondsBetweenUpdates = 10;
        private const float _maxTimeToShiftSpeed = 5f;
        private static double _timeOfLastUpdate;

        public static void Update()
        {
            if (TimeManager.SecondsSince(_timeOfLastUpdate) >= _secondsBetweenUpdates)
            {
                var oldWindSpeed = WindEffectiveness;
                var newWindSpeed = FlatRedBallServices.Random.Between(0, 1);
                var shiftTime = FlatRedBallServices.Random.Between(1, _maxTimeToShiftSpeed);

                var windSpeedTweener = new Tweener();
                windSpeedTweener = new Tweener(0, 1, shiftTime, InterpolationType.Bounce, Easing.InOut);
                windSpeedTweener.PositionChanged += (a) =>
                {
                    WindEffectiveness = ((1-a) * oldWindSpeed) + (a * newWindSpeed);
                };
                windSpeedTweener.Start();
                TweenerManager.Self.Add(windSpeedTweener);
            }

        }
    }
}
