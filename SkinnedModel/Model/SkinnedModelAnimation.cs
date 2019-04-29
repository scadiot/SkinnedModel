using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DopaEngine
{
    public class SkinnedModelAnimation
    {
        public class Node
        {
            public string Name { get; set; }
            public Matrix Transformation { get; set; }
            public Node Parent { get; set; }
            public List<Node> Children { get; set; }
            public List<Vector3> Positions { get; set; }
            public List<Vector3> Scales { get; set; }
            public List<Quaternion> Rotations { get; set; }
            public bool IsAnimate { get; set; }
        }

        public string FilePath { get; set; }
        public List<Node> Nodes { get; set; }
        public Node RootNode { get; set; }

        public void Load()
        {
            Nodes = new List<Node>();

            var importer = new Assimp.AssimpContext();
            var aScene = importer.ImportFile(FilePath, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);

            RootNode = LoadNode(aScene, aScene.RootNode, null);
        }

        public Node LoadNode(Assimp.Scene aScene, Assimp.Node aNode, Node parent)
        {
            var node = new Node()
            {
                Parent = parent,
                Children = new List<Node>(),
                Positions = new List<Vector3>(),
                Rotations = new List<Quaternion>(),
                Scales = new List<Vector3>()
            };
            node.Name = aNode.Name;

            var animationChanel = aScene.Animations[0].NodeAnimationChannels.Where(n => n.NodeName == node.Name).FirstOrDefault();

            if (animationChanel != null)
            {
                node.IsAnimate = true;

                foreach (var aScale in animationChanel.ScalingKeys)
                {
                    var scale = new Vector3(aScale.Value.X, aScale.Value.Y, aScale.Value.Z);
                    node.Scales.Add(scale);
                }

                foreach (var aRotation in animationChanel.RotationKeys)
                {
                    var rotation = new Quaternion(aRotation.Value.X, aRotation.Value.Y, aRotation.Value.Z, aRotation.Value.W);
                    node.Rotations.Add(rotation);
                }

                foreach (var aTranslate in animationChanel.PositionKeys)
                {
                    var translate = new Vector3(aTranslate.Value.X, aTranslate.Value.Y, aTranslate.Value.Z);
                    node.Positions.Add(translate);
                }
            }
            else
            {
                node.IsAnimate = false;
                node.Transformation = Matrix.Transpose(AssimpHelper.MatrixAssimpToXna(aNode.Transform));
            }

            foreach (var child in aNode.Children)
            {
                node.Children.Add(LoadNode(aScene, child, node));
            }
            Nodes.Add(node);
            return node;
        }
    }
}
