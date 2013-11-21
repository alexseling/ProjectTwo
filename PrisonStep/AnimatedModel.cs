using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XnaAux;

namespace PrisonStep
{
    public class AnimatedModel
    {
        /// <summary>
        /// Reference to the game that uses this class
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The XNA model we will be animating
        /// </summary>
        private Model model;


        private Matrix[] bindTransforms;
        private Matrix[] boneTransforms;
        private Matrix[] absoTransforms;

        /// <summary>
        /// Name of the asset we are going to load
        /// </summary>
        private string asset;

        private Matrix[] skinTransforms = null;

        private List<int> skelToBone = null;
        private Matrix[] inverseBindTransforms = null;

        private Matrix rootMatrixRaw = Matrix.Identity;
        private Matrix deltaMatrix = Matrix.Identity;

        public Matrix DeltaMatrix { get { return deltaMatrix; } }
        public Vector3 DeltaPosition;
        public Matrix RootMatrix { get { return inverseBindTransforms[skelToBone[0]] * rootMatrixRaw; } }

        /// <summary>
        /// Access the current animation player
        /// </summary>
        public AnimationPlayer Player { get { return player; } }

        private bool rotateSpine = false;
        private float spineRotationX = 0;
        private float spineRotationZ = 0;

        public bool RotateSpine { get { return rotateSpine; } set { rotateSpine = value; } }
        public float SpineRotationX { get { return spineRotationX; } set { spineRotationX = value; } }
        public float SpineRotationZ { get { return spineRotationZ; } set { spineRotationZ = value; } }

        /// <summary>
        /// This class describes a single animation clip we load from
        /// an asset.
        /// </summary>
        private class AssetClip
        {
            public AssetClip(string name, string asset)
            {
                Name = name;
                Asset = asset;
                TheClip = null;
            }

            public string Name { get; set; }
            public string Asset { get; set; }
            public AnimationClips.Clip TheClip { get; set; }
        }

        /// <summary>
        /// A dictionary that allows us to look up animation clips
        /// by name. 
        /// </summary>
        private Dictionary<string, AssetClip> assetClips = new Dictionary<string, AssetClip>();

        /// <summary>
        /// The number of skinning matrices in SkinnedEffect.fx. This must
        /// match the number in SkinnedEffect.fx.
        /// </summary>
        public const int NumSkinBones = 57;

        public AnimatedModel(PrisonGame game, string asset)
        {
            skinTransforms = new Matrix[57];
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = Matrix.Identity;
            }
            this.game = game;
            this.asset = asset;
        }


        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>(asset);

            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            boneTransforms = new Matrix[boneCnt];
            absoTransforms = new Matrix[boneCnt];

            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);

            AnimationClips clips = model.Tag as AnimationClips;
            if (clips != null && clips.SkelToBone.Count > 0)
            {
                skelToBone = clips.SkelToBone;

                inverseBindTransforms = new Matrix[boneCnt];
                skinTransforms = new Matrix[NumSkinBones];

                model.CopyAbsoluteBoneTransformsTo(inverseBindTransforms);

                for (int b = 0; b < inverseBindTransforms.Length; b++)
                    inverseBindTransforms[b] = Matrix.Invert(inverseBindTransforms[b]);

                for (int i = 0; i < skinTransforms.Length; i++)
                    skinTransforms[i] = Matrix.Identity;
            }

            foreach (AssetClip clip in assetClips.Values)
            {
                Model clipmodel = content.Load<Model>(clip.Asset);
                AnimationClips modelclips = clipmodel.Tag as AnimationClips;
                clip.TheClip = modelclips.Clips["Take 001"];
            }
        }

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double delta)
        {

            if (player != null)
            {
                // Update the clip
                player.Update(delta);

                for (int b = 0; b < player.BoneCount; b++)
                {
                    AnimationPlayer.Bone bone = player.GetBone(b);
                    if (!bone.Valid)
                        continue;

                    Vector3 scale = new Vector3(bindTransforms[b].Right.Length(),
                        bindTransforms[b].Up.Length(),
                        bindTransforms[b].Backward.Length());

                    boneTransforms[b] = Matrix.CreateScale(scale) *
                        Matrix.CreateFromQuaternion(bone.Rotation) *
                        Matrix.CreateTranslation(bone.Translation);
                }

                if (skelToBone != null)
                {
                    int rootBone = skelToBone[0];

                    deltaMatrix = Matrix.Invert(rootMatrixRaw) * boneTransforms[rootBone];
                    DeltaPosition = boneTransforms[rootBone].Translation - rootMatrixRaw.Translation;

                    rootMatrixRaw = boneTransforms[rootBone];

                    boneTransforms[rootBone] = bindTransforms[rootBone];
                }

                model.CopyBoneTransformsFrom(boneTransforms);
            }

            if (rotateSpine)
            {
                int index = model.Bones["Bip01 Spine1"].Index;

                spineRotationX = spineRotationX < -1.2 ? -1.2f : spineRotationX;
                spineRotationX = spineRotationX > 1.2 ? 1.2f : spineRotationX;
                spineRotationZ = spineRotationZ < -0.5 ? -0.5f : spineRotationZ;
                spineRotationZ = spineRotationZ > 0.5 ? 0.5f : spineRotationZ;
                boneTransforms[index] = Matrix.CreateRotationX(spineRotationX) * Matrix.CreateRotationZ(spineRotationZ) * bindTransforms[index];
            }

            System.Diagnostics.Trace.WriteLine(spineRotationZ);

            model.CopyBoneTransformsFrom(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);
        }

        private AnimationPlayer player = null;

        /// <summary>
        /// Play an animation clip on this model.
        /// </summary>
        /// <param name="name"></param>
        public AnimationPlayer PlayClip(string name)
        {
            player = null;

            if (name != "Take 001")
            {
                player = new AnimationPlayer(this, assetClips[name].TheClip);
                Update(0);
                return player;
            }

            AnimationClips clips = model.Tag as AnimationClips;
            if (clips != null)
            {
                player = new AnimationPlayer(this, clips.Clips[name]);
                Update(0);
            }

            return player;
        }

        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics">Device to draw the model on.</param>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="transform">Transform that puts the model where we want it.</param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform)
        {
            DrawModel(graphics, model, transform);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            if (skelToBone != null)
            {
                for (int b = 0; b < skelToBone.Count; b++)
                {
                    int n = skelToBone[b];
                    skinTransforms[b] = inverseBindTransforms[n] * absoTransforms[n];
                }
            }
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);

                    effect.Parameters["Bones"].SetValue(skinTransforms);
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// Add an asset clip to the dictionary.
        /// </summary>
        /// <param name="name">Name we will use for the clip</param>
        /// <param name="asset">The FBX asset to load</param>
        public void AddAssetClip(string name, string asset)
        {
            assetClips[name] = new AssetClip(name, asset);
        }

        public Matrix CalculateBazookaPosition(Matrix transform)
        {
            int handIndex = model.Bones["Bip01 R Hand"].Index;

            return Matrix.CreateRotationX(MathHelper.ToRadians(109.5f)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(9.7f)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(72.9f)) *
                Matrix.CreateTranslation(new Vector3(-9.6f, 11.85f, 21.1f)) *
                absoTransforms[handIndex];
        }

        public void ChangeBoneTransform(string bone, Matrix transform)
        {
        }
    }
}
