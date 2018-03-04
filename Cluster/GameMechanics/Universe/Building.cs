using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cluster.GameMechanics.Content;

namespace Cluster.GameMechanics.Universe
{

	class Building
	{
		public const byte STATUS_NONE = 0;
		public const byte STATUS_UNDERCONSTRUCTION = 1;
		public const byte STATUS_DESTROYED = 2;
		public const byte STATUS_DESTROYED_AND_UNDERCONSTRUCTION = 3;


		Planet planet;
		int location;
		public Civilisation owner;

		public Blueprint bp;
		public float health, health_max;
		public byte status;
		public float timer;

		public List<Prototype> production;
		public float production_timer;


		public Building(Planet p, int pos, Blueprint bp, Civilisation owner)
		{
			planet = p;
			location = pos;
			this.bp = bp;
			this.owner = owner;

			health = 1.0f;
			health_max = bp.getHealth(owner);
			timer = 0.0f;
			status = STATUS_UNDERCONSTRUCTION;

			production = new List<Prototype>();
			production_timer = 0.0f;
		}





		public Planet getPlanet()
		{
			return planet;
		}
		public int getSpotID()
		{
			return location;
		}
		public float getSpotRotation()
		{
			return (float) (((float)location + 0.5f) / (float)planet.size) * 2.0f * (float)Math.PI;
		}
		public float getHealthFraction()
		{
			return health / health_max;
		}
		public List<Prototype> ListOfPrototypes(Civilisation civ)
		{
			List<Prototype> list = new List<Prototype>();

			if (bp.specials == Blueprint.SPECIAL_SHIPS)
			{
				foreach (Prototype pr in Prototype.data)
				{
					if (pr.activation.researchedBy(civ) && bp.special_strength >= pr.infra_level) list.Add(pr);
				}
			}

			return list;
		}

		public void produceUnit(Prototype type)
		{
			production.Add(type);
			owner.ress -= type.cost;
		}
		public void abortUnit(Prototype type)
		{
			for (int i = production.Count - 1; i >= 0; i--)
			{
				if (production[i] == type)
				{
					production.RemoveAt(i);
					if (i == 0) production_timer = 0.0f;
					owner.ress += type.cost;
					Console.WriteLine(i.ToString());
					break;
				}
			}
		}
		public Unit createUnit(Prototype type)
		{
			return new Unit(type, this);
		}
		public int getProductionCount(Prototype type)
		{
			int ct = 0;
			foreach (Prototype p in production)
			{
				if (p == type) ct++;
			}
			return ct;
		}



		// Updated das Gebäude
		public void sim(float dt, float efficiency)
		{

			//Status berücksichtigen
			switch (status)
			{
				case STATUS_UNDERCONSTRUCTION:
					if (health < health_max) health += dt * 1.25f;
					else
					{
						health = health_max;
						status = STATUS_NONE;
					}
					break;

				case STATUS_DESTROYED_AND_UNDERCONSTRUCTION:
				case STATUS_DESTROYED:
					health = Math.Max(0.0f, health);
					timer += dt;
					if (timer > 100.0f)
					{
						planet.infra[location] = null;
					}
					return;
			}


			if (health <= 0.0f)
			{
				if (status == STATUS_UNDERCONSTRUCTION) status = STATUS_DESTROYED_AND_UNDERCONSTRUCTION;
				else status = STATUS_DESTROYED;
				timer = 0.0f;
				return;
			}
			if (status == STATUS_UNDERCONSTRUCTION) return;




			//Wenn nicht zerstört oder noch im Bau, dann die Spezialeigenschaft des Gebäudes berücksichtigen:
			switch (bp.specials)
			{
				case Blueprint.SPECIAL_SETTLEMENT:
					owner.max_population_new += (int)bp.special_strength;
					break;

				case Blueprint.SPECIAL_MINING:
					float factor = 0.005f;
					if (planet.terra[location] == Planet.TERRA_MOUNTAIN) factor = 0.0075f;
					else if (planet.terra[location] == Planet.TERRA_RESSOURCES) factor = 0.02f;

					owner.ress += dt * factor * bp.special_strength * owner.getMultiplicator(Civilisation.BONUS_RESSOURCES);
					break;

				case Blueprint.SPECIAL_RESEARCH:
					owner.science += dt * 0.0025f * bp.special_strength * owner.getMultiplicator(Civilisation.BONUS_RESEARCH);
					break;

				case Blueprint.SPECIAL_SHIPS:
					if (production.Count > 0)
					{
						float construction_speed = dt * 0.02f / (float)production[0].cost * owner.getMultiplicator(Civilisation.BONUS_CONSTRUCTION_SPEED);
						production_timer += construction_speed;
						if (production_timer >= 1.0f)
						{
							production_timer = 0.0f;
							createUnit(production[0]);
							production.RemoveAt(0);
						}

					}
					break;
			}
		}



		public bool damage(float dmg)
		{
			if (status == STATUS_DESTROYED || status == STATUS_DESTROYED_AND_UNDERCONSTRUCTION) return false;
			health -= dmg;
			if (health <= 0.0f)
			{
				health = 0.0f;
				if (status == STATUS_UNDERCONSTRUCTION) status = STATUS_DESTROYED_AND_UNDERCONSTRUCTION;
				else status = STATUS_DESTROYED;
				timer = 0.0f;
				return true;
			}
			return false;
		}













	}
}
