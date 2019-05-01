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
        public List<NodeInstance> NodeInstances;

        public SkinnedModelAnimation PreviousAnimation { get; set; }
        public List<NodeInstance> PreviousNodeInstances;

        public TimeSpan TimeAnimationChanged;
        public float SpeedTransitionSecond { get; set; } = 1.0f;

        public struct NodeInstanceToBone
        {
            public NodeInstance NodeInstance { get; set; }
            public SkinnedModel.Bone Bone { get; set; }
        }

        public class MeshInstance
        {
            public SkinnedModel.Mesh Mesh { get; set; }
            public List<NodeInstanceToBone> NodeInstanceToBoneIndex { get; set; }
            public Matrix[] BonesOffsets { get; set; }
        }

        public class NodeInstance
        {
            public SkinnedModelAnimation.Node Node { get; set; }
            public NodeInstance Parent { get; set; }
            public NodeInstance PreviousNodeInstance { get; set; }
            public Matrix AdditionalTransform { get; set; }
            public Matrix Transform { get; set; }
            public bool Updated { get; set; }
        }

        public SkinnedModelInstance()
        {

        }

        public void Initialize()
        {
            NodeInstances = new List<NodeInstance>();
            MeshInstances = new List<MeshInstance>();
            PreviousNodeInstances = new List<NodeInstance>();

            foreach (var skinnedMesh in Mesh.Meshes)
            {
                MeshInstance meshInstance = new MeshInstance();
                meshInstance.Mesh = skinnedMesh;
                meshInstance.NodeInstanceToBoneIndex = new List<NodeInstanceToBone>();
                meshInstance.BonesOffsets = new Matrix[MaxBones];
                for (int i = 0; i < meshInstance.BonesOffsets.Length; i++)
                {
                    meshInstance.BonesOffsets[i] = Matrix.Identity;
                }

                MeshInstances.Add(meshInstance);
            }
        }

        public Matrix GetNodeTransform(SkinnedModelAnimation.Node node, GameTime gt)
        {
            if (!node.IsAnimate && node.Parent != null)
            {
                return node.Transformation;
            }

            Matrix transform = Matrix.Identity;
            if (node.Scales.Any())
            {
                int frameIndex = (int)(gt.TotalGameTime.TotalSeconds * FramePerSecond) % node.Scales.Count;
                transform *= Matrix.CreateScale(node.Scales[frameIndex]);
            }
            if (node.Scales.Any())
            {
                int frameIndex = (int)(gt.TotalGameTime.TotalSeconds * FramePerSecond) % node.Rotations.Count;
                transform *= Matrix.CreateFromQuaternion(node.Rotations[frameIndex]);
            }
            if (node.Scales.Any())
            {
                int frameIndex = (int)(gt.TotalGameTime.TotalSeconds * FramePerSecond) % node.Positions.Count;
                transform *= Matrix.CreateTranslation(node.Positions[frameIndex]);
            }

            return transform;
        }

        void UpdateNodeInstance(NodeInstance nodeInstance, GameTime gameTime)
        {
            if(nodeInstance.Updated)
            {
                return;
            }

            Matrix parentTransform = Matrix.Identity;
            if(nodeInstance.Parent != null)
            {
                UpdateNodeInstance(nodeInstance.Parent, gameTime);
                parentTransform = nodeInstance.Parent.Transform;
            }

            nodeInstance.Transform = GetNodeTransform(nodeInstance.Node, gameTime) * nodeInstance.AdditionalTransform * parentTransform;
        }

        public void UpdateNodes(GameTime gameTime)
        {
            foreach (var nodeInstance in NodeInstances)
            {
                nodeInstance.Updated = false;
            }

            foreach (var nodeInstance in NodeInstances)
            {
                UpdateNodeInstance(nodeInstance, gameTime);
            }

            foreach (var nodeInstance in PreviousNodeInstances)
            {
                UpdateNodeInstance(nodeInstance, gameTime);
            }
        }

        public void UpdateBones(GameTime gameTime)
        {
            foreach (var meshInstance in MeshInstances)
            {
                foreach (var nodeInstanceToBone in meshInstance.NodeInstanceToBoneIndex)
                {
                    Matrix transform = nodeInstanceToBone.NodeInstance.Transform;
                    if (nodeInstanceToBone.NodeInstance.PreviousNodeInstance != null)
                    {
                        float transition = (float)(gameTime.TotalGameTime.TotalSeconds - TimeAnimationChanged.TotalSeconds);
                        if(transition < SpeedTransitionSecond)
                        {
                            transform = Matrix.Lerp(nodeInstanceToBone.NodeInstance.PreviousNodeInstance.Transform, nodeInstanceToBone.NodeInstance.Transform, transition / SpeedTransitionSecond);
                        }
                    }
                    meshInstance.BonesOffsets[nodeInstanceToBone.Bone.Index] = nodeInstanceToBone.Bone.Offset * transform;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            UpdateNodes(gameTime);
            UpdateBones(gameTime);
        }

        public NodeInstance GetNodeInstance(string name)
        {
            return NodeInstances.FirstOrDefault(ni => ni.Node.Name == name);
        }

        public Matrix GetTransform(NodeInstance nodeInstance, GameTime gameTime)
        {
            Matrix transform = nodeInstance.Transform;
            if (nodeInstance.PreviousNodeInstance != null)
            {
                float transition = (float)(gameTime.TotalGameTime.TotalSeconds - TimeAnimationChanged.TotalSeconds);
                if (transition < SpeedTransitionSecond)
                {
                    transform = Matrix.Lerp(nodeInstance.PreviousNodeInstance.Transform, nodeInstance.Transform, transition / SpeedTransitionSecond);
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
            PreviousNodeInstances.Clear();
            PreviousNodeInstances.AddRange(NodeInstances);

            Animation = animation;

            NodeInstances.Clear();
            foreach(var node in animation.Nodes)
            {
                NodeInstance nodeInstance = new NodeInstance();
                nodeInstance.Node = node;
                nodeInstance.Updated = false;
                nodeInstance.AdditionalTransform = Matrix.Identity;
                nodeInstance.PreviousNodeInstance = PreviousNodeInstances.FirstOrDefault(ni => ni.Node.Name == node.Name);
                NodeInstances.Add(nodeInstance);
            }

            foreach (var nodeInstances in NodeInstances)
            {
                nodeInstances.Parent = NodeInstances.FirstOrDefault(ni => ni.Node == nodeInstances.Node.Parent);
            }

            foreach(var meshInstance in MeshInstances)
            {
                meshInstance.NodeInstanceToBoneIndex.Clear();
                foreach(var bone in meshInstance.Mesh.Bones)
                {
                    var nodeInstanceToBoneIndex = new NodeInstanceToBone();
                    nodeInstanceToBoneIndex.Bone = bone;
                    nodeInstanceToBoneIndex.NodeInstance = NodeInstances.FirstOrDefault(ni => ni.Node.Name == bone.Name);
                    meshInstance.NodeInstanceToBoneIndex.Add(nodeInstanceToBoneIndex);
                }
            }
        }
    }
}
