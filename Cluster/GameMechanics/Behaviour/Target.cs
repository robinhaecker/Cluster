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
		public enum TargetType
		{
			SPACE,
			PLANET,
			UNIT
		}
		public enum MissionType
		{
			NONE,
			ATTACK,
			COLONIZE,
			PROTECT
		}

		TargetType type;
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
			type = TargetType.SPACE;
			waypoints = new List<Vec2>();
		}
		public Target(float x, float y)
		{
			planet = null;
			planetField = -1;
			unit = null;
			type = TargetType.SPACE;
			spaceX = (float)x;
			spaceY = (float)y;
			waypoints = new List<Vec2>();
		}

		public void update(Target target)
		{
			type = target.type;
			planet = target.planet;
			planetField = target.planetField;
			unit = target.unit;
			spaceX = target.spaceX;
			spaceY = target.spaceY;
			waypoints.Clear();
			previously = 0.0;
		}
		

		public void update(Planet p, int index = -1, int softIndex = -1)
		{
			unit = null;
			type = TargetType.PLANET;
			planet = p;
			planetField = index;
			if (softIndex != -1) previously = softIndex;
			else previously = index;
		}
		public void update(Unit u)
		{
			planet = null;
			type = TargetType.UNIT;
			unit = u;
		}
		public void update(double x, double y)
		{
			planet = null;
			unit = null;
			type = TargetType.SPACE;
		}

		public float getX()
		{
			switch (type)
			{
				case TargetType.PLANET:
					return (float)planet.x;

				case TargetType.UNIT:
					if (unit.isDead())
					{
						type = TargetType.SPACE;
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
				case TargetType.PLANET:
					return (float)planet.y;

				case TargetType.UNIT:
					if (unit.isDead())
					{
						type = TargetType.SPACE;
						spaceX = unit.x;
						spaceY = unit.y;
					}
					return unit.y;

				default:
					return spaceY;
			}
		}

		public Vec2 getPosition()
		{
			return new Vec2(getX(), getY());
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
				case TargetType.PLANET:
					double phi = 0.0f;
					float height = planet.size * 20.0f + 150.0f + (float)GameWindow.random.NextDouble() * 350.0f;
					if (planetField == -1)
					{
						double delta = (GameWindow.random.NextDouble()-0.5) * 4.0 + 0.5;
						phi = (previously + delta) / planet.size * 2.0 * Math.PI;
						previously = (previously + delta + planet.size) % planet.size;
					}
					else
					{
						double delta = (GameWindow.random.NextDouble()-0.5) * 2.0 + 0.5;
						phi = ((planetField + delta) / planet.size) * 2.0 * Math.PI;
						previously = (planetField + delta + planet.size) % planet.size;
					}
					wp = new Vec2(planet.x + (float)Math.Cos(phi) * height,
								  planet.y + (float)Math.Sin(phi) * height);
					waypoints.Add(wp);
					return wp;

				case TargetType.UNIT:
					wp = new Vec2(unit.x + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f),
								  unit.y + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f));
					waypoints.Add(wp);
					return wp;

				default:
					wp = new Vec2(spaceX + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f),
								  spaceY + 200.0f * ((float)GameWindow.random.NextDouble() - 0.5f));
					waypoints.Add(wp);
					return wp;
			}
		}


	}
}
