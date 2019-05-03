using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace DopaEngine
{
    class SimpleModel
    {
        public GraphicsDevice GraphicsDevice { get; set; }
        public string FilePath { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int FaceCount { get; set; }

        class MeshVerticeInfo
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
            public Vector2 TextureCoordinate { get; set; }

            public SimpleModelVertex ToSimpleModelVertex()
            {
                return new SimpleModelVertex(Position, Normal, TextureCoordinate);
            }
        }

        public void Initialize()
        {
            var vertices = new List<MeshVerticeInfo>();
            var indices = new List<int>();

            AssimpContext importer = new AssimpContext();
            Scene aScene = importer.ImportFile(FilePath, PostProcessPreset.TargetRealTimeMaximumQuality);

            foreach (var aMesh in aScene.Meshes)
            {
                for (int faceIndex = 0; faceIndex < aMesh.FaceCount; faceIndex++)
                {
                    for (int vertexNum = 0; vertexNum < 3; vertexNum++)
                    {
                        int verticeIndice = aMesh.Faces[faceIndex].Indices[vertexNum];
                        Vector3 verticePosition = AssimpHelper.VectorAssimpToXna(aMesh.Vertices[verticeIndice]);
                        Vector3 verticeNormal = AssimpHelper.VectorAssimpToXna(aMesh.Normals[verticeIndice]);
                        Vector3 uv = AssimpHelper.VectorAssimpToXna(aMesh.TextureCoordinateChannels[0][verticeIndice]);
                        var verticeUv = new Vector2(uv.X, uv.Y);

                        var vertice = new MeshVerticeInfo()
                        {
                            Position = verticePosition,
                            Normal = verticeNormal,
                            TextureCoordinate = verticeUv
                        };

                        indices.Add(vertices.Count);
                        vertices.Add(vertice);
                    }
                }
                FaceCount += aMesh.FaceCount;
            }

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(SimpleModelVertex), vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData<SimpleModelVertex>(vertices.Select(v => v.ToSimpleModelVertex()).ToArray());
            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices.ToArray());
        }
    }
}
