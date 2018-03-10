using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Cluster.math;
using Cluster.Rendering.Appearance;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Cluster.Rendering.Draw2D
{
    class Image
    {
        static Shader _imageShader;
        static List<Image> _rendered;
        const int BUFFER_SIZE = 300;


        float[] rgba; //r, g, b, a
        float[] uv; //texture coordinates
        float[] pos; //x, y, depth


        static int _bufRgba;
        private static int _bufPos;
        static int _bufUv;
        static int _vertexBufferArray;
        int _numVertices;
        private bool _inrenderlist;


        public int tex;
        float width, height;
        float _scalex;
        float _scaley;
        private bool _center;
        List<vec4> frames;
        public bool additiveBlending;


        public Image(int glTextureId, int count = 1, int count2 = 512)
        {
            rgba = new float[BUFFER_SIZE * 4];
            pos = new float[BUFFER_SIZE * 3];
            uv = new float[BUFFER_SIZE * 2];

            tex = glTextureId;
            width = 512.0f;
            height = 512.0f;
            _scalex = 1.0f;
            _scaley = 1.0f;
            frames = new List<vec4>();
            frames.Add(new vec4(0.0f, 0.0f, 1.0f / count, 1.0f / count2));
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
            _scalex = 1.0f;
            _scaley = 1.0f;

            frames = new List<vec4>();
            frames.Add(new vec4(0.0f, 0.0f, 1.0f, 1.0f));
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
            _scalex = 1.0f;
            _scaley = 1.0f;

            frames = new List<vec4>();
            float fsc_x = 1.0f / (float) xframes, fsc_y = 1.0f / (float) yframes;
            for (int y = 0; y < yframes; y++)
            {
                for (int x = 0; x < xframes; x++)
                {
                    frames.Add(new vec4(fsc_x * (float) x, fsc_y * (float) y, fsc_x * (float) (x + 1),
                        fsc_y * (float) (y + 1)));
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
            _scalex = sx;
            _scaley = sy;
        }

        public void draw(float x, float y, float rot = 0.0f, int frame = 0)
        {
            frame = frame % frames.Count;
            rot = -rot;

            if (!_inrenderlist)
            {
                _inrenderlist = true;
                _rendered.Add(this);
            }

            vec2 a = new vec2(0.5f * width * _scalex * (float) Math.Cos(rot),
                0.5f * width * _scalex * (float) Math.Sin(rot));
            vec2 b = new vec2(-0.5f * height * _scaley * (float) Math.Sin(rot),
                0.5f * height * _scaley * (float) Math.Cos(rot));

            if (!_center)
            {
                x += _scalex * width * 0.5f;
                y += _scaley * height * 0.5f;
            }

            rgba[_numVertices * 4 + 0] = Primitives.default_red;
            rgba[_numVertices * 4 + 1] = Primitives.default_green;
            rgba[_numVertices * 4 + 2] = Primitives.default_blue;
            rgba[_numVertices * 4 + 3] = Primitives.default_alpha;
            pos[_numVertices * 3 + 2] = Primitives.default_depth;
            pos[_numVertices * 3 + 0] = x - a.x - b.x;
            pos[_numVertices * 3 + 1] = y - a.y - b.y;
            uv[_numVertices * 2 + 0] = (float) frames[frame].x;
            uv[_numVertices * 2 + 1] = (float) frames[frame].y;
            _numVertices++;
            rgba[_numVertices * 4 + 0] = Primitives.default_red;
            rgba[_numVertices * 4 + 1] = Primitives.default_green;
            rgba[_numVertices * 4 + 2] = Primitives.default_blue;
            rgba[_numVertices * 4 + 3] = Primitives.default_alpha;
            pos[_numVertices * 3 + 2] = Primitives.default_depth;
            pos[_numVertices * 3 + 0] = x - a.x + b.x;
            pos[_numVertices * 3 + 1] = y - a.y + b.y;
            uv[_numVertices * 2 + 0] = (float) frames[frame].x;
            uv[_numVertices * 2 + 1] = (float) frames[frame].w;
            _numVertices++;
            rgba[_numVertices * 4 + 0] = Primitives.default_red;
            rgba[_numVertices * 4 + 1] = Primitives.default_green;
            rgba[_numVertices * 4 + 2] = Primitives.default_blue;
            rgba[_numVertices * 4 + 3] = Primitives.default_alpha;
            pos[_numVertices * 3 + 2] = Primitives.default_depth;
            pos[_numVertices * 3 + 0] = x + a.x + b.x;
            pos[_numVertices * 3 + 1] = y + a.y + b.y;
            uv[_numVertices * 2 + 0] = (float) frames[frame].z;
            uv[_numVertices * 2 + 1] = (float) frames[frame].w;
            _numVertices++;

            rgba[_numVertices * 4 + 0] = Primitives.default_red;
            rgba[_numVertices * 4 + 1] = Primitives.default_green;
            rgba[_numVertices * 4 + 2] = Primitives.default_blue;
            rgba[_numVertices * 4 + 3] = Primitives.default_alpha;
            pos[_numVertices * 3 + 2] = Primitives.default_depth;
            pos[_numVertices * 3 + 0] = x - a.x - b.x;
            pos[_numVertices * 3 + 1] = y - a.y - b.y;
            uv[_numVertices * 2 + 0] = (float) frames[frame].x;
            uv[_numVertices * 2 + 1] = (float) frames[frame].y;
            _numVertices++;
            rgba[_numVertices * 4 + 0] = Primitives.default_red;
            rgba[_numVertices * 4 + 1] = Primitives.default_green;
            rgba[_numVertices * 4 + 2] = Primitives.default_blue;
            rgba[_numVertices * 4 + 3] = Primitives.default_alpha;
            pos[_numVertices * 3 + 2] = Primitives.default_depth;
            pos[_numVertices * 3 + 0] = x + a.x + b.x;
            pos[_numVertices * 3 + 1] = y + a.y + b.y;
            uv[_numVertices * 2 + 0] = (float) frames[frame].z;
            uv[_numVertices * 2 + 1] = (float) frames[frame].w;
            _numVertices++;
            rgba[_numVertices * 4 + 0] = Primitives.default_red;
            rgba[_numVertices * 4 + 1] = Primitives.default_green;
            rgba[_numVertices * 4 + 2] = Primitives.default_blue;
            rgba[_numVertices * 4 + 3] = Primitives.default_alpha;
            pos[_numVertices * 3 + 2] = Primitives.default_depth;
            pos[_numVertices * 3 + 0] = x + a.x - b.x;
            pos[_numVertices * 3 + 1] = y + a.y - b.y;
            uv[_numVertices * 2 + 0] = (float) frames[frame].z;
            uv[_numVertices * 2 + 1] = (float) frames[frame].y;
            _numVertices++;
            //System.Diagnostics.Debugger.Break();
            if (_numVertices >= BUFFER_SIZE) renderImages();
        }


        public static void init()
        {
            _imageShader = new Shader("image.vert", "image.frag");
            //image_shader = new shader(manager.FILE_DIRECTORY + "/shaders/poly.vert", manager.FILE_DIRECTORY + "/shaders/poly.frag");
            _rendered = new List<Image>();
        }

        void set_buffers()
        {
            if (_vertexBufferArray == 0)
            {
                _vertexBufferArray = GL.GenVertexArray();

                _bufRgba = GL.GenBuffer();
                _bufPos = GL.GenBuffer();
                _bufUv = GL.GenBuffer();
            }


            GL.BindVertexArray(_vertexBufferArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, pos.Length * sizeof(float), pos, BufferUsageHint.StaticDraw);
            GL.EnableVertexArrayAttrib(_vertexBufferArray, 0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufRgba);
            GL.BufferData(BufferTarget.ArrayBuffer, rgba.Length * sizeof(float), rgba, BufferUsageHint.StaticDraw);
            GL.EnableVertexArrayAttrib(_vertexBufferArray, 1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufUv);
            GL.BufferData(BufferTarget.ArrayBuffer, uv.Length * sizeof(float), uv, BufferUsageHint.StaticDraw);
            GL.EnableVertexArrayAttrib(_vertexBufferArray, 2);
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
            _imageShader.bind();
            GL.Uniform3(_imageShader.getUniformLocation("viewport"), 1.0f / (float) GameWindow.active.width,
                1.0f / (float) GameWindow.active.height, 0.0f);

            foreach (Image im in _rendered)
            {
                if (im._numVertices == 0) return;
                im.set_buffers();

                if (im.additiveBlending) GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, im.tex);
                GL.DrawArrays(PrimitiveType.Triangles, 0, im._numVertices);

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                im._numVertices = 0;
                im._inrenderlist = false;
            }

            _rendered.Clear();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            Shader.unbind();
        }
    }
}