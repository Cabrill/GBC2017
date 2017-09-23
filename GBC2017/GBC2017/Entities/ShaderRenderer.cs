using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.StaticManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBC2017.Entities
{
    public partial class ShaderRenderer
    {
        #region Fields/Properties

        SpriteBatch spriteBatch;

        public Effect Effect { get; set; }

        public Texture2D WorldTexture { get; set; }
        public Texture2D DarknessTexture { get; set; }
        public Texture2D BlowoutTexture { get; set; }

        public float DarknessAlpha { get; set; }

        public PositionedObject Viewer { get; set; }

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

        private void DrawToScreen(Camera camera)
        {
            var destinationRectangle = camera.DestinationRectangle;
            if (global::RenderingLibrary.Graphics.Renderer.SubtractViewportYForMonoGameGlBug)
            {
                destinationRectangle.Y -= RenderingLibrary.SystemManagers.Default.Renderer.GraphicsDevice.Viewport.Y;
            }

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(WorldTexture, destinationRectangle, Color.White);
            spriteBatch.Draw(RenderTargetInstance, destinationRectangle, Color.White);
            spriteBatch.End();

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(null);
        }
        
        private void DrawDarknessToRenderTarget(Camera camera)
        {
            DarknessAlpha = 0.8f;

            var destinationRectangle = camera.DestinationRectangle;
            if (global::RenderingLibrary.Graphics.Renderer.SubtractViewportYForMonoGameGlBug)
            {
                destinationRectangle.Y -= RenderingLibrary.SystemManagers.Default.Renderer.GraphicsDevice.Viewport.Y;
            }

            var darknessColor = new Color(0, 0, 0, DarknessAlpha);
            var blendState = new BlendState
            {
                AlphaSourceBlend = Blend.DestinationColor,
                ColorSourceBlend = Blend.DestinationColor,
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorBlendFunction = BlendFunction.Add
            };

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(RenderTargetInstance);

            //First draw the objects as blackness
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(WorldTexture, destinationRectangle, darknessColor);
            spriteBatch.End();

            blendState = new BlendState
            {
                AlphaSourceBlend = Blend.DestinationAlpha,
                ColorSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.InverseSourceColor,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorBlendFunction = BlendFunction.Add
            };

            //Then subtract darkness where light sources are at
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(DarknessTexture, destinationRectangle, Color.White);
            spriteBatch.End();
        }
    }
}
