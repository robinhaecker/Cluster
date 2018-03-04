using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Cluster.math;


namespace Cluster.Rendering.Draw3D.ProceduralGeneration
{

	class Biome
	{
		public static List<Biome> biomes;
		public static float sum_score;


		string name;
		float r, g, b;
		vec2 fav_height;
		vec2 fav_slope;
		bool limit;
		public float score;

		public Biome(string name, float red, float green, float blue, vec2 height, vec2 slope, bool overwater_only = false)
		{
			this.name = name;
			this.r = red;
			this.g = green;
			this.b = blue;
			this.fav_height = height;
			//this.fav_height.x -= 0.5f*(TerrainGenerator.max_elevation + TerrainGenerator.min_elevation) / (TerrainGenerator.max_elevation - TerrainGenerator.min_elevation);
			this.fav_slope = slope;
			this.limit = overwater_only;
			biomes.Add(this);
		}

		public float getScore(float h, float elev)
		{
			score = (float)Math.Exp(-(h - fav_height.x) * (h - fav_height.x) / (fav_height.y * fav_height.y)) + (float)Math.Exp(-(elev - fav_slope.x) * (elev - fav_slope.x) / (fav_slope.y*fav_slope.y));
			if (h < 0.0f && limit) score *= 0.03f;
			score = score * score * score * score;
			//score = score * score * score * score;
			//score /= (fav_slope.y + fav_height.y);
			sum_score += score;
			return score;
		}

		public static Biome getBiome(float h, float elev)
		{
			sum_score = 0.0f;
			foreach (Biome b in biomes)
			{
				b.getScore(h, elev);
			}

			float decide = (float)GameWindow.random.NextDouble();

			foreach (Biome b in biomes)
			{
				if (b.score / sum_score > decide) return b;
				decide -= b.score / sum_score;
			}
			return biomes[0];
		}

		public static Biome getBiome2(float h, float elev)
		{
			sum_score = 0.0f;
			foreach (Biome b in biomes)
			{
				b.getScore(h, elev);
			}

			float decide = 0.0f;
			Biome best = biomes[0];

			foreach (Biome b in biomes)
			{
				if (b.score > decide)
				{
					best = b;
					decide = b.score;
				}
			}
			return best;
		}

		public vec3 getColor()
		{
			return new vec3(r, g, b);
		}


		public static vec3 getColor(float h, float elev)
		{
			sum_score = 0.0f;
			foreach (Biome b in biomes)
			{
				b.getScore(h, elev);
			}

			vec3 col = new vec3(); 

			foreach (Biome b in biomes)
			{
				col = col + b.getColor() * (b.score / sum_score);
			}
			return col;
		}

	}

	class TerrainGenerator
	{

		public static int seed;
		public static Random random;

		public static float min_elevation;
		public static float max_elevation;
		public static int num_frequencies;
		public static float frequency_weights;

		static float[,] saverands;


		public static void init()
		{
			random = new Random();
			min_elevation = -8.0f;
			max_elevation = 15.0f;
			num_frequencies = 5;
			frequency_weights = 0.4f;


			Biome.biomes = new List<Biome>();
			new Biome("sand", 1.0f, 0.8f, 0.1f, new vec2(0.5f, 0.45f), new vec2(0.0f, 0.05f));
			new Biome("sand2", 1.0f, 0.85f, 0.2f, new vec2(0.52f, 0.45f), new vec2(0.1f, 0.05f));
			new Biome("sand2", 0.7f, 0.65f, 0.1f, new vec2(0.42f, 0.55f), new vec2(0.1f, 0.05f));
			new Biome("sand3", 0.95f, 0.85f, 0.1f, new vec2(0.20f, 0.75f), new vec2(0.1f, 0.15f));
			new Biome("tiefsee", 0.8f, 0.6f, 0.2f, new vec2(0.2f, 2.0f), new vec2(0.0f, 0.05f));
			new Biome("schlamm", 0.7f, 0.45f, 0.1f, new vec2(0.6f, 0.26f), new vec2(0.0f, 0.15f));
			new Biome("rock", 0.5f, 0.5f, 0.5f, new vec2(0.69f, 0.25f), new vec2(0.8f, 0.45f), true);
			new Biome("rock", 0.22f, 0.2f, 0.2f, new vec2(0.75f, 0.05f), new vec2(0.9f, 0.35f));
			new Biome("brown rock", 0.7f, 0.45f, 0.1f, new vec2(0.55f, 0.15f), new vec2(0.75f, 0.25f));
			/*
			new Biome("grass", 0.0f, 0.6f, 0.0f, new vec2(0.9f, 0.47f), new vec2(0.2f, 0.25f), true);
			new Biome("grass", -0.01f, 0.5f, -0.01f, new vec2(0.75f, 0.47f), new vec2(0.23f, 0.25f), true);
			new Biome("dunkelgrün", 0.0f, 0.3f, 0.0f, new vec2(0.95f, 0.44f), new vec2(0.1f, 0.18f), true);
			 * */

			new Biome("grass", 0.5f, 0.7f, 0.5f, new vec2(0.9f, 0.47f), new vec2(0.2f, 0.25f), true);
			new Biome("grass", 0.7f, 0.9f, 0.7f, new vec2(0.75f, 0.47f), new vec2(0.23f, 0.25f), true);
			new Biome("dunkelgrün", 0.2f, 0.4f, 0.2f, new vec2(0.95f, 0.44f), new vec2(0.1f, 0.18f), true);

			new Biome("blassgrün", 0.3f, 0.9f, 0.3f, new vec2(0.86f, 0.34f), new vec2(0.3f, 0.28f), true);

			/*
			colors.Add(new vec3(0.5f, 0.5f, 0.5f));  //Grau
			colors.Add(new vec3(0.0f, 0.6f, 0.0f));  //Grün
			colors.Add(new vec3(0.7f, 0.45f, 0.1f)); //Braun
			colors.Add(new vec3(1.0f, 0.8f, 0.1f));  //Gelb-Sandig
			colors.Add(new vec3(0.3f, 0.9f, 0.3f));  //Blassgrün
			colors.Add(new vec3(0.22f, 0.2f, 0.2f));  //Dunkler Fels
			colors.Add(new vec3(0.12f, 0.1f, 0.1f));  //Sehr dunkler Fels
			colors.Add(new vec3(0.7f, 0.25f, 0.1f)); //Orange
			 */

		}


		public static void Generate(Terrain ter)
		{
			if (num_frequencies == 0) init();
			seed = GameWindow.random.Next();
			saverands = new float[Terrain.getSize()+1, Terrain.getSize()+1];


			for (int x = 0; x < Terrain.getSize(); x++)
			{
				for (int y = 0; y < Terrain.getSize(); y++)
				{
					ter.data[x, y, 0] = getHeight(x, y);
					ter.data[x, y, 4] = (float)(GameWindow.random.NextDouble() - 0.5) * 0.2f;
				}
			}

			for (int x = 0; x < Terrain.getSize(); x++)
			{
				for (int y = 0; y < Terrain.getSize(); y++)
				{
					vec3 color = getColor(x, y);
					ter.data[x, y, 1] = color.x;// (ter.data[x, y, 0] - min_elevation) / (max_elevation - min_elevation);//
					ter.data[x, y, 2] = color.y;// (ter.data[x, y, 0] - min_elevation) / (max_elevation - min_elevation);//color.y;
					ter.data[x, y, 3] = color.z;// (ter.data[x, y, 0] - min_elevation) / (max_elevation - min_elevation);// color.z;

					ter.data[x, y, 4] = 0.05f * (ter.data[(x + 1) % Terrain.getSize(), y, 4] + ter.data[x, (y + 1) % Terrain.getSize(), 4] + ter.data[(x + 1) % Terrain.getSize(), (y + 1) % Terrain.getSize(), 4]) + 0.85f * ter.data[x, y, 4];
				}
			}
	
		
		}






		public static float getNoise(int x, int y)
		{
			if (saverands[x, y] == 0) saverands[x, y] = 1.0f + (float)random.NextDouble();
			return saverands[x, y] - 1.0f;
			//random = new Random(seed + x * 123056 + y * 530019);
			//return (float)random.NextDouble();
		}

		public static float getSmoothNoise(float x, float y)
		{
			int lx = (int)Math.Floor(x), ly = (int)Math.Floor(y);
			float fracx = blend(x-lx), fracy = blend(y-ly);

			float a00 = getNoise(lx, ly);
			float a10 = getNoise(lx+1, ly);
			float a01 = getNoise(lx, ly+1);
			float a11 = getNoise(lx+1, ly+1);

			return (a00 * (1.0f - fracx) + a10 * fracx) * (1.0f - fracy) + fracy * (a01 * (1.0f - fracx) + a11 * fracx);
		}

		public static float blend(float x)
		{
			return (1.0f + (float)Math.Cos((1.0 - (double)x) * Math.PI)) * 0.5f;
		}

		public static float getHeight(float x, float y)
		{
			//float height = getSmoothNoise((int)x, (int)y);
			//return height;
			//return min_elevation * (1.0f - height) + max_elevation * height;


			float sum = 0.0f;
			float weight = 1.0f;
			float height = 0.0f;
			float sampling = (float)Math.Pow(0.5, (double)num_frequencies);

			for (int i = 0; i < num_frequencies; i++)
			{
				sum += weight;

				height += getSmoothNoise(x * sampling, y * sampling) * weight;

				sampling *= 2.0f;
				weight *= frequency_weights;
			}
			height = (height / sum);
			height = (float)Math.Sqrt(height) * height;

			float radius = (float)Math.Sqrt((x - (float)Terrain.getSize() * 0.5f) * (x - (float)Terrain.getSize() * 0.5f) + (y - (float)Terrain.getSize() * 0.5f) * (y - (float)Terrain.getSize() * 0.5f));
			if (radius > Terrain.getSize()*0.5f - 25.0f) height *= blend((Terrain.getSize()*0.5f - radius) * 0.04f);


			return min_elevation * (1.0f - height) + max_elevation * height;
		}

		public static vec3 getColor(int x, int y)
		{
			float h = (Terrain.active.data[x, y, 0] - min_elevation) / (max_elevation - min_elevation);
			float h1 = 0.0f, h2 = 0.0f;
			if (x>1 && y >1)
			{
				h1 = Terrain.active.data[x-1, y, 0];
				h2 = Terrain.active.data[x, y-1, 0];

			}
			float e = Math.Max(Math.Abs(Terrain.active.data[x, y, 0]-h1), Math.Abs(Terrain.active.data[x, y, 0]-h2));

			return Biome.getColor(Terrain.active.data[x, y, 0], e) * 0.85f + 0.05f * Biome.getBiome2(Terrain.active.data[x, y, 0], e).getColor() + 0.05f * Biome.getBiome(Terrain.active.data[x, y, 0], e).getColor() + 0.05f * Biome.getBiome(Terrain.active.data[x, y, 0], e).getColor(); //

			return new vec3(x / 512.0f, y / 512.0f, 0.4f);
		}




		/*
		void old()
		{

			time = 100.0f;
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					data[x, y, 0] = -30.0f;
					//if (x == 0 || y == 0 || x == size - 1 || y == size - 1) data[x, y, 0] = -20.0f;
					data[x, y, 1] = 0.5f;
					data[x, y, 2] = 0.5f;
					data[x, y, 3] = 0.5f;
					data[x, y, 4] = -0.1f + (float)GameWindow.random.NextDouble() * 0.2f;
				}
			}

			for (int i = 0; i < size * size; i++)
			{
				double rangeX = 2.0 + GameWindow.random.NextDouble() * 30.0;//* (double)size * 0.1025;
				double rangeY = rangeX * (GameWindow.random.NextDouble() * 0.2 + 1.0);

				//double direction = GameWindow.random.NextDouble() * Math.PI * 2.0;
				//double posR = (GameWindow.random.NextDouble() - 0.5) * (size - 2.0 * rangeX);
				//size * 0.5 + Math.Cos(direction) * posR;//
				//size * 0.5 + Math.Sin(direction) * posR;//

				double cx = GameWindow.random.NextDouble();
				double cy = 0.5 + (GameWindow.random.NextDouble() - 0.5) * (1.0 - (cx - 0.5) * (cx - 0.5) * 4.0);
				cx = cx * (size - 4 * rangeX) + 2 * rangeX;
				cy = cy * (size - 4 * rangeY) + 2 * rangeY;

				//double cx = GameWindow.random.NextDouble() * (size - 2 * rangeX) + rangeX;
				//double cy = GameWindow.random.NextDouble() * (size - 2 * rangeY) + rangeY;

				double height = (GameWindow.random.NextDouble() - 0.49) * 4.0;// *rangeX / (double)size * 4.0;


				vec3 rgb = colors[GameWindow.random.Next(colors.Count - 1)] * (0.95f + 0.1f * (float)GameWindow.random.NextDouble());//new vec3((float)GameWindow.random.NextDouble(), (float)GameWindow.random.NextDouble(), (float)GameWindow.random.NextDouble());

				float water = (float)(GameWindow.random.NextDouble() - 0.5) * 0.2f;

				/*
				if (i == 0)
				{
					cx = cy = size * 0.5;
					rangeX = rangeY = cx;
					height = 4.0;
				}* /

				for (int x = (int)Math.Max(1, cx - (int)rangeX - 1); x < (int)Math.Min(size - 1, cx + (int)rangeX + 1); x++)
				{
					for (int y = (int)Math.Max(1, cy - (int)rangeY - 1); y < (int)Math.Min(size - 1, cy + (int)rangeY + 1); y++)
					{
						double dist = Math.Sqrt((cx - x) * (cx - x) / rangeX / rangeX + (cy - y) * (cy - y) / rangeY / rangeY);
						if (dist < 1.0)
						{
							float blend = (float)(Math.Cos(dist * Math.PI) + 1.0) * 0.5f;
							data[x, y, 0] += (float)height * blend;

							if (data[x, y, 0] <= -30.0f)
							{
								data[x, y, 0] = -30.0f;
							}

							if ((i % 3) == 0) continue;
							blend = Math.Min(1.0f, Math.Max(0.0f, blend + (-0.2f + (float)GameWindow.random.NextDouble() * 0.4f)));
							float brightness = 0.4f + (float)GameWindow.random.NextDouble() * 0.6f;
							data[x, y, 1] = data[x, y, 1] * (1.0f - blend) + rgb.x * brightness * blend;
							data[x, y, 2] = data[x, y, 2] * (1.0f - blend) + rgb.y * brightness * blend;
							data[x, y, 3] = data[x, y, 3] * (1.0f - blend) + rgb.z * brightness * blend;
							//data[x, y, 4] = data[x, y, 4] * (1.0f - blend) + water * blend;


						}

					}
				}
			}

			float maxheight = 7.0f;
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					/*
					if (data[x, y, 0] <= 0.0f)
					{
						data[x, y, 0] *= 0.5f;
					}*/
					/*
					if (data[x, y, 0] <= -2.5f)
					{
						data[x, y, 0] = -2.5f + (data[x, y, 0] + 2.5f)*0.75f;
					}*/
					/*
					if (data[x, y, 0] <= -7.0f)
					{
						data[x, y, 0] = -7.0f;
					}*/
					//else 
					/*if (data[x, y, 0] > maxheight*0.25f)
                    {
						data[x, y, 0] = (data[x, y, 0] - maxheight * 0.25f) * 0.75f + maxheight * 0.25f;
					}* /
					if (data[x, y, 0] > maxheight) maxheight = data[x, y, 0];

					if (data[x, y, 0] < -maxheight * 2.0f) maxheight = -data[x, y, 0] * 0.5f;

					float ddx = ((float)x / (float)size - 0.5f) * 2.0f;
					float ddy = ((float)y / (float)size - 0.5f) * 2.0f;
					float dst = (float)Math.Sqrt(ddx * ddx + ddy * ddy);
					/*
					if (dst > 0.97f)
					{
						data[x, y, 0] = 0.0f;
						if (dst > 1.0f)
						{
							data[x, y, 0] = -1000.0f;
							data[x, y, 1] = -1000.0f;
							data[x, y, 2] = -1000.0f;
							data[x, y, 3] = -1000.0f;
						}

						//data[x, y, 4] = 1000.0f;
					}* /

				}
			}

			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{

					//if (data[x, y, 0] > -5.0f) 
					if (data[x, y, 0] >= 0.0f)
					{
						data[x, y, 0] = 10.0f * (float)Math.Sin(data[x, y, 0] / maxheight * Math.PI * 0.5);
					}
					else
					{
						data[x, y, 0] = 20.0f * data[x, y, 0] / maxheight * (float)Math.PI * 0.5f;// (float)Math.Sin(data[x, y, 0] / maxheight * Math.PI * 0.25);
					}

					//if (x == 0 || y == 0 || x == size - 1 || y == size - 1) minter = Math.Min(minter, data[x, y, 0]);

					data[x, y, 4] = 0.05f * (data[(x + 1) % size, y, 4] + data[x, (y + 1) % size, 4] + data[(x + 1) % size, (y + 1) % size, 4]) + 0.85f * data[x, y, 4];

					if (data[x, y, 0] <= 1.0f)
					{
						float sandy = 1.0f - Math.Max(0.0f, data[x, y, 0]);
						float zuf = (float)GameWindow.random.NextDouble() * 0.1f - 0.05f;
						data[x, y, 1] = data[x, y, 1] * (1.0f - sandy) + sandy * (0.7f + zuf);
						data[x, y, 2] = data[x, y, 2] * (1.0f - sandy) + sandy * (0.5f + zuf);
						data[x, y, 3] = data[x, y, 3] * (1.0f - sandy) + sandy * (0.1f + zuf);
					}
					else
					{
						float sandy = Math.Min(1.0f, Math.Max(0.0f, data[x, y, 0] - 1.0f)) * 0.25f;
						data[x, y, 1] = data[x, y, 1] * (1.0f - sandy) + sandy * 0.2f;
						data[x, y, 2] = data[x, y, 2] * (1.0f - sandy) + sandy * 0.8f;
						data[x, y, 3] = data[x, y, 3] * (1.0f - sandy) + sandy * 0.1f;
					}
				}
			}

		}
		*/




	}
}
