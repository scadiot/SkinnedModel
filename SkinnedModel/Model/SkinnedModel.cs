using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DopaEngine
{
    public class SkinnedModel
    {
        public GraphicsDevice GraphicsDevice { get; set; }

        public string FilePath { get; set; }
        public List<Mesh> Meshes { get; set; }

        class VerticeWeight
        {
            public Bone Bone { get; set; }
            public float Weight { get; set; }
        }

        public class Bone
        {
            public string Name { get; set; }
            public Matrix OffsetInverse { get; set; }
            public Matrix Offset { get; set; }
            public int Index { get; set; }
        }

        public class Mesh
        {
            public String Name { get; set; }

            public Vector3 OffsetPosition { get; set; }
            public List<Bone> Bones { get; set; }

            public VertexBuffer VertexBuffer { get; set; }
            public IndexBuffer IndexBuffer { get; set; }

            public string TextureFilePath { get; set; }
            public Texture2D Texture { get; set; }

            public int FaceCount { get; set; }
        }

        class MeshSkinnedVerticeInfo
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
            public Vector2 TextureCoordinate { get; set; }
            public Color Color { get; set; }
            public int MaterialIndex { get; set; }
            public Vector4 BoneID { get; set; }
            public Vector4 BoneWeight { get; set; }

            public SkinnedModelVertex ToVertexPositionNormalTextureBones()
            {
                return new SkinnedModelVertex(Position, Normal, TextureCoordinate, Color, BoneID, BoneWeight);
            }
        }

        struct BlendInfo
        {
            public Vector4 Weight;
            public Vector4 BoneId;
        }

        public SkinnedModel(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        public void Initialize()
        {
            var importer = new AssimpContext();
            var aScene = importer.ImportFile(FilePath, PostProcessPreset.TargetRealTimeMaximumQuality);
            Meshes = new List<Mesh>();

            foreach (var aMesh in aScene.Meshes)
            {
                var verticesResult = new List<MeshSkinnedVerticeInfo>();
                var indicesResult = new List<int>();

                var mesh = new Mesh();
                mesh.Bones = new List<Bone>();

                mesh.Name = aMesh.Name;

                Dictionary<int, List<VerticeWeight>> VerticeWeights = new Dictionary<int, List<VerticeWeight>>();
                foreach (var aBone in aMesh.Bones)
                {
                    Bone bone = GetBone(mesh, aBone);

                    foreach (var vw in aBone.VertexWeights)
                    {
                        if (!VerticeWeights.ContainsKey(vw.VertexID))
                        {
                            VerticeWeights.Add(vw.VertexID, new List<VerticeWeight>());
                        }
                        VerticeWeights[vw.VertexID].Add(new VerticeWeight() { Bone = bone, Weight = vw.Weight });
                    }
                }

                var c = aScene.Materials[aMesh.MaterialIndex].ColorDiffuse;
                var color = new Color(new Vector4(c.R, c.G, c.B, c.A));

                for (int faceIndex = 0; faceIndex < aMesh.FaceCount; faceIndex++)
                {
                    for (int vertexNum = 0; vertexNum < 3; vertexNum++)
                    {
                        int verticeIndice = aMesh.Faces[faceIndex].Indices[vertexNum];
                        Vector3 verticePosition = AssimpHelper.VectorAssimpToXna(aMesh.Vertices[verticeIndice]);
                        Vector3 verticeNormal = AssimpHelper.VectorAssimpToXna(aMesh.Normals[verticeIndice]);

                        var uv = AssimpHelper.VectorAssimpToXna(aMesh.TextureCoordinateChannels[0][verticeIndice]);
                        var verticeUv = new Vector2(uv.X, uv.Y);

                        BlendInfo blendInfo = GetBlendInfo(VerticeWeights, verticeIndice);

                        var vertice = new MeshSkinnedVerticeInfo()
                        {
                            Position = verticePosition,
                            Normal = verticeNormal,
                            TextureCoordinate = verticeUv,
                            Color = color,
                            MaterialIndex = aMesh.MaterialIndex,
                            BoneID = blendInfo.BoneId,
                            BoneWeight = blendInfo.Weight
                        };

                        indicesResult.Add(verticesResult.Count);
                        verticesResult.Add(vertice);
                    }
                }

                mesh.TextureFilePath = aScene.Materials[aMesh.MaterialIndex].TextureDiffuse.FilePath;

                mesh.VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(SkinnedModelVertex), verticesResult.Count, BufferUsage.WriteOnly);
                mesh.VertexBuffer.SetData<SkinnedModelVertex>(verticesResult.Select(v => v.ToVertexPositionNormalTextureBones()).ToArray());

                mesh.IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), indicesResult.Count, BufferUsage.WriteOnly);
                mesh.IndexBuffer.SetData(indicesResult.ToArray());

                mesh.FaceCount = aMesh.FaceCount;

                Meshes.Add(mesh);
            }

        }

        Bone GetBone(Mesh mesh, Assimp.Bone aBone)
        {
            var bone = mesh.Bones.FirstOrDefault(b => b.Name == aBone.Name);
            if(bone == null)
            {
                var offsetMatrix = aBone.OffsetMatrix;
                offsetMatrix.Transpose();

                bone = new Bone();
                bone.Name = aBone.Name;
                bone.Index = mesh.Bones.Count;
                bone.Offset = AssimpHelper.MatrixAssimpToXna(offsetMatrix);
                bone.OffsetInverse = Matrix.Invert(AssimpHelper.MatrixAssimpToXna(offsetMatrix));
                mesh.Bones.Add(bone);
            }
            return bone;
        }

        BlendInfo GetBlendInfo(Dictionary<int, List<VerticeWeight>> VerticeWeights, int verticeIndex)
        {
            const uint BlendCount = 4;
        
            var weight = new float[BlendCount];
            var boneId = new float[BlendCount];
        
            for (int i = 0; i < BlendCount; i++)
            {
                weight[i] = 0f;
                boneId[i] = 0f;
            }
        
            if (VerticeWeights.ContainsKey(verticeIndex))
            {
                var weightInfo = VerticeWeights[verticeIndex];
                weightInfo = weightInfo.OrderByDescending(w => w.Weight).ToList();
                var count = Math.Min(weightInfo.Count, 4);
        
                for (int i = 0; i < count; i++)
                {
                    weight[i] = weightInfo[i].Weight;
                    boneId[i] = (float)weightInfo[i].Bone.Index;
                }
            }
        
            BlendInfo result;
            result.Weight = new Vector4(weight[0], weight[1], weight[2], weight[3]);
            result.BoneId = new Vector4(boneId[0], boneId[1], boneId[2], boneId[3]);
            return result;
        }
    }
}
