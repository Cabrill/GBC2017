using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class ResourceBarRuntime
    {
        public void UpdateBar(double fill, double capacity, bool showText = true)
        {
            CapacityText.Visible = showText;
            if (showText)
            {
                CapacityText.Text = CreateStorageString(fill, capacity);
            }

            if (fill < 0.1)
            {
                CurrentFillLevelState = FillLevel.Empty;
            }
            else if (Math.Abs(fill - capacity) < 0.1)
            {
                CurrentFillLevelState = FillLevel.Full;
            }
            else
            {
                CurrentFillLevelState = FillLevel.Partial;

                var energyPct = (fill / capacity) * 100;

                var pctDiffFromBarEnds = FillStartSprite.Width + FillStartSprite.Width - energyPct;

                if (pctDiffFromBarEnds < 0)
                {
                    FillStartSprite.Visible = ShadeStartSprite.Visible = true;
                    FillMiddleSprite.Visible = ShadeMiddleSprite.Visible = true;
                    FillEndSprite.Visible = ShadeEndSprite.Visible = true;

                    FillMiddleSprite.Width = ShadeMiddleSprite.Width = -(float)pctDiffFromBarEnds;
                }
                else if (pctDiffFromBarEnds > 0)
                {//Difference is more than just removing the middle part of the bar, consider hiding the end of the bar
                    var shouldShowBarEnds = pctDiffFromBarEnds < FillEndSprite.Width;

                    FillStartSprite.Visible = ShadeStartSprite.Visible = true;
                    FillMiddleSprite.Visible = ShadeMiddleSprite.Visible = false;
                    FillEndSprite.Visible = ShadeEndSprite.Visible = shouldShowBarEnds;

                    FillMiddleSprite.Width = ShadeMiddleSprite.Width = 0f;
                }
                else //Exactly equal to the width of the start/end sprites
                {
                    FillStartSprite.Visible = ShadeStartSprite.Visible = true;
                    FillMiddleSprite.Visible = ShadeMiddleSprite.Visible = false;
                    FillEndSprite.Visible = ShadeEndSprite.Visible = true;

                    FillMiddleSprite.Width = ShadeMiddleSprite.Width = 0f;
                }

            }
        }

        private static string CreateStorageString(double storedEnergy, double maxStorage)
        {
            var storedText = Math.Round(storedEnergy).ToString();
            var maxText = Math.Round(maxStorage).ToString();

            var storageString = $"{storedText} / {maxText}";

            return storageString;
        }
    }
}
