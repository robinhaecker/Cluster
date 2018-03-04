using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using OpenTK.Graphics.OpenGL;


using Cluster.math;
using Cluster.Rendering.Appearance;
using Cluster.Rendering.Draw3D.ProceduralGeneration;


namespace Cluster.Rendering.Draw3D
{
    class Terrain
    {
        public static Terrain active;


        public static vec3 sun;
        int size;
        public float[,,] data;
        float time;


		int water_tex;



        int vertexArrayObject;
        int buf_pos, buf_col, buf_nor, buf_water;
		int index_count;




        static Shader terrain_shader, terrain_shader_prerender;
        static Shader water_shader;

        public static FrameBuffer oben, unten;




        

        public Terrain(int size = 256)
        {
			/*
			// Die Farbpalette, die vorkommen darf
			List<vec3> colors = new List<vec3>();
			colors.Add(new vec3(0.5f, 0.5f, 0.5f));  //Grau
			colors.Add(new vec3(0.0f, 0.6f, 0.0f));  //Grün
			colors.Add(new vec3(0.7f, 0.45f, 0.1f)); //Braun
			colors.Add(new vec3(1.0f, 0.8f, 0.1f));  //Gelb-Sandig
			colors.Add(new vec3(0.3f, 0.9f, 0.3f));  //Blassgrün
			colors.Add(new vec3(0.22f, 0.2f, 0.2f));  //Dunkler Fels
			colors.Add(new vec3(0.12f, 0.1f, 0.1f));  //Sehr dunkler Fels
			colors.Add(new vec3(0.7f, 0.25f, 0.1f)); //Orange
			*/


            sun = new vec3(1.05f, 1.0f, -0.5f);
            this.size = size;
			this.data = new float[size + 1, size + 1, 5];
			active = this;
			TerrainGenerator.Generate(this);



            int index = 0;
			float[] grid = new float[size * size * 3 * 6 ];
			float[] normals = new float[size * size * 3 * 6 ];
			float[] dat_col = new float[size * size * 3 * 6 ];
			float[] water_height = new float[size * size * 4 * 6 ];

			for (int chunk_i = 0; chunk_i < SceneGraph.chunkCount; chunk_i++)
			{
				for (int chunk_j = 0; chunk_j < SceneGraph.chunkCount; chunk_j++)
				{

					SceneGraph.data[chunk_i, chunk_j].terrain_start_index = index;
					int x0 = Math.Max(0, (int)Math.Floor(SceneGraph.chunkSize * chunk_i));// - SceneGraph.worldSize - size * 0.5f));
					int y0 = Math.Max(0, (int)Math.Floor(SceneGraph.chunkSize * chunk_j));// - SceneGraph.worldSize - size * 0.5f));

					int x1 = Math.Min(size-1, (int)Math.Floor(SceneGraph.chunkSize * (chunk_i+1)));// - SceneGraph.worldSize - size * 0.5f));
					int y1 = Math.Min(size-1, (int)Math.Floor(SceneGraph.chunkSize * (chunk_j+1)));// - SceneGraph.worldSize - size * 0.5f));

					for (int x = x0; x < x1; x++)
					{
						for (int y = y0; y < y1; y++)
						{
							if ( (x - size/2)*(x - size/2) + (y - size/2)*(y - size/2) > (size+1)*(size+1)/4 ) continue;


							int ox = GameWindow.random.Next(2), oy = GameWindow.random.Next(2);
							vec3 color = new vec3(data[x + ox, y + oy, 1], data[x + ox, y + oy, 2], data[x + ox, y + oy, 3]);

							if ((x + y) % 2 == 1)
							{
								vec3 nor = (new vec3(1.0f, data[x + 1, y, 0] - data[x, y, 0], 0.0f)) % (new vec3(1.0f, data[x + 1, y + 1, 0] - data[x, y, 0], 1.0f));
								nor /= -nor.length();
								grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x, y, 0];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x + 1, y + 1, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x + 1, y, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;


								ox = GameWindow.random.Next(2); oy = GameWindow.random.Next(2);
								color = new vec3(data[x + ox, y + oy, 1], data[x + ox, y + oy, 2], data[x + ox, y + oy, 3]);

								nor = (new vec3(1.0f, data[x + 1, y + 1, 0] - data[x, y, 0], 1.0f)) % (new vec3(0.0f, data[x, y + 1, 0] - data[x, y, 0], 1.0f));
								nor /= -nor.length();
								grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x, y, 0];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x, y + 1, 0];
								//dat_col[index * 3 + 0] = data[x, y + 1, 1]; dat_col[index * 3 + 1] = data[x, y + 1, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								//dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x + 1, y + 1, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;
							}
							else
							{
								vec3 nor = (new vec3(1.0f, data[x + 1, y, 0] - data[x, y, 0], 0.0f)) % (new vec3(0.0f, data[x, y + 1, 0] - data[x, y, 0], 1.0f));
								nor /= -nor.length();
								grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x, y, 0];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1); grid[index * 3 + 2] = data[x, y + 1, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x + 1, y, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								ox = GameWindow.random.Next(2); oy = GameWindow.random.Next(2);
								color = new vec3(data[x + ox, y + oy, 1], data[x + ox, y + oy, 2], data[x + ox, y + oy, 3]);

								nor = (new vec3(-1.0f, data[x, y + 1, 0] - data[x + 1, y + 1, 0], 0.0f)) % (new vec3(0.0f, data[x + 1, y, 0] - data[x + 1, y + 1, 0], -1.0f));
								nor /= -nor.length();

								grid[index * 3 + 0] = (x + 1); grid[index * 3 + 1] = (y + 1); grid[index * 3 + 2] = data[x + 1, y + 1, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x + 1, y, 0];
								//dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
								//dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

								grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x, y + 1, 0];
								//dat_col[index * 3 + 0] = data[x, y + 1, 1]; dat_col[index * 3 + 1] = data[x, y + 1, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
								//dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
								dat_col[index * 3 + 0] = color.x; dat_col[index * 3 + 1] = color.y; dat_col[index * 3 + 2] = color.z;
								water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
								normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

							}
						}
					}

					SceneGraph.data[chunk_i, chunk_j].terrain_end_index = index;
				}
			}
			index_count = index;
			/*
			 * 
			for (int x = 0; x < size - 1; x++)
			{
				for (int y = 0; y < size - 1; y++)
				{
					if ((x + y) % 2 == 1)
					{
						vec3 nor = (new vec3(1.0f, data[x + 1, y, 0] - data[x, y, 0], 0.0f)) % (new vec3(1.0f, data[x + 1, y + 1, 0] - data[x, y, 0], 1.0f));
						nor /= -nor.length();
						grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x, y, 0];
						dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x + 1, y + 1, 0];
						dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x + 1, y, 0];
						dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						nor = (new vec3(1.0f, data[x + 1, y + 1, 0] - data[x, y, 0], 1.0f)) % (new vec3(0.0f, data[x, y + 1, 0] - data[x, y, 0], 1.0f));
						nor /= -nor.length();
						grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x, y, 0];
						dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x, y + 1, 0];
						dat_col[index * 3 + 0] = data[x, y + 1, 1]; dat_col[index * 3 + 1] = data[x, y + 1, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x + 1, y + 1, 0];
						dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;
					}
					else
					{
						vec3 nor = (new vec3(1.0f, data[x + 1, y, 0] - data[x, y, 0], 0.0f)) % (new vec3(0.0f, data[x, y + 1, 0] - data[x, y, 0], 1.0f));
						nor /= -nor.length();
						grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x, y, 0];
						dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1); grid[index * 3 + 2] = data[x, y + 1, 0];
						dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x + 1, y, 0];
						dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
						water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						nor = (new vec3(-1.0f, data[x, y+1, 0] - data[x+1, y+1, 0], 0.0f)) % (new vec3(0.0f, data[x+1, y, 0] - data[x+1, y+1, 0], -1.0f));
						nor /= -nor.length();
						grid[index * 3 + 0] = (x +1); grid[index * 3 + 1] = (y+1); grid[index * 3 + 2] = data[x+1, y+1, 0];
						dat_col[index * 3 + 0] = data[x+1, y+1, 1]; dat_col[index * 3 + 1] = data[x+1, y+1, 2]; dat_col[index * 3 + 2] = data[x+1, y+1, 3];
						water_height[index * 4 + 0] = data[x+1, y+1, 4]; water_height[index * 4 + 1] = data[x, y+1, 4]; water_height[index * 4 + 2] = data[x+1, y, 4]; water_height[index * 4 + 3] = data[x+1, y+1, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = data[x + 1, y, 0];
						dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
						water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

						grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x, y + 1, 0];
						dat_col[index * 3 + 0] = data[x, y + 1, 1]; dat_col[index * 3 + 1] = data[x, y + 1, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
						water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 1] = data[x, y + 1, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
						normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					}
				}
			}
			 * 
			 * 
			 * // Alte Gittermethode
            for (int x = 0; x < size-1; x++)
            {
                for (int y = 0; y < size-1; y++)
                {
                    vec3 nor = (new vec3(1.0f, data[x + 1, y, 0] - data[x, y, 0], 0.0f)) % (new vec3(1.0f, data[x + 1, y + 1, 0] - data[x, y, 0], 1.0f));
                    nor /= -nor.length();
                    grid[index * 3 + 0]    = (x);              grid[index * 3 + 1] = (y);              grid[index * 3 + 2] = data[x, y, 0];
                    dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
                    water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
                    normals[index * 3 + 0] = nor.x;         normals[index * 3 + 1] = nor.y;         normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x + 1, y + 1, 0];
					dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
					water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

                    grid[index * 3 + 0]    = (x + 1.0f);           grid[index * 3 + 1] = (y);                  grid[index * 3 + 2] = data[x + 1, y, 0];
                    dat_col[index * 3 + 0] = data[x + 1, y, 1]; dat_col[index * 3 + 1] = data[x + 1, y, 2]; dat_col[index * 3 + 2] = data[x + 1, y, 3];
                    water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x+1, y, 4];
                    normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

                    nor = (new vec3(1.0f, data[x + 1, y + 1, 0] - data[x, y, 0], 1.0f)) % (new vec3(0.0f, data[x, y + 1, 0] - data[x, y, 0], 1.0f));
                    nor /= -nor.length();
                    grid[index * 3 + 0]    = (x);           grid[index * 3 + 1] = (y);              grid[index * 3 + 2] = data[x, y, 0];
                    dat_col[index * 3 + 0] = data[x, y, 1]; dat_col[index * 3 + 1] = data[x, y, 2]; dat_col[index * 3 + 2] = data[x, y, 3];
                    water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
                    normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = data[x, y + 1, 0];
					dat_col[index * 3 + 0] = data[x, y + 1, 1]; dat_col[index * 3 + 1] = data[x, y + 1, 2]; dat_col[index * 3 + 2] = data[x, y + 1, 3];
					water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

                    grid[index * 3 + 0]    = (x + 1.0f);            grid[index * 3 + 1] = (y + 1.0f);               grid[index * 3 + 2] = data[x + 1, y + 1, 0];
                    dat_col[index * 3 + 0] = data[x + 1, y + 1, 1]; dat_col[index * 3 + 1] = data[x + 1, y + 1, 2]; dat_col[index * 3 + 2] = data[x + 1, y + 1, 3];
                    water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x+1, y+1, 4];
                    normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

                }
            }
			/* //Erweiterung des Wassers
			for (int x = -size; x < 2*size - 1; x++)
			{
				for (int y = -size; y < 2*size - 1; y++)
				{
					if ((x>=0 && x< size-1) && (y>=0 && y< size-1))
					{
						continue;
					}
					vec3 nor = new vec3(0.0f, 1.0f, 0.0f);
					grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = minter;
					dat_col[index * 3 + 0] = 0.7f; dat_col[index * 3 + 1] = 0.5f; dat_col[index * 3 + 2] = 0.1f;
					water_height[index * 4 + 0] = data[(x + size) % size, (y + size) % size, 4]; water_height[index * 4 + 2] = data[(x + 1 + size) % size, (y + size) % size, 4]; water_height[index * 4 + 1] = data[(x + 1 + size) % size, (y + 1 + size) % size, 4]; water_height[index * 4 + 3] = data[(x + size) % size, (y + size) % size, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = minter;
					dat_col[index * 3 + 0] = 0.7f; dat_col[index * 3 + 1] = 0.5f; dat_col[index * 3 + 2] = 0.1f;
					//water_height[index * 4 + 1] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
					//water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
					water_height[index * 4 + 0] = data[(x + size) % size, (y + size) % size, 4]; water_height[index * 4 + 2] = data[(x + 1 + size) % size, (y + size) % size, 4]; water_height[index * 4 + 1] = data[(x + 1 + size) % size, (y + 1 + size) % size, 4]; water_height[index * 4 + 3] = data[(x+1 + size) % size, (y+1 + size) % size, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = minter;
					dat_col[index * 3 + 0] = 0.7f; dat_col[index * 3 + 1] = 0.5f; dat_col[index * 3 + 2] = 0.1f;
					//water_height[index * 4 + 2] = data[x, y, 4]; water_height[index * 4 + 0] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
					//water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y, 4];
					water_height[index * 4 + 0] = data[(x + size) % size, (y + size) % size, 4]; water_height[index * 4 + 2] = data[(x + 1 + size) % size, (y + size) % size, 4]; water_height[index * 4 + 1] = data[(x + 1 + size) % size, (y + 1 + size) % size, 4]; water_height[index * 4 + 3] = data[(x+1 + size) % size, (y + size) % size, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y); grid[index * 3 + 2] = minter;
					dat_col[index * 3 + 0] = 0.7f; dat_col[index * 3 + 1] = 0.5f; dat_col[index * 3 + 2] = 0.1f;
					//water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y, 4];
					water_height[index * 4 + 0] = data[(x + size) % size, (y + size) % size, 4]; water_height[index * 4 + 1] = data[(x + 1 + size) % size, (y + size) % size, 4]; water_height[index * 4 + 2] = data[(x + 1 + size) % size, (y + 1 + size) % size, 4]; water_height[index * 4 + 3] = data[(x + size) % size, (y + size) % size, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = minter;
					dat_col[index * 3 + 0] = 0.7f; dat_col[index * 3 + 1] = 0.5f; dat_col[index * 3 + 2] = 0.1f;
					//water_height[index * 4 + 1] = data[x, y, 4]; water_height[index * 4 + 2] = data[x + 1, y, 4]; water_height[index * 4 + 0] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
					//water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x, y + 1, 4];
					water_height[index * 4 + 0] = data[(x + size) % size, (y + size) % size, 4]; water_height[index * 4 + 1] = data[(x + 1 + size) % size, (y + size) % size, 4]; water_height[index * 4 + 2] = data[(x + 1 + size) % size, (y + 1 + size) % size, 4]; water_height[index * 4 + 3] = data[(x + size) % size, (y +1+ size) % size, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

					grid[index * 3 + 0] = (x + 1.0f); grid[index * 3 + 1] = (y + 1.0f); grid[index * 3 + 2] = minter;
					dat_col[index * 3 + 0] = 0.7f; dat_col[index * 3 + 1] = 0.5f; dat_col[index * 3 + 2] = 0.1f;
					//water_height[index * 4 + 2] = data[x, y, 4]; water_height[index * 4 + 0] = data[x + 1, y, 4]; water_height[index * 4 + 1] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
					//water_height[index * 4 + 0] = data[x, y, 4]; water_height[index * 4 + 1] = data[x + 1, y, 4]; water_height[index * 4 + 2] = data[x + 1, y + 1, 4]; water_height[index * 4 + 3] = data[x + 1, y + 1, 4];
					water_height[index * 4 + 0] = data[(x + size) % size, (y + size) % size, 4]; water_height[index * 4 + 1] = data[(x + 1 + size) % size, (y + size) % size, 4]; water_height[index * 4 + 2] = data[(x + 1 + size) % size, (y + 1 + size) % size, 4]; water_height[index * 4 + 3] = data[(x+1 + size) % size, (y+1 + size) % size, 4];
					normals[index * 3 + 0] = nor.x; normals[index * 3 + 1] = nor.y; normals[index * 3 + 2] = nor.z; index += 1;

				}
			}
			*/



			water_tex = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, water_tex);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size, size, 0, PixelFormat.Rgba, PixelType.Float, water_height);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.BindTexture(TextureTarget.Texture2D, 0);

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            buf_pos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
            GL.BufferData(BufferTarget.ArrayBuffer, index_count*3* sizeof(float), grid, BufferUsageHint.StaticDraw);//grid.Length
            GL.EnableVertexArrayAttrib(vertexArrayObject, 0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            
            buf_nor = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_nor);
			GL.BufferData(BufferTarget.ArrayBuffer, index_count*3*sizeof(float), normals, BufferUsageHint.StaticDraw);//normals.Length
            GL.EnableVertexArrayAttrib(vertexArrayObject, 1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            buf_col = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_col);
			GL.BufferData(BufferTarget.ArrayBuffer, index_count*3*sizeof(float), dat_col, BufferUsageHint.StaticDraw);//dat_col.Length
            GL.EnableVertexArrayAttrib(vertexArrayObject, 2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            
            buf_water = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_water);
			GL.BufferData(BufferTarget.ArrayBuffer, index_count*4*sizeof(float), water_height, BufferUsageHint.StaticDraw);//water_height.Length
            GL.EnableVertexArrayAttrib(vertexArrayObject, 3);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            
            GL.BindVertexArray(0);

            /*
                
            buf_nor = new VertexBuffer();
            buf_nor.Create(manager.gl);
            buf_nor.Bind(manager.gl);
            buf_nor.SetData(manager.gl, 1, normals, true, 3);

            buf_col = new VertexBuffer();
            buf_col.Create(manager.gl);
            buf_col.Bind(manager.gl);
            buf_col.SetData(manager.gl, 2, dat_col, false, 3);

            buf_water = new VertexBuffer();
            buf_water.Create(manager.gl);
            buf_water.Bind(manager.gl);
            buf_water.SetData(manager.gl, 3, water_height, false, 4);


            gl_data.Unbind(manager.gl);
             * */
        }


        
        public void cleanUp()
        {
            if (vertexArrayObject != 0)
            {
                GL.DeleteVertexArray(vertexArrayObject);
                GL.DeleteBuffer(buf_pos);
                GL.DeleteBuffer(buf_nor);
                GL.DeleteBuffer(buf_col);
                GL.DeleteBuffer(buf_water);
            }
        }


		static public float getHeight(float x, float y)
		{
			if ((x * x + y * y) >= (active.size - 1) * (active.size - 1) * 0.25f) return -100.0f;

			x += active.size * 0.5f;
			y += active.size * 0.5f;
			if (x <= 0 || y <= 0 || x >= active.size || y >= active.size) return -100.0f;

			float blendx = x - (float)Math.Floor(x);
			float blendy = y - (float)Math.Floor(y);

			float h0 = active.data[(int)Math.Floor(x), (int)Math.Floor(y), 0] * (1.0f - blendx) + blendx * active.data[(int)Math.Ceiling(x), (int)Math.Floor(y), 0];
			float h1 = active.data[(int)Math.Floor(x), (int)Math.Ceiling(y), 0] * (1.0f - blendx) + blendx * active.data[(int)Math.Ceiling(x), (int)Math.Ceiling(y), 0];
			return (h0 * (1.0f - blendy) + h1 * (blendy));
		}

		static public vec3 getNormal(float x, float y)
		{
			if ((x * x + y * y) >= (active.size - 1) * (active.size - 1) * 0.25f) return new vec3();

			x += active.size * 0.5f;
			y += active.size * 0.5f;
			if (x <= 0 || y <= 0 || x >= active.size-1 || y >= active.size-1) return new vec3();

			float h0 = active.data[(int)Math.Floor(x), (int)Math.Floor(y), 0];

			return ((new vec3(0.0f, active.data[(int)Math.Floor(x), (int)Math.Floor(y)+1, 0] - h0, 1.0f)) % (new vec3(1.0f, active.data[(int)Math.Floor(x) + 1, (int)Math.Floor(y), 0] - h0, 0.0f))).normalize();

		}

		static public vec2 getGradient(float x, float y)
		{
			float h0 = getHeight(x, y);
			return new vec2(getHeight(x + 1.0f, y) - h0, getHeight(x, y + 1.0f) - h0);
		}



		static public int getSize()
		{
			return active.size;
		}



        public static void init()
        {
            terrain_shader = new Shader("terrain.vert", "terrain.frag");
            terrain_shader_prerender = new Shader("terrain.vert", "terrain_pre.frag");
            water_shader = new Shader("water.vert", "water.frag");


			oben = new FrameBuffer(GameWindow.active.width, GameWindow.active.height);//512, 512);//GameWindow.active.width, GameWindow.active.height);
			unten = new FrameBuffer(GameWindow.active.width, GameWindow.active.height);//512, 512);//GameWindow.active.width, GameWindow.active.height);
        }
        


		

        public void render(Camera cam, int prerender = 0)
        {
			//if (prerender == 3) prerender = 0;


            GL.BindVertexArray(vertexArrayObject);

			float cldir = 0.0f;
			if (prerender == 1) cldir = 1.0f;
			if (prerender == 2) cldir = -1.0f;
			if (prerender == 0)
			{
				cldir = 2.0f;
				GL.Enable(EnableCap.ClipDistance0);
			}


			GameWindow.active.shadows.BindDepthTexture(0);

            //if (true)//prerender == 0)
            //{



                terrain_shader.bind();
                GL.UniformMatrix4(terrain_shader.getUniformLocation("projection"), 1, false, cam.getProjection());
                GL.UniformMatrix4(terrain_shader.getUniformLocation("camera"), 1, false, cam.getInversePosition());
				//if (prerender == 3) GL.UniformMatrix4(terrain_shader.getUniformLocation("camera"), 1, false, GameWindow.active.shadowMatrix.convertToGL());
                GL.Uniform3(terrain_shader.getUniformLocation("sun"), sun.x, sun.y, sun.z);
				GL.Uniform1(terrain_shader.getUniformLocation("clip_direction"), cldir);
				GL.Uniform1(terrain_shader.getUniformLocation("terrainsize"), (float)size);
				
				if (false)//prerender == 0)
				{
					//GL.UniformMatrix4(terrain_shader.getUniformLocation("shadow"), 1, false, GameWindow.active.shadowMatrix.convertToGL());
					GL.Uniform1(terrain_shader.getUniformLocation("shadow_enabled"), 1.0f);
				}
				else
				{
					GL.Uniform1(terrain_shader.getUniformLocation("shadow_enabled"), 0.0f);
				}
            /*}
            else
            {
				Console.WriteLine("prerender = " + prerender.ToString());
                terrain_shader_prerender.bind();
                GL.UniformMatrix4(terrain_shader_prerender.getUniformLocation("projection"), 1, false, cam.getProjection());
                GL.UniformMatrix4(terrain_shader_prerender.getUniformLocation("camera"), 1, false, cam.getInversePosition());
				GL.Uniform3(terrain_shader_prerender.getUniformLocation("sun"), sun.x, sun.y, sun.z);
				GL.Uniform1(terrain_shader_prerender.getUniformLocation("clip_direction"), cldir);
				GL.Uniform1(terrain_shader.getUniformLocation("terrainsize"), (float)size);


				GL.Uniform1(terrain_shader.getUniformLocation("clip_direction"), 0.0f);
            }*/

            /*
            terrain_shader.bind();

            GL.UniformMatrix4(terrain_shader.getUniformLocation("projection"), 1, false, cam.getProjection());
            GL.UniformMatrix4(terrain_shader.getUniformLocation("camera"), 1, false, cam.getInversePosition());
            GL.Uniform3(terrain_shader.getUniformLocation("sun"), sun.x, sun.y, sun.z);
            if (prerender == 0) GL.Uniform1(terrain_shader.getUniformLocation("clip_direction"), 0.0f);
            if (prerender == 1) GL.Uniform1(terrain_shader.getUniformLocation("clip_direction"), 1.0f);
            if (prerender == 2) GL.Uniform1(terrain_shader.getUniformLocation("clip_direction"), -1.0f);

            /*
            //terrain_shader.id.SetUniformMatrix4(manager.gl, "projection", cam.getProjection());
            //terrain_shader.id.SetUniformMatrix4(manager.gl, "camera", cam.getInversePosition());
            //terrain_shader.id.SetUniform3(manager.gl, "sun", 0.1f, 1.0f, 0.1f);
            // //terrain_shader.id.SetUniform3(manager.gl, "scale", position.x, position.y, position.z); --> deaktiviert
            if (prerender == 0) terrain_shader.id.SetUniform1(manager.gl, "clip_direction", 0.0f);
            if (prerender == 1) terrain_shader.id.SetUniform1(manager.gl, "clip_direction", 1.0f);
            if (prerender == 2) terrain_shader.id.SetUniform1(manager.gl, "clip_direction", -1.0f);
            //terrain_shader.id.SetUniform3(manager.gl, "offset", cam.getX(), cam.getY(), cam.getZ() - 50.0f);
            //terrain_shader.id.SetUniform1(manager.gl, "clamping", (float)size);
            */
			/*
			foreach (SceneGraph chunk in SceneGraph.data)
			{
				if (chunk.visible) GL.DrawArrays(PrimitiveType.Triangles, chunk.terrain_start_index, chunk.terrain_end_index - chunk.terrain_start_index);
			}*/

            GL.DrawArrays(PrimitiveType.Triangles, 0, index_count);//size*size*6
			GL.UseProgram(0);


			if (prerender == 0)
			{
				GL.Disable(EnableCap.ClipDistance0);
			}


			//Console.WriteLine("Framebuffer: " + GL.GetInteger(GetPName.FramebufferBinding).ToString());
            if (prerender==0) render_water(cam);
        }

        
        public void render_water(Camera cam)
        {
			//Console.WriteLine("Framebuffer bound now: " + GL.GetInteger(GetPName.FramebufferBinding).ToString());

			//Console.WriteLine("sd");
            unten.BindTexture(0);
            oben.BindTexture(1);
			unten.BindDepthTexture(3);

			GL.ActiveTexture(TextureUnit.Texture2);
			GL.BindTexture(TextureTarget.Texture2D, water_tex);

            /*
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, unten.getTexture());
            manager.gl.BindTexture(OpenGL.GL_TEXTURE_2D, render_textures[0]);
            manager.gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            manager.gl.BindTexture(OpenGL.GL_TEXTURE_2D, render_textures[1]);
            */
            //gl_data.Bind(manager.gl);
            water_shader.bind();


            GL.UniformMatrix4(water_shader.getUniformLocation("projection"), 1, false, cam.getProjection());
            GL.UniformMatrix4(water_shader.getUniformLocation("camera"), 1, false, cam.getInversePosition());
			GL.Uniform3(water_shader.getUniformLocation("sun"), sun.x, sun.y, sun.z);
			GL.Uniform1(water_shader.getUniformLocation("terrainsize"), (float)Terrain.active.size);

            GL.Uniform1(water_shader.getUniformLocation("time"), time); time += 1.0f;
            GL.Uniform1(water_shader.getUniformLocation("near"), cam.getNear());
            GL.Uniform1(water_shader.getUniformLocation("far"), cam.getFar());
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, size * size * 6);

            /*
            water_shader.id.SetUniform3(manager.gl, "bounds", manager.width / (float)waterTexSize, manager.height / (float)waterTexSize, 0.0f);
            water_shader.id.SetUniform1(manager.gl, "scale", position.x);
            water_shader.id.SetUniform1(manager.gl, "time", time); time += 1.1f;
            water_shader.id.SetUniform1(manager.gl, "near", cam.getNear());
            water_shader.id.SetUniform1(manager.gl, "far", cam.getFar());
            
            manager.gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, size * size * 6);
            */


            water_shader.unbind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        









        
    }







}
