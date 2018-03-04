using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cluster.Rendering.Draw2D;
using Cluster.GameMechanics.Universe;

namespace Cluster.GameMechanics.Content
{
	class Blueprint
	{

		public const byte SPECIAL_NONE = 0,
						  SPECIAL_SETTLEMENT = 1,
						  SPECIAL_SHIPS = 2,
						  SPECIAL_ENERGY = 3,
						  SPECIAL_TERRAFORM = 4,
						  SPECIAL_DEFEND = 5,
						  SPECIAL_REPAIR_BUILDINGS = 6,
						  SPECIAL_REPAIR_SHIPS = 7,
						  SPECIAL_RESEARCH = 8,
						  SPECIAL_MINING = 9;


		public static List<Blueprint> data;
		public static int count;




		public int id;         // ID des Blueprints
		public float health_max; // Maximale Gesundheit
		public int cost;       // Ressourcenkosten zum Bauen bzw. Upgraden
		public string name;    // Bezeichnung

		public Mesh shape; // Grafische Darstellung des Gebäudes

		public byte specials;          // Besondere Eigenschaft
		public float special_strength; // Wie stark ist die Eigenschaft ausgeprägt
		public string[] description;   // Textbeschreibung (3 Zeilen)
		public float energy;           // Energiebedarf des Gebäudes
		public bool[] build_on;        // Gibt an, auf welchen Terraintypen das Gebäude gebaut werden kann.


		public bool upgrade_only;              // Kann das Gebäude nur als Upgrade aus einem bereits bestehenden hervorgehen?
		public List<Blueprint> develop_into;   // Liste an weiteren Gebäuden, in die geupgradet werden kann
		public Technology activation;          // Die Technologie, die benötigt wird, um das Gebäude freizuschalten.


		public string getName()
		{
			return name;
		}
		public float getHealth(Civilisation civ)
		{
			return health_max;
		}
		public int getCost()
		{
			return cost;
		}
		public float getEnergyNeeds()
		{
			return energy;
		}
		public bool isBuildable(byte terrain_type)
		{
			return build_on[terrain_type];
		}
		public bool isBuildable(Planet p, int pos)
		{
			return build_on[p.getTerrain(pos)];
		}







		public static void init()
		{
			data = new List<Blueprint>();
			count = 0;
			string directory = GameWindow.BASE_FOLDER + "structures/";


			while (File.Exists(directory + count.ToString().PadLeft(3, '0') + ".building"))
			{
				new Blueprint(directory, count.ToString().PadLeft(3, '0') + ".building");
			}
		}

		public Blueprint(string file_directory, string file_name)
		{
			//Console.WriteLine("Load Building: " + file_directory + file_name);

			using (StreamReader file = new StreamReader(file_directory + file_name, System.Text.Encoding.Default))
			{
				id = count; count++; data.Add(this);
				develop_into = new List<Blueprint>();

				file.ReadLine();        //.Rohmaterial für Technologiedaten
				file.ReadLine();        //.
				file.ReadLine();        //.Name der Technologie
				name = file.ReadLine();
				file.ReadLine();        //.Beschreibung
				description = new string[3];
				description[0] = file.ReadLine();
				description[1] = file.ReadLine();
				description[2] = file.ReadLine();
				file.ReadLine();        //.Grafikdatei
				string gfx_file = file.ReadLine();
				if (File.Exists(file_directory + "gfx/" + gfx_file)) shape = new Mesh(file_directory + "gfx/" + gfx_file, false, true);
				file.ReadLine();        //.Spezialeigenschaft
				specials = getSpecialsFromTxt(file.ReadLine());
				file.ReadLine();        // Stärke der Spezialeigenschaft
				special_strength = float.Parse(file.ReadLine());
				file.ReadLine();        //.Ressourcenkosten
				cost = int.Parse(file.ReadLine());
				file.ReadLine();        //.Energieverbrauch
				energy = float.Parse(file.ReadLine());
				file.ReadLine();        //.Maximale Gesundheit
				health_max = float.Parse(file.ReadLine());
				file.ReadLine();        //.Gelände, auf das man das Gebäude bauen kann
				getBuildingTerrain(file.ReadLine());
				file.ReadLine();        //.Upgrade From
				getUpgradeFrom(file.ReadLine());
				file.ReadLine();        //.Technologie, die das Gebäude freischaltet
				string techname = file.ReadLine();
				activation = Technology.findWithName(techname);
				if (activation == null) activation = Technology.data[0];

				Console.WriteLine("Blueprint loaded: " + name);

			}
		}





		Blueprint getUpgradeFrom(string bpname)
		{
			foreach (Blueprint b in Blueprint.data)
			{
				//Console.Write("" + t.name + ", ");
				if (bpname.Contains(b.name) && b != this)
				{
					//Console.WriteLine("Building '"+b.name+"' can be upgraded into '" + this.name + "'.");
					upgrade_only = true;
					b.develop_into.Add(this);
					return b;
				}
			}

			//Console.WriteLine("Warning: Technology with name '" + techname + "' has not been found.");
			return null;
		}
		void getBuildingTerrain(string info)
		{
			build_on = new bool[Planet.NUMBER_OF_TERRAIN_TYPES];

			if (info == null) return;
			if (info.Contains("TERRA_WATER")) build_on[Planet.TERRA_WATER] = true;
			if (info.Contains("TERRA_FERTILE")) build_on[Planet.TERRA_FERTILE] = true;
			if (info.Contains("TERRA_DESERT")) build_on[Planet.TERRA_DESERT] = true;
			if (info.Contains("TERRA_MOUNTAIN")) build_on[Planet.TERRA_MOUNTAIN] = true;
			if (info.Contains("TERRA_VOLCANO")) build_on[Planet.TERRA_VOLCANO] = true;
			if (info.Contains("TERRA_ICE")) build_on[Planet.TERRA_ICE] = true;
			if (info.Contains("TERRA_RESSOURCES")) build_on[Planet.TERRA_RESSOURCES] = true;
			if (info.Contains("TERRA_JUNGLE")) build_on[Planet.TERRA_JUNGLE] = true;
		}
		static byte getSpecialsFromTxt(string text)
		{
			switch (text)
			{
				case "SPECIAL_SETTLEMENT":
					return SPECIAL_SETTLEMENT;
				case "SPECIAL_SHIPS":
					return SPECIAL_SHIPS;
				case "SPECIAL_ENERGY":
					return SPECIAL_ENERGY;
				case "SPECIAL_TERRAFORM":
					return SPECIAL_TERRAFORM;
				case "SPECIAL_DEFEND":
					return SPECIAL_DEFEND;
				case "SPECIAL_REPAIR_BUILDINGS":
					return SPECIAL_REPAIR_BUILDINGS;
				case "SPECIAL_REPAIR_SHIPS":
					return SPECIAL_REPAIR_SHIPS;
				case "SPECIAL_RESEARCH":
					return SPECIAL_RESEARCH;
				case "SPECIAL_MINING":
					return SPECIAL_MINING;

			}
			return SPECIAL_NONE;
		}



	}
}
