using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cluster.Mathematics
{
    class Mat4
    {
        public float[,] m;


        public Mat4()
        {
            m = new float[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i, j] = 0.0f;
                }
            }

            m[0, 0] = m[1, 1] = m[2, 2] = m[3, 3] = 1.0f;
        }

        public static Mat4 zeros()
        {
            Mat4 m = new Mat4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m.m[i, j] = 0.0f;
                }
            }

            return m;
        }

        public Mat4(Mat4 other)
        {
            m = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i, j] = other.m[i, j];
                }
            }
        }


        public static Mat4 operator +(Mat4 b, Mat4 c)
        {
            Mat4 a = new Mat4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    a.m[i, j] = b.m[i, j] + c.m[i, j];
                }
            }

            return a;
        }

        public static Mat4 operator -(Mat4 b, Mat4 c)
        {
            Mat4 a = new Mat4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    a.m[i, j] = b.m[i, j] - c.m[i, j];
                }
            }

            return a;
        }

        public static Mat4 operator *(Mat4 b, Mat4 c)
        {
            Mat4 a = new Mat4();
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    a.m[x, y] = 0.0f;
                    for (int k = 0; k < 4; k++)
                    {
                        a.m[x, y] += b.m[k, y] * c.m[x, k];
                    }
                }
            }

            return a;
        }

        public static Vec4 operator *(Mat4 b, Vec4 c)
        {
            Vec4 a = new Vec4();
            a.x = b.m[0, 0] * c.x + b.m[1, 0] * c.y + b.m[2, 0] * c.z + b.m[3, 0] * c.w;
            a.y = b.m[0, 1] * c.x + b.m[1, 1] * c.y + b.m[2, 1] * c.z + b.m[3, 1] * c.w;
            a.z = b.m[0, 2] * c.x + b.m[1, 2] * c.y + b.m[2, 2] * c.z + b.m[3, 2] * c.w;
            a.w = b.m[0, 3] * c.x + b.m[1, 3] * c.y + b.m[2, 3] * c.z + b.m[3, 3] * c.w;
            return a;
        }

        public Mat4 inverse()
        {
            float determinant = m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) -
                                m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
                                m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
            if (determinant == 0) return null;
            Mat4 inverse = new Mat4();

            inverse.m[0, 0] = (m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) / determinant;
            inverse.m[0, 1] = (m[2, 1] * m[0, 2] - m[0, 1] * m[2, 2]) / determinant;
            inverse.m[0, 2] = (m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1]) / determinant;

            inverse.m[1, 0] = (-m[1, 0] * m[2, 2] + m[2, 0] * m[1, 2]) / determinant;
            inverse.m[1, 1] = (-m[2, 0] * m[0, 2] + m[0, 0] * m[2, 2]) / determinant;
            inverse.m[1, 2] = (-m[0, 0] * m[1, 2] + m[0, 2] * m[1, 0]) / determinant;

            inverse.m[2, 0] = (m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]) / determinant;
            inverse.m[2, 1] = (m[2, 0] * m[0, 1] - m[0, 0] * m[2, 1]) / determinant;
            inverse.m[2, 2] = (m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0]) / determinant;


            //inverse.m[3, 0] = -m[3, 0];
            //inverse.m[3, 1] = -m[3, 1];
            //inverse.m[3, 2] = -m[3, 2];
            return inverse * translate(-m[3, 0], -m[3, 1], -m[3, 2]);
        }
        /*
        public mat4 inverse()
        {
            float determinant=m[0,0] * (m[1,1]*m[2,2]-m[1,2]*m[2,1]) - m[0,1] * (m[1,0]*m[2,2]-m[1,2]*m[2,0]) + m[0,2] * (m[1,0]*m[2,1]-m[1,1]*m[2,0]);
            if(determinant==0) return null;
            mat4 inverse = new mat4();
    
            inverse.m[0,0]=(m[1,1]*m[2,2]-m[2,1]*m[1,2])/determinant;
            inverse.m[0,1]=(m[1,2]*m[2,0]-m[1,0]*m[2,2])/determinant;
            inverse.m[0,2]=(m[1,0]*m[2,1]-m[2,0]*m[1,1])/determinant;
    
            inverse.m[1,0]=(-m[0,1]*m[2,2]+m[0,2]*m[2,1])/determinant;
            inverse.m[1,1]=(-m[2,0]*m[0,2]+m[0,0]*m[2,2])/determinant;
            inverse.m[1,2]=(-m[0,0]*m[2,1]+m[2,0]*m[0,1])/determinant;
    
            inverse.m[2,0]=(m[0,1]*m[1,2]-m[0,2]*m[1,1])/determinant;
            inverse.m[2,1]=(m[0,2]*m[1,0]-m[0,0]*m[1,2])/determinant;
            inverse.m[2,2]=(m[0,0]*m[1,1]-m[0,1]*m[1,0])/determinant;
            
            inverse.m[3, 0] = -m[3, 0];
            inverse.m[3, 0] = -m[3, 1];
            inverse.m[3, 0] = -m[3, 2];

            return inverse;
        }
        */


        public static Mat4 scale(float sx, float sy, float sz, float sw = 1.0f)
        {
            Mat4 s = new Mat4();
            s.m[0, 0] = sx;
            s.m[1, 1] = sy;
            s.m[2, 2] = sz;
            s.m[3, 3] = sw;
            return s;
        }

        public static Mat4 scale(Vec3 v, float sw = 1.0f)
        {
            Mat4 s = new Mat4();
            s.m[0, 0] = v.x;
            s.m[1, 1] = v.y;
            s.m[2, 2] = v.z;
            s.m[3, 3] = sw;
            return s;
        }

        public static Mat4 scale(Vec4 v)
        {
            Mat4 s = new Mat4();
            s.m[0, 0] = v.x;
            s.m[1, 1] = v.y;
            s.m[2, 2] = v.z;
            s.m[3, 3] = v.w;
            return s;
        }

        public static Mat4 translate(float x, float y, float z)
        {
            Mat4 t = new Mat4();
            t.m[3, 0] = x;
            t.m[3, 1] = y;
            t.m[3, 2] = z;
            return t;
        }

        public static Mat4 translate(Vec3 v)
        {
            Mat4 t = new Mat4();
            t.m[3, 0] = v.x;
            t.m[3, 1] = v.y;
            t.m[3, 2] = v.z;
            return t;
        }

        public static Mat4 translate(Vec4 v)
        {
            Mat4 t = new Mat4();
            t.m[3, 0] = v.x;
            t.m[3, 1] = v.y;
            t.m[3, 2] = v.z;
            t.m[3, 3] = v.w;
            return t;
        }

        public static Mat4 rotate(float alpha, float x, float y, float z)
        {
            float w = (float) Math.Cos(alpha / 2.0);
            float ll = (float) (Math.Sin(alpha / 2.0) / Math.Sqrt(x * x + y * y + z * z));
            x *= ll;
            y *= ll;
            z *= ll;
            float xx = x * x * 2.0f, yy = y * y * 2.0f, zz = z * z * 2.0f;
            Mat4 mat = new Mat4();

            mat.m[0, 0] = 1.0f - (yy + zz);
            mat.m[0, 1] = 2.0f * (x * y + w * z);
            mat.m[0, 2] = 2.0f * (x * z - w * y);

            mat.m[1, 0] = 2.0f * (x * y - w * z);
            mat.m[1, 1] = 1.0f - (xx + zz);
            mat.m[1, 2] = 2.0f * (y * z + w * x);

            mat.m[2, 0] = 2.0f * (x * z + w * y);
            mat.m[2, 1] = 2.0f * (y * z - w * x);
            mat.m[2, 2] = 1.0f - (xx + yy);
            return mat;
        }

        public static Mat4 rotate(float alpha, Vec3 v)
        {
            return rotate(alpha, v.x, v.y, v.z);
        }

        public static Mat4 rotateX(double alpha)
        {
            Mat4 mat = new Mat4();
            mat.m[1, 1] = (float) Math.Cos(alpha);
            mat.m[1, 2] = (float) Math.Sin(alpha);
            mat.m[2, 1] = -mat.m[1, 2]; //=-Sin(angle)
            mat.m[2, 2] = mat.m[1, 1]; //= Cos(angle)
            return mat;
        }

        public static Mat4 rotateY(double alpha)
        {
            Mat4 mat = new Mat4();
            mat.m[2, 2] = (float) Math.Cos(alpha);
            mat.m[2, 0] = (float) Math.Sin(alpha);
            mat.m[0, 2] = -mat.m[2, 0]; //=-Sin(angle)
            mat.m[0, 0] = mat.m[2, 2]; //= Cos(angle)
            return mat;
        }

        public static Mat4 rotateZ(double alpha)
        {
            Mat4 mat = new Mat4();
            mat.m[0, 0] = (float) Math.Cos(alpha);
            mat.m[0, 1] = (float) Math.Sin(alpha);
            mat.m[1, 0] = -mat.m[0, 1]; //=-Sin(angle)
            mat.m[1, 1] = mat.m[0, 0]; //= Cos(angle)
            return mat;
        }

        public static Mat4 projectionOrtho(float left, float right, float bottom, float top, float near, float far)
        {
            Mat4 proj = new Mat4();
            proj.m[0, 0] = 2.0f / (left - right);
            proj.m[1, 1] = 2.0f / (top - bottom);
            proj.m[2, 2] = 2.0f / (near - far);

            proj.m[3, 0] = (right + left) / (right - left);
            proj.m[3, 1] = (bottom + top) / (bottom - top);
            proj.m[3, 2] = (near + far) / (near - far);
            return proj;
        }

        public static Mat4 projectionPerspective(float left = -1.0f, float right = 1.0f, float bottom = -1.0f,
            float top = 1.0f, float near = 0.1f, float far = 10000.0f)
        {
            Mat4 proj = new Mat4();
            proj.m[0, 0] = 2.0f * near / (right - left);
            proj.m[1, 1] = 2.0f * near / (top - bottom);
            proj.m[2, 0] = (right + left) / (right - left);
            proj.m[2, 1] = (top + bottom) / (top - bottom);
            proj.m[2, 2] = -(far + near) / (far - near);
            proj.m[2, 3] = -1.0f;
            proj.m[3, 2] = -2.0f / (far - near);
            return proj;
        }

        public void setValue(int x, int y, float value)
        {
            m[x, y] = value;
        }

        public void setAxis(int axis, Vec3 values)
        {
            m[axis, 0] = values.x;
            m[axis, 1] = values.y;
            m[axis, 2] = values.z;
        }

        public void setAxis(int axis, float valuesx, float valuesy, float valuesz)
        {
            m[axis, 0] = valuesx;
            m[axis, 1] = valuesy;
            m[axis, 2] = valuesz;
        }

        public void setAxis(int axis, float valuesx, float valuesy, float valuesz, float valuesw)
        {
            m[axis, 0] = valuesx;
            m[axis, 1] = valuesy;
            m[axis, 2] = valuesz;
            m[axis, 3] = valuesw;
        }

        public void setAxis(int axis, Vec4 values)
        {
            m[axis, 0] = values.x;
            m[axis, 1] = values.y;
            m[axis, 2] = values.z;
            m[axis, 3] = values.w;
        }

        public double getValue(int x, int y)
        {
            return m[x, y];
        }

        public Vec3 getAxis3(int axis)
        {
            return new Vec3(m[axis, 0], m[axis, 1], m[axis, 2]);
        }

        public Vec4 getAxis(int axis)
        {
            return new Vec4(m[axis, 0], m[axis, 1], m[axis, 2], m[axis, 3]);
        }

        public double getAxisLength(int axis)
        {
            return Math.Sqrt(m[axis, 0] * m[axis, 0] + m[axis, 1] * m[axis, 1] + m[axis, 2] * m[axis, 2] +
                                    m[axis, 3] * m[axis, 3]);
        }

        public Vec4 getLine(int i)
        {
            return new Vec4(m[0, i], m[1, i], m[2, i], m[3, i]);
        }

        public float[] convertToGl()
        {
            return new float[16]
            {
                (float) m[0, 0], (float) m[0, 1], (float) m[0, 2], (float) m[0, 3],
                (float) m[1, 0], (float) m[1, 1], (float) m[1, 2], (float) m[1, 3],
                (float) m[2, 0], (float) m[2, 1], (float) m[2, 2], (float) m[2, 3],
                (float) m[3, 0], (float) m[3, 1], (float) m[3, 2], (float) m[3, 3]
            };
        }

        public void writeLog(string text = "")
        {
            Console.WriteLine("Matrix: " + text);
            Console.WriteLine("[{0}   {1}   {2}   {3}]", m[0, 0], m[1, 0], m[2, 0], m[3, 0]);
            Console.WriteLine("[{0}   {1}   {2}   {3}]", m[0, 1], m[1, 1], m[2, 1], m[3, 1]);
            Console.WriteLine("[{0}   {1}   {2}   {3}]", m[0, 2], m[1, 2], m[2, 2], m[3, 2]);
            Console.WriteLine("[{0}   {1}   {2}   {3}]", m[0, 3], m[1, 3], m[2, 3], m[3, 3]);
        }
    }
}