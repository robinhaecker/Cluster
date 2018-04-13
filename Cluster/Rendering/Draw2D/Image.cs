using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Cluster.Mathematics;
using Cluster.Properties;
using Cluster.Rendering.Appearance;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Cluster.Rendering.Draw2D
{
    class Image
    {
        static Shader imageShader;
        static List<Image> rendered;
        const int BUFFER_SIZE = 300;


        float[] rgba; //r, g, b, a
        float[] uv; //texture coordinates
        float[] pos; //x, y, depth


        static int bufRgba;
        private static int bufPos;
        static int bufUv;
        static int vertexBufferArray;
        int numVertices;
        private bool inrenderlist;


        public int tex;
        float width, height;
        float scalex;
        float scaley;
        private bool center;
        List<Vec4> frames;
        public bool additiveBlending;


        public Image(int glTextureId, int count = 1, int count2 = 512)
        {
            rgba = new float[BUFFER_SIZE * 4];
            pos = new float[BUFFER_SIZE * 3];
            uv = new float[BUFFER_SIZE * 2];

            tex = glTextureId;
            width = 512.0f;
            height = 512.0f;
            scalex = 1.0f;
            scaley = 1.0f;
            frames = new List<Vec4>();
            frames.Add(new Vec4(0.0f, 0.0f, 1.0f / count, 1.0f / count2));
        }


        public Image(string url)
        {
            rgba = new float[BUFFER_SIZE * 4];
            pos = new float[BUFFER_SIZE * 3];
            uv = new float[BUFFER_SIZE * 2];

            frames = null;

            Bitmap data = new Bitmap(GameWindow.BASE_FOLDER + "textures/" + url);
            BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.LinearMipmapLinear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            data.UnlockBits(bdat);


            width = (float) data.Width;
            height = (float) data.Height;
            scalex = 1.0f;
            scaley = 1.0f;

            frames = new List<Vec4>();
            frames.Add(new Vec4(0.0f, 0.0f, 1.0f, 1.0f));
        }

        public Image(string url, int xframes = 1, int yframes = 1)
        {
            rgba = new float[BUFFER_SIZE * 4];
            pos = new float[BUFFER_SIZE * 3];
            uv = new float[BUFFER_SIZE * 2];


            frames = null;

            Bitmap data = new Bitmap(GameWindow.BASE_FOLDER + "textures/" + url);
            BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.LinearMipmapLinear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            data.UnlockBits(bdat);

            width = (float) data.Width / (float) xframes;
            height = (float) data.Height / (float) yframes;
            scalex = 1.0f;
            scaley = 1.0f;

            frames = new List<Vec4>();
            float fscX = 1.0f / (float) xframes, fscY = 1.0f / (float) yframes;
            for (int y = 0; y < yframes; y++)
            {
                for (int x = 0; x < xframes; x++)
                {
                    frames.Add(new Vec4(fscX * (float) x, fscY * (float) y, fscX * (float) (x + 1),
                        fscY * (float) (y + 1)));
                }
            }
        }

        public Image setClamp(TextureWrapMode clampS, TextureWrapMode clampT)
        {
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) clampS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) clampT);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return this;
        }

        public void setScale(float sx, float sy)
        {
            scalex = sx;
            scaley = sy;
        }

        public void draw(float x, float y, float rot = 0.0f, int frame = 0)
        {
            frame = frame % frames.Count;
            rot = -rot;

            if (!inrenderlist)
            {
                inrenderlist = true;
                rendered.Add(this);
            }

            Vec2 a = new Vec2(0.5f * width * scalex * (float) Math.Cos(rot),
                0.5f * width * scalex * (float) Math.Sin(rot));
            Vec2 b = new Vec2(-0.5f * height * scaley * (float) Math.Sin(rot),
                0.5f * height * scaley * (float) Math.Cos(rot));

            if (!center)
            {
                x += scalex * width * 0.5f;
                y += scaley * height * 0.5f;
            }

            rgba[numVertices * 4 + 0] = Primitives.defaultRed;
            rgba[numVertices * 4 + 1] = Primitives.defaultGreen;
            rgba[numVertices * 4 + 2] = Primitives.defaultBlue;
            rgba[numVertices * 4 + 3] = Primitives.defaultAlpha;
            pos[numVertices * 3 + 2] = Primitives.defaultDepth;
            pos[numVertices * 3 + 0] = x - a.x - b.x;
            pos[numVertices * 3 + 1] = y - a.y - b.y;
            uv[numVertices * 2 + 0] = (float) frames[frame].x;
            uv[numVertices * 2 + 1] = (float) frames[frame].y;
            numVertices++;
            rgba[numVertices * 4 + 0] = Primitives.defaultRed;
            rgba[numVertices * 4 + 1] = Primitives.defaultGreen;
            rgba[numVertices * 4 + 2] = Primitives.defaultBlue;
            rgba[numVertices * 4 + 3] = Primitives.defaultAlpha;
            pos[numVertices * 3 + 2] = Primitives.defaultDepth;
            pos[numVertices * 3 + 0] = x - a.x + b.x;
            pos[numVertices * 3 + 1] = y - a.y + b.y;
            uv[numVertices * 2 + 0] = (float) frames[frame].x;
            uv[numVertices * 2 + 1] = (float) frames[frame].w;
            numVertices++;
            rgba[numVertices * 4 + 0] = Primitives.defaultRed;
            rgba[numVertices * 4 + 1] = Primitives.defaultGreen;
            rgba[numVertices * 4 + 2] = Primitives.defaultBlue;
            rgba[numVertices * 4 + 3] = Primitives.defaultAlpha;
            pos[numVertices * 3 + 2] = Primitives.defaultDepth;
            pos[numVertices * 3 + 0] = x + a.x + b.x;
            pos[numVertices * 3 + 1] = y + a.y + b.y;
            uv[numVertices * 2 + 0] = (float) frames[frame].z;
            uv[numVertices * 2 + 1] = (float) frames[frame].w;
            numVertices++;

            rgba[numVertices * 4 + 0] = Primitives.defaultRed;
            rgba[numVertices * 4 + 1] = Primitives.defaultGreen;
            rgba[numVertices * 4 + 2] = Primitives.defaultBlue;
            rgba[numVertices * 4 + 3] = Primitives.defaultAlpha;
            pos[numVertices * 3 + 2] = Primitives.defaultDepth;
            pos[numVertices * 3 + 0] = x - a.x - b.x;
            pos[numVertices * 3 + 1] = y - a.y - b.y;
            uv[numVertices * 2 + 0] = (float) frames[frame].x;
            uv[numVertices * 2 + 1] = (float) frames[frame].y;
            numVertices++;
            rgba[numVertices * 4 + 0] = Primitives.defaultRed;
            rgba[numVertices * 4 + 1] = Primitives.defaultGreen;
            rgba[numVertices * 4 + 2] = Primitives.defaultBlue;
            rgba[numVertices * 4 + 3] = Primitives.defaultAlpha;
            pos[numVertices * 3 + 2] = Primitives.defaultDepth;
            pos[numVertices * 3 + 0] = x + a.x + b.x;
            pos[numVertices * 3 + 1] = y + a.y + b.y;
            uv[numVertices * 2 + 0] = (float) frames[frame].z;
            uv[numVertices * 2 + 1] = (float) frames[frame].w;
            numVertices++;
            rgba[numVertices * 4 + 0] = Primitives.defaultRed;
            rgba[numVertices * 4 + 1] = Primitives.defaultGreen;
            rgba[numVertices * 4 + 2] = Primitives.defaultBlue;
            rgba[numVertices * 4 + 3] = Primitives.defaultAlpha;
            pos[numVertices * 3 + 2] = Primitives.defaultDepth;
            pos[numVertices * 3 + 0] = x + a.x - b.x;
            pos[numVertices * 3 + 1] = y + a.y - b.y;
            uv[numVertices * 2 + 0] = (float) frames[frame].z;
            uv[numVertices * 2 + 1] = (float) frames[frame].y;
            numVertices++;
            //System.Diagnostics.Debugger.Break();
            if (numVertices >= BUFFER_SIZE) renderImages();
        }


        public static void init()
        {
            imageShader = new Shader(Resources.image_vert, Resources.image_frag);
            rendered = new List<Image>();
        }

        void set_buffers()
        {
            if (vertexBufferArray == 0)
            {
                vertexBufferArray = GL.GenVertexArray();

                bufRgba = GL.GenBuffer();
                bufPos = GL.GenBuffer();
                bufUv = GL.GenBuffer();
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufUv);
            GL.BufferData(BufferTarget.ArrayBuffer, uv.Length * sizeof(float), uv, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);

            /*
            if (vertexBufferArray == 0)
            {
                gl_data = new VertexBufferArray();
                gl_data.Create(manager.gl);
            }
            gl_data.Bind(manager.gl);


            //Vertex-Daten uebergeben
            if (buf_rgba == null)
            {
                buf_rgba = new VertexBuffer();
                buf_rgba.Create(manager.gl);
                buf_pos = new VertexBuffer();
                buf_pos.Create(manager.gl);
                buf_uv = new VertexBuffer();
                buf_uv.Create(manager.gl);
            }

            buf_pos.Bind(manager.gl);
            buf_pos.SetData(manager.gl, 0, pos, false, 3);

            buf_rgba.Bind(manager.gl);
            buf_rgba.SetData(manager.gl, 1, rgba, false, 4);
            
            buf_uv.Bind(manager.gl);
            buf_uv.SetData(manager.gl, 2, uv, false, 2);

            tex.bind();*/
        }

        internal static void renderImages()
        {
            imageShader.bind();
            GL.Uniform3(imageShader.getUniformLocation("viewport"), 1.0f / (float) GameWindow.active.width,
                1.0f / (float) GameWindow.active.height, 0.0f);

            foreach (Image im in rendered)
            {
                if (im.numVertices == 0) return;
                im.set_buffers();

                if (im.additiveBlending) GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, im.tex);
                GL.DrawArrays(PrimitiveType.Triangles, 0, im.numVertices);

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                im.numVertices = 0;
                im.inrenderlist = false;
            }

            rendered.Clear();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            Shader.unbind();
        }
    }
}