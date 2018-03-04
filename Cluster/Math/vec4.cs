using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cluster
{
    namespace math
    {

        class vec4
        {
            public float x, y, z, w;

            public vec4()
            {
                x = y = z = w = 0.0f;
            }
            public vec4(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }
            public vec4(vec3 v, float w)
            {
                this.x = v.x;
                this.y = v.y;
                this.z = v.z;
                this.w = w;
            }

            public static double operator *(vec4 a, vec4 b) //Skalarprodukt
            {
                return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            }
            public static vec4 operator %(vec4 a, vec4 b)   //Kreuzprodukt
            {
                vec4 c = new vec4(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x, 0.0f);
                return c;
            }
            public static vec4 operator +(vec4 a, vec4 b)
            {
                vec4 c = new vec4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
                return c;
            }
			public static vec4 operator -(vec4 a, vec4 b)
			{
				vec4 c = new vec4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
				return c;
			}


            public float length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
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
}
