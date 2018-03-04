using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cluster
{
    namespace math
    {
        using math;

        class vec2
        {

            public float x, y;

            public vec2(float vx = 0.0f, float vy = 0.0f)
            {
                x = vx;
                y = vy;
            }

            public static vec2 operator +(vec2 a, vec2 b)
            {
                return new vec2(a.x + b.x, a.y + b.y);
            }
            public static vec2 operator -(vec2 a, vec2 b)
            {
                return new vec2(a.x - b.x, a.y - b.y);
            }
            public static float operator *(vec2 a, vec2 b)
            {
                return a.x * b.x + a.y + b.y;
            }
            public static vec2 operator *(vec2 a, float b)
            {
                return new vec2(a.x * b, a.y * b);
            }
            public static vec2 operator *(float b, vec2 a)
            {
                return new vec2(a.x * b, a.y * b);
            }
            public static vec2 operator /(vec2 a, float b)
            {
                return new vec2(a.x / b, a.y / b);
            }
            public static vec2 operator /(float b, vec2 a)
            {
                return new vec2(b / a.x, b / a.y);
            }


            public float length()
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
            public void normalize()
            {
                float l = length();
                x /= l;
                y /= l;
            }
            public vec2 vertical()
            {
                float l = length();
                return new vec2(-y / l, x / l);
            }


        }
    }
}
