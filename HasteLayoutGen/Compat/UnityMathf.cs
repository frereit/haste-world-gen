
namespace HasteLayoutGen.Compat
{
    internal static class UnityMathf
    {
        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return Clamp01((value - a) / (b - a));
            }
            return 0;
        }
        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            if (value > 1f)
            {
                return 1f;
            }
            return value;
        }

        internal static int RoundToInt(float v)
        {
            return (int)MathF.Round(v, MidpointRounding.ToEven);
        }

        internal static int Sign(float v)
        {
            return MathF.Sign(v);
        }

        internal static float Abs(float v)
        {
            return MathF.Abs(v);
        }

        internal static int Max(int a, int b)
        {
            return Math.Max(a, b);
        }
    }
}

