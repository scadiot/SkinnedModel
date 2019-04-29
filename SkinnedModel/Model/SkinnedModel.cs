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

                for (int i = 0; i < aMesh.FaceCount; i++)
                {
                    int verticeIndice1 = aMesh.Faces[i].Indices[0];
                    Vector3 verticePosition1 = AssimpHelper.VectorAssimpToXna(aMesh.Vertices[verticeIndice1]);

                    int verticeIndice2 = aMesh.Faces[i].Indices[1];
                    Vector3 verticePosition2 = AssimpHelper.VectorAssimpToXna(aMesh.Vertices[verticeIndice2]);

                    int verticeIndice3 = aMesh.Faces[i].Indices[2];
                    Vector3 verticePosition3 = AssimpHelper.VectorAssimpToXna(aMesh.Vertices[verticeIndice3]);

                    var direction = Vector3.Cross(verticePosition2 - verticePosition1, verticePosition3 - verticePosition1);
                    var normal = Vector3.Normalize(direction);

                    var uv = AssimpHelper.VectorAssimpToXna(aMesh.TextureCoordinateChannels[0][verticeIndice1]);
                    var verticeUv1 = new Vector2(uv.X, uv.Y);

                    uv = AssimpHelper.VectorAssimpToXna(aMesh.TextureCoordinateChannels[0][verticeIndice2]);
                    var verticeUv2 = new Vector2(uv.X, uv.Y);

                    uv = AssimpHelper.VectorAssimpToXna(aMesh.TextureCoordinateChannels[0][verticeIndice3]);
                    var verticeUv3 = new Vector2(uv.X, uv.Y);

                    var vertice1 = new MeshSkinnedVerticeInfo()
                    {
                        Position = verticePosition1,
                        Normal = normal,
                        TextureCoordinate = verticeUv1,
                        Color = color,
                        MaterialIndex = aMesh.MaterialIndex,
                        BoneID = GetBlendIndices(VerticeWeights, verticeIndice1),
                        BoneWeight = GetBlendWeight(VerticeWeights, verticeIndice1)
                    };
                    indicesResult.Add(verticesResult.Count);
                    verticesResult.Add(vertice1);

                    var vertice2 = new MeshSkinnedVerticeInfo()
                    {
                        Position = verticePosition2,
                        Normal = normal,
                        TextureCoordinate = verticeUv2,
                        Color = color,
                        MaterialIndex = aMesh.MaterialIndex,
                        BoneID = GetBlendIndices(VerticeWeights, verticeIndice2),
                        BoneWeight = GetBlendWeight(VerticeWeights, verticeIndice2)
                    };
                    indicesResult.Add(verticesResult.Count);
                    verticesResult.Add(vertice2);

                    var vertice3 = new MeshSkinnedVerticeInfo()
                    {
                        Position = verticePosition3,
                        Normal = normal,
                        TextureCoordinate = verticeUv3,
                        Color = color,
                        MaterialIndex = aMesh.MaterialIndex,
                        BoneID = GetBlendIndices(VerticeWeights, verticeIndice3),
                        BoneWeight = GetBlendWeight(VerticeWeights, verticeIndice3)
                    };
                    indicesResult.Add(verticesResult.Count);
                    verticesResult.Add(vertice3);
                }

                mesh.TextureFilePath = aScene.Materials[aMesh.MaterialIndex].TextureDiffuse.FilePath;

                mesh.VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(SkinnedModelVertex), verticesResult.Count, BufferUsage.WriteOnly);
                mesh.VertexBuffer.SetData<SkinnedModelVertex>(verticesResult.Select(v => v.ToVertexPositionNormalTextureBones()).ToArray());

                mesh.IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), indicesResult.Count, BufferUsage.WriteOnly);
                mesh.IndexBuffer.SetData(indicesResult.ToArray());

                mesh.FaceCount = indicesResult.Count / 3;

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

        Vector4 GetBlendWeight(Dictionary<int, List<VerticeWeight>> VerticeWeights, int verticeIndex)
        {
            var blendWeight = new float[4];
            for (int j = 0; j < 4; j++)
            {
                blendWeight[j] = 0;
            }

            if (VerticeWeights.ContainsKey(verticeIndex))
            {
                var weightInfo = VerticeWeights[verticeIndex];
                weightInfo = weightInfo.OrderByDescending(w => w.Weight).ToList();
                var count = Math.Min(weightInfo.Count, 4);

                for (int j = 0; j < count; j++)
                {
                    blendWeight[j] = weightInfo[j].Weight;
                }
            }
            var result = new Vector4(blendWeight[0], blendWeight[1], blendWeight[2], blendWeight[3]);
            return result;
        }

        Vector4 GetBlendIndices(Dictionary<int, List<VerticeWeight>> VerticeWeights, int verticeIndex)
        {
            var blendIndices = new float[4];
            for (int j = 0; j < 4; j++)
            {
                blendIndices[j] = 0;
            }

            if (VerticeWeights.ContainsKey(verticeIndex))
            {
                var weightInfo = VerticeWeights[verticeIndex];
                weightInfo = weightInfo.OrderByDescending(w => w.Weight).ToList();
                var count = Math.Min(weightInfo.Count, 4);

                for (int j = 0; j < count; j++)
                {
                    blendIndices[j] = (float)weightInfo[j].Bone.Index;
                }
            }

            return new Vector4(blendIndices[0], blendIndices[1], blendIndices[2], blendIndices[3]);
        }
    }
}
