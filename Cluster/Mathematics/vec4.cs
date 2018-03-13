using System;

namespace Cluster.Mathematics
{
    public class Vec4
    {
        public float x, y, z, w;

        public Vec4()
        {
            x = y = z = w = 0.0f;
        }

        public Vec4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vec4(Vec3 v, float w)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = w;
        }

        public static double operator *(Vec4 a, Vec4 b) //Skalarprodukt
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static Vec4 operator %(Vec4 a, Vec4 b) //Kreuzprodukt
        {
            Vec4 c = new Vec4(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x, 0.0f);
            return c;
        }

        public static Vec4 operator +(Vec4 a, Vec4 b)
        {
            Vec4 c = new Vec4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
            return c;
        }

        public static Vec4 operator -(Vec4 a, Vec4 b)
        {
            Vec4 c = new Vec4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
            return c;
        }


        public float length()
        {
            return (float) Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        public float r()
        {
            return x;
        }

        public float g()
        {
            return y;
        }

        public float b()
        {
            return z;
        }

        public float a()
        {
            return w;
        }

        public void mult(float scalar)
        {
            x *= scalar;
            y *= scalar;
            z *= scalar;
            w *= scalar;
        }
    }
}