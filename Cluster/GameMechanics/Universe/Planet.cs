using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using Cluster.math;
using Cluster.Rendering.Appearance;
using Cluster.Rendering.Draw2D;
using Cluster.GameMechanics.Content;

namespace Cluster.GameMechanics.Universe
{
	class Planet
	{
		// constants
		public const int PLANET_DETAIL = 15;
		public const int NUMBER_OF_TERRAIN_TYPES = 8;

		public const byte EFFECT_NONE = 0;
		public const byte EFFECT_RAIN = 1;
		public const byte EFFECT_VOLCANO_ERUPTION = 2;

		public const byte TERRA_WATER = 0;
		public const byte TERRA_FERTILE = 1;
		public const byte TERRA_DESERT = 2;
		public const byte TERRA_MOUNTAIN = 3;
		public const byte TERRA_VOLCANO = 4;
		public const byte TERRA_ICE = 5;
		public const byte TERRA_RESSOURCES = 6;
		public const byte TERRA_JUNGLE = 7;


		public const byte CLIMATE_NORMAL = 0;
		public const byte CLIMATE_COLD = 1;
		public const byte CLIMATE_HOT = 2;
		public const byte CLIMATE_RAINY = 3;
		public const byte CLIMATE_TOXIC = 4;

		public const int NUMBER_OF_CLIMATES = 5;

		public const byte JUNGLE_VARIETY = 2;




		// statics
		public static List<Planet> planets = new List<Planet>();
		static int count_ID;

		static Mesh[,] jungle;
		public static Mesh[] terra_image;


		// fields
		public float x, y;
		public int size;
		public string name;

		public byte climate;
		public byte[] terra;
		public byte[] effect;
		public float[] timer;
		public Building[] infra;

		int random;
		int id;

		bool cansee_temp, seenbefore;
		float r, g, b;

		int gl_data, buf_pos, buf_ter, buf_col;
		bool gl_data_update;


		// constructors
		public Planet(float x, float y, int size = 15)
		{
			planets.Add(this);
			this.id = count_ID; count_ID++;
			gl_data_update = true;

			this.x = x;
			this.y = y;
			random = GameWindow.random.Next(1000);

			this.size = size;
			terra = new byte[size];
			effect = new byte[size];
			timer = new float[size];
			infra = new Building[size];

			//r = 0.41f; g = 0.25f; b = 0.0f;
			//b = 1.0f;
			//g = 0.91f; r = 0.15f; b = 0.0f;
			r = 1.0f; g = 1.0f; b = 1.0f;

			r = Civilisation.data[random % Civilisation.count].r;
			g = Civilisation.data[random % Civilisation.count].g;
			b = Civilisation.data[random % Civilisation.count].b;

			seenbefore = false;
			cansee_temp = false;

			name = "Planet_" + id.ToString();
			climate = (byte)GameWindow.random.Next(NUMBER_OF_CLIMATES);

			//Planeten mit Landschaft fuellen
			for (int i = 0; i < size; i++)
			{
				double percentage = GameWindow.random.NextDouble();
				if (percentage < 0.15)
				{
					terra[i] = TERRA_DESERT;
				}
				else if (percentage < 0.3)
				{
					terra[i] = TERRA_MOUNTAIN;
				}
				else if (percentage < 0.5)
				{
					terra[i] = TERRA_WATER;
				}
				else if (percentage < 0.65)
				{
					switch (climate)
					{
						case CLIMATE_COLD:
							terra[i] = TERRA_ICE;
							break;
						case CLIMATE_HOT:
							terra[i] = TERRA_DESERT;
							break;
						case CLIMATE_RAINY:
							terra[i] = TERRA_JUNGLE;
							break;
						case CLIMATE_TOXIC:
							terra[i] = TERRA_VOLCANO;
							break;
						default:
							terra[i] = TERRA_FERTILE;
							break;
					}
				}
				else if (percentage < 0.7)
				{
					terra[i] = TERRA_JUNGLE;

					//build(i, Blueprint.data[5], Civilisation.data[0]);
				}
				else
				{
					terra[i] = TERRA_FERTILE;
					build(i, Blueprint.data[GameWindow.random.Next(Blueprint.count)], Civilisation.data[GameWindow.random.Next(Civilisation.count)], false);
				}
				//Console.WriteLine(terra[i].ToString());
			}

		}



		// Initiation
		static public void init()
		{
			jungle = new Mesh[JUNGLE_VARIETY, NUMBER_OF_CLIMATES];
			for (byte i = 0; i < JUNGLE_VARIETY; i++)
			{
				for (byte j = 0; j < NUMBER_OF_CLIMATES; j++)
				{
					//jungle[i,j] = new Mesh("planets/tree" + i.ToString() + ".vg");
					jungle[i, j] = new Mesh("planets/tree_" + j.ToString() + "_" + i.ToString() + ".vg");
				}
			}

			terra_image = new Mesh[NUMBER_OF_TERRAIN_TYPES];
			terra_image[0] = new Mesh("planets/terra0.vg");
			terra_image[1] = new Mesh("planets/terra1.vg");
			terra_image[2] = new Mesh("planets/terra2.vg");
			terra_image[3] = new Mesh("planets/terra3.vg");
			terra_image[4] = new Mesh("planets/terra4.vg");
			terra_image[5] = new Mesh("planets/terra5.vg");
			terra_image[6] = new Mesh("planets/terra6.vg");
			terra_image[7] = new Mesh("planets/terra7.vg");
		}



		// rendering stuff
		static public void render()
		{

			//manager.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);

			//Console.WriteLine("Number of Planets: "+Planet.count().ToString());

			Space.planet_shader.bind();
			GL.Uniform3(Space.planet_shader.getUniformLocation("scroll"), (float)Space.scroll_x, (float)Space.scroll_y, (float)Space.zoom);
			GL.Uniform3(Space.planet_shader.getUniformLocation("viewport"), GameWindow.active.mult_x, GameWindow.active.mult_y, Space.animation);
			Space.animation += 0.001f;

			float maxwidth = GL.GetFloat(GetPName.AliasedLineWidthRange);
			float linewidth = Math.Min(maxwidth, 3.0f * (float)Math.Max(Space.zoom, 1.0));
			GL.LineWidth(linewidth);
			foreach (Planet pl in planets)
			{
				if (true)//pl.cansee_temp || pl.seenbefore)
				{
					pl._render();
				}
			}

			if (Space.zoom > 0.2)
			{
				GL.LineWidth(Math.Min(maxwidth, 1.5f));
				Space.building_shader.bind();
				GL.Uniform3(Space.building_shader.getUniformLocation("scroll"), (float)Space.scroll_x, (float)Space.scroll_y, (float)Space.zoom);
				GL.Uniform3(Space.building_shader.getUniformLocation("viewport"), GameWindow.active.mult_x, GameWindow.active.mult_y, Space.animation);
				foreach (Planet pl in planets)
				{
					if (true)//pl.cansee_temp)
					{
						pl._render_buildings();
					}
				}
			}

			Space.planet_shader.unbind();
		}
		void prepare_vba()
		{

			float[] vertices = new float[(size * PLANET_DETAIL + 2) * 2];
			float[] terrain = new float[(size * PLANET_DETAIL + 2) * 1];
			float[] colour = new float[(size * PLANET_DETAIL + 2) * 3];

			//Mitte
			terrain[0] = -1.0f;
			vertices[0] = vertices[1] = 0.0f;
			colour[0] = colour[1] = colour[2] = 0.0f;
			float height_base = 20.0f * (float)size;

			//Vertices außen herum.
			for (int i = 0; i < size; i++)
			{
				//terra[i] = TERRA_MOUNTAIN;
				double randmont = (1.0 + (double)GameWindow.random.NextDouble() * 3.0) * Math.PI;
				if (terra[(i + 1) % size] == TERRA_MOUNTAIN)
				{
					randmont = (1.0 + (double)GameWindow.random.Next(3)) * Math.PI;
				}
				double randwat = (1.0 + (double)GameWindow.random.NextDouble() * 3.0) * Math.PI;
				if (terra[(i + 1) % size] == TERRA_WATER)
				{
					randwat = (1.0 + (double)GameWindow.random.Next(3)) * Math.PI;
				}
				for (int j = 0; j < PLANET_DETAIL; j++)
				{
					float height = height_base;
					double randvulk = (1.0 + (double)GameWindow.random.Next(2)) * Math.PI;

					//Terrain setzen
					terrain[(i * PLANET_DETAIL + j + 1) + 0] = (float)terra[i % size];


					//Farben setzen
					switch (terra[i])
					{
						case TERRA_DESERT:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 1.0f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 1.0f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.0f;
							if (climate == CLIMATE_TOXIC)
							{

								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.13f + Math.Max(0.0f, (float)GameWindow.random.Next(3)-1.0f) * 0.35f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.13f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.1f;
							}
							else if (climate == CLIMATE_COLD)
							{
								float hotness = (float)GameWindow.random.NextDouble();
								if (hotness > 0.5)
								{
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = hotness * 0.7f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = hotness * 0.7f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = hotness * 0.75f;
								}
							}
							else if (climate == CLIMATE_RAINY)
							{
								float hotness = (float)GameWindow.random.NextDouble();
								if (hotness > 0.75)
								{
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = hotness * 0.97f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = hotness * 0.64f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = hotness * 0.05f;
								}
							}

							height += (float)GameWindow.random.NextDouble() * 4.0f;
							break;

						case TERRA_FERTILE:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.2f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.5f + (float)GameWindow.random.NextDouble() * 0.5f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.2f;
							if (climate == CLIMATE_TOXIC)
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.13f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.13f + Math.Min(1.0f, (float)GameWindow.random.Next(3)) * 0.35f + (float)GameWindow.random.NextDouble() * 0.15f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.1f;
							}
							else if (climate == CLIMATE_HOT)
							{
								double hotness = GameWindow.random.NextDouble();
								if (hotness > 0.5)
								{
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = (float)hotness;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 1.0f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.0f;
								}
							}
							else if (climate == CLIMATE_COLD)
							{
								double hotness = GameWindow.random.NextDouble();
								if (hotness > 0.75)
								{
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 1.0f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 1.0f;
									colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
								}
							}
							height -= (float)GameWindow.random.NextDouble() * 4.0f;
							break;

						case TERRA_JUNGLE:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.1f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.2f + (float)GameWindow.random.NextDouble() * 0.5f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.1f;
							height += (float)(GameWindow.random.NextDouble() - 0.2) * 8.0f;
							break;

						case TERRA_ICE:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.8f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.8f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
							break;

						case TERRA_MOUNTAIN:
							float tone = (float)GameWindow.random.NextDouble();
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.35f + tone * 0.03f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.35f + tone * 0.03f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.35f + tone * 0.03f;
							if (climate == CLIMATE_COLD)
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] += 0.5f ;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] += 0.5f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] += 0.6f;
							}
							else if (climate == CLIMATE_HOT)
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] += 0.3f + tone * 0.2f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] += 0.1f + tone * 0.3f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] -= 0.05f;
							}
							else if (climate == CLIMATE_RAINY)
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] -= 0.15f + tone * 0.02f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] -= 0.1f + tone * 0.075f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] -= 0.15f + tone * 0.02f;
							}
							else if (climate == CLIMATE_TOXIC)
							{
								tone = (float)GameWindow.random.NextDouble();
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] -= 0.15f + tone * 0.03f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] -= 0.2f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] -= 0.2f + tone * 0.02f;
							}
							float hilly = (float)Math.Sin((double)j / (double)PLANET_DETAIL * randmont);
							float maxamp = (1.0f - Math.Abs((float)j / (float)PLANET_DETAIL - 0.5f) * 2.0f);
							if (j > PLANET_DETAIL / 2 && terra[(i + 1) % size] == TERRA_MOUNTAIN)
							{
								maxamp = 1.0f;
							}
							else if (j < PLANET_DETAIL / 2 && terra[(i - 1 + size) % size] == TERRA_MOUNTAIN)
							{
								maxamp = 1.0f;
							}

							height += maxamp * 30.0f * (hilly * (hilly + 0.013f) + 0.5f);
							break;

						case TERRA_RESSOURCES:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 1.0f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.0f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
							break;

						case TERRA_VOLCANO:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.2f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.2f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.2f;
							if (((double)j > (double)PLANET_DETAIL * 0.45) && ((double)j < (double)PLANET_DETAIL * 0.55))
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.5f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.0f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.0f;
							}
							float hilly2 = (float)Math.Sin((double)j / (double)PLANET_DETAIL * randvulk * 0.3);
							float maxamp2 = (1.0f - Math.Abs((float)j / (float)PLANET_DETAIL - 0.5f) * 2.0f);
							if (j > PLANET_DETAIL / 2 && terra[(i + 1) % size] == TERRA_MOUNTAIN)
							{
								maxamp = 1.0f;
							}
							else if (j < PLANET_DETAIL / 2 && terra[(i - 1 + size) % size] == TERRA_MOUNTAIN)
							{
								maxamp = 1.0f;
							}

							height += maxamp2 * 30.0f * (hilly2 * (hilly2 + 0.513f) + 0.5f);
							break; 

						case TERRA_WATER:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.051f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.051f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.8f;
							if (climate == CLIMATE_TOXIC)
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.003f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.43f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.31f;
							}
							else if (climate == CLIMATE_COLD)
							{
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.8f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.8f;
								colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
							}
							
							hilly = Math.Abs((float)Math.Sin((double)j / (double)PLANET_DETAIL * randwat));
							maxamp = (1.0f - Math.Abs((float)j / (float)PLANET_DETAIL - 0.5f));
							if (j > PLANET_DETAIL / 2 && terra[(i + 1) % size] == TERRA_WATER)
							{
								maxamp = 1.0f;
							}
							else if (j < PLANET_DETAIL / 2 && terra[(i - 1 + size) % size] == TERRA_WATER)
							{
								maxamp = 1.0f;
							}

							float multor = 10.0f;
							if (climate == CLIMATE_HOT || climate == CLIMATE_TOXIC) multor = 6.0f;
							else if (climate == CLIMATE_RAINY) multor = 13.0f;

							height -= maxamp * multor * (1.0f + hilly*0.12f + (hilly+1.05f)*(hilly+1.75f)*0.15f);// * (hilly + 0.015f) + 0.5f);
							break;

						default:
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.41f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.25f;
							colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.05f;
							break;
					}
					
					if (j == 0 && terra[i] != TERRA_WATER && terra[(i - 1 + size) % size] == TERRA_WATER)
					{
						height = height_base;
						//colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.25f;
						//colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.25f;
						//colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.75f;
					}
					if (j == PLANET_DETAIL-1 && terra[i] != TERRA_WATER && terra[(i + 1 + size) % size] == TERRA_WATER)
					{
						height = height_base;// -2.0f;
						//colour[(i * PLANET_DETAIL + j - 0) * 3 + 0] = 0.25f;
						//colour[(i * PLANET_DETAIL + j - 0) * 3 + 1] = 0.25f;
						//colour[(i * PLANET_DETAIL + j - 0) * 3 + 2] = 0.75f;
					}
					/*
					if (j == 0 && terra[i] == TERRA_WATER && terra[(i - 1 + size) % size] != TERRA_WATER)
					{
						//height = height_base;
						colour[(i * PLANET_DETAIL + j ) * 3 + 0] = 0.05f;
						colour[(i * PLANET_DETAIL + j ) * 3 + 1] = 0.05f;
						colour[(i * PLANET_DETAIL + j ) * 3 + 2] = 0.75f;
					}
					if (j == PLANET_DETAIL - 1 && terra[i] == TERRA_WATER && terra[(i + 1 + size) % size] != TERRA_WATER)
					{
						//height = height_base;// -2.0f;
						colour[(i * PLANET_DETAIL + j ) * 3 + 0] = 0.05f;
						colour[(i * PLANET_DETAIL + j ) * 3 + 1] = 0.05f;
						colour[(i * PLANET_DETAIL + j ) * 3 + 2] = 0.75f;
					}*/



					//Vertices setzen
					double alpha = 2.0 * (double)Math.PI * ((double)i + ((double)j / (double)PLANET_DETAIL)) / (double)size;
					vertices[(i * PLANET_DETAIL + j + 1) * 2 + 0] = (float)Math.Cos(alpha) * height;
					vertices[(i * PLANET_DETAIL + j + 1) * 2 + 1] = (float)Math.Sin(alpha) * height;

				}
			}


			vertices[(size * PLANET_DETAIL + 1) * 2 + 0] = vertices[2];
			vertices[(size * PLANET_DETAIL + 1) * 2 + 1] = vertices[3];
			terrain[(size * PLANET_DETAIL + 1) + 0] = terrain[1];
			colour[(size * PLANET_DETAIL + 1) * 3 + 0] = colour[3];
			colour[(size * PLANET_DETAIL + 1) * 3 + 1] = colour[4];
			colour[(size * PLANET_DETAIL + 1) * 3 + 2] = colour[5];


			//Create the vertex array object.
			if (gl_data == 0)
			{
				gl_data = GL.GenVertexArray();
				buf_pos = GL.GenBuffer();
				buf_ter = GL.GenBuffer();
				buf_col = GL.GenBuffer();

			}
			GL.BindVertexArray(gl_data);


			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(gl_data, 0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_ter);
			GL.BufferData(BufferTarget.ArrayBuffer, terrain.Length * sizeof(float), terrain, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(gl_data, 1);
			GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_col);
			GL.BufferData(BufferTarget.ArrayBuffer, colour.Length * sizeof(float), colour, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(gl_data, 2);
			GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindVertexArray(0);
			/*
			//Vertex-Daten uebergeben
			VertexBuffer vertexDataBuffer = new VertexBuffer();
			vertexDataBuffer.Create(manager.gl);
			vertexDataBuffer.Bind(manager.gl);
			vertexDataBuffer.SetData(manager.gl, 0, vertices, false, 2);

			//Terrain-Daten uebergeben
			VertexBuffer terraDataBuffer = new VertexBuffer();
			terraDataBuffer.Create(manager.gl);
			terraDataBuffer.Bind(manager.gl);
			terraDataBuffer.SetData(manager.gl, 1, terrain, false, 1);

			//Terrain-Daten uebergeben
			VertexBuffer colourDataBuffer = new VertexBuffer();
			colourDataBuffer.Create(manager.gl);
			colourDataBuffer.Bind(manager.gl);
			colourDataBuffer.SetData(manager.gl, 2, colour, false, 3);

			gl_data.Unbind(manager.gl);
			*/
			gl_data_update = false;

		}
		public void _render()
		{
			if (gl_data_update) prepare_vba();

			GL.Uniform1(Space.planet_shader.getUniformLocation("pos_x"), (float)x);
			GL.Uniform1(Space.planet_shader.getUniformLocation("pos_y"), (float)y);
			GL.Uniform1(Space.planet_shader.getUniformLocation("size"), (float)size);
			GL.Uniform3(Space.planet_shader.getUniformLocation("rgb"), r,g,b);
			//Space.planet_shader.id.SetUniform1(manager.gl, "pos_x", (float)x);
			//Space.planet_shader.id.SetUniform1(manager.gl, "pos_y", (float)y);
			//Space.planet_shader.id.SetUniform3(manager.gl, "rgb", r, g, b);
			//Space.planet_shader.id.SetUniform1(manager.gl, "size", (float)size);

			GL.BindVertexArray(gl_data);
			GL.DrawArrays(PrimitiveType.TriangleFan, 0, size * PLANET_DETAIL + 2);
			GL.DrawArrays(PrimitiveType.LineLoop, 1, size * PLANET_DETAIL + 1);
			GL.BindVertexArray(0);
		}
		public void _render_buildings()
		{
			GL.LineWidth(2.0f);
			GL.Enable(EnableCap.LineSmooth);
			GL.Disable(EnableCap.DepthTest);

			GL.Uniform1(Space.building_shader.getUniformLocation("pos_x"), (float)x);
			GL.Uniform1(Space.building_shader.getUniformLocation("pos_y"), (float)y);
			GL.Uniform1(Space.building_shader.getUniformLocation("size"), (float)size);
			//Space.building_shader.id.SetUniform1(manager.gl, "pos_x", (float)x);
			//Space.building_shader.id.SetUniform1(manager.gl, "pos_y", (float)y);
			//Space.building_shader.id.SetUniform1(manager.gl, "size", (float)size);

			for (int ii = 0; ii < size; ii++)
			{
				
				if (terra[ii] == TERRA_JUNGLE)
				{
					int cc = (random * size + ii) % JUNGLE_VARIETY;
					float transp = 1.0f;
					if (infra[ii] != null) transp = 1.0f - infra[ii].getHealthFraction() * 0.75f;
					GL.Uniform3(Space.building_shader.getUniformLocation("info"), (float)ii, 1.0f, transp);
					GL.Uniform3(Space.building_shader.getUniformLocation("rgb"), 1.0f, 1.0f, 1.0f);
					GL.BindVertexArray(jungle[cc, climate].gl_data);
					GL.DrawArrays(PrimitiveType.Lines, 0, jungle[cc, climate].num_lines * 2);
				}

				if (infra[ii] != null)
				{
					//Console.WriteLine(i.ToString()+" x "+infra[i].bp.shape.num_lines.ToString());

					float ycut = 1.0f;
					float transp = 1.0f;
					if (infra[ii].status == Building.STATUS_DESTROYED || infra[ii].status == Building.STATUS_DESTROYED_AND_UNDERCONSTRUCTION) transp = 1.0f - infra[ii].timer * 0.01f;
					if (infra[ii].status == Building.STATUS_UNDERCONSTRUCTION || infra[ii].status == Building.STATUS_DESTROYED_AND_UNDERCONSTRUCTION) ycut = infra[ii].getHealthFraction() * infra[ii].bp.shape.box_y;


					GL.Uniform3(Space.building_shader.getUniformLocation("info"), (float)ii, (float)ycut, (float)transp);
					GL.Uniform3(Space.building_shader.getUniformLocation("rgb"), 0.5f + 0.5f * infra[ii].owner.r, 0.5f + 0.5f * infra[ii].owner.g, 0.5f + 0.5f * infra[ii].owner.b);
					//Space.building_shader.id.SetUniform3(manager.gl, "info", (float)ii, ycut, transp);
					//Space.building_shader.id.SetUniform3(manager.gl, "rgb", 0.5f + 0.5f * infra[ii].owner.r, 0.5f + 0.5f * infra[ii].owner.g, 0.5f + 0.5f * infra[ii].owner.b);

					GL.BindVertexArray(infra[ii].bp.shape.gl_data);
					GL.DrawArrays(PrimitiveType.Lines, 0, infra[ii].bp.shape.num_lines * 2);

					//infra[ii].bp.shape.gl_data.Bind(manager.gl);
					//manager.gl.DrawArrays(OpenGL.GL_LINES, 0, infra[ii].bp.shape.num_lines * 2);
					//infra[ii].bp.shape.gl_data.Unbind(manager.gl);
				}
				
				
				
			}
			GL.BindVertexArray(0);

			GL.Enable(EnableCap.DepthTest);
		}

		// static functions
		static public int count()
		{
			return planets.Count;
		}


		// methods
		public byte getTerrain(int i)
		{
			return terra[i];
		}
		public string getTerrainType(int i)
		{
			if (i < 0 || i >= size) return "";
			switch (terra[i])
			{
				case TERRA_DESERT:
					return "Wüste";
				case TERRA_FERTILE:
					return "Ebene";
				case TERRA_ICE:
					return "Eis";
				case TERRA_JUNGLE:
					return "Dschungel";
				case TERRA_MOUNTAIN:
					return "Gebirge";
				case TERRA_RESSOURCES:
					return "Ressourcenanhäufung";
				case TERRA_VOLCANO:
					return "Vulkan";
				case TERRA_WATER:
					return "Ozean";
				default:
					return "Unbekanntes Gelände";
			}
		}
		public string getClimate()
		{
			switch (climate)
			{
				case CLIMATE_COLD:
					return "Kalt";
				case CLIMATE_HOT:
					return "Heiß";
				case CLIMATE_NORMAL:
					return "Gemäßigt";
				case CLIMATE_TOXIC:
					return "Toxisch";
				case CLIMATE_RAINY:
					return "Humid";
				default:
					return "Unbekannt";
			}
		}
		public Civilisation getDominantCiv()
		{
			byte[] points = new byte[Civilisation.count];
			for (int i = 0; i < size; i++)
			{
				if (infra[i] != null)
				{
					points[infra[i].owner.getID()]++;
				}
			}
			Civilisation max_civ = null;
			byte max_points = 0;
			for (int i = 0; i < Civilisation.count; i++)
			{
				if (points[i] > max_points)
				{
					max_points = points[i];
					max_civ = Civilisation.data[i];
				}
			}
			return max_civ;
		}
		public string getDominantCivName()
		{
			Civilisation civ = getDominantCiv();
			if (civ == null)
			{
				return "Status:\tunbesiedelt";
			}
			else if (civ == Civilisation.getPlayer())
			{
				return "Status:\teigene Kolonie";
			}
			return "Status:\tbesetzt (" + civ.name + ")";
		}
		public Building build(int i, Blueprint bp, Civilisation civ, bool pay_for_it = true)
		{
			if (pay_for_it) civ.ress -= bp.getCost();
			infra[i] = new Building(this, i, bp, civ);
			return infra[i];
		}
		public Building upgrade(int i, Blueprint bp, bool pay_for_it = true)
		{
			if (infra[i].bp.develop_into.Contains(bp))
			{
				if (pay_for_it) infra[i].owner.ress -= (bp.getCost() - infra[i].bp.getCost());
				infra[i].bp = bp;
				infra[i].status = Building.STATUS_UNDERCONSTRUCTION;
				infra[i].health_max = bp.getHealth(infra[i].owner);
			}
			return infra[i];
		}

		public List<Blueprint> ListOfBuildables(int spot, Civilisation civ)
		{
			List<Blueprint> list = new List<Blueprint>();
			if (!canBuild(civ)) return list;

			if (infra[spot] == null)
			{
				foreach (Blueprint bp in Blueprint.data)
				{
					if (bp.activation.researchedBy(civ) && bp.build_on[terra[spot]] && !bp.upgrade_only) list.Add(bp);
				}
			}
			else if (infra[spot].owner == civ && infra[spot].status == Building.STATUS_NONE)
			{
				foreach (Blueprint bp in infra[spot].bp.develop_into)
				{
					if (bp.activation.researchedBy(civ) && bp.build_on[terra[spot]]) list.Add(bp);
				}
			}


			return list;
		}

		public bool canBuild(Civilisation civ)
		{
			return true;
			for (int i = 0; i < size; i++)
			{
				if (infra[i] != null && infra[i].owner == civ && infra[i].bp.specials == Blueprint.SPECIAL_SETTLEMENT && infra[i].status == Building.STATUS_NONE) return true;
			}
			return false;
		}
		public float getEnergy(Civilisation civ)
		{
			float energy = 100.0f;
			for (int i = 0; i < size; i++)
			{
				if (infra[i] != null)
				{
					if (infra[i].owner == civ)
					{
						energy -= infra[i].bp.energy;
						if (infra[i].bp.specials == Blueprint.SPECIAL_ENERGY)
						{
							energy += infra[i].bp.special_strength;
						}
					}
				}
			}
			return energy;
		}

		static public void simulate(float dt = 1.0f)
		{
			foreach (Civilisation civ in Civilisation.data)
			{
				civ.max_population_new = 10;
			}

			foreach (Planet pl in planets)
			{
				pl.sim(dt);
			}

			int maxpop = Civilisation.getMaxPopulationPerCiv();
			foreach (Civilisation civ in Civilisation.data)
			{
				civ.max_population = Math.Min(civ.max_population_new, maxpop);
			}

		}
		void sim(float dt)
		{
			float[] efficiency = new float[Civilisation.count];


			for (int i = 0; i < size; i++)
			{
				if (infra[i] != null)
				{
					if (infra[i].owner == null)
					{
						infra[i].sim(dt, 0.0f);
					}
					else
					{
						infra[i].sim(dt, efficiency[infra[i].owner.getID()]);
					}
				}


				// Natureffekte wechseln durch
				timer[i] -= dt*0.01f;
				if (timer[i] <= 0.0f)
				{
					timer[i] = (float)GameWindow.random.NextDouble()*100.0f + 10.0f;
					effect[i] = EFFECT_NONE;

					if ( (climate == CLIMATE_RAINY && GameWindow.random.Next(20) == 0) || (climate == CLIMATE_NORMAL && GameWindow.random.Next(40) == 0) || (climate == CLIMATE_COLD && GameWindow.random.Next(10) == 0) ) // Fängt an zu regnen
					{
						effect[i] = EFFECT_RAIN;
					}
					else if (terra[i] == TERRA_VOLCANO && GameWindow.random.Next(100) == 0) // Vulkanausbruch
					{
						effect[i] = EFFECT_VOLCANO_ERUPTION;
					}
				}

				//Regnet es gerade?
				float height, rot;
				Particle p;
				switch (effect[i])
				{
					case EFFECT_RAIN:
						if ((float)GameWindow.random.NextDouble() < Math.Min(0.5f, timer[i]*0.1f))
						{
							float subfield = (float)GameWindow.random.NextDouble();
							height = 20.0f * size + 100.0f + 30.0f * ((float)GameWindow.random.NextDouble()-0.5f) * (1.0f - 4.0f*(subfield-0.5f)*(subfield-0.5f));
							if (climate == CLIMATE_COLD) { height = 20.0f * size + 10.0f + 50.0f * ((float)GameWindow.random.NextDouble()) * (2.0f - 4.0f * (subfield - 0.5f) * (subfield - 0.5f)); subfield *= 4.0f - 1.5f;}
							else { subfield *= 2.0f - 0.5f; }
							rot = (float)Math.PI * 2.0f * (subfield + (float)i)/(float)size;
							p = new Particle(x + (float)Math.Cos(rot) * height, y + (float)Math.Sin(rot) * height);
							p.setSpeed(-(float)Math.Sin(rot) * (float)Math.Sin(timer[i] * 0.1f + (float)i) * 30.0f, (float)Math.Cos(rot) * (float)Math.Sin(timer[i] * 0.1f + (float)i) * 30.0f);
							if (climate == CLIMATE_COLD)
							{
								p.setColor(1.0f, 1.0f, 1.0f, 0.05f);
							}
							else
							{
								p.setColor(0.7f, 0.7f, 1.0f, 0.1f);
								p.setGravity(this);
							}
						}
						break;
					case EFFECT_VOLCANO_ERUPTION:
						rot = (float)Math.PI * 2.0f * (0.5f + (float)i) / (float)size;
						height = 21.5f * size;
						p = new Particle(x + (float)Math.Cos(rot) * height, y + (float)Math.Sin(rot) * height);

						height = (float)GameWindow.random.NextDouble() * 20.0f + 20.0f * (1.5f + (float)Math.Cos(timer[i] * 0.13f));
						if ((float)GameWindow.random.NextDouble() < Math.Min(0.5f, timer[i]*0.1f)) // Lava
						{
							rot += (((float)GameWindow.random.NextDouble() - 0.5f) * 0.3f + 0.3f * (float)Math.Sin(timer[i] * 0.1f + (float)i)) * (float)Math.PI;
							p.setColor(0.8f, (float)GameWindow.random.NextDouble()*0.7f, 0.0f, 0.2f);
							p.setSpeed((float)Math.Cos(rot) * height, (float)Math.Sin(rot) * height);
							p.setGravity(this);
						}
						else // Nur Qualm
						{
							rot += 0.75f*(((float)GameWindow.random.NextDouble() - 0.5f) * 0.7f + 0.4f * (float)Math.Sin(timer[i] * 0.1f + (float)i)) * (float)Math.PI;
							p.setColor(0.72f, 0.72f, 0.72f, 0.06f);
							p.setSpeed((float)Math.Cos(rot) * height, (float)Math.Sin(rot) * height);

						}
						break;
				}

			}
		}
















	}
}
