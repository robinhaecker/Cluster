using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;
using Cluster.Rendering.Appearance;




namespace Cluster.Rendering.Draw3D
{
	class PostProcessing
	{
		static Shader standard;
		static Shader bloom;
		static Shader depth_of_field;

		static int vertexArrayObject;
		static int buf_pos;



		static FrameBuffer screen;
		static int low_res;
		const int BLUR_STRENGTH = 1;//16;




		public static void init()
		{
			vertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(vertexArrayObject);
			
			float[] data = {-1.0f, -1.0f,
						     1.0f, -1.0f,
						     1.0f,  1.0f,
							-1.0f, -1.0f,
						     1.0f,  1.0f,
						    -1.0f,  1.0f};


			buf_pos = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexArrayObject, 0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindVertexArray(0);

			screen = new FrameBuffer();
			screen.BindTexture(0);
			//GL.GenerateTextureMipmap(screen.GetTexture());
			GL.BindTexture(TextureTarget.Texture2D, 0);

			standard = new Shader("pp_std.vert", "pp_std.frag");
			bloom = new Shader("pp_bloom.vert", "pp_bloom.frag");


			low_res = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, low_res);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, GameWindow.active.width / BLUR_STRENGTH, GameWindow.active.height / BLUR_STRENGTH, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

		}




		public static void BindTexture(int layer = 0)
		{
			screen.BindTexture(layer);
		}
		public static void BindDepthTexture(int layer = 0)
		{
			screen.BindDepthTexture(layer);
		}



		public static void render(Camera cam)
		{
			//Vorbereiten
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			screen.BindTexture(0);
			screen.BindDepthTexture(2);
			GL.BindVertexArray(vertexArrayObject);

			//Low-Resolution Textur befüllen
			standard.bind();
			GL.Uniform1(standard.getUniformLocation("near"), cam.getNear());
			GL.Uniform1(standard.getUniformLocation("far"), cam.getFar());
			GL.Viewport(0, 0, GameWindow.active.width / BLUR_STRENGTH, GameWindow.active.height / BLUR_STRENGTH);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			GL.BindTexture(TextureTarget.Texture2D, low_res);
			GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, GameWindow.active.width / BLUR_STRENGTH, GameWindow.active.height / BLUR_STRENGTH, 0);
			GL.Viewport(0, 0, GameWindow.active.width, GameWindow.active.height);


			//Die richtigen Effekte ausführen
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			screen.BindTexture(0);//			screen.BindDepthTexture(0);
			//GameWindow.active.shadows.BindDepthTexture(0);
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, low_res);
			screen.BindDepthTexture(2);

			bloom.bind();
			GL.Uniform1(bloom.getUniformLocation("near"), cam.getNear());
			GL.Uniform1(bloom.getUniformLocation("far"), cam.getFar());
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			GL.UseProgram(0);
			GL.Enable(EnableCap.DepthTest);
		}


		public static void render()
		{
			//Vorbereiten
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			screen.BindTexture(0);
			screen.BindDepthTexture(2);
			GL.BindVertexArray(vertexArrayObject);

			//Low-Resolution Textur befüllen
			standard.bind();
			GL.Uniform1(standard.getUniformLocation("near"), 0.0f);
			GL.Uniform1(standard.getUniformLocation("far"), 1.0f);
			GL.Viewport(0, 0, GameWindow.active.width / BLUR_STRENGTH, GameWindow.active.height / BLUR_STRENGTH);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			GL.BindTexture(TextureTarget.Texture2D, low_res);
			GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, GameWindow.active.width / BLUR_STRENGTH, GameWindow.active.height / BLUR_STRENGTH, 0);
			GL.Viewport(0, 0, GameWindow.active.width, GameWindow.active.height);


			//Die richtigen Effekte ausführen
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			screen.BindTexture(0);//			screen.BindDepthTexture(0);
			//GameWindow.active.shadows.BindDepthTexture(0);
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, low_res);
			screen.BindDepthTexture(2);

			bloom.bind();
			GL.Uniform1(bloom.getUniformLocation("near"), 0.0f);
			GL.Uniform1(bloom.getUniformLocation("far"), 1.0f);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			GL.UseProgram(0);
			GL.Enable(EnableCap.DepthTest);
		}






	}
}
