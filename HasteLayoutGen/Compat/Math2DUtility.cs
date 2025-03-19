using System.Numerics;

namespace HasteLayoutGen.Compat
{
    public class Math2DUtility
    {
        public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
        {
            float num = 1E-05f;
            bool flag = false;
            float num2 = (l2_p2.Y - l2_p1.Y) * (l1_p2.X - l1_p1.X) - (l2_p2.X - l2_p1.X) * (l1_p2.Y - l1_p1.Y);
            if (num2 != 0f)
            {
                float num3 = ((l2_p2.X - l2_p1.X) * (l1_p1.Y - l2_p1.Y) - (l2_p2.Y - l2_p1.Y) * (l1_p1.X - l2_p1.X)) / num2;
                float num4 = ((l1_p2.X - l1_p1.X) * (l1_p1.Y - l2_p1.Y) - (l1_p2.Y - l1_p1.Y) * (l1_p1.X - l2_p1.X)) / num2;
                if (shouldIncludeEndPoints)
                {
                    if (num3 >= 0f + num && num3 <= 1f - num && num4 >= 0f + num && num4 <= 1f - num)
                    {
                        flag = true;
                    }
                }
                else if (num3 > 0f + num && num3 < 1f - num && num4 > 0f + num && num4 < 1f - num)
                {
                    flag = true;
                }
            }
            return flag;
        }
    }
}
