using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Mathematics;

namespace Cluster.GameMechanics.Behaviour
{
	public class Target
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
		int planetField;
		Unit unit;
		float spaceX, spaceY;

		List<Vec2> waypoints;
		double previously;


		public Target()
		{
			planet = null;
			planetField = -1;
			unit = null;
			type = TARGET_SPACE;
			waypoints = new List<Vec2>();
		}
		public Target(float x, float y)
		{
			planet = null;
			planetField = -1;
			unit = null;
			type = TARGET_SPACE;
			spaceX = (float)x;
			spaceY = (float)y;
			waypoints = new List<Vec2>();
		}


		public void update(Planet p, int index = -1, int softIndex = -1)
		{
			unit = null;
			type = TARGET_PLANET;
			planet = p;
			planetField = index;
			if (softIndex != -1) previously = softIndex;
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
						spaceX = (float)unit.x;
						spaceY = (float)unit.y;
					}
					return (float)unit.x;

				default:
					return spaceX;
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
						spaceX = (float)unit.x;
						spaceY = (float)unit.y;
					}
					return (float)unit.y;

				default:
					return spaceY;
			}
		}

		public Vec2 getPosition()
		{
			return new Vec2((float)getX(), (float)getY());
		}

		public Vec2 getWaypoint()
		{
			return waypoints.Count == 0 ? addWaypoint() : waypoints[0];
		}


		public Vec2 nextWaypoint() // Fokussiert auf den nachfolgenden Wegpunkt
		{
			waypoints.RemoveAt(0);
			return getWaypoint();
		}

		public Vec2 addWaypoint(float x, float y, bool insertFirst = false)
		{
			Vec2 wp = new Vec2(x,y);
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
		public Vec2 addWaypoint()
		{
			Vec2 wp;
			switch (type)
			{
				case TARGET_PLANET:
					double phi = 0.0f;
					float height = planet.size * 20.0f + 150.0f + (float)GameWindow.random.NextDouble() * 350.0f;
					if (planetField == -1)
					{
						double delta = (GameWindow.random.NextDouble()-0.5) * 4.0 + 0.5;
						phi = (((double)previously + delta) / (double)planet.size) * 2.0 * Math.PI;
						previously = (previously + delta + (double)planet.size) % (double)planet.size;
					}
					else
					{
						double delta = (GameWindow.random.NextDouble()-0.5) * 2.0 + 0.5;
						phi = (((double)planetField + delta) / (double)planet.size) * 2.0 * Math.PI;
						previously = ((double)planetField + delta + (double)planet.size) % (double)planet.size;
					}
					wp = new Vec2((float)planet.x + (float)Math.Cos(phi) * height,
								  (float)planet.y + (float)Math.Sin(phi) * height);
					waypoints.Add(wp);
					return wp;

				case TARGET_UNIT:
					wp = new Vec2((float)unit.x + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f),
								  (float)unit.y + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f));
					waypoints.Add(wp);
					return wp;

				default:
					wp = new Vec2(spaceX + 100.0f * ((float)GameWindow.random.NextDouble() - 0.5f),
								  spaceY + 100.0f * ((float)GameWindow.random.NextDouble() - 0.5f));
					waypoints.Add(wp);
					return wp;
			}
		}


	}
}
