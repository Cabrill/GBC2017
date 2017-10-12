using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Glue.StateInterpolation;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StateInterpolationPlugin;

namespace GBC2017.Entities
{
    public partial class ShaderRenderer
    {
        #region Fields/Properties

        SpriteBatch spriteBatch;

        public Effect Effect { get; set; }

        public Texture2D WorldTexture { get; set; }
        public Texture2D DarknessTexture { get; set; }
        public Texture2D BackgroundTexture { get; set; }
        public Texture2D BlowoutTexture { get; set; }

        public float DarknessAlpha { get; set; }

        public PositionedObject Viewer { get; set; }

        private Color WorldColor = Color.White;
        private Tweener _lastColorTween;
        private Color DayColor = Color.White;
        private Color NightColor = Color.SkyBlue;
        private Vector3 DayColorAsVector;
        private Vector3 NightColorAsVector;


        #endregion

        #region Initialize
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
        {
            spriteBatch = new SpriteBatch(FlatRedBallServices.GraphicsDevice);
            DayColorAsVector = DayColor.ToVector3();
            NightColorAsVector = NightColor.ToVector3();
        }

        #endregion

        private void CustomActivity()
        {

            //if (InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Q))
            //    DisplacementStart--;
            //if (InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.E))
            //    DisplacementStart++;
            //Effect.Parameters["DisplacementStart"].SetValue(DisplacementStart);
        }

        private void CustomDestroy()
        {
            SpriteManager.RemoveDrawableBatch(this);
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
        public void InitializeRenderVariables()
        {
            //Effect.Parameters["BlurStrength"].SetValue(BlurStrength);
            //Effect.Parameters["DisplacementStart"].SetValue(DisplacementStart);
        }

        public void Draw(Camera camera)
        {
            //DrawWorld(camera);

            DrawDarknessToRenderTarget(camera);
            DrawToScreen(camera);
        }

        private void UpdateWorldColor()
        {
            const float secondsToTransitionColor = 5f;
            if (_lastColorTween != null && _lastColorTween.Running) return;

            if (WorldColor == DayColor && !SunlightManager.SunIsUp && SunlightManager.MoonIsUp)
            {
                _lastColorTween =
                    new Tweener(1, 0, secondsToTransitionColor, InterpolationType.Linear, Easing.InOut)
                    {
                        PositionChanged = HandleColorPositionChanged
                    };

                _lastColorTween.Ended += () =>
                {
                    WorldColor = NightColor;
                    _lastColorTween.Stop();
                };

                _lastColorTween.Owner = this;

                TweenerManager.Self.Add(_lastColorTween);
                _lastColorTween.Start();
            }
            else if (WorldColor == NightColor && SunlightManager.SunIsUp)
            {
                _lastColorTween =
                    new Tweener(0, 1, secondsToTransitionColor, InterpolationType.Linear, Easing.InOut)
                    {
                        PositionChanged = HandleColorPositionChanged
                    };

                _lastColorTween.Ended += () =>
                {
                    WorldColor = DayColor;
                    _lastColorTween.Stop();
                };

                _lastColorTween.Owner = this;

                TweenerManager.Self.Add(_lastColorTween);
                _lastColorTween.Start();
            }
        }

        private void HandleColorPositionChanged(float newposition)
        {
            WorldColor = new Color(DayColorAsVector * newposition + NightColorAsVector * (1 - newposition));
        }

        private void DrawToScreen(Camera camera)
        {
            var destinationRectangle = camera.DestinationRectangle;
            if (global::RenderingLibrary.Graphics.Renderer.SubtractViewportYForMonoGameGlBug)
            {
                destinationRectangle.Y -= RenderingLibrary.SystemManagers.Default.Renderer.GraphicsDevice.Viewport.Y;
            }

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(null);

            UpdateWorldColor();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(BackgroundTexture, destinationRectangle, Color.White);
            spriteBatch.Draw(WorldTexture, destinationRectangle, WorldColor);
            spriteBatch.Draw(RenderTargetInstance, destinationRectangle, Color.White);
            spriteBatch.End();

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(null);
        }
        
        private void DrawDarknessToRenderTarget(Camera camera)
        {
            const float minimumBrightness = 0.7f;
            DarknessAlpha = minimumBrightness * (1 - SunlightManager.SunlightEffectiveness);

            var destinationRectangle = camera.DestinationRectangle;
            if (global::RenderingLibrary.Graphics.Renderer.SubtractViewportYForMonoGameGlBug)
            {
                destinationRectangle.Y -= RenderingLibrary.SystemManagers.Default.Renderer.GraphicsDevice.Viewport.Y;
            }

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(RenderTargetInstance);
            FlatRedBallServices.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            var darknessColor = new Color(0, 0, 0, DarknessAlpha);

            //First draw the objects as blackness
            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.Draw(WorldTexture, destinationRectangle, darknessColor);
            spriteBatch.End();

            //using (Stream stream = System.IO.File.Create("worldtexture.png"))
            //{
            //    RenderTargetInstance.SaveAsPng(stream, destinationRectangle.Width, destinationRectangle.Height);
            //}

            var blendState = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
            };

            //using (Stream stream = System.IO.File.Create("DarknessTexture.png"))
            //{
            //    DarknessTexture.SaveAsPng(stream, destinationRectangle.Width, destinationRectangle.Height);
            //}

            //Then subtract darkness where light sources are at
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
            spriteBatch.Draw(DarknessTexture, destinationRectangle, new Color(DarknessAlpha, DarknessAlpha, DarknessAlpha, DarknessAlpha));
            spriteBatch.End();
        }
    }
}
