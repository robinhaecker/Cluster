using System;
using Cluster.Mathematics;

namespace Cluster.Mathematics
{
    public class Vec3
    {
        public float x, y, z;

        public Vec3()
        {
            x = y = z = 0.0f;
        }

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3(Vec3 v3)
        {
            this.x = v3.x;
            this.y = v3.y;
            this.z = v3.z;
        }

        public Vec3(Vec4 v4)
        {
            this.x = v4.x;
            this.y = v4.y;
            this.z = v4.z;
        }

        public static float operator *(Vec3 a, Vec3 b) //Skalarprodukt
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vec3 operator %(Vec3 a, Vec3 b) //Kreuzprodukt
        {
            Vec3 c = new Vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
            return c;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            Vec3 c = new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
            return c;
        }

        public static Vec3 operator +(float a, Vec3 b)
        {
            Vec3 c = new Vec3(a + b.x, a + b.y, a + b.z);
            return c;
        }

        public static Vec3 operator +(Vec3 b, float a)
        {
            Vec3 c = new Vec3(a + b.x, a + b.y, a + b.z);
            return c;
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            Vec3 c = new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
            return c;
        }

        public static Vec3 operator *(Vec3 a, float b)
        {
            return new Vec3(a.x * b, a.y * b, a.z * b);
        }

        public static Vec3 operator *(float b, Vec3 a)
        {
            return new Vec3(a.x * b, a.y * b, a.z * b);
        }

        public static Vec3 operator /(Vec3 a, float b)
        {
            return new Vec3(a.x / b, a.y / b, a.z / b);
        }

        public static Vec3 operator /(float b, Vec3 a)
        {
            return new Vec3(b / a.x, b / a.y, b / a.z);
        }

        public float length()
        {
            return (float) Math.Sqrt(x * x + y * y + z * z);
        }

        public Vec3 normalize()
        {
            return this / length();
        }
    }
}