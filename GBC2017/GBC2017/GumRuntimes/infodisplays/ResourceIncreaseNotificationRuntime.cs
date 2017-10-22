using FlatRedBall.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;

namespace GBC2017.GumRuntimes
{
    public partial class ResourceIncreaseNotificationRuntime
    {
        FlatRedBall.Gum.Animation.GumAnimation _floatUpAnimation;

        IEnumerable<Instruction> GetMoveInstructions(object target)
        {
            {
                var toReturn = new DelegateInstruction(() => InterpolateToRelative(
                    FloatAnimationStates.FloatAnimationEnd, 1,
                    FlatRedBall.Glue.StateInterpolation.InterpolationType.Linear,
                    FlatRedBall.Glue.StateInterpolation.Easing.Out, _floatUpAnimation))
                {
                    Target = target,
                    TimeToExecute = TimeManager.CurrentTime + 0
                };
                yield return toReturn;
            }
            {
                var toReturn = new DelegateInstruction(() => Visible = false)
                {
                    Target = target,
                    TimeToExecute = TimeManager.CurrentTime + 1
                };
                yield return toReturn;
            }
        }

        partial void CustomInitialize()
        {
            _floatUpAnimation = new FlatRedBall.Gum.Animation.GumAnimation(5, GetMoveInstructions);
        }

        public void Play()
        {
            CurrentFloatAnimationStatesState = FloatAnimationStates.FloatAnimationStart;
            Visible = true;
            _floatUpAnimation.Play(this);
        }
    }
}
