using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DopaEngine
{
    public class SkinnedModelInstance
    {
        public const uint MaxBones = 50;
        public const uint FramePerSecond = 30;

        public SkinnedModel Mesh { get; set; }
        public Matrix Transformation { get; set; }
        public SkinnedModelAnimation Animation { get; set; }

        public List<MeshInstance> MeshInstances;
        public List<BoneAnimationInstance> BoneAnimationInstances;

        public SkinnedModelAnimation PreviousAnimation { get; set; }
        public List<BoneAnimationInstance> PreviousBoneAnimationInstances;

        public TimeSpan TimeAnimationChanged;
        public float SpeedTransitionSecond { get; set; } = 1.0f;

        public struct NodeInstanceToBone
        {
            public BoneAnimationInstance NodeInstance { get; set; }
            public SkinnedModel.Bone Bone { get; set; }
        }

        public class MeshInstance
        {
            public SkinnedModel.Mesh Mesh { get; set; }
            public List<NodeInstanceToBone> BoneAnimationInstanceToBoneIndex { get; set; }
            public Matrix[] BonesOffsets { get; set; }
        }

        public class BoneAnimationInstance
        {
            public SkinnedModelAnimation.BoneAnimation BoneAnimation { get; set; }
            public BoneAnimationInstance Parent { get; set; }
            public BoneAnimationInstance PreviousBoneAnimationInstance { get; set; }
            public Matrix AdditionalTransform { get; set; }
            public Matrix Transform { get; set; }
            public bool Updated { get; set; }
        }

        public SkinnedModelInstance()
        {

        }

        public void Initialize()
        {
            BoneAnimationInstances = new List<BoneAnimationInstance>();
            MeshInstances = new List<MeshInstance>();
            PreviousBoneAnimationInstances = new List<BoneAnimationInstance>();

            foreach (var skinnedMesh in Mesh.Meshes)
            {
                MeshInstance meshInstance = new MeshInstance();
                meshInstance.Mesh = skinnedMesh;
                meshInstance.BoneAnimationInstanceToBoneIndex = new List<NodeInstanceToBone>();
                meshInstance.BonesOffsets = new Matrix[MaxBones];
                for (int i = 0; i < meshInstance.BonesOffsets.Length; i++)
                {
                    meshInstance.BonesOffsets[i] = Matrix.Identity;
                }

                MeshInstances.Add(meshInstance);
            }
        }

        public Matrix GetNodeTransform(SkinnedModelAnimation.BoneAnimation boneAnimation, GameTime gt)
        {
            if (!boneAnimation.IsAnimate && boneAnimation.Parent != null)
            {
                return boneAnimation.Transformation;
            }

            Matrix transform = Matrix.Identity;
            if (boneAnimation.Scales.Any())
            {
                int frameIndex = (int)(gt.TotalGameTime.TotalSeconds * FramePerSecond) % boneAnimation.Scales.Count;
                transform *= Matrix.CreateScale(boneAnimation.Scales[frameIndex]);
            }
            if (boneAnimation.Scales.Any())
            {
                int frameIndex = (int)(gt.TotalGameTime.TotalSeconds * FramePerSecond) % boneAnimation.Rotations.Count;
                transform *= Matrix.CreateFromQuaternion(boneAnimation.Rotations[frameIndex]);
            }
            if (boneAnimation.Scales.Any())
            {
                int frameIndex = (int)(gt.TotalGameTime.TotalSeconds * FramePerSecond) % boneAnimation.Positions.Count;
                transform *= Matrix.CreateTranslation(boneAnimation.Positions[frameIndex]);
            }

            return transform;
        }

        void UpdateBoneAnimationInstance(BoneAnimationInstance boneAnimationInstance, GameTime gameTime)
        {
            if(boneAnimationInstance.Updated)
            {
                return;
            }

            Matrix parentTransform = Matrix.Identity;
            if(boneAnimationInstance.Parent != null)
            {
                UpdateBoneAnimationInstance(boneAnimationInstance.Parent, gameTime);
                parentTransform = boneAnimationInstance.Parent.Transform;
            }

            boneAnimationInstance.Transform = GetNodeTransform(boneAnimationInstance.BoneAnimation, gameTime) * boneAnimationInstance.AdditionalTransform * parentTransform;
        }

        public void UpdateBoneAnimations(GameTime gameTime)
        {
            foreach (var boneAnimationInstance in BoneAnimationInstances)
            {
                boneAnimationInstance.Updated = false;
            }

            foreach (var boneAnimationInstance in BoneAnimationInstances)
            {
                UpdateBoneAnimationInstance(boneAnimationInstance, gameTime);
            }

            foreach (var boneAnimationInstance in PreviousBoneAnimationInstances)
            {
                UpdateBoneAnimationInstance(boneAnimationInstance, gameTime);
            }
        }

        public void UpdateBones(GameTime gameTime)
        {
            foreach (var meshInstance in MeshInstances)
            {
                foreach (var boneAnimationInstanceToBone in meshInstance.BoneAnimationInstanceToBoneIndex)
                {
                    Matrix transform = boneAnimationInstanceToBone.NodeInstance.Transform;
                    if (boneAnimationInstanceToBone.NodeInstance.PreviousBoneAnimationInstance != null)
                    {
                        float transition = (float)(gameTime.TotalGameTime.TotalSeconds - TimeAnimationChanged.TotalSeconds);
                        if(transition < SpeedTransitionSecond)
                        {
                            transform = Matrix.Lerp(boneAnimationInstanceToBone.NodeInstance.PreviousBoneAnimationInstance.Transform, boneAnimationInstanceToBone.NodeInstance.Transform, transition / SpeedTransitionSecond);
                        }
                    }
                    meshInstance.BonesOffsets[boneAnimationInstanceToBone.Bone.Index] = boneAnimationInstanceToBone.Bone.Offset * transform;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            UpdateBoneAnimations(gameTime);
            UpdateBones(gameTime);
        }

        public BoneAnimationInstance GetNodeInstance(string name)
        {
            return BoneAnimationInstances.FirstOrDefault(ni => ni.BoneAnimation.Name == name);
        }

        public Matrix GetTransform(BoneAnimationInstance boneAnimationInstance, GameTime gameTime)
        {
            Matrix transform = boneAnimationInstance.Transform;
            if (boneAnimationInstance.PreviousBoneAnimationInstance != null)
            {
                float transition = (float)(gameTime.TotalGameTime.TotalSeconds - TimeAnimationChanged.TotalSeconds);
                if (transition < SpeedTransitionSecond)
                {
                    transform = Matrix.Lerp(boneAnimationInstance.PreviousBoneAnimationInstance.Transform, boneAnimationInstance.Transform, transition / SpeedTransitionSecond);
                }
            }
            return transform;
        }

        public void SetAnimation(SkinnedModelAnimation animation, GameTime gameTime = null)
        {
            if(gameTime != null)
            {
                TimeAnimationChanged = gameTime.TotalGameTime;
            }

            PreviousAnimation = animation;
            PreviousBoneAnimationInstances.Clear();
            PreviousBoneAnimationInstances.AddRange(BoneAnimationInstances);

            Animation = animation;

            BoneAnimationInstances.Clear();
            foreach(var boneAnimation in animation.BoneAnimations)
            {
                BoneAnimationInstance boneAnimationInstance = new BoneAnimationInstance();
                boneAnimationInstance.BoneAnimation = boneAnimation;
                boneAnimationInstance.Updated = false;
                boneAnimationInstance.AdditionalTransform = Matrix.Identity;
                boneAnimationInstance.PreviousBoneAnimationInstance = PreviousBoneAnimationInstances.FirstOrDefault(ni => ni.BoneAnimation.Name == boneAnimation.Name);
                BoneAnimationInstances.Add(boneAnimationInstance);
            }

            foreach (var boneAnimationInstance in BoneAnimationInstances)
            {
                boneAnimationInstance.Parent = BoneAnimationInstances.FirstOrDefault(ni => ni.BoneAnimation == boneAnimationInstance.BoneAnimation.Parent);
            }

            foreach(var meshInstance in MeshInstances)
            {
                meshInstance.BoneAnimationInstanceToBoneIndex.Clear();
                foreach(var bone in meshInstance.Mesh.Bones)
                {
                    var nodeInstanceToBoneIndex = new NodeInstanceToBone();
                    nodeInstanceToBoneIndex.Bone = bone;
                    nodeInstanceToBoneIndex.NodeInstance = BoneAnimationInstances.FirstOrDefault(ni => ni.BoneAnimation.Name == bone.Name);
                    meshInstance.BoneAnimationInstanceToBoneIndex.Add(nodeInstanceToBoneIndex);
                }
            }
        }
    }
}
