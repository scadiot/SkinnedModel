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
            public int Index { get; set; }
            public Matrix Transformation { get; set; }
            public Node Parent { get; set; }
            public List<Node> Children { get; set; }
            public List<Vector3> Positions { get; set; }
            public List<Vector3> Scales { get; set; }
            public List<Quaternion> Rotations { get; set; }
        }

        public string FilePath { get; set; }
        public List<Node> Nodes { get; set; }
        public Node RootNode;
        public Dictionary<string, int> NodeIndexByName { get; set; }

        public void Load()
        {
            Nodes = new List<Node>();

            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            Assimp.Scene scene = importer.ImportFile(FilePath, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);

            RootNode = LoadNode(scene, scene.RootNode, null);

            NodeIndexByName = new Dictionary<string, int>();
            for (int i = 0;i < Nodes.Count;i++)
            {
                Nodes[i].Index = i;
                NodeIndexByName.Add(Nodes[i].Name, i);
            }
        }

        public Node LoadNode(Assimp.Scene aScene, Assimp.Node aNode, Node parent)
        {
            Node node = new Node()
            {
                Parent = parent,
                Children = new List<Node>(),
                Positions = new List<Vector3>(),
                Rotations = new List<Quaternion>(),
                Scales = new List<Vector3>()
            };
            node.Name = aNode.Name;

            var animationChanel = aScene.Animations[0].NodeAnimationChannels.Where(n => n.NodeName == node.Name).FirstOrDefault();

            if(animationChanel != null)
            {
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
                node.Transformation = Matrix.Identity;
            }

            foreach (var child in aNode.Children)
            {
                node.Children.Add(LoadNode(aScene, child, node));
            }
            Nodes.Add(node);
            return node;
        }

        static Matrix MatrixAssimpToXna(Assimp.Matrix4x4 matrix)
        {
            return new Matrix(
                matrix.A1,
                matrix.A2,
                matrix.A3,
                matrix.A4,
                matrix.B1,
                matrix.B2,
                matrix.B3,
                matrix.B4,
                matrix.C1,
                matrix.C2,
                matrix.C3,
                matrix.C4,
                matrix.D1,
                matrix.D2,
                matrix.D3,
                matrix.D4
                );
        }
    }
}
