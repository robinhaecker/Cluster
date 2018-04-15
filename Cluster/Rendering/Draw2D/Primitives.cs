using Cluster.Mathematics;
using Cluster.Properties;
using Cluster.Rendering.Appearance;
using OpenTK.Graphics.OpenGL;

namespace Cluster.Rendering.Draw2D
{
    class Primitives
    {
        // Textdarstellung
        static Shader polygonShader;


        const int BUFFER_SIZE = 3000;

        static float[] rgba; //r, g, b, a
        static float[] pos; //x, y, depth

        static int bufRgba, bufPos;
        static int vertexBufferArray;
        static int numVertices;

        static public float defaultRed = 1.0f, defaultGreen = 1.0f, defaultBlue = 1.0f, defaultAlpha = 1.0f;
        static public float defaultDepth = 0.0f;
        static public float defaultLineWidth = 1.0f;


        static public void setDepth(float z)
        {
            defaultDepth = z;
        }

        static public void setColor(float r, float g, float b, float a)
        {
            defaultAlpha = a;
            defaultRed = r;
            defaultGreen = g;
            defaultBlue = b;
        }

        static public void setColor(float r, float g, float b)
        {
            defaultRed = r;
            defaultGreen = g;
            defaultBlue = b;
        }

        static public void setAlpha(float a)
        {
            defaultAlpha = a;
        }

        static public void setLineWidth(float w = 1.0f)
        {
            defaultLineWidth = w;
        }

        static public float getDepth()
        {
            return defaultDepth;
        }

        static public Vec4 getColor()
        {
            return new Vec4(defaultRed, defaultGreen, defaultBlue, defaultAlpha);
        }

        static public float getLineWidth()
        {
            return defaultLineWidth;
        }


        static public void drawRect(float x, float y, float width, float height)
        {
            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 2] = defaultDepth;
            pos[numVertices * 3 + 0] = x;
            pos[numVertices * 3 + 1] = y;
            numVertices++;

            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 2] = defaultDepth;
            pos[numVertices * 3 + 0] = x;
            pos[numVertices * 3 + 1] = y + height;
            numVertices++;

            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 2] = defaultDepth;
            pos[numVertices * 3 + 0] = x + width;
            pos[numVertices * 3 + 1] = y + height;
            numVertices++;
            if (numVertices >= BUFFER_SIZE) renderPolys();

            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 2] = defaultDepth;
            pos[numVertices * 3 + 0] = x;
            pos[numVertices * 3 + 1] = y;
            numVertices++;

            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 2] = defaultDepth;
            pos[numVertices * 3 + 0] = x + width;
            pos[numVertices * 3 + 1] = y + height;
            numVertices++;

            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 2] = defaultDepth;
            pos[numVertices * 3 + 0] = x + width;
            pos[numVertices * 3 + 1] = y;
            numVertices++;
            if (numVertices >= BUFFER_SIZE) renderPolys();
        }

        static public void drawTriangle(Vec2 v0, Vec2 v1, Vec2 v2)
        {
            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 0] = v0.x;
            pos[numVertices * 3 + 1] = v0.y;
            pos[numVertices * 3 + 2] = defaultDepth;
            numVertices++;
            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 0] = v1.x;
            pos[numVertices * 3 + 1] = v1.y;
            pos[numVertices * 3 + 2] = defaultDepth;
            numVertices++;
            rgba[numVertices * 4 + 0] = defaultRed;
            rgba[numVertices * 4 + 1] = defaultGreen;
            rgba[numVertices * 4 + 2] = defaultBlue;
            rgba[numVertices * 4 + 3] = defaultAlpha;
            pos[numVertices * 3 + 0] = v2.x;
            pos[numVertices * 3 + 1] = v2.y;
            pos[numVertices * 3 + 2] = defaultDepth;
            numVertices++;

            if (numVertices >= BUFFER_SIZE) renderPolys();
        }

        public static void drawTriangle(Vec2 v0, Vec2 v1, Vec2 v2, float depth, float r = 1.0f, float g = 1.0f,
            float b = 1.0f, float a = 1.0f)
        {
            rgba[numVertices * 4 + 0] = r;
            rgba[numVertices * 4 + 1] = g;
            rgba[numVertices * 4 + 2] = b;
            rgba[numVertices * 4 + 3] = a;
            pos[numVertices * 3 + 0] = v0.x;
            pos[numVertices * 3 + 1] = v0.y;
            pos[numVertices * 3 + 2] = depth;
            numVertices++;
            rgba[numVertices * 4 + 0] = r;
            rgba[numVertices * 4 + 1] = g;
            rgba[numVertices * 4 + 2] = b;
            rgba[numVertices * 4 + 3] = a;
            pos[numVertices * 3 + 0] = v1.x;
            pos[numVertices * 3 + 1] = v1.y;
            pos[numVertices * 3 + 2] = depth;
            numVertices++;
            rgba[numVertices * 4 + 0] = r;
            rgba[numVertices * 4 + 1] = g;
            rgba[numVertices * 4 + 2] = b;
            rgba[numVertices * 4 + 3] = a;
            pos[numVertices * 3 + 0] = v2.x;
            pos[numVertices * 3 + 1] = v2.y;
            pos[numVertices * 3 + 2] = depth;
            numVertices++;

            if (numVertices >= BUFFER_SIZE) renderPolys();
        }

        public static void drawLine(float x1, float y1, float x2, float y2)
        {
            drawLine(new Vec2(x1, y1), new Vec2(x2, y2));
        }

        public static void drawLine(Vec2 a, Vec2 b)
        {
            Vec2 vertical = (b - a).vertical() * (defaultLineWidth * 0.5f);
            drawTriangle(a - vertical, a + vertical, b + vertical);
            drawTriangle(a - vertical, b + vertical, b - vertical);
        }

        public static void init()
        {
            polygonShader = new Shader(Resources.poly_vert, Resources.poly_frag);
            rgba = new float[BUFFER_SIZE * 4];
            pos = new float[BUFFER_SIZE * 3];
        }

        static void set_buffers()
        {
            if (vertexBufferArray == 0)
            {
                vertexBufferArray = GL.GenVertexArray();

                bufRgba = GL.GenBuffer();
                bufPos = GL.GenBuffer();
            }


            GL.BindVertexArray(vertexBufferArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, pos.Length * sizeof(float), pos, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);


            GL.BindBuffer(BufferTarget.ArrayBuffer, bufRgba);
            GL.BufferData(BufferTarget.ArrayBuffer, rgba.Length * sizeof(float), rgba, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, 0);
        }

        internal static void renderPolys()
        {
            if (numVertices == 0) return;
            set_buffers();
            polygonShader.bind();

            GL.Uniform3(polygonShader.getUniformLocation("viewport"), 1.0f / (float) GameWindow.active.width,
                1.0f / (float) GameWindow.active.height, 0.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 0, numVertices);
            Shader.unbind();
            numVertices = 0;
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}