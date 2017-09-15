using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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

        private static float minimumHeight;

        public static void Initialize()
        {
            OriginalOrthogonalHeight = Camera.Main.OrthogonalHeight;
            minimumHeight = OriginalOrthogonalHeight / 4;
            ZoomFactor = 1f;
        }

        public static void ZoomBy(float scale)
        {
            var newHeight = MathHelper.Clamp(Camera.Main.OrthogonalHeight * scale, minimumHeight, OriginalOrthogonalHeight);


            Camera.Main.OrthogonalHeight = newHeight;
            Camera.Main.FixAspectRatioYConstant();

            ZoomFactor = newHeight / OriginalOrthogonalHeight;

            //GumIdb.UpdateDisplayToMainFrbCamera();
            //BuildBarInstance.UpdateLayout();
            //HorizonBoxInstance.UpdateLayout();
        }
    }
}