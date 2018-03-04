using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

using Cluster.math;
using Cluster.Rendering.Appearance;

namespace Cluster.Rendering.Draw3D
{

	class Particle
	{
		public vec3 pos;
		public vec3 v;
		public float lifespan;
		public float r, g, b, a;
		public float size, rotation, rotation_speed;

		public Particle()
		{
			r = 1.0f;
			g = 1.0f;
			b = 1.0f;
			a = 1.0f;
			lifespan = 10.0f;
			pos = new vec3();
			v = new vec3((float)GameWindow.random.NextDouble() - 0.5f, (float)GameWindow.random.NextDouble() - 0.5f, (float)GameWindow.random.NextDouble() - 0.5f);
			size = 1.0f;
			rotation = (float)GameWindow.random.NextDouble() * 360.0f;
			rotation_speed = ((float)GameWindow.random.NextDouble() - 0.5f);
		}


		public float neardist(vec4 nearplane)
		{
			return (nearplane.x * pos.x + nearplane.y * pos.y + nearplane.z * pos.z);
		}
	}

	class ParticleSystem
	{

		public const int DEFAULT = 0;
		public const int RAIN = 1;
		public const int SMOKE = 2;
		public const int DUST = 3;
		public const int BOUNCING = 4;
		public const int TRACE = 5;

		public static List<ParticleSystem> list;
		static Shader particle_shader;


		public static void init()
		{
			list = new List<ParticleSystem>();
			particle_shader = new Shader("particle.vert", "particle.frag", "particle.geom"); 
		}




		int texture;
		int num_particles, buffer_size;

		List<Particle> particles;



		int vertexBufferArray;
		int buf_pos, buf_col, buf_extra;
		float[] data_pos, data_col, data_extra;


		public int particle_type;
		public vec3 gravity;
		public bool additive_blending;




		public ParticleSystem(string url__of_texture)
		{
			particles = new List<Particle>();
			list.Add(this);


			Bitmap data = new Bitmap(GameWindow.BASE_FOLDER + "textures/" + url__of_texture);
			BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			texture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, texture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			data.UnlockBits(bdat);

			vertexBufferArray = GL.GenVertexArray();
			buf_pos = GL.GenBuffer();
			buf_col = GL.GenBuffer();
			buf_extra = GL.GenBuffer();
			buffer_size = 100;
			data_pos = new float[buffer_size * 4];
			data_col = new float[buffer_size * 4];
			data_extra = new float[buffer_size * 1];
		}



		public Particle spawn(float x, float y, float z, float size = 1.0f)
		{
			Particle p = new Particle();
			particles.Add(p);

			p.pos.x = x;
			p.pos.y = y;
			p.pos.z = z;
			p.size = size;

			switch (particle_type)
			{
				case RAIN:
					p.rotation_speed += 0.5f;
					p.pos.x += p.rotation_speed * (float)Math.Cos(p.rotation * Math.PI / 180.0) * 5.0f;
					p.pos.z += p.rotation_speed * (float)Math.Sin(p.rotation * Math.PI / 180.0) * 5.0f;
					//p.r = p.g = 0.5f;
					//p.b = 0.6f;
					//p.a = 0.87f;
					p.lifespan = 2.0f * p.pos.y;
					p.rotation = 0.0f;
					p.rotation_speed = 0.0f;
					p.v.x = 0.0f;
					p.v.y = -0.5f;
					p.v.z = 0.0f;
					break;
				case SMOKE:
					p.a = 0.125f;
					p.lifespan *= 2.0f;
					p.pos.x += p.rotation_speed * (float)Math.Cos(p.rotation * Math.PI / 180.0);
					p.pos.z += p.rotation_speed * (float)Math.Sin(p.rotation * Math.PI / 180.0);

					p.v.x *= 0.125f;
					p.v.y = p.v.y * 0.125f + 0.25f;
					p.v.z *= 0.125f;
					break;
				case TRACE:
					//p.lifespan *= 2.0f;
					p.v.x *= 0.0125f;
					p.v.y = p.v.y * 0.0125f;
					p.v.z *= 0.0125f;
					break;
			}

			return p;
		}


		public void update(float time = 0.05f)
		{
			foreach (Particle p in particles)
			{
				p.lifespan -= time;
				if (p.lifespan > 0.0f)
				{
					//Console.WriteLine(p.v.x.ToString() + " " + p.v.y.ToString() + " " + p.v.z.ToString() + " " + time);
					p.pos.x += (p.v.x * time);
					p.pos.y += (p.v.y * time);
					p.pos.z += (p.v.z * time);
					p.rotation += p.rotation_speed*time;

					if (gravity != null)
					{
						p.v.x += (gravity.x * time);
						p.v.y += (gravity.y * time);
						p.v.z += (gravity.z * time);
					}

					switch (particle_type)
					{
						case BOUNCING:
							if(p.pos.y< 0.0f)
							{
								p.lifespan -= time;
								continue;
							}
							float h = Terrain.getHeight(p.pos.x, p.pos.z);
							if (h > p.pos.y)
							{
								p.pos.y = h;
								vec3 normal = Terrain.getNormal(p.pos.x, p.pos.z);
								p.v -= 2.0f*((p.v * normal)) * normal;
								p.v *= 0.8f;
							}
							break;
						case SMOKE:
							p.v.x += ((float)GameWindow.random.NextDouble() - 0.5f) * 0.01f * time;
							break;
					}

				}
			}
			particles.RemoveAll(p => (p.lifespan <= 0.0f));
		}

		public static void updateAll(float time = 0.05f)
		{
			foreach (ParticleSystem ps in ParticleSystem.list)
			{
				ps.update(time);
			}
		}



		public void updateBuffers(Camera cam)
		{
			if (buffer_size < particles.Count+100)
			{
				buffer_size = particles.Count+100;
				data_pos = new float[buffer_size * 4];
				data_col = new float[buffer_size * 4];
				data_extra = new float[buffer_size * 1];
			}

			vec4 nearplane = cam.getFrustumPlanes()[4];

			particles.Sort((x, y) => x.neardist(nearplane).CompareTo(y.neardist(nearplane)));

			num_particles = 0;
			foreach (Particle p in particles)
			{
				if (p.a * p.lifespan > 0)
				{
					data_pos[num_particles * 4 + 0] = p.pos.x;
					data_pos[num_particles * 4 + 1] = p.pos.y;
					data_pos[num_particles * 4 + 2] = p.pos.z;
					data_pos[num_particles * 4 + 3] = p.size;

					data_col[num_particles * 4 + 0] = p.r;
					data_col[num_particles * 4 + 1] = p.g;
					data_col[num_particles * 4 + 2] = p.b;
					data_col[num_particles * 4 + 3] = p.a*Math.Min(1.0f, p.lifespan * 0.1f);

					data_extra[num_particles * 1 + 0] = p.rotation;
					num_particles++;
				}
			}

			//Console.WriteLine("#Particles2: " + num_particles.ToString());

			GL.BindVertexArray(vertexBufferArray);
			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, num_particles * 4 * sizeof(float), data_pos, BufferUsageHint.DynamicDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_col);
			GL.BufferData(BufferTarget.ArrayBuffer, num_particles * 4 * sizeof(float), data_col, BufferUsageHint.DynamicDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 1);
			GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_extra);
			GL.BufferData(BufferTarget.ArrayBuffer, num_particles * 1 * sizeof(float), data_extra, BufferUsageHint.DynamicDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 2);
			GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 0, 0);
		}


		public void render(Camera cam, int prerender = 0)
		{
			if (prerender != 0) return;
			//Console.WriteLine("#Particles: "+num_particles.ToString());

			//if (prerender == 1 || Terrain.active == null) 
				updateBuffers(cam);



			if(additive_blending) GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			particle_shader.bind();
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, texture);

			GL.UniformMatrix4(particle_shader.getUniformLocation("projection"), 1, false, cam.getProjection());
			GL.UniformMatrix4(particle_shader.getUniformLocation("camera"), 1, false, cam.getInversePosition());
			GL.Uniform1(particle_shader.getUniformLocation("ratio"), (float)GameWindow.active.height/(float)GameWindow.active.width);
			float dirf = 0.0f;
			if (prerender == 1) dirf = 1.0f;
			if (prerender == 2) dirf = -1.0f;
			GL.Uniform1(particle_shader.getUniformLocation("clip_direction"), dirf);

			/*
			if (prerender == 0) model_shader.id.SetUniform1(manager.gl, "clip_direction", 0.0f);
			if (prerender == 1) model_shader.id.SetUniform1(manager.gl, "clip_direction", 1.0f);
			if (prerender == 2) model_shader.id.SetUniform1(manager.gl, "clip_direction", -1.0f);*/

			GL.DrawArrays(PrimitiveType.Points, 0, num_particles);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			particle_shader.unbind();


			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
		}





	}
}
