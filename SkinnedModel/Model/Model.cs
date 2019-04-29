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
    class Model
    {
        public Engine Engine;
        public string FilePath { get; set; }
        MeshVerticeInfo[] Vertices { get; set; }
        public int[] Indices { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }

        class MeshVerticeInfo
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
            public Vector2 TextureCoordinate { get; set; }
            public Color Color { get; set; }
            public int MaterialIndex { get; set; }

            public VertexPositionNormalTexture ToVertexPositionNormalTexture()
            {
                return new VertexPositionNormalTexture(Position, Normal, TextureCoordinate, Color.ToVector4());
            }
        }

        public Model(Engine engine)
        {
            Engine = engine;
        }

        public void Initialize()
        {
            var verticesResult = new List<MeshVerticeInfo>();
            var indicesResult = new List<int>();

            AssimpContext importer = new AssimpContext();
            Scene scene = importer.ImportFile(FilePath, PostProcessPreset.TargetRealTimeMaximumQuality);

            foreach (var mesh in scene.Meshes)
            {
                var c = scene.Materials[mesh.MaterialIndex].ColorDiffuse;
                Color color = new Color(new Vector4(c.R, c.G, c.B, c.A));

                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    int verticeIndice1 = mesh.Faces[i].Indices[0];
                    Vector3 verticePosition1 = FromVector(mesh.Vertices[verticeIndice1]);

                    int verticeIndice2 = mesh.Faces[i].Indices[1];
                    Vector3 verticePosition2 = FromVector(mesh.Vertices[verticeIndice2]);

                    int verticeIndice3 = mesh.Faces[i].Indices[2];
                    Vector3 verticePosition3 = FromVector(mesh.Vertices[verticeIndice3]);

                    var direction = Vector3.Cross(verticePosition2 - verticePosition1, verticePosition3 - verticePosition1);
                    var normal = Vector3.Normalize(direction);

                    Vector3 uv = FromVector(mesh.TextureCoordinateChannels[0][verticeIndice1]);
                    var verticeUv1 = new Vector2(uv.X, uv.Y);

                    uv = FromVector(mesh.TextureCoordinateChannels[0][verticeIndice2]);
                    var verticeUv2 = new Vector2(uv.X, uv.Y);

                    uv = FromVector(mesh.TextureCoordinateChannels[0][verticeIndice3]);
                    var verticeUv3 = new Vector2(uv.X, uv.Y);

                    var vertice1 = new MeshVerticeInfo()
                    {
                        Position = verticePosition1,
                        Normal = normal,
                        TextureCoordinate = verticeUv1,
                        Color = color,
                        MaterialIndex = mesh.MaterialIndex
                    };
                    indicesResult.Add(verticesResult.Count);
                    verticesResult.Add(vertice1);

                    var vertice2 = new MeshVerticeInfo()
                    {
                        Position = verticePosition2,
                        Normal = normal,
                        TextureCoordinate = verticeUv2,
                        Color = color,
                        MaterialIndex = mesh.MaterialIndex
                    };
                    indicesResult.Add(verticesResult.Count);
                    verticesResult.Add(vertice2);

                    var vertice3 = new MeshVerticeInfo()
                    {
                        Position = verticePosition3,
                        Normal = normal,
                        TextureCoordinate = verticeUv3,
                        Color = color,
                        MaterialIndex = mesh.MaterialIndex
                    };
                    indicesResult.Add(verticesResult.Count);
                    verticesResult.Add(vertice3);
                }
            }


            Vertices = verticesResult.ToArray();
            Indices = indicesResult.ToArray();

            VertexBuffer = new VertexBuffer(Engine.GraphicsDevice, typeof(VertexPositionNormalTexture), Vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionNormalTexture>(Vertices.Select(v => v.ToVertexPositionNormalTexture()).ToArray());
            IndexBuffer = new IndexBuffer(Engine.GraphicsDevice, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices);
        }

        private Vector3 FromVector(Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }

        public void Draw(GameTime gameTime, Matrix Transformation)
        {
            Engine.MainEffect.Parameters["World"].SetValue(Transformation);
            Engine.MainEffect.Parameters["WorldViewProjection"].SetValue(Transformation * Engine.ViewMatrix * Engine.ProjectionMatrix);
        
            Engine.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Engine.GraphicsDevice.Indices = IndexBuffer;
            Engine.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (EffectPass pass in Engine.MainEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.GraphicsDevice.DrawIndexedPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, 0, 0, Indices.Length / 3);
            }
        }
    }
}
