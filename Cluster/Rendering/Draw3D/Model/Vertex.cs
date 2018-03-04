using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;


namespace Cluster.Rendering.Draw3D
{
	class Vertex
	{
		public int index;
		public List<vec3> pos;
		public List<vec3> col;
		public bool flag;

		public Vertex()
		{
			pos = new List<vec3>();
			col = new List<vec3>();
		}

		public Vertex(float x, float y, float z, float r = 1.0f, float g = 1.0f, float b = 1.0f)
		{
			pos = new List<vec3>();
			col = new List<vec3>();

			pos.Add(new vec3(x, y, z));
			col.Add(new vec3(r, g, b));
		}



		public int getFrameCount()
		{
			return pos.Count;
		}
		public vec3 getPos(int frame = 0)
		{
			frame = Math.Min(frame, pos.Count - 1);
			return pos[frame];
		}
		public vec3 getCol(int frame = 0)
		{
			frame = Math.Min(frame, col.Count - 1);
			return col[frame];
		}

		public void rescale(float mult)
		{
			for (int i = 0; i < pos.Count; i++)
			{
				pos[i] = pos[i] * mult;
			}
		}



		public void addFrame(float x, float y, float z)
		{
			vec3 color = col[col.Count - 1];
			if (color == null) color = new vec3(1.0f, 1.0f, 1.0f);
			pos.Add(new vec3(x, y, z));
			col.Add(new vec3(color.x, color.y, color.z));
		}
		public void addFrame(float x, float y, float z, float r, float g, float b)
		{
			pos.Add(new vec3(x, y, z));
			col.Add(new vec3(r, g, b));
		}








	}
}
