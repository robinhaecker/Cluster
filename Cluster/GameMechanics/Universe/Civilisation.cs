using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cluster.GameMechanics.Content;


namespace Cluster.GameMechanics.Universe
{
	class Civilisation
	{
		public const int TOTAL_MAX_POPULATION = 1000;

		public const byte BONUS_RESSOURCES = 0,
							BONUS_RESEARCH = 1,
							BONUS_SPEED = 2,
							BONUS_SHIELDING = 3,
							BONUS_HULL = 4,
							BONUS_WEAPONS_TORPEDO = 5,
							BONUS_WEAPONS_LASER = 6,
							BONUS_WEAPONS_EXPLOSIVE = 7,
							BONUS_REPAIR = 8,
							BONUS_BUILDING_STABILITY = 9,
							BONUS_ENERGY = 10,
							BONUS_CONSTRUCTION_SPEED = 11;

		public const int NUMBER_OF_BONI = 12;




		public static List<Civilisation> data;
		public static int player;
		public static int count;


		static List<string> names;

		int id;
		public string name;
		public float r, g, b;
		bool alive;


		public bool[] tech;
		float[] boni;
		public float ress;
		public float science;
		public int population, max_population, max_population_new;


		static public void init(int number = 5)
		{
			loadNames();

			data = new List<Civilisation>();
			player = 0;
			count = 0;

			for (int i = 0; i < number; i++)
			{
				new Civilisation();
			}


		}


		public Civilisation()
		{
			id = count; count++; data.Add(this);

			alive = true;
			this.getRandomName();
			this.getRandomColors();

			tech = new bool[Technology.count];
			boni = new float[NUMBER_OF_BONI];
			ress = 1000.0f;
			science = 0.0f;

			population = 0;
			max_population = 10;
		}
		public Civilisation(string name, float r, float g, float b)
		{
			id = count; count++; data.Add(this);

			alive = true;
			this.name = name;
			this.r = r;
			this.g = g;
			this.b = b;

			tech = new bool[Technology.count];
			boni = new float[NUMBER_OF_BONI];
			ress = 1000.0f;
			science = 0.0f;

			population = 0;
			max_population = 10;
		}

		static void loadNames()
		{
			names = new List<string>();
			using (StreamReader file = new StreamReader(GameWindow.BASE_FOLDER + "civs/names.txt", System.Text.Encoding.Default))
			{
				while (!file.EndOfStream)
				{
					names.Add(file.ReadLine());
				}
			}
			//Console.WriteLine("Number of civilisation names loaded: " + names.Count.ToString());
		}
		void getRandomName()
		{
			int i = GameWindow.random.Next(names.Count);
			name = names[i];
			names.RemoveAt(i);
		}
		void getRandomColors()
		{
			switch (id)
			{
				case 0:
					r = 0.2f; g = 0.2f; b = 1.0f;
					break;
				case 1:
					r = 1.0f; g = 0.2f; b = 0.2f;
					break;
				case 2:
					r = 0.2f; g = 1.0f; b = 0.2f;
					break;
				case 3:
					r = 1.0f; g = 1.0f; b = 1.0f;
					break;
				case 4:
					r = 1.0f; g = 1.0f; b = 0.1f;
					break;
				case 5:
					r = 1.0f; g = 0.1f; b = 1.0f;
					break;
				case 6:
					r = 0.1f; g = 1.0f; b = 1.0f;
					break;
				case 7:
					r = 0.2f; g = 0.2f; b = 0.2f;
					break;
				case 8:
					r = 1.0f; g = 0.5f; b = 0.05f;
					break;
				case 9:
					r = 0.57f; g = 0.47f; b = 0.05f;
					break;
				default:
					r = (float)GameWindow.random.NextDouble();
					g = (float)GameWindow.random.NextDouble();
					b = (float)GameWindow.random.NextDouble();
					break;

			}
		}



		static public Civilisation getPlayer()
		{
			return data[player];
		}
		static public int countCivilisations()
		{
			int num = 0;
			foreach (Civilisation civ in data)
			{
				if (civ.alive) num++;
			}
			return num;
		}
		static public int getMaxPopulationPerCiv()
		{
			return TOTAL_MAX_POPULATION / countCivilisations();
		}



		public int getID()
		{
			return id;
		}
		public float getMultiplicator(byte id)
		{
			return (1.0f + boni[id]);
		}
		public int populationLeft()
		{
			return max_population - population;
		}





	}
}
