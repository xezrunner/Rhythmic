public static class AnimEasing
{
    public static float ease_out_quadratic(float a, float b, float t)
    {
        b -= a;
        return -b * t * (t - 2) + a;
    }
}
