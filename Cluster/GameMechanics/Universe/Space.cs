using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using Cluster.math;
using Cluster.Rendering.Appearance;
using Cluster.Rendering.Draw2D;
using Cluster.GameMechanics.Behaviour;

namespace Cluster.GameMechanics.Universe
{

	static class Space
	{
		public static float scroll_x = 0.0f, scroll_y = 0.0f, zoom = 1.0f;
		public static float animation = 0.0f;

		public static Shader planet_shader;
		public static Shader building_shader;
		public static Shader unit_shader, unit_shield_shader;
		public static Shader shot_shader;
		public static Shader particle_shader;
		public static Shader space_shader;
		

		static Image space_tex0, space_tex1, space_tex2, space_tex3;
		static int gl_data, buf_pos;
		//static VertexBuffer gl_data_sub;

		static int msecs0, msecs1;


		public static void init()
		{
			planet_shader = new Shader("planet.vert", "planet.frag");
			building_shader = new Shader("building.vert", "building.frag");
			space_shader = new Shader("space.vert", "space.frag");
			unit_shader = new Shader("unit.vert", "unit.frag");
			unit_shield_shader = new Shader("shield.vert", "shield.frag", "shield.geom");
			shot_shader = new Shader("shot.vert", "shot.frag", "shot.geom");
			particle_shader = new Shader("particle2D.vert", "particle2D.frag");


			space_tex0 = new Image("space0.png");
			space_tex1 = new Image("space1.png");
			space_tex2 = new Image("space2.png");
			space_tex3 = new Image("space3.png");

			create_vba();


			Planet.init();

			fillUniverse();

			Sector.init(); // Muss nach dem Erstellen der Zivilisationen gemacht werden, damit die Listen für Raumschiffe die richtige Anzahl haben.

			msecs0 = msecs1 = 0;
		}


		public static void fillUniverse()
		{
			Civilisation.init(5);

			new Planet(0.0f, 0.0f, 25);
			for (int i = 0; i < 100; i++)
			{
				float px = ((float)GameWindow.random.NextDouble() - 0.5f) * 20000.0f;
				float py = ((float)GameWindow.random.NextDouble() - 0.5f) * 20000.0f;

				bool tooclose = false;
				foreach (Planet p in Planet.planets)
				{
					if (Math.Sqrt((px - p.x) * (px - p.x) + (py - p.y) * (py - p.y)) < 2000.0) { tooclose = true; break; }
				}
				if (!tooclose) new Planet(px, py, 7 + GameWindow.random.Next(19));
			}

		}

		static void create_vba()
		{
			float[] vertices = new float[] { -1.0f, -1.0f, -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f };

			if (gl_data == 0)
			{
				gl_data = GL.GenVertexArray();
				buf_pos = GL.GenBuffer();
			}


			GL.BindVertexArray(gl_data);
			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(gl_data, 0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindVertexArray(0);
			/*
			gl_data = new VertexBufferArray();
			gl_data.Create(manager.gl);
			gl_data.Bind(manager.gl);
			gl_data_sub = new VertexBuffer();
			gl_data_sub.Create(manager.gl);
			gl_data_sub.Bind(manager.gl);
			gl_data_sub.SetData(manager.gl, 0, vertices, false, 2);
			gl_data.Unbind(manager.gl);*/
		}




		public static void render()
		{
			//Console.WriteLine("space.render()");
			space_shader.bind();
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, space_tex0.tex);
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, space_tex1.tex);

			GL.Uniform3(space_shader.getUniformLocation("scroll"), (float)Space.scroll_x, (float)Space.scroll_y, (float)Space.zoom);
			GL.Uniform3(space_shader.getUniformLocation("viewport"), GameWindow.active.width, GameWindow.active.height, Space.animation);

			GL.BindVertexArray(gl_data);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 4);
			GL.BindVertexArray(0);
			/*
			gl_data.Bind(manager.gl);

			manager.gl.DrawArrays(OpenGL.GL_TRIANGLE_FAN, 0, 4);
			gl_data.Unbind(manager.gl);
			//texture.unbind(0); texture.unbind(1); texture.unbind(2); texture.unbind(3);
			space_shader.unbind();*/
		}



		public static void update()
		{
			msecs0 = System.Environment.TickCount;
			float time = Math.Min(5.0f, (float)(msecs0 - msecs1) / 10.0f);
			msecs1 = msecs0;

			Planet.simulate(time);
			Unit.update(time * 5.0f);
			Shot.update(time * 5.0f);
			Particle.update(time* 5.0f);
		}




		public static float spaceToScreenX(float space_X)
		{
			return ((space_X - scroll_x) * zoom + (float)GameWindow.active.width) * 0.5f;
		}
		public static float spaceToScreenY(float space_Y)
		{
			return (-(space_Y - scroll_y) * zoom + (float)GameWindow.active.height) * 0.5f;
		}
		public static float screenToSpaceX(float screen_X)
		{
			return (screen_X * 2.0f - (float)GameWindow.active.width) / zoom + scroll_x;
		}
		public static float screenToSpaceY(float screen_Y)
		{
			return -((screen_Y * 2.0f - (float)GameWindow.active.height) / zoom - scroll_y);
		}

		public static bool isVisible(float space_X, float space_Y)
		{
			if ((Math.Abs(space_X - scroll_x) * zoom <= 1.0f) && (Math.Abs(space_Y - scroll_y) * zoom <= 1.0f)) return true;
			return false;
		}


	}
}
