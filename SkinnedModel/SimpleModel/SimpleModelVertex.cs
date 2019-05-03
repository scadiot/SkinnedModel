using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DopaEngine
{
    public struct SimpleModelVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public SimpleModelVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            this.Position = position;
            this.Normal = normal;
            this.TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return string.Format("{{Position:{0} Normal:{1} TextureCoordinate:{2}}}", new object[] { this.Position, this.Normal, this.TextureCoordinate });
        }

        public static bool operator ==(SimpleModelVertex left, SimpleModelVertex right)
        {
            return (((left.Position == right.Position) && (left.Normal == right.Normal)) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        public static bool operator !=(SimpleModelVertex left, SimpleModelVertex right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((SimpleModelVertex)obj));
        }

        static SimpleModelVertex()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            };
            var declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }
    }
}
