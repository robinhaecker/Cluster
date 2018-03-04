using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;


namespace Cluster.Rendering.Draw3D.Collisions
{
	class Ray
	{
		public vec3 o;
		public vec3 d;
		public float t;
		public float min_t;

		public Ray(vec3 origin, vec3 direction, float min_t = 0.0f)
		{
			this.o		= origin;
			this.d		= direction.normalize();
			this.min_t	= min_t;
			this.t		= 1e10f;
		}

		public Ray(float screen_x, float screen_y)
		{

		}

		public Ray Transform(mat4 matrix)
		{
			vec3 new_origin = new vec3(matrix * new vec4(o, 1.0f));
			vec3 new_direction = new vec3(matrix * new vec4(d, 0.0f));
			Ray r = new Ray(new_origin, new_direction, min_t);
			r.t = t;
			return r;
		}


	}
}