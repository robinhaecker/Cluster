using System;

namespace Cluster.Mathematics
{
    public class Vec2
    {
        public float x, y;

        public Vec2(float vx = 0.0f, float vy = 0.0f)
        {
            x = vx;
            y = vy;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x + b.x, a.y + b.y);
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x - b.x, a.y - b.y);
        }

        public static float operator *(Vec2 a, Vec2 b)
        {
            return a.x * b.x + a.y + b.y;
        }

        public static Vec2 operator *(Vec2 a, float b)
        {
            return new Vec2(a.x * b, a.y * b);
        }

        public static Vec2 operator *(float b, Vec2 a)
        {
            return new Vec2(a.x * b, a.y * b);
        }

        public static Vec2 operator /(Vec2 a, float b)
        {
            return new Vec2(a.x / b, a.y / b);
        }

        public static Vec2 operator /(float b, Vec2 a)
        {
            return new Vec2(b / a.x, b / a.y);
        }


        public float length()
        {
            return (float) Math.Sqrt(x * x + y * y);
        }

        public void normalize()
        {
            float l = length();
            x /= l;
            y /= l;
        }

        public Vec2 vertical()
        {
            float l = length();
            return new Vec2(-y / l, x / l);
        }
    }
}