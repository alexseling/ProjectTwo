using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace PrisonStep
{
    class Gort
    {
        private AnimatedModel gort;
        private PrisonGame game;

        private Player player;

        public Gort(PrisonGame game, Player player)
        {
            this.game = game;
            this.player = player;
            gort = new AnimatedModel(game, "Dalek");
            //victoria.AddAssetClip("dance", "Victoria-dance");
        }

        public void LoadContent(ContentManager content)
        {

        }

        private string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            foreach (KeyValuePair<string, List<Vector2>> region in player.Regions)
            {
                // For now we ignore the walls
                if(region.Key.StartsWith("W"))
                    continue;

                for (int i = 0; i < region.Value.Count; i += 3)
                {
                    float x1 = region.Value[i].X;
                    float x2 = region.Value[i + 1].X;
                    float x3 = region.Value[i + 2].X;
                    float y1 = region.Value[i].Y;
                    float y2 = region.Value[i + 1].Y;
                    float y3 = region.Value[i + 2].Y;

                    float d = 1.0f / ((x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3));
                    float l1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) * d;
                    if (l1 < 0)
                        continue;

                    float l2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) * d;
                    if (l2 < 0)
                        continue;

                    float l3 = 1 - l1 - l2;
                    if (l3 < 0)
                        continue;

                    return region.Key;
                }
            }

            return "";
        }

        public void Update(GameTime gameTime)
        {
      
        }

        private bool TestRegionForCollision(string region)
        {
            bool collision = false;

            if (region == "")
            {
                // If not in a region, we have stepped out of bounds
                collision = true;
            }
            else if (region.StartsWith("R_D"))
            {
                int dnum = int.Parse(region.Substring(6));
                bool isOpen = false;
                foreach (PrisonModel model in game.PhibesModels)
                {
                    if (model.DoorIsOpen(dnum))
                    {
                        isOpen = true;
                        break;
                    }
                }

                if (!isOpen)
                    collision = true;
            }
            return collision;
        }

        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            gort.Draw(graphics, gameTime, Matrix.Identity);
        }
    }
}
