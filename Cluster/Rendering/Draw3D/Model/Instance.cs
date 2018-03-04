using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;

namespace Cluster.Rendering.Draw3D
{
	class Instance
	{
		public static byte ANIMATION_ONCE = 0;
		public static byte ANIMATION_LOOP = 1;
		public static byte ANIMATION_PONG = 2;




		Model reference;

        public bool hidden;

        public float x,y,z;
        public float rotation;
        public float scale;
        public float animation;
		public vec2 animation_bounds;
		public byte animation_type;
		public float animation_speed;


		int chunk_x, chunk_y;


        public Instance(Model parent)
        {
            reference = parent;
            parent.instances.Add(this);

			scale = 1.0f;
			animation = 0.0f;
			rotation = 0.0f;
			animation_bounds = new vec2(0.0f, reference.getFrameCount());
			animation_type = 1;
			animation_speed = 1.0f;
        }



		public void updateAnimation(float t = 0.01f)
		{
			animation += t*animation_speed;
			if (animation > animation_bounds.y)
			{
				if (animation_type == 1)
				{
					animation = animation_bounds.x + (animation - animation_bounds.y);
				}
				else if (animation_type == 2)
				{
					animation = animation_bounds.y - (animation - animation_bounds.y);
					if (animation_speed > 0) animation_speed *= -1.0f;
				}
				else
				{
					animation = animation_bounds.y;
				}
			}
			else if (animation < animation_bounds.x)
			{
				if (animation_type == 2)
				{
					animation = animation_bounds.x + (animation_bounds.x-animation);
					if(animation_speed < 0) animation_speed *= -1.0f;
				}
				else
				{
					animation = animation_bounds.x;
				}
			}
		}

		public void setAnimationMode(float start = 0, float end = -1, byte anim_type = 1, float speed = 1.0f)
		{
			if (end < 0) end = reference.getFrameCount();
			if (animation < start) animation = start;
			if (animation >= end) animation = end;
			animation_bounds.x = start;
			animation_bounds.y = end;
			animation_type = anim_type;
			animation_speed = speed;
		}


		public void updateChunk(bool enforce=false)
		{
			reference.sortInstance(this, enforce);
		}


		
		public void _setChunk(int chx, int chy)
		{
			chunk_x = chx;
			chunk_y = chy;
		}
		public int getChunkX()
		{
			return chunk_x;
		}
		public int getChunkY()
		{
			return chunk_y;
		}
		public bool updateChunkNeeded()
		{
			return !SceneGraph.data[chunk_x, chunk_y].isInside(x, z);
		}

		public Model getReference()
		{
			return reference;
		}



		public void Delete()
		{
			reference.instances.Remove(this);
			reference.chunks[chunk_x, chunk_y].Remove(this);
		}

	}
}
