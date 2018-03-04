using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using Cluster.Rendering.Appearance;
using Cluster.math;


namespace Cluster.Rendering.Draw2D
{
	class Image
	{

		
        static Shader image_shader;
        static List<Image> rendered;
        const int BUFFER_SIZE = 300;


        float[] rgba;   //r, g, b, a
        float[] uv;     //texture coordinates
        float[] pos;    //x, y, depth



        static int buf_rgba, buf_pos, buf_uv;
        static int vertexBufferArray;
        int num_vertices;
        bool inrenderlist;


        public int tex;
        float width, height;
        float scalex, scaley;
        bool center;
        List<vec4> frames;
		public bool additive_blending;


		public Image(int gl_texture_id, int count = 1, int count2 = 512)
		{
			rgba = new float[BUFFER_SIZE * 4];
			pos = new float[BUFFER_SIZE * 3];
			uv = new float[BUFFER_SIZE * 2];

			tex = gl_texture_id;
			width = 512.0f;
			height = 512.0f;
			scalex = 1.0f;
			scaley = 1.0f;
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
			BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			tex = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, tex);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			data.UnlockBits(bdat);


            width = (float)data.Width;
            height = (float)data.Height;
            scalex = 1.0f;
            scaley = 1.0f;

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
			BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			tex = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, tex);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			data.UnlockBits(bdat);

            width = (float)data.Width / (float)xframes;
            height = (float)data.Height / (float)yframes;
            scalex = 1.0f;
            scaley = 1.0f;

            frames = new List<vec4>();
            float fsc_x = 1.0f / (float)xframes, fsc_y = 1.0f / (float)yframes;
            for (int y = 0; y < yframes; y++)
            {
                for (int x = 0; x < xframes; x++)
                {
                    frames.Add(new vec4(fsc_x * (float)x, fsc_y * (float)y, fsc_x * (float)(x+1), fsc_y * (float)(y+1)));
                }
            }

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

            vec2 a = new vec2(0.5f * width * scalex * (float)Math.Cos(rot), 0.5f * width * scalex * (float)Math.Sin(rot));
            vec2 b = new vec2(-0.5f * height * scaley * (float)Math.Sin(rot), 0.5f * height * scaley * (float)Math.Cos(rot));

            if (!center)
            {
                x += scalex * width * 0.5f;
                y += scaley * height * 0.5f;
            }

			rgba[num_vertices * 4 + 0] = Primitives.default_red; rgba[num_vertices * 4 + 1] = Primitives.default_green; rgba[num_vertices * 4 + 2] = Primitives.default_blue; rgba[num_vertices * 4 + 3] = Primitives.default_alpha;
			pos[num_vertices * 3 + 2] = Primitives.default_depth;
            pos[num_vertices * 3 + 0] = x - a.x - b.x; pos[num_vertices * 3 + 1] = y - a.y - b.y;
            uv[num_vertices * 2 + 0] = (float)frames[frame].x; uv[num_vertices * 2 + 1] = (float)frames[frame].y;
            num_vertices++;
			rgba[num_vertices * 4 + 0] = Primitives.default_red; rgba[num_vertices * 4 + 1] = Primitives.default_green; rgba[num_vertices * 4 + 2] = Primitives.default_blue; rgba[num_vertices * 4 + 3] = Primitives.default_alpha;
			pos[num_vertices * 3 + 2] = Primitives.default_depth;
            pos[num_vertices * 3 + 0] = x - a.x + b.x; pos[num_vertices * 3 + 1] = y - a.y + b.y;
            uv[num_vertices * 2 + 0] = (float)frames[frame].x; uv[num_vertices * 2 + 1] = (float)frames[frame].w;
            num_vertices++;
			rgba[num_vertices * 4 + 0] = Primitives.default_red; rgba[num_vertices * 4 + 1] = Primitives.default_green; rgba[num_vertices * 4 + 2] = Primitives.default_blue; rgba[num_vertices * 4 + 3] = Primitives.default_alpha;
			pos[num_vertices * 3 + 2] = Primitives.default_depth;
            pos[num_vertices * 3 + 0] = x + a.x + b.x; pos[num_vertices * 3 + 1] = y + a.y + b.y;
            uv[num_vertices * 2 + 0] = (float)frames[frame].z; uv[num_vertices * 2 + 1] = (float)frames[frame].w;
            num_vertices++;

			rgba[num_vertices * 4 + 0] = Primitives.default_red; rgba[num_vertices * 4 + 1] = Primitives.default_green; rgba[num_vertices * 4 + 2] = Primitives.default_blue; rgba[num_vertices * 4 + 3] = Primitives.default_alpha;
			pos[num_vertices * 3 + 2] = Primitives.default_depth;
            pos[num_vertices * 3 + 0] = x - a.x - b.x; pos[num_vertices * 3 + 1] = y - a.y - b.y;
            uv[num_vertices * 2 + 0] = (float)frames[frame].x; uv[num_vertices * 2 + 1] = (float)frames[frame].y;
            num_vertices++;
			rgba[num_vertices * 4 + 0] = Primitives.default_red; rgba[num_vertices * 4 + 1] = Primitives.default_green; rgba[num_vertices * 4 + 2] = Primitives.default_blue; rgba[num_vertices * 4 + 3] = Primitives.default_alpha;
			pos[num_vertices * 3 + 2] = Primitives.default_depth;
            pos[num_vertices * 3 + 0] = x + a.x + b.x; pos[num_vertices * 3 + 1] = y + a.y + b.y;
            uv[num_vertices * 2 + 0] = (float)frames[frame].z; uv[num_vertices * 2 + 1] = (float)frames[frame].w;
            num_vertices++;
			rgba[num_vertices * 4 + 0] = Primitives.default_red; rgba[num_vertices * 4 + 1] = Primitives.default_green; rgba[num_vertices * 4 + 2] = Primitives.default_blue; rgba[num_vertices * 4 + 3] = Primitives.default_alpha;
			pos[num_vertices * 3 + 2] = Primitives.default_depth;
            pos[num_vertices * 3 + 0] = x + a.x - b.x; pos[num_vertices * 3 + 1] = y + a.y - b.y;
            uv[num_vertices * 2 + 0] = (float)frames[frame].z; uv[num_vertices * 2 + 1] = (float)frames[frame].y;
            num_vertices++;
            //System.Diagnostics.Debugger.Break();
            if (num_vertices >= BUFFER_SIZE) renderImages();
            

        }








        public static void init()
        {
            image_shader = new Shader("image.vert", "image.frag");
            //image_shader = new shader(manager.FILE_DIRECTORY + "/shaders/poly.vert", manager.FILE_DIRECTORY + "/shaders/poly.frag");
            rendered = new List<Image>();
        }
        void set_buffers()
        {

			if (vertexBufferArray == 0)
			{
				vertexBufferArray = GL.GenVertexArray();

				buf_rgba = GL.GenBuffer();
				buf_pos = GL.GenBuffer();
				buf_uv = GL.GenBuffer();
			}


			GL.BindVertexArray(vertexBufferArray);
			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, pos.Length * sizeof(float), pos, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_rgba);
			GL.BufferData(BufferTarget.ArrayBuffer, rgba.Length * sizeof(float), rgba, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 1);
			GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_uv);
			GL.BufferData(BufferTarget.ArrayBuffer, uv.Length * sizeof(float), uv, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 2);
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
			image_shader.bind();
			GL.Uniform3(image_shader.getUniformLocation("viewport"), 1.0f / (float)GameWindow.active.width, 1.0f / (float)GameWindow.active.height, 0.0f);

            foreach (Image im in rendered)
            {
                if (im.num_vertices == 0) return;
				im.set_buffers();

				if (im.additive_blending) GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, im.tex);
				GL.DrawArrays(PrimitiveType.Triangles, 0, im.num_vertices);

				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                im.num_vertices = 0;
                im.inrenderlist = false;
            }
            rendered.Clear();

			GL.BindTexture(TextureTarget.Texture2D, 0);
            image_shader.unbind();
        }

	}
}
