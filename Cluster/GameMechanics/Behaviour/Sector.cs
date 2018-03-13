using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cluster.Mathematics;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.LivingThings;
using System.Threading;

namespace Cluster.GameMechanics.Behaviour
{
	class Sector
	{
		private const int SECTOR_COUNT = 20;
		private const float SECTOR_SIZE = 700.0f;
		private const float ORIGIN = -SECTOR_SIZE * SECTOR_COUNT * 0.5f;

		private static Sector[,] _data;

		Vec2 _p0, _p1;
		int _posX;
		int _posY;

		private List<Sector> _neighbors;
		private List<Sector> _extendedSector;

		public readonly List<Unit>[] ships;
		//List<Shot> shots;
		//List<Asteroid> asteroids;


		/*
		public Thread thread;
		public void sim_units()
		{
			float t = 1.0f;
			for(int i = 0; i < Civilisation.count+1; i++)
			{
				foreach(Unit u in ships[i])
				{
					u.simulate(t);
				}
			}
		}*/


		public static void init()
		{
			_data = new Sector[SECTOR_COUNT+1, SECTOR_COUNT+1];

			for (int x = 0; x < SECTOR_COUNT+1; x++)
			{
				for (int y = 0; y < SECTOR_COUNT+1; y++)
				{
					_data[x, y] = new Sector();
					_data[x, y]._p0 = new Vec2(ORIGIN + x * SECTOR_SIZE, ORIGIN + y * SECTOR_SIZE);
					_data[x, y]._p1 = _data[x, y]._p0 + new Vec2(SECTOR_SIZE, SECTOR_SIZE);
					_data[x, y]._posX = x;
					_data[x, y]._posY = y;
					_data[x, y]._neighbors = new List<Sector>();
					_data[x, y]._extendedSector = new List<Sector>();
				}
			}

			for (int x = 0; x < SECTOR_COUNT; x++)
			{
				for (int y = 0; y < SECTOR_COUNT; y++)
				{
					_data[x, y]._extendedSector.Add(_data[x    , y]);
					_data[x, y]._extendedSector.Add(_data[x + 1, y]);
					_data[x, y]._extendedSector.Add(_data[x, y + 1]);
					_data[x, y]._extendedSector.Add(_data[x + 1, y + 1]);

					//--------------------------------------------------
					_data[x, y]._neighbors.Add(_data[x + 1, y    ]);
					_data[x, y]._neighbors.Add(_data[x,     y + 1]);
					_data[x, y]._neighbors.Add(_data[x + 1, y+1  ]);
					//--------------------------------------------------
					_data[x+1, y  ]._neighbors.Add(_data[x, y]);
					_data[x,   y+1]._neighbors.Add(_data[x, y]);
					_data[x+1, y+1]._neighbors.Add(_data[x, y]);
				}
			}
		}

		private Sector()
		{
			ships = new List<Unit>[Civilisation.count+1];
			for (int i = 0; i < Civilisation.count+1; i++)
			{
				ships[i] = new List<Unit>();
			}
			//shots = new List<Shot>();
			//asteroids = new List<Asteroid>();
		}


		public static Sector get(float x, float y)
		{
			return _data[Math.Min(SECTOR_COUNT - 1, Math.Max(1, (int)Math.Floor((x - ORIGIN) / SECTOR_SIZE))),
						Math.Min(SECTOR_COUNT - 1, Math.Max(1, (int)Math.Floor((y - ORIGIN) / SECTOR_SIZE)))];
		}

		public bool containsPoint(float x, float y)
		{
			return _p0.x >= x && _p0.y >= y && _p1.x <= x && _p1.y <= y;
		}
		public bool containsPoint(double x, double y)
		{
			return _p0.x >= x && _p0.y >= y && _p1.x <= x && _p1.y <= y;
		}


		public void removeUnit(Unit u)
		{
			ships[u.getOwner().getId()].Remove(u);
		}
		public void addUnit(Unit u)
		{
			ships[u.getOwner().getId()].Add(u);
		}

		public static void findNearestEnemies()
		{
			resetAllEnemyDistanceValues();

			// Für alle Sektoren...
			foreach (var sector0 in _data)
			{
				foreach (var sector1 in sector0._extendedSector)
				{
					// Alle Zivilisationen checken...
					for (int i = 0; i < Civilisation.count + 1; i++)
					{
						for (int j = 0; j < Civilisation.count + 1; j++)
						{
							if (i == j) continue;

							// Und jeweils alle lebendingen Schiffe überprüfen.
							foreach (var unit0 in sector0.ships[i])
							{
								if (unit0.isDead()) continue;
								foreach (var unit1 in sector1.ships[j])
								{
									checkNearestEnemyFor(unit1, unit0);
								}
							}
						}
					}
				}
			}
		}

		private static void resetAllEnemyDistanceValues()
		{
			foreach (Unit u in Unit.units)
			{
				u.enemy = null;
				u.enemyDistance = 1500;
				u.inrange = 0;
			}
		}

		private static void checkNearestEnemyFor(Unit unit1, Unit unit0)
		{
			if (unit1.isDead()) return;

			float dst = (float) Math.Sqrt((unit0.x - unit1.x) * (unit0.x - unit1.x) +
			                              (unit0.y - unit1.y) * (unit0.y - unit1.y));
			if (dst < 100.0f)
			{
				return;
			} // Darf nicht zu nahe dran sein. Hier wird noch der Code für die Selbstzerstörungsdrohnen reinkommen.

			if (dst < unit0.enemyDistance)
			{
				unit0.enemyDistance = dst;
				unit0.enemy = unit1;
				unit0.inrange = 1;
			}

			if (dst < unit1.enemyDistance)
			{
				unit1.enemyDistance = dst;
				unit1.enemy = unit0;
				unit1.inrange = 1;
			}
		}
	}
}
