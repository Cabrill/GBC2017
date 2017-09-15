using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall;
using FlatRedBall.Utilities;
using GBC2017.StaticManagers;

namespace GBC2017.GumRuntimes
{
    public partial class HorizonBoxRuntime
    {
        private int skyRed;
        private int skyGreen;
        private int skyBlue;

        partial void CustomInitialize()
        {
            var xOffset = FlatRedBallServices.Random.Between(-3, 3);
            var yOffset = Math.Sign(xOffset) == -1
                ? FlatRedBallServices.Random.Between(0, 3)
                : FlatRedBallServices.Random.Between(-3, 0);

            StarrySkySprite.X = 50 + xOffset;
            StarrySkySprite.Y = 50 + yOffset;
            skyRed = SkyRectangle.Red;
            skyGreen = SkyRectangle.Green;
            skyBlue = SkyRectangle.Blue;
        }

        public void Update(DateTime timeOfDay)
        {
            if (SkyRectangle.Alpha < 255)
            {
                StarrySkySprite.Visible = true;
                StarrySkySprite.Rotation = (float) (timeOfDay.TimeOfDay.TotalSeconds%86400) * -0.004167f;
            }
            else
            {
                StarrySkySprite.Visible = false;
            }

            UpdateStarrySky(timeOfDay);
            UpdateSunAndMoon(timeOfDay);
        }

        private void UpdateStarrySky(DateTime timeOfDay)
        {
            //TODO:  Make sunrise/sunset use latitude/longitude of location
            //https://www.esrl.noaa.gov/gmd/grad/solcalc/main.js
            if (timeOfDay.Hour >= 18)
            {
                const int duskStartInMinutes = 22 * 60;
                const int duskLastsInMinutes = 4 * 60;

                var skyOpacity = Math.Max(0, (duskStartInMinutes - timeOfDay.TimeOfDay.TotalMinutes) / duskLastsInMinutes);

                SkyRectangle.Alpha = (int) (255 * skyOpacity);

                SkyRectangle.Red = (int)(skyRed * skyOpacity)/2;
                SkyRectangle.Green = (int)(skyGreen * skyOpacity)/2;
                SkyRectangle.Blue = (int)(skyBlue * skyOpacity)/2;
            }
            else if (timeOfDay.Hour < 6)
            {
                const int dawnStartInMinutes = 4 * 60;
                const int dawnLastsInMinutes = 4 * 60;

                var skyOpacity = Math.Max(0, (timeOfDay.TimeOfDay.TotalMinutes - dawnStartInMinutes) / dawnLastsInMinutes);

                SkyRectangle.Alpha = (int) (255 * skyOpacity);

                SkyRectangle.Red = (int)(skyRed * skyOpacity) / 2;
                SkyRectangle.Green = (int)(skyGreen * skyOpacity) / 2;
                SkyRectangle.Blue = (int)(skyBlue * skyOpacity) / 2;
            }
            else
            {
                const int fullSunTimeInMinutes = 14 * 60;
                const float fullSunWindowInMinutes = 12 * 60;
                var minutesFromFullSun = (float)Math.Abs(timeOfDay.TimeOfDay.TotalMinutes - fullSunTimeInMinutes);

                var percentFullSun = 1-Math.Min(1f, minutesFromFullSun / fullSunWindowInMinutes);

                SkyRectangle.Red = (int) (skyRed * percentFullSun);
                SkyRectangle.Green = (int)(skyGreen * percentFullSun);
                SkyRectangle.Blue = (int)(skyBlue * percentFullSun);

                SkyRectangle.Alpha = (int)(255 * Math.Min(1f,percentFullSun*1.5f));
            }
        }

        private void UpdateSunAndMoon(DateTime timeOfDay)
        {
            const int angleAdjust = 180 * 240;
            var angle = ((timeOfDay.TimeOfDay.TotalMinutes+ angleAdjust)/ 240) %360;

            var radius = CameraZoomManager.OriginalOrthogonalHeight * 0.9f * CameraZoomManager.GumCoordOffset;
            SunSprite.X = (float)Math.Cos(angle) * radius;
            SunSprite.Y = (float)Math.Sin(angle) * radius;

            MoonSprite.X = (float)Math.Cos(angle) * -radius;
            MoonSprite.Y = (float)Math.Sin(angle) * -radius;
        }
    }
}
