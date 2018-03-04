using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cluster
{
    namespace math
    {
        class vec3
        {
            public float x, y, z;

            public vec3()
            {
                x = y = z = 0.0f;
            }
            public vec3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public vec3(vec3 v3)
            {
                this.x = v3.x;
                this.y = v3.y;
                this.z = v3.z;
            }
            public vec3(vec4 v4)
            {
                this.x = v4.x;
                this.y = v4.y;
                this.z = v4.z;
            }

            public static float operator *(vec3 a, vec3 b) //Skalarprodukt
            {
                return a.x * b.x + a.y * b.y + a.z * b.z;
            }
            public static vec3 operator %(vec3 a, vec3 b)   //Kreuzprodukt
            {
                vec3 c = new vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
                return c;
            }
            public static vec3 operator +(vec3 a, vec3 b)
            {
                vec3 c = new vec3(a.x + b.x, a.y + b.y, a.z + b.z);
                return c;
            }
            public static vec3 operator +(float a, vec3 b)
            {
                vec3 c = new vec3(a + b.x, a + b.y, a + b.z);
                return c;
            }
            public static vec3 operator +(vec3 b, float a)
            {
                vec3 c = new vec3(a + b.x, a + b.y, a + b.z);
                return c;
            }
            public static vec3 operator -(vec3 a, vec3 b)
            {
                vec3 c = new vec3(a.x - b.x, a.y - b.y, a.z - b.z);
                return c;
            }

            public static vec3 operator *(vec3 a, float b)
            {
                return new vec3(a.x * b, a.y * b, a.z * b);
            }
            public static vec3 operator *(float b, vec3 a)
            {
                return new vec3(a.x * b, a.y * b, a.z * b);
            }
            public static vec3 operator /(vec3 a, float b)
            {
                return new vec3(a.x / b, a.y / b, a.z / b);
            }
            public static vec3 operator /(float b, vec3 a)
            {
                return new vec3(b / a.x, b / a.y, b / a.z);
            }

            public float length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }

            public vec3 normalize()
            {
                return this / length();
            }

        }
    }
}
