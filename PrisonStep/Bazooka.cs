using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Bazooka
    {
        private PrisonGame game;
        private Model model;

        public Bazooka(PrisonGame game)
        {
            this.game = game;
        }

        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("PieBazooka");
        }

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform)
        {
            DrawModel(graphics, model, transform);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = game.Camera.View;
                    effect.Projection = game.Camera.Projection;
                }
                mesh.Draw();
            }
        }
    }
}
