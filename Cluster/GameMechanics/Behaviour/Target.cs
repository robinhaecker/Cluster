using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cluster.GameMechanics.Universe;
using Cluster.math;

namespace Cluster.GameMechanics.Behaviour
{
	class Target
	{
		public const byte  TARGET_SPACE  = 0,
						   TARGET_PLANET = 1,
						   TARGET_UNIT   = 2;

		public const byte	MISSION_NONE     = 0,
							MISSION_ATTACK   = 1,
							MISSION_COLONIZE = 2,
							MISSION_PROTECT  = 3;


		byte type;
		Planet planet;
		int planet_field;
		Unit unit;
		float space_x, space_y;

		List<vec2> waypoints;
		double previously;


		public Target()
		{
			planet = null;
			planet_field = -1;
			unit = null;
			type = TARGET_SPACE;
			waypoints = new List<vec2>();
		}
		public Target(float x, float y)
		{
			planet = null;
			planet_field = -1;
			unit = null;
			type = TARGET_SPACE;
			space_x = (float)x;
			space_y = (float)y;
			waypoints = new List<vec2>();
		}


		public void update(Planet p, int index = -1, int soft_index = -1)
		{
			unit = null;
			type = TARGET_PLANET;
			planet = p;
			planet_field = index;
			if (soft_index != -1) previously = soft_index;
			else previously = index;
		}
		public void update(Unit u)
		{
			planet = null;
			type = TARGET_UNIT;
			unit = u;
		}
		public void update(double x, double y)
		{
			planet = null;
			unit = null;
			type = TARGET_SPACE;
		}

		public float getX()
		{
			switch (type)
			{
				case TARGET_PLANET:
					return (float)planet.x;

				case TARGET_UNIT:
					if (unit.isDead())
					{
						type = TARGET_SPACE;
						space_x = (float)unit.x;
						space_y = (float)unit.y;
					}
					return (float)unit.x;

				default:
					return space_x;
			}
		}
		public float getY()
		{
			switch (type)
			{
				case TARGET_PLANET:
					return (float)planet.y;

				case TARGET_UNIT:
					if (unit.isDead())
					{
						type = TARGET_SPACE;
						space_x = (float)unit.x;
						space_y = (float)unit.y;
					}
					return (float)unit.y;

				default:
					return space_y;
			}
		}

		public vec2 getPosition()
		{
			return new vec2((float)getX(), (float)getY());
		}

		public vec2 getWaypoint()
		{
			if (waypoints.Count == 0) return addWaypoint();
			return waypoints[0];
		}


		public vec2 nextWaypoint() // Fokussiert auf den nachfolgenden Wegpunkt
		{
			waypoints.RemoveAt(0);
			return getWaypoint();
		}

		public vec2 addWaypoint(float x, float y, bool insertFirst = false)
		{
			vec2 wp = new vec2(x,y);
			if (insertFirst)
			{
				waypoints.Insert(0, wp);
				return wp;
			}
			else
			{
				waypoints.Add(wp);
				return wp;
			}

		}
		public vec2 addWaypoint()
		{
			vec2 wp;
			switch (type)
			{
				case TARGET_PLANET:
					double phi = 0.0f;
					float height = planet.size * 20.0f + 150.0f + (float)GameWindow.random.NextDouble() * 350.0f;
					if (planet_field == -1)
					{
						double delta = (GameWindow.random.NextDouble()-0.5) * 4.0 + 0.5;
						phi = (((double)previously + delta) / (double)planet.size) * 2.0 * Math.PI;
						previously = (previously + delta + (double)planet.size) % (double)planet.size;
					}
					else
					{
						double delta = (GameWindow.random.NextDouble()-0.5) * 2.0 + 0.5;
						phi = (((double)planet_field + delta) / (double)planet.size) * 2.0 * Math.PI;
						previously = ((double)planet_field + delta + (double)planet.size) % (double)planet.size;
					}
					wp = new vec2((float)planet.x + (float)Math.Cos(phi) * height,
								  (float)planet.y + (float)Math.Sin(phi) * height);
					waypoints.Add(wp);
					return wp;

				case TARGET_UNIT:
					wp = new vec2((float)unit.x + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f),
								  (float)unit.y + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f));
					waypoints.Add(wp);
					return wp;

				default:
					wp = new vec2(space_x + 100.0f * ((float)GameWindow.random.NextDouble() - 0.5f),
								  space_y + 100.0f * ((float)GameWindow.random.NextDouble() - 0.5f));
					waypoints.Add(wp);
					return wp;
			}
		}


	}
}
