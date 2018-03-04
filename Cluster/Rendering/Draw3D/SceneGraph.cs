using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;

namespace Cluster.Rendering.Draw3D
{
	class SceneGraph
	{
		public static SceneGraph[,] data;
		public static float chunkSize;
		public static float worldSize;
		public static int chunkCount;
		static float sceneHeight;


		vec3 bmin, bmax;
		public bool visible;
		public bool far_away;

		public int terrain_start_index, terrain_end_index;


		public static void init(float world_size, int num_subdivisions = 8)
		{
			data = new SceneGraph[num_subdivisions, num_subdivisions];

			chunkSize = (world_size) / (float)num_subdivisions;
			worldSize = world_size * 0.5f;
			chunkCount = num_subdivisions;
			sceneHeight = 20.0f;


			for (int x = 0; x < num_subdivisions; x++)
			{
				for (int y = 0; y < num_subdivisions; y++)
				{
					data[x, y] = new SceneGraph();//-world_size + ((float)x + 0.5f) * chunkSize * 2.0f, -world_size + ((float)y + 0.5f) * chunkSize * 2.0f);
					data[x, y].bmin = new vec3(-worldSize + x * chunkSize, 0.0f, -worldSize + y * chunkSize);
					data[x, y].bmax = new vec3(-worldSize + (x+1) * chunkSize, sceneHeight, -worldSize + (y+1) * chunkSize);

					//data[x, y].visible = (((x + y) % 2) == 0);// (x == y);// ;
					//if (x < y) data[x, y].visible = true;
				}
			}
		}


		public SceneGraph()
		{
			bmin = new vec3();
			bmax = new vec3();
		}


		public static void checkVisibility(Camera cam)
		{
			
			vec4[] planes = cam.getFrustumPlanes();

			//int visch = 0;
			foreach (SceneGraph chunk in data)
			{
				chunk.visible = true;
				chunk.far_away = false;
				if (chunk.terrain_end_index == chunk.terrain_start_index)
				{
					chunk.visible = false;
					continue;
				}
				foreach (vec4 plane in planes)
				{
					vec3 c = (chunk.bmax + chunk.bmin) * 0.5f;
					vec3 h = (chunk.bmax - chunk.bmin) * 0.5f;

					float e = Math.Abs(plane.x) * h.x + Math.Abs(plane.y) * h.y + Math.Abs(plane.z) * h.z;
					float s = plane.x * c.x + 0.0f + plane.z * c.z + plane.w;
					if (s - e > 0)
					{
						chunk.visible = false;
						break;
					}


					//Far-Plane. Hier schauen, ob Objekt weit weg ist.
					if (plane == planes[5] && chunk.visible)
					{
						if (s-0.5f*plane.w - e > 0)
						{
							chunk.far_away = true;
						}
					}
				}

				//if (chunk.visible) visch++;
			}
			//Console.WriteLine(visch.ToString());
		}






		public static bool isVisible(float x, float y)
		{
			
			int i = (int)Math.Floor(((float)x + worldSize) / chunkSize);
			int j = (int)Math.Floor(((float)y + worldSize) / chunkSize);

			if (i < 0 || j < 0 || i >= chunkCount || j >= chunkCount) return false;



			return SceneGraph.data[i,j].visible;
		}

		public bool isInside(float x, float z)
		{
			if (x >= bmin.x && x <= bmax.x && z >= bmin.z && z <= bmax.z) return false;
			return true;
		}






	}
}
