using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Globalization;
using OpenTK.Graphics.OpenGL;
using Cluster.Rendering.Appearance;
using Cluster.math;
using Cluster.Rendering.Draw2D;


namespace Cluster.Rendering.Draw3D
{

	class Model
	{
		public static List<Model> models = new List<Model>();


		public List<Instance> instances;
		public List<Instance>[,] chunks;


		public List<Vertex> vertices;
		List<Triangle> triangles;


		public bool static_instances;
		public bool render_both_sides;
		public bool tiny_model;



		int vertexBufferArray;
		int buf_pos, buf_col, buf_nor, buf_instA, buf_instB;
		

		int num_frames;
		int anim_tex;
		int ANIM_TEXTURE_SIZE, ANIM_TEXTURE_VERTICES, ANIM_TEXTURE_FRAMES;


		int draw_count;
		int num_instances;
		float[] array1, array2;




		static Shader model_shader;
		public static void init()
		{
			model_shader = new Shader("model.vert", "model.frag");
		}

		public void clearData()
		{
			vertices.Clear();
			triangles.Clear();
			num_frames = 0;
		}


		public Model(string url = "", bool facedirection = true)
		{
			url = GameWindow.BASE_FOLDER + "/models/" + url;

			render_both_sides = false;
			num_frames = 1;
			models.Add(this);
			array1 = new float[1];
			array2 = new float[1];
			instances = new List<Instance>();
			chunks = new List<Instance>[SceneGraph.chunkCount, SceneGraph.chunkCount];
			for (int x = 0; x < SceneGraph.chunkCount; x++)
			{
				for (int y = 0; y < SceneGraph.chunkCount; y++)
				{
					chunks[x,y] = new List<Instance>();
				}
			}


			vertices = new List<Vertex>();
			triangles = new List<Triangle>();


			if (url.EndsWith(".krak"))
			{
				load_krak(url, facedirection);
				return;
			}
		}


		public void load_krak(string url, bool facedirection = false)
		{
			using (StreamReader file = new StreamReader(url))
			{
				string[] separator = { ";" };

				//float maxscale = 1.0f;

				int num_vertices = int.Parse(file.ReadLine());
				//Console.WriteLine(num_vertices.ToString());
				for (int i = 0; i < num_vertices; i++)
				{
					string line = file.ReadLine();
					string[] infos = line.Split(separator, System.StringSplitOptions.None);

					vec3 vex = new vec3(float.Parse(infos[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(infos[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(infos[3], CultureInfo.InvariantCulture.NumberFormat));
					vex = new vec3(vex.x, vex.z, -vex.y) * 0.2f;
					vec3 col = new vec3(float.Parse(infos[4], CultureInfo.InvariantCulture.NumberFormat), float.Parse(infos[5], CultureInfo.InvariantCulture.NumberFormat), float.Parse(infos[6], CultureInfo.InvariantCulture.NumberFormat));
					Vertex v0 = new Vertex(vex.x, vex.y, vex.z, col.x, col.y, col.z);
					v0.index = int.Parse(infos[0]);
					vertices.Add(v0);
				}


				int num_polys = int.Parse(file.ReadLine());
				for (int i = 0; i < num_polys; i++)
				{
					string line = file.ReadLine();
					string[] infos = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
					for (int j = 0; j < infos.Length - 2; j++)
					{
						if (facedirection)
						{
							triangles.Add(new Triangle(vertices[int.Parse(infos[0])], vertices[int.Parse(infos[j + 1])], vertices[int.Parse(infos[j + 2])]));
						}
						else
						{
							triangles.Add(new Triangle(vertices[int.Parse(infos[0])], vertices[int.Parse(infos[j + 2])], vertices[int.Parse(infos[j + 1])]));
						}
					}
				}

				if (!file.EndOfStream)
				{

					num_frames = int.Parse(file.ReadLine());
					Console.WriteLine("Number of frames: " + num_frames.ToString());
					for (int f = 0; f < num_frames; f++)
					{
						for (int i = 0; i < num_vertices; i++)
						{
							string line = file.ReadLine();
							string[] infos = line.Split(separator, System.StringSplitOptions.None);

							vec3 vex = new vec3(float.Parse(infos[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(infos[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(infos[3], CultureInfo.InvariantCulture.NumberFormat));
							vex = new vec3(vex.x, vex.z, -vex.y) * 0.2f;
							vertices[i].addFrame(vex.x, vex.y, vex.z);
						}
					}

					createAnimationTexture();

				}



				file.Close();
			}
			//updateFrameCount();
			//updateNormals();
			finishPreparation();
		}



		public void Remove()
		{
			models.Remove(this);

			GL.DeleteTexture(anim_tex);
			GL.DeleteVertexArray(vertexBufferArray);
			GL.DeleteBuffer(buf_pos);
			GL.DeleteBuffer(buf_col);
			GL.DeleteBuffer(buf_nor);
			GL.DeleteBuffer(buf_instA);
			GL.DeleteBuffer(buf_instB);
		}


		public void sortInstance(Instance inst, bool enforce=false)
		{
			if(enforce || inst.updateChunkNeeded())
			{
				int oldx = inst.getChunkX(), oldy = inst.getChunkY();
				int i = (int)Math.Floor((inst.x + SceneGraph.worldSize) / SceneGraph.chunkSize);
				int j = (int)Math.Floor((inst.z + SceneGraph.worldSize) / SceneGraph.chunkSize);
				//Console.WriteLine(oldx.ToString() + ", " + oldy.ToString() + ", " + i.ToString() + ", " + j.ToString());
				chunks[oldx, oldy].Remove(inst);
				chunks[i, j].Add(inst);
				inst._setChunk(i, j);
			}
		}

		public void finishPreparation()
		{
			//num_frames = 1;
			/*Console.WriteLine("Prepare Model");
			Console.WriteLine("Number of Vertices: " + vertices.Count.ToString());
			Console.WriteLine("Number of Triangles: " + triangles.Count.ToString());*/
			updateFrameCount();
			updateNormals();
			createAnimationTexture();
			prepareRendering();
		}

		void createAnimationTexture()
		{
			if (anim_tex == 0)
			{
				anim_tex = GL.GenTexture();
			}

			int pixels_needed = vertices.Count * num_frames;
			ANIM_TEXTURE_SIZE = 4;
			while(pixels_needed > ANIM_TEXTURE_SIZE*ANIM_TEXTURE_SIZE)
			{
				ANIM_TEXTURE_SIZE *= 2;
			}
			ANIM_TEXTURE_VERTICES = vertices.Count;
			ANIM_TEXTURE_FRAMES = num_frames;

			Console.WriteLine("createAnimationTexture() --> size: " + ANIM_TEXTURE_SIZE.ToString() + ", vertices: " + ANIM_TEXTURE_VERTICES.ToString() + ", frames: " + ANIM_TEXTURE_FRAMES.ToString());
			float[] anim_array = new float[ANIM_TEXTURE_SIZE * ANIM_TEXTURE_SIZE * 3];
			int index = 0;
			for (int v = 0; v < vertices.Count; v++)
			{
				for (int f = 0; f < num_frames; f++)
				{
					vec3 col = vertices[v].getPos(f); // Pos(f) ! -------------------------------------
					/*
					anim_array[((v * ANIM_TEXTURE_SIZE) + f) * 3 + 0] = col.x;// (byte)Math.Min(255, Math.Max(0, (int)((col.x * 0.5f + 0.5f) * 255.0f)));
					anim_array[((v * ANIM_TEXTURE_SIZE) + f) * 3 + 1] = col.y;//(byte)Math.Min(255, Math.Max(0, (int)((col.y * 0.5f + 0.5f) * 255.0f)));
					anim_array[((v * ANIM_TEXTURE_SIZE) + f) * 3 + 2] = col.z;//(byte)Math.Min(255, Math.Max(0, (int)((col.z * 0.5f + 0.5f) * 255.0f)));
					*/
					anim_array[index * 3 + 0] = col.x;// (byte)Math.Min(255, Math.Max(0, (int)((col.x * 0.5f + 0.5f) * 255.0f)));
					anim_array[index * 3 + 1] = col.y;//(byte)Math.Min(255, Math.Max(0, (int)((col.y * 0.5f + 0.5f) * 255.0f)));
					anim_array[index * 3 + 2] = col.z;//(byte)Math.Min(255, Math.Max(0, (int)((col.z * 0.5f + 0.5f) * 255.0f)));
					index++;
				}
			}

			

			GL.BindTexture(TextureTarget.Texture2D, anim_tex);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, ANIM_TEXTURE_SIZE, ANIM_TEXTURE_SIZE, 0, PixelFormat.Rgb, PixelType.Float, anim_array);

			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		void prepareRendering()
		{
			draw_count = triangles.Count * 3; //Anzahl Vertices

			float[] dat_pos = new float[draw_count * 3];
			float[] dat_nor = new float[draw_count * 3];
			float[] dat_col = new float[draw_count * 3];

			int frame = 0;
			int index = 0;
			foreach (Triangle tri in triangles)
			{
				vec3 vpos = tri.v0.getPos(frame), vcol = tri.v0.getCol(frame), vnor = tri.normals[frame];
				//dat_pos[index * 3 + 0] = ((float)tri.v0.index + 0.5f) / (float)ANIM_TEXTURE_SIZE; dat_pos[index * 3 + 1] = ((float)tri.v1.index + 0.5f) / (float)ANIM_TEXTURE_SIZE; dat_pos[index * 3 + 2] = ((float)tri.v2.index + 0.5f) / (float)ANIM_TEXTURE_SIZE;//vpos.z;
				dat_pos[index * 3 + 0] = ((float)tri.v0.index); dat_pos[index * 3 + 1] = ((float)tri.v1.index); dat_pos[index * 3 + 2] = ((float)tri.v2.index);
				dat_nor[index * 3 + 0] = vnor.x; dat_nor[index * 3 + 1] = vnor.y; dat_nor[index * 3 + 2] = vnor.z;
				dat_col[index * 3 + 0] = vcol.x; dat_col[index * 3 + 1] = vcol.y; dat_col[index * 3 + 2] = vcol.z;
				index++;

				vpos = tri.v1.getPos(frame); vcol = tri.v1.getCol(frame);
				//dat_pos[index * 3 + 0] = ((float)tri.v1.index + 0.5f) / (float)ANIM_TEXTURE_SIZE; dat_pos[index * 3 + 1] = ((float)tri.v2.index + 0.5f) / (float)ANIM_TEXTURE_SIZE; dat_pos[index * 3 + 2] = ((float)tri.v0.index + 0.5f) / (float)ANIM_TEXTURE_SIZE;//vpos.z;
				dat_pos[index * 3 + 0] = ((float)tri.v1.index); dat_pos[index * 3 + 1] = ((float)tri.v2.index); dat_pos[index * 3 + 2] = ((float)tri.v0.index);
				dat_nor[index * 3 + 0] = vnor.x; dat_nor[index * 3 + 1] = vnor.y; dat_nor[index * 3 + 2] = vnor.z;
				dat_col[index * 3 + 0] = vcol.x; dat_col[index * 3 + 1] = vcol.y; dat_col[index * 3 + 2] = vcol.z;
				index++;

				vpos = tri.v2.getPos(frame); vcol = tri.v2.getCol(frame);
				//dat_pos[index * 3 + 0] = ((float)tri.v2.index + 0.5f) / (float)ANIM_TEXTURE_SIZE; dat_pos[index * 3 + 1] = ((float)tri.v0.index + 0.5f) / (float)ANIM_TEXTURE_SIZE; dat_pos[index * 3 + 2] = ((float)tri.v1.index + 0.5f) / (float)ANIM_TEXTURE_SIZE;//vpos.z;
				dat_pos[index * 3 + 0] = ((float)tri.v2.index); dat_pos[index * 3 + 1] = ((float)tri.v0.index); dat_pos[index * 3 + 2] = ((float)tri.v1.index);
				dat_nor[index * 3 + 0] = vnor.x; dat_nor[index * 3 + 1] = vnor.y; dat_nor[index * 3 + 2] = vnor.z;
				dat_col[index * 3 + 0] = vcol.x; dat_col[index * 3 + 1] = vcol.y; dat_col[index * 3 + 2] = vcol.z;
				index++;
			}




			if (vertexBufferArray == 0)
			{
				vertexBufferArray = GL.GenVertexArray();

				buf_pos = GL.GenBuffer();
				buf_nor = GL.GenBuffer();
				buf_col = GL.GenBuffer();
				buf_instA = GL.GenBuffer();
				buf_instB = GL.GenBuffer();
			}


			GL.BindVertexArray(vertexBufferArray);
			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, dat_pos.Length * sizeof(float), dat_pos, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_nor);
			GL.BufferData(BufferTarget.ArrayBuffer, dat_nor.Length * sizeof(float), dat_nor, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 1);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_col);
			GL.BufferData(BufferTarget.ArrayBuffer, dat_col.Length * sizeof(float), dat_col, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 2);
			GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindVertexArray(0);
		}


		public void updateNormals()
		{
			foreach (Triangle tri in triangles)
			{
				tri.updateNormals();
			}
		}



		void instanceBuffers()
		{
			List<Instance> seen = new List<Instance>();
			for (int x = 0; x < SceneGraph.chunkCount; x++)
			{
				for (int y = 0; y < SceneGraph.chunkCount; y++)
				{
					if (SceneGraph.data[x, y].visible && !(tiny_model && SceneGraph.data[x,y].far_away))
					{
						seen.AddRange(chunks[x, y]);
					}
				}
			}
			/*
			foreach(Instance inst in instances)
			{
				if (SceneGraph.isVisible(inst.x, inst.z)) seen.Add(inst);
			}*/


			if (static_instances && num_instances == seen.Count) return;
			int instanceCount = 0;
			if (array1.Length < seen.Count * 3)
			{
				array1 = new float[seen.Count * 3];
				array2 = new float[seen.Count * 3];
			}
			foreach (Instance inst in seen)
			{
				array1[instanceCount * 3 + 0] = inst.x;
				array1[instanceCount * 3 + 1] = inst.y;
				array1[instanceCount * 3 + 2] = inst.z;

				array2[instanceCount * 3 + 0] = inst.rotation;// inst.rotation += 0.01f;
				array2[instanceCount * 3 + 1] = inst.scale;
				array2[instanceCount * 3 + 2] = (inst.animation % (num_frames - 1));

				//inst.animation += 0.05f;
				instanceCount++;
			}
			num_instances = instanceCount;
			//Console.WriteLine(instanceCount.ToString());

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_instA);
			GL.BufferData(BufferTarget.ArrayBuffer, num_instances *3 * sizeof(float), array1, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 3);
			GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
			GL.VertexAttribDivisor(3, 1);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_instB);
			GL.BufferData(BufferTarget.ArrayBuffer, num_instances * 3 * sizeof(float), array2, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 4);
			GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
			GL.VertexAttribDivisor(4, 1);

			//Console.WriteLine("Number of Instances: " + num_instances.ToString());

		}


		public void render(Camera cam, int prerender = 0)
		{
			if (prerender == 2 && tiny_model) return;
			if (vertexBufferArray == 0) prepareRendering();

			if (render_both_sides) GL.Disable(EnableCap.CullFace);

			GL.BindVertexArray(vertexBufferArray);
			if (prerender == 1 || Terrain.active == null) instanceBuffers();

			model_shader.bind();
			//GameWindow.glError("A");
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, anim_tex);

			GL.UniformMatrix4(model_shader.getUniformLocation("projection"), 1, false, cam.getProjection());
			GL.UniformMatrix4(model_shader.getUniformLocation("camera"), 1, false, cam.getInversePosition());
			if (prerender == 3) GL.UniformMatrix4(model_shader.getUniformLocation("camera"), 1, false, GameWindow.active.shadowMatrix.convertToGL());
			GL.Uniform3(model_shader.getUniformLocation("sun"), Terrain.sun.x, Terrain.sun.y, Terrain.sun.z);
			//GameWindow.glError("B");
			GL.Uniform3(model_shader.getUniformLocation("texture_packing"), (float)ANIM_TEXTURE_SIZE, (float)ANIM_TEXTURE_VERTICES, (float)ANIM_TEXTURE_FRAMES);
			//GameWindow.glError("C");
			float dirf = 0.0f;
			if (prerender == 1) dirf = 1.0f;
			if (prerender == 2) dirf = -1.0f;
			GL.Uniform1(model_shader.getUniformLocation("clip_direction"), dirf);

			/*
			if (prerender == 0) model_shader.id.SetUniform1(manager.gl, "clip_direction", 0.0f);
			if (prerender == 1) model_shader.id.SetUniform1(manager.gl, "clip_direction", 1.0f);
			if (prerender == 2) model_shader.id.SetUniform1(manager.gl, "clip_direction", -1.0f);*/
			
			GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, draw_count, num_instances);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			model_shader.unbind();

			GameWindow.glError("D");

			//if (render_both_sides) 
			GL.Enable(EnableCap.CullFace);
		}


		public void AddVertex(Vertex v)
		{
			v.index = vertices.Count;
			vertices.Add(v);
		}
		public void AddTriangle(Triangle t)
		{
			triangles.Add(t);
		}



		public int getFrameCount()
		{
			return num_frames;
		}
		public int updateFrameCount()
		{
			num_frames = 1;
			foreach (Vertex v in vertices)
			{
				int tmp = v.getFrameCount();
				if (num_frames < tmp) num_frames = tmp;
			}
			return num_frames;
		}
		public int setFrameCount(int frames)
		{
			num_frames = frames;
			return num_frames;
		}



		public Image ViewTexture()
		{
			return new Image(anim_tex, num_frames, vertices.Count);
		}




		public void updateAnimations(float t = 0.01f)
		{
			foreach (Instance inst in instances)
			{
				inst.updateAnimation(t);
			}
		}


		public float getDimensionX()
		{
			float m0 = 0.0f, m1 = 0.0f;
			foreach (Vertex v in vertices)
			{
				m0 = Math.Min(v.pos[0].x, m0);
				m1 = Math.Max(v.pos[0].x, m0);
			}
			return m1 - m0;
		}








	}
}
