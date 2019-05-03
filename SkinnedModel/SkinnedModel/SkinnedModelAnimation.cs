using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DopaEngine
{
    public class SkinnedModelAnimation
    {
        public class BoneAnimation
        {
            public string Name { get; set; }
            public Matrix Transformation { get; set; }
            public BoneAnimation Parent { get; set; }
            public List<BoneAnimation> Children { get; set; }
            public List<Vector3> Positions { get; set; }
            public List<Vector3> Scales { get; set; }
            public List<Quaternion> Rotations { get; set; }
            public bool IsAnimate { get; set; }
        }

        public string FilePath { get; set; }
        public List<BoneAnimation> BoneAnimations { get; set; }
        public BoneAnimation RootBoneAnimation { get; set; }

        public void Load()
        {
            BoneAnimations = new List<BoneAnimation>();

            var importer = new Assimp.AssimpContext();
            var aScene = importer.ImportFile(FilePath, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);

            RootBoneAnimation = LoadBoneAnimation(aScene, aScene.RootNode, null);
        }

        public BoneAnimation LoadBoneAnimation(Assimp.Scene aScene, Assimp.Node aNode, BoneAnimation parent)
        {
            var boneAnimation = new BoneAnimation()
            {
                Parent = parent,
                Children = new List<BoneAnimation>(),
                Positions = new List<Vector3>(),
                Rotations = new List<Quaternion>(),
                Scales = new List<Vector3>()
            };
            boneAnimation.Name = aNode.Name;

            var animationChanel = aScene.Animations[0].NodeAnimationChannels.Where(n => n.NodeName == boneAnimation.Name).FirstOrDefault();

            if (animationChanel != null)
            {
                boneAnimation.IsAnimate = true;

                foreach (var aScale in animationChanel.ScalingKeys)
                {
                    var scale = new Vector3(aScale.Value.X, aScale.Value.Y, aScale.Value.Z);
                    boneAnimation.Scales.Add(scale);
                }

                foreach (var aRotation in animationChanel.RotationKeys)
                {
                    var rotation = new Quaternion(aRotation.Value.X, aRotation.Value.Y, aRotation.Value.Z, aRotation.Value.W);
                    boneAnimation.Rotations.Add(rotation);
                }

                foreach (var aTranslate in animationChanel.PositionKeys)
                {
                    var translate = new Vector3(aTranslate.Value.X, aTranslate.Value.Y, aTranslate.Value.Z);
                    boneAnimation.Positions.Add(translate);
                }
            }
            else
            {
                boneAnimation.IsAnimate = false;
                boneAnimation.Transformation = Matrix.Transpose(AssimpHelper.MatrixAssimpToXna(aNode.Transform));
            }

            foreach (var child in aNode.Children)
            {
                boneAnimation.Children.Add(LoadBoneAnimation(aScene, child, boneAnimation));
            }
            BoneAnimations.Add(boneAnimation);
            return boneAnimation;
        }
    }
}
