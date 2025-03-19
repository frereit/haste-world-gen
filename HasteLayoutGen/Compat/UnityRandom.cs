namespace HasteLayoutGen.Compat
{
    internal static class UnityRandom
    {
        public static float Range(this Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }
        public static T Choice<T>(this Random random, List<T> array)
        {
            return array[random.Next(array.Count)];
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }
    }
}

