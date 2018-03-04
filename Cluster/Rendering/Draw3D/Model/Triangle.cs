using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;


namespace Cluster.Rendering.Draw3D
{
	class Triangle
	{
		public Vertex v0, v1, v2;

		public List<vec3> normals;



		public Triangle(Vertex v0, Vertex v1, Vertex v2)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			normals = new List<vec3>();

			updateNormals();
		}




		public void updateNormals()
		{
			normals.Clear();
			int frames = v0.getFrameCount();
			for (int i = 0; i < frames; i++)
			{
				normals.Add(((v2.getPos(i) - v0.getPos(i)) % (v1.getPos(i) - v0.getPos(i))).normalize());
			}
		}




	}
}
