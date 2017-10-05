using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.StaticManagers;

namespace GBC2017.GumRuntimes
{
    public partial class ConditionsDisplayRuntime
    {

        public void Update()
        {
            UpdateSunlightDisplay(SunlightManager.SunlightEffectiveness);
            UpdateWindDisplay(WindManager.windSpeed);
        }

        private void UpdateSunlightDisplay(float sunlight)
        {
            if (sunlight >= 0f && sunlight < 0.25f) SunlightConditionInstance.CurrentSunLevelState = SunlightConditionRuntime.SunLevel.Level1;
            else if (sunlight >= 0.25f && sunlight < 0.5f) SunlightConditionInstance.CurrentSunLevelState = SunlightConditionRuntime.SunLevel.Level2;
            else if (sunlight >= 0.50f && sunlight < 0.75f) SunlightConditionInstance.CurrentSunLevelState = SunlightConditionRuntime.SunLevel.Level3;
            else if (sunlight >= 0.75f) SunlightConditionInstance.CurrentSunLevelState = SunlightConditionRuntime.SunLevel.Level4;
        }

        private void UpdateWindDisplay(float windSpeed)
        {
            if (windSpeed >= 0f && windSpeed < 0.25f) WindConditionInstance.CurrentWindLevelState = WindConditionRuntime.WindLevel.Level1;
            else if (windSpeed >= 0.25f && windSpeed < 0.5f) WindConditionInstance.CurrentWindLevelState = WindConditionRuntime.WindLevel.Level2;
            else if (windSpeed >= 0.50f && windSpeed < 0.75f) WindConditionInstance.CurrentWindLevelState = WindConditionRuntime.WindLevel.Level3;
            else if (windSpeed >= 0.75f) WindConditionInstance.CurrentWindLevelState = WindConditionRuntime.WindLevel.Level4;
        }

    }
}
