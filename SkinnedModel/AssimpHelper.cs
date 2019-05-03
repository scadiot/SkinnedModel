using Assimp;
using Microsoft.Xna.Framework;

namespace DopaEngine
{
    class AssimpHelper
    {
        static public Vector3 VectorAssimpToXna(Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }

        static public Matrix MatrixAssimpToXna(Assimp.Matrix4x4 matrix)
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
