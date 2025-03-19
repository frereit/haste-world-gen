using System.Numerics;

namespace HasteLayoutGen.Compat
{
    internal static class UnityVector3
    {
        public static Vector2 xz(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Z);
        }
    }
}
