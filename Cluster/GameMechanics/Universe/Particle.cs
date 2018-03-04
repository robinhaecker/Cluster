using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;


namespace Cluster.GameMechanics.Universe
{
	class Particle
	{

		public static List<Particle> list = new List<Particle>();
		public static List<Particle> removed = new List<Particle>();

		const float GRAVITATION_CONSTANT = 18100.0f;

		const int MAX_RENDERED_PARTICLES = 25000;
		const int RENDER_ARRSIZE_POS = 2;
		const int RENDER_ARRSIZE_COL = 4;

		public static int rendered_count = 0;
		static int shot_gl_data, buf_sh0, buf_sh1;

		static float[] render_buf_pos = new float[MAX_RENDERED_PARTICLES * RENDER_ARRSIZE_POS];
		static float[] render_buf_col = new float[MAX_RENDERED_PARTICLES * RENDER_ARRSIZE_COL];


		float x, y, vx, vy;
		float r, g, b, a;
		float lifespan;

		Planet gravity;

		public Particle(float x, float y, float vx = 0.0f, float vy = 0.0f)
		{
			this.x = x;
			this.y = y;
			this.vx = vx;
			this.vy = vy;
			this.gravity = null;

			r = g = b = a = 1.0f;
			lifespan = 2.0f;

			list.Add(this);
		}

		public static void update(float t = 1.0f)
		{
			t = t * 0.002f;
			foreach (Particle p in list)
			{
				p.lifespan -= t;
				if (p.lifespan <= 0.0f)
				{
					removed.Add(p);
					continue;
				}
				p.x += p.vx*t;
				p.y += p.vy*t;

				if (p.gravity != null)
				{
					float dx = p.gravity.x - p.x;
					float dy = p.gravity.y - p.y;
					float dst2 = (dx * dx + dy * dy);
					if (dst2 < (p.gravity.size * 20.0f)*(p.gravity.size * 20.0f))
					{
						removed.Add(p);
						continue;
					}
					else// if (dst2 < 1000000.0f)
					{
						float dst = 1.0f;// (float)Math.Sqrt(dst2);
						p.vx += dx / (dst2 * dst) * t * GRAVITATION_CONSTANT;
						p.vy += dy / (dst2 * dst) * t * GRAVITATION_CONSTANT;
					}
				}

			}

			foreach (Particle p in removed)
			{
				list.Remove(p);
			}
			removed.Clear();
		}



		public void setColor(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}
		public void setColor(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
		public void setSpeed(float vx, float vy)
		{
			this.vx = vx;
			this.vy = vy;
		}
		public void setGravity(Planet p)
		{
			this.gravity = p;
		}



		public static void render()
		{
			rendered_count = 0;
			foreach (Particle p in Particle.list)
			{
				render_buf_pos[rendered_count * RENDER_ARRSIZE_POS + 0] = p.x;
				render_buf_pos[rendered_count * RENDER_ARRSIZE_POS + 1] = p.y;

				render_buf_col[rendered_count * RENDER_ARRSIZE_COL + 0] = p.r;
				render_buf_col[rendered_count * RENDER_ARRSIZE_COL + 1] = p.g;
				render_buf_col[rendered_count * RENDER_ARRSIZE_COL + 2] = p.b;
				render_buf_col[rendered_count * RENDER_ARRSIZE_COL + 3] = p.a * Math.Min(1.0f, p.lifespan) * Space.zoom;
				rendered_count++;
				if (rendered_count >= MAX_RENDERED_PARTICLES) break;
			}


			GL.PointSize(4.0f);
			GL.Enable(EnableCap.PointSmooth);
			//GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			GL.Disable(EnableCap.DepthTest);

			if (shot_gl_data == 0)
			{
				shot_gl_data = GL.GenVertexArray();
				buf_sh0 = GL.GenBuffer();
				buf_sh1 = GL.GenBuffer();
			}
			GL.BindVertexArray(shot_gl_data);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh0);
			GL.BufferData(BufferTarget.ArrayBuffer, (rendered_count + 1) * RENDER_ARRSIZE_POS * sizeof(float), render_buf_pos, BufferUsageHint.DynamicDraw);
			GL.EnableVertexArrayAttrib(shot_gl_data, 0);
			GL.VertexAttribPointer(0, RENDER_ARRSIZE_POS, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh1);
			GL.BufferData(BufferTarget.ArrayBuffer, (rendered_count + 1) * RENDER_ARRSIZE_COL * sizeof(float), render_buf_col, BufferUsageHint.DynamicDraw);
			GL.EnableVertexArrayAttrib(shot_gl_data, 1);
			GL.VertexAttribPointer(1, RENDER_ARRSIZE_COL, VertexAttribPointerType.Float, false, 0, 0);


			Space.particle_shader.bind();
			GL.Uniform3(Space.particle_shader.getUniformLocation("viewport"), GameWindow.active.mult_x, GameWindow.active.mult_y, Space.animation);
			GL.Uniform3(Space.particle_shader.getUniformLocation("scroll"), Space.scroll_x, Space.scroll_y, Space.zoom);
			GL.DrawArrays(PrimitiveType.Points, 0, rendered_count);



			GL.BindVertexArray(0);
			GL.Enable(EnableCap.DepthTest);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

		}





	}
}
