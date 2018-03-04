using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cluster.math;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe;
using System.Threading;

namespace Cluster.GameMechanics.Behaviour
{
	class Sector
	{
		const int SECTOR_COUNT = 20;
		const float SECTOR_SIZE = 700.0f;
		public static float ORIGIN;

		public static Sector[,] data;

		vec2 p0, p1;
		int pos_x, pos_y;

		public List<Sector> neighbors, extendedSector;
		public List<Unit>[] ships;
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
			ORIGIN = -SECTOR_SIZE * SECTOR_COUNT * 0.5f;
			data = new Sector[SECTOR_COUNT+1, SECTOR_COUNT+1];

			for (int x = 0; x < SECTOR_COUNT+1; x++)
			{
				for (int y = 0; y < SECTOR_COUNT+1; y++)
				{
					data[x, y] = new Sector();
					data[x, y].p0 = new vec2(ORIGIN + x * SECTOR_SIZE, ORIGIN + y * SECTOR_SIZE);
					data[x, y].p1 = data[x, y].p0 + new vec2(SECTOR_SIZE, SECTOR_SIZE);
					data[x, y].pos_x = x;
					data[x, y].pos_y = y;
					data[x, y].neighbors = new List<Sector>();
					data[x, y].extendedSector = new List<Sector>();
				}
			}

			for (int x = 0; x < SECTOR_COUNT; x++)
			{
				for (int y = 0; y < SECTOR_COUNT; y++)
				{
					data[x, y].extendedSector.Add(data[x    , y]);
					data[x, y].extendedSector.Add(data[x + 1, y]);
					data[x, y].extendedSector.Add(data[x, y + 1]);
					data[x, y].extendedSector.Add(data[x + 1, y + 1]);

					//--------------------------------------------------
					data[x, y].neighbors.Add(data[x + 1, y    ]);
					data[x, y].neighbors.Add(data[x,     y + 1]);
					data[x, y].neighbors.Add(data[x + 1, y+1  ]);
					//--------------------------------------------------
					data[x+1, y  ].neighbors.Add(data[x, y]);
					data[x,   y+1].neighbors.Add(data[x, y]);
					data[x+1, y+1].neighbors.Add(data[x, y]);
				}
			}
		}

		public Sector()
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
			return data[Math.Min(SECTOR_COUNT - 1, Math.Max(1, (int)Math.Floor((x - ORIGIN) / SECTOR_SIZE))),
						Math.Min(SECTOR_COUNT - 1, Math.Max(1, (int)Math.Floor((y - ORIGIN) / SECTOR_SIZE)))];
		}

		public bool containsPoint(float x, float y)
		{
			if (p0.x >= x && p0.y >= y && p1.x <= x && p1.y <= y) return true;
			return false;
		}
		public bool containsPoint(double x, double y)
		{
			if (p0.x >= x && p0.y >= y && p1.x <= x && p1.y <= y) return true;
			return false;
		}


		public void removeUnit(Unit u)
		{
			ships[u.getOwner().getID()].Remove(u);
		}
		public void addUnit(Unit u)
		{
			ships[u.getOwner().getID()].Add(u);
		}



		public static void getShipEnemies()
		{
			foreach (Unit u in Unit.units)
			{
				u.enemy = null;
				u.enemy_distance = 1500;
				u.inrange = 0;
			}

			// Für alle Sektoren...
			foreach (Sector s0 in Sector.data)
			{
				foreach (Sector s1 in s0.extendedSector)
				{
					// Alle Zivilisationen checken...
					for (int i = 0; i < Civilisation.count + 1; i++)
					{
						for (int j = 0; j < Civilisation.count + 1; j++)
						{
							if (i == j) continue;

							// Und jeweils alle lebendingen Schiffe überprüfen.
							foreach (Unit u0 in s0.ships[i])
							{
								if (u0.isDead()) continue;
								foreach (Unit u1 in s1.ships[j])
								{
									if (u1.isDead()) continue;

									float dst = (float)Math.Sqrt((u0.x - u1.x) * (u0.x - u1.x) + (u0.y - u1.y) * (u0.y - u1.y));
									if (dst < 100.0f) { continue; } // Darf nicht zu nahe dran sein. Hier wird noch der Code für die Selbstzerstörungsdrohnen reinkommen.
									if (dst < u0.enemy_distance)
									{
										u0.enemy_distance = dst;
										u0.enemy = u1;
										u0.inrange = 1;
									}
									if (dst < u1.enemy_distance)
									{
										u1.enemy_distance = dst;
										u1.enemy = u0;
										u1.inrange = 1;
									}

								}
							}
							// END Ships
						}
					}
					// END Civs
				}



			}
			// END Sectors

		}









	}
}
