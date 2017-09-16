using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall;
using Microsoft.Xna.Framework;

namespace GBC2017.StaticManagers
{
    public static class CameraZoomManager
    {
        /// <summary>
        /// Represents percentage of camera compared to starting size.  0.25f to 1f range.
        /// </summary>
        public static float ZoomFactor { get; private set; }
        public static float GumCoordOffset => 1 / ZoomFactor;
        public static float OriginalOrthogonalHeight { get; private set; }
        public static float OriginalOrthogonalWidth { get; private set; }

        private static float minimumHeight;

        public static void Initialize()
        {
            OriginalOrthogonalHeight = Camera.Main.OrthogonalHeight;
            OriginalOrthogonalWidth = Camera.Main.OrthogonalWidth;
            minimumHeight = OriginalOrthogonalHeight / 4;
            ZoomFactor = 1f;
        }

        public static void PerformZoom(float scale, float towardsX, float towardsY)
        {
            ZoomFactor = MathHelper.Clamp(ZoomFactor - scale, 0.25f, 1f);
            var newHeight = MathHelper.Clamp(Camera.Main.OrthogonalHeight * ZoomFactor, minimumHeight, OriginalOrthogonalHeight);

            Camera.Main.OrthogonalHeight = newHeight;
            Camera.Main.FixAspectRatioYConstant();

            var newWidth = Camera.Main.OrthogonalWidth;

            var newX = towardsX * (1 - ZoomFactor);
            var newY = towardsY * (1 - ZoomFactor);

            newX = MathHelper.Clamp(newX, -newWidth / 2, newWidth / 2);
            newY = MathHelper.Clamp(newY, -newHeight / 2, newHeight / 2);

            Camera.Main.X = newX;
            Camera.Main.Y = newY;

            //GumIdb.UpdateDisplayToMainFrbCamera();
            //BuildBarInstance.UpdateLayout();
            //HorizonBoxInstance.UpdateLayout();
        }
    }
}