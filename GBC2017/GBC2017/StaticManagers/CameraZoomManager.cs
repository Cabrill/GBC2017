using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall;
using FlatRedBall.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

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
        private static float startX;
        private static float startY;
        private static float zoomTowardsX;
        private static float zoomTowardsY;
        private static float currentZoom;
        private const float maxZoom = 4f;
        private static float currentZoomInMax;
        private static float currentZoomOutMax;


        public static void Initialize()
        {
            OriginalOrthogonalHeight = Camera.Main.OrthogonalHeight;
            OriginalOrthogonalWidth = Camera.Main.OrthogonalWidth;
            minimumHeight = OriginalOrthogonalHeight / maxZoom;
            ZoomFactor = 1f;
        }

        public static void PerformZoom(GestureSample gesture, bool pinchStarted)
        {
            // current positions
            var a = gesture.Position;
            var b = gesture.Position2;
            var dist = Vector2.Distance(a, b);

            // prior positions
            var aOld = gesture.Position - gesture.Delta;
            var bOld = gesture.Position2 - gesture.Delta2;
            var distOld = Vector2.Distance(aOld, bOld);

            // work out zoom amount based on pinch distance...
            var zoomIncrement = (distOld - dist) * 0.0005f;

            if (pinchStarted)
            {
                zoomTowardsX = GuiManager.Cursor.WorldXAt((a.X + b.X) / 2) * GumCoordOffset;
                zoomTowardsY = GuiManager.Cursor.WorldYAt((a.Y + b.Y) / 2) * GumCoordOffset;
                startX = Camera.Main.X;
                startY = Camera.Main.Y;
                currentZoom = zoomIncrement;
                currentZoomInMax = (1 / maxZoom) - ZoomFactor;
                currentZoomOutMax = 1 - ZoomFactor;
            }
            else
            {
                currentZoom += zoomIncrement;
            }

            //Calculate the ZoomFactor after adding in the current zoom
            ZoomFactor = MathHelper.Clamp(ZoomFactor + zoomIncrement, 1f / maxZoom, 1f);

            //Determine effective screen height with the new Zoom
            var newHeight = MathHelper.Clamp(OriginalOrthogonalHeight * ZoomFactor, minimumHeight, OriginalOrthogonalHeight);
            //Modify width to match via aspect ratio of 16:9
            Camera.Main.OrthogonalHeight = newHeight;
            Camera.Main.FixAspectRatioYConstant();

            //Now update the current pinch operation to adjust camera center
            currentZoom = MathHelper.Clamp(currentZoom, currentZoomInMax, currentZoomOutMax);
            var currentZoomRatio = 0f;

            if (currentZoom < 0)
            {
                currentZoomRatio = currentZoom / currentZoomInMax;
            }
            else if (currentZoom > 0)
            {
                currentZoomRatio = currentZoom / currentZoomOutMax;
            }

            var newX = ((startX * (1 - currentZoomRatio)) + (zoomTowardsX * currentZoomRatio));
            var newY = ((startY * (1 - currentZoomRatio)) + (zoomTowardsY * currentZoomRatio));
            var effectiveScreenLimitX = (OriginalOrthogonalWidth - Camera.Main.OrthogonalWidth) / 2;
            var effectiveScreenLimitY = (OriginalOrthogonalHeight - Camera.Main.OrthogonalHeight) / 2;

            newX = MathHelper.Clamp(newX, -effectiveScreenLimitX, effectiveScreenLimitX);
            newY = MathHelper.Clamp(newY, -effectiveScreenLimitY, effectiveScreenLimitY);

            Camera.Main.X = newX;
            Camera.Main.Y = newY;

            //GumIdb.UpdateDisplayToMainFrbCamera();
            //BuildBarInstance.UpdateLayout();
            //HorizonBoxInstance.UpdateLayout();
        }
    }
}