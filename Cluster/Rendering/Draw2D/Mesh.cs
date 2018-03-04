using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK.Graphics.OpenGL;
using Cluster.Rendering.Appearance;

namespace Cluster.Rendering.Draw2D
{
	class Mesh
	{
		public static List<MeshDraw> drawCall;


		static Shader mesh_shader;
		public int gl_data, buf_pos, buf_col;

		public float box_x, box_y, radius, del_y, cx, cy;
		public int num_vertices, num_lines, num_triangles;

		List<vertex> vex;
		List<line> lines;

		public static void init()
		{
			mesh_shader = new Shader("mesh.vert", "mesh.frag");
			drawCall = new List<MeshDraw>();
		}

		public static Shader getShader()
		{
			return mesh_shader;
		}

		public static void render()
		{
			GL.Disable(EnableCap.DepthTest);
			GL.LineWidth(2.0f);
			foreach (MeshDraw md in drawCall)
			{
				if (md.fit) md.mesh.drawFit(md.x, md.y, md.width, md.height, md.clamp, md.r, md.g, md.b, md.a);
				else    	md.mesh.draw(md.x, md.y, md.width, md.height, md.clamp, md.r, md.g, md.b, md.a);
			}
			GL.Enable(EnableCap.DepthTest);
			drawCall.Clear();
		}


		public Mesh(string filename, bool centering = false, bool absolutePath = false)
		{
			FileStream file;
			if (absolutePath) file = File.OpenRead(filename);
			else			  file = File.OpenRead(GameWindow.BASE_FOLDER + filename);
			BinaryReader reader = new BinaryReader(file);

			vex = new List<vertex>();
			lines = new List<line>();

			num_vertices = reader.ReadInt32();
			num_lines = reader.ReadInt32();
			num_triangles = reader.ReadInt32();

			float xa = 1000.0f, xb = -1000.0f, ya = 1000.0f, yb = -1000.0f;

			for (int i = 0; i < num_vertices; i++)
			{
				vertex v = new vertex();
				v.r = ((float)reader.ReadByte()) / 255.0f;
				v.g = ((float)reader.ReadByte()) / 255.0f;
				v.b = ((float)reader.ReadByte()) / 255.0f;

				v.x = reader.ReadSingle();
				v.y = reader.ReadSingle();
				//Console.WriteLine("(x = "+v.x.ToString() + "| y = " + v.y.ToString()+" )");

				box_x = Math.Max(box_x, Math.Abs(v.x));
				box_y = Math.Max(box_y, Math.Abs(v.y));

				xa = Math.Min(xa, v.x);
				xb = Math.Max(xb, v.x);

				ya = Math.Min(ya, v.y);
				yb = Math.Max(yb, v.y);

				del_y = Math.Min(del_y, v.y);
				radius = Math.Max(radius, (v.x * v.x + v.y + v.y));
				vex.Add(v);
			}
			radius = (float)Math.Sqrt((double)radius);
			cx = (xa + xb) * 0.5f;
			cy = (ya + yb) * 0.5f;

			if (centering)
			{
				//Console.WriteLine("centering "+cx.ToString()+" "+cy.ToString());
				foreach (vertex v in vex)
				{
					v.x -= cx;
					v.y -= cy;
				}
			}

			for (int i = 0; i < num_lines; i++)
			{
				line l = new line();
				l.a = vex[reader.ReadInt16()];
				l.b = vex[reader.ReadInt16()];
				lines.Add(l);
			}


			/* Das hier wird bisher nicht gebraucht.
			For Local i:Int=0 Until num_t
				Local t:Triangle=New Triangle
				ListAddLast(m.tri, t)
				t.v0=array[file.ReadShort()]
				t.v1=array[file.ReadShort()]
				t.v2=array[file.ReadShort()]
				't.setnormal(m.radius*0.8)
			Next
		
		
			For Local t:Triangle=EachIn m.tri
				For Local l:Line=EachIn m.lines
					If t.Contains(l) Then
						t.setnormal2(l)
					EndIf
				Next
			Next
			 * 
			For Local v:Vertex=EachIn m.vex
				v.nx:+(v.x/m.radius)*3.0
				v.ny:+(v.y/m.radius)*3.0
				v.ct:+3
				v.nx:/Float(v.ct)
				v.ny:/Float(v.ct)
			Next
			 */


			reader.Close();
			file.Close();

			prepare_vba();
		}



		void prepare_vba()
		{
			float[] vertices = new float[num_lines * 4];
			float[] colour = new float[num_lines * 6];


			//Vertices außen herum.
			int i = 0;
			foreach (line l in lines)
			{
				vertices[i * 2 + 0] = l.a.x;
				vertices[i * 2 + 1] = l.a.y;
				colour[i * 3 + 0] = l.a.r;
				colour[i * 3 + 1] = l.a.g;
				colour[i * 3 + 2] = l.a.b;
				i++;
				//Console.WriteLine(l.a.x.ToString() + ", " + l.a.y.ToString());
				vertices[i * 2 + 0] = l.b.x;
				vertices[i * 2 + 1] = l.b.y;
				colour[i * 3 + 0] = l.b.r;
				colour[i * 3 + 1] = l.b.g;
				colour[i * 3 + 2] = l.b.b;
				i++;
			}

			//Create the vertex array object.
			if (gl_data == 0)
			{
				gl_data = GL.GenVertexArray();// new VertexBufferArray();
				//gl_data.Create(manager.gl);
				buf_pos = GL.GenBuffer();
				buf_col = GL.GenBuffer();
			}
			GL.BindVertexArray(gl_data); //gl_data.Bind(manager.gl);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(gl_data, 0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_col);
			GL.BufferData(BufferTarget.ArrayBuffer, colour.Length * sizeof(float), colour, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(gl_data, 1);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindVertexArray(0);
			/*
			//Vertex-Daten uebergeben
			VertexBuffer vertexDataBuffer = new VertexBuffer();
			vertexDataBuffer.Create(manager.gl);
			vertexDataBuffer.Bind(manager.gl);
			vertexDataBuffer.SetData(manager.gl, 0, vertices, false, 2);

			//Terrain-Daten uebergeben
			VertexBuffer colourDataBuffer = new VertexBuffer();
			colourDataBuffer.Create(manager.gl);
			colourDataBuffer.Bind(manager.gl);
			colourDataBuffer.SetData(manager.gl, 1, colour, false, 3);

			gl_data.Unbind(manager.gl);*/
		}


		public void deferred_draw(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
		{
			MeshDraw tmp = new MeshDraw();
			tmp.mesh = this;
			tmp.x = x;
			tmp.y = y;
			tmp.width = width;
			tmp.height = height;
			tmp.clamp = clamp;
			tmp.r = r;
			tmp.g = g;
			tmp.b = b;
			tmp.a = a;
			tmp.fit = false;
			drawCall.Add(tmp);
		}
		public void deferred_drawFit(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
		{
			MeshDraw tmp = new MeshDraw();
			tmp.mesh = this;
			tmp.x = x;
			tmp.y = y;
			tmp.width = width;
			tmp.height = height;
			tmp.clamp = clamp;
			tmp.r = r;
			tmp.g = g;
			tmp.b = b;
			tmp.a = a;
			tmp.fit = true;
			drawCall.Add(tmp);
		}


		public void draw(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
		{
			mesh_shader.bind();
			//GL.Uniform1(mesh_shader.getUniformLocation("pos_x"), (x + width * 0.5f) * GameWindow.active.mult_x * 2.0f - 1.0f);
			//GL.Uniform1(mesh_shader.getUniformLocation("pos_y"), (y + height * 0.5f * (1.0f + del_y)) * GameWindow.active.mult_y * 2.0f - 0.0f);
			GL.Uniform2(mesh_shader.getUniformLocation("pos"), (x + width * 0.5f) * GameWindow.active.mult_x * 2.0f - 1.0f, (y + height * 0.5f * (1.0f + del_y)) * GameWindow.active.mult_y * 2.0f - 0.0f);
			GL.Uniform3(mesh_shader.getUniformLocation("scale"), width * GameWindow.active.mult_x, height * GameWindow.active.mult_y, clamp);
			GL.Uniform4(mesh_shader.getUniformLocation("col"), r, g, b, a);
			/*
			manager.mesh_shader.id.SetUniform1(manager.gl, "pos_x", (x + width * 0.5f) * manager.mult_x * 2.0f - 1.0f);
			manager.mesh_shader.id.SetUniform1(manager.gl, "pos_y", (y + height * 0.5f * (1.0f + del_y)) * manager.mult_y * 2.0f - 0.0f);
			manager.mesh_shader.id.SetUniform3(manager.gl, "scale", width * manager.mult_x, height * manager.mult_y, clamp);
			manager.mesh_shader.id.SetUniform3(manager.gl, "col", r, g, b);
			manager.mesh_shader.id.SetUniform1(manager.gl, "alpha", a);
			*/
			GL.BindVertexArray(gl_data);
			GL.DrawArrays(PrimitiveType.Lines, 0, num_lines * 2);
			GL.BindVertexArray(0);
			/*
			gl_data.Bind(manager.gl);
			manager.gl.DrawArrays(OpenGL.GL_LINES, 0, num_lines * 2);
			//manager.gl.UseProgram(0);
			//manager.gl.BindVertexArray(0);
			//gl_data.Unbind(manager.gl);
			//manager.mesh_shader.unbind();
			manager.null_vao.Bind(manager.gl);*/
		}


		public void drawFit(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
		{
			float sc = Math.Max(Math.Max(box_x, box_y), 0.5f) * 2.5f;
			mesh_shader.bind();
			GL.Uniform2(mesh_shader.getUniformLocation("pos"), (x + width * 0.5f) * GameWindow.active.mult_x * 2.0f - 1.0f, (y + height * 0.5f) * GameWindow.active.mult_y * 2.0f - 0.0f);
			//GL.Uniform1(mesh_shader.getUniformLocation("pos_x"), (x + width * 0.5f) * GameWindow.active.mult_x * 2.0f - 1.0f);
			//GL.Uniform1(mesh_shader.getUniformLocation("pos_y"), (y + height * 0.5f) * GameWindow.active.mult_y * 2.0f - 0.0f);
			GL.Uniform3(mesh_shader.getUniformLocation("scale"), width * GameWindow.active.mult_x / sc, height * GameWindow.active.mult_y / sc, clamp);
			GL.Uniform4(mesh_shader.getUniformLocation("col"), r, g, b, a);

			GL.BindVertexArray(gl_data);
			GL.DrawArrays(PrimitiveType.Lines, 0, num_lines * 2);
			GL.BindVertexArray(0);
		}



	}








	class vertex
	{
		public float x, y;
		public float r, g, b;
		//public float nx, ny, ct;
	}

	class line
	{
		public vertex a, b;
	}

	class MeshDraw
	{
		public Mesh mesh;
		public float x, y, width, height, clamp, r, g, b, a;
		public bool fit;
	}


}
