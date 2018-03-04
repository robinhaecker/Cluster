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
	class Primitives
	{

		// Textdarstellung
		static Shader polygon_shader;


		const int BUFFER_SIZE = 3000;

		static float[] rgba;   //r, g, b, a
		static float[] pos;    //x, y, depth

		static int buf_rgba, buf_pos;
		static int vertexBufferArray;
		static int num_vertices;

		static public float default_red = 1.0f, default_green = 1.0f, default_blue = 1.0f, default_alpha = 1.0f;
		static public float default_depth = 0.0f;
		static public float default_line_width = 1.0f;



		static public void setDepth(float z)
		{
			default_depth = z;
		}
		static public void setColor(float r, float g, float b, float a)
		{
			default_alpha = a;
			default_red = r;
			default_green = g;
			default_blue = b;
		}
		static public void setColor(float r, float g, float b)
		{
			default_red = r;
			default_green = g;
			default_blue = b;
		}
		static public void setAlpha(float a)
		{
			default_alpha = a;
		}
		static public void setLineWidth(float w = 1.0f)
		{
			default_line_width = w;
		}
		static public float getDepth()
		{
			return default_depth;
		}
		static public vec4 getColor()
		{
			return new vec4(default_red, default_green, default_blue, default_alpha);
		}
		static public float getLineWidth()
		{
			return default_line_width;
		}


		static public void drawRect(float x, float y, float width, float height)
		{
			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 2] = default_depth;
			pos[num_vertices * 3 + 0] = x; pos[num_vertices * 3 + 1] = y;
			num_vertices++;

			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 2] = default_depth;
			pos[num_vertices * 3 + 0] = x; pos[num_vertices * 3 + 1] = y + height;
			num_vertices++;

			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 2] = default_depth;
			pos[num_vertices * 3 + 0] = x + width; pos[num_vertices * 3 + 1] = y + height;
			num_vertices++;
			if (num_vertices >= BUFFER_SIZE) renderPolys();

			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 2] = default_depth;
			pos[num_vertices * 3 + 0] = x; pos[num_vertices * 3 + 1] = y;
			num_vertices++;

			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 2] = default_depth;
			pos[num_vertices * 3 + 0] = x + width; pos[num_vertices * 3 + 1] = y + height;
			num_vertices++;

			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 2] = default_depth;
			pos[num_vertices * 3 + 0] = x + width; pos[num_vertices * 3 + 1] = y;
			num_vertices++;
			if (num_vertices >= BUFFER_SIZE) renderPolys();
		}
		static public void drawTriangle(vec2 v0, vec2 v1, vec2 v2)
		{
			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 0] = v0.x; pos[num_vertices * 3 + 1] = v0.y; pos[num_vertices * 3 + 2] = default_depth;
			num_vertices++;
			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 0] = v1.x; pos[num_vertices * 3 + 1] = v1.y; pos[num_vertices * 3 + 2] = default_depth;
			num_vertices++;
			rgba[num_vertices * 4 + 0] = default_red; rgba[num_vertices * 4 + 1] = default_green; rgba[num_vertices * 4 + 2] = default_blue; rgba[num_vertices * 4 + 3] = default_alpha;
			pos[num_vertices * 3 + 0] = v2.x; pos[num_vertices * 3 + 1] = v2.y; pos[num_vertices * 3 + 2] = default_depth;
			num_vertices++;

			if (num_vertices >= BUFFER_SIZE) renderPolys();
		}
		static public void drawTriangle(vec2 v0, vec2 v1, vec2 v2, float depth, float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
		{
			rgba[num_vertices * 4 + 0] = r; rgba[num_vertices * 4 + 1] = g; rgba[num_vertices * 4 + 2] = b; rgba[num_vertices * 4 + 3] = a;
			pos[num_vertices * 3 + 0] = v0.x; pos[num_vertices * 3 + 1] = v0.y; pos[num_vertices * 3 + 2] = depth;
			num_vertices++;
			rgba[num_vertices * 4 + 0] = r; rgba[num_vertices * 4 + 1] = g; rgba[num_vertices * 4 + 2] = b; rgba[num_vertices * 4 + 3] = a;
			pos[num_vertices * 3 + 0] = v1.x; pos[num_vertices * 3 + 1] = v1.y; pos[num_vertices * 3 + 2] = depth;
			num_vertices++;
			rgba[num_vertices * 4 + 0] = r; rgba[num_vertices * 4 + 1] = g; rgba[num_vertices * 4 + 2] = b; rgba[num_vertices * 4 + 3] = a;
			pos[num_vertices * 3 + 0] = v2.x; pos[num_vertices * 3 + 1] = v2.y; pos[num_vertices * 3 + 2] = depth;
			num_vertices++;

			if (num_vertices >= BUFFER_SIZE) renderPolys();
		}
		static public void drawLine(float x1, float y1, float x2, float y2)
		{
			drawLine(new vec2(x1, y1), new vec2(x2, y2));
		}
		static public void drawLine(vec2 a, vec2 b)
		{
			vec2 vertical = (b - a).vertical() * (default_line_width * 0.5f);
			drawTriangle(a - vertical, a + vertical, b + vertical);
			drawTriangle(a - vertical, b + vertical, b - vertical);
		}




		public static void init()
		{
			polygon_shader = new Shader("poly.vert", "poly.frag");
			rgba = new float[BUFFER_SIZE * 4];
			pos = new float[BUFFER_SIZE * 3];
		}

		static void set_buffers()
		{

			if (vertexBufferArray == 0)
			{
				vertexBufferArray = GL.GenVertexArray();

				buf_rgba = GL.GenBuffer();
				buf_pos = GL.GenBuffer();
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


			/*
			if (vertexBufferArray == null)
			{
				vertexBufferArray = new VertexBufferArray();
				vertexBufferArray.Create(manager.gl);
			}
			vertexBufferArray.Bind(manager.gl);


			//Vertex-Daten uebergeben
			if (buf_rgba == null)
			{
				buf_rgba = new VertexBuffer();
				buf_rgba.Create(manager.gl);
				buf_pos = new VertexBuffer();
				buf_pos.Create(manager.gl);
			}

			buf_rgba.Bind(manager.gl);
			//manager.gl.BufferData(OpenGL.GL_ARRAY_BUFFER, text_rgba, OpenGL.GL_DYNAMIC_DRAW);
			//manager.gl.VertexAttribPointer(0, num_chars, )
			buf_rgba.SetData(manager.gl, 1, rgba, false, 4);

			buf_pos.Bind(manager.gl);
			buf_pos.SetData(manager.gl, 0, pos, false, 3);
			//text_gl_data.Unbind(manager.gl);
			 * */
		}
		internal static void renderPolys()
		{
			if (num_vertices == 0) return;
			set_buffers();
			polygon_shader.bind();

			GL.Uniform3(polygon_shader.getUniformLocation("viewport"), 1.0f / (float)GameWindow.active.width, 1.0f / (float)GameWindow.active.height, 0.0f);
			GL.DrawArrays(PrimitiveType.Triangles, 0, num_vertices);
			Shader.unbind();
			num_vertices = 0;
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}





	}
}
