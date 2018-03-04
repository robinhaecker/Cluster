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
	class Prototype
	{
		public const byte SPECIAL_NONE = 0,
						  SPECIAL_COLONIZE = 1,
						  SPECIAL_ASTEROID_MINING = 2,
						  SPECIAL_REPAIR_UNITS = 3,
						  SPECIAL_SPAWN_UNITS = 4;

		public const byte WEAPON_NONE = 0,
						  WEAPON_STD = 1,
						  WEAPON_FIND_AIM = 2,
						  WEAPON_PERSECUTE = 3,
						  WEAPON_LASER = 4,
						  WEAPON_EXPLOSIVE = 5,
						  WEAPON_PERSECUTE_AND_LASER = 6;

		public const byte CLASS_NONE = 0,
						  CLASS_HUNTER = 1,
						  CLASS_CRUISER = 2,
						  CLASS_WARSHIP = 3,
						  CLASS_CIVIL = 4,
						  CLASS_SPECIAL = 5;

		public static List<Prototype> data;
		public static int count;


		public int id;                  // ID des Prototypen
		public string name;             // Bezeichnung
		public string description;      // Textbeschreibung (4 Zeilen)

		public Mesh shape;              // Grafische Darstellung des Gebäudes
		public float shape_scaling;     // Skalierung der Grafik (Standard: 1.0)

		public byte specials;           // Besondere Eigenschaft
		public float special_strength;  // Wie stark ist die Eigenschaft ausgeprägt

		public int cost;                // Ressourcenkosten zum Bauen bzw. Upgraden
		public int population;          // Bevölkerungskosten

		public float health_max;        // Maximale Gesundheit
		public float shields;           // Schildstärke
		public float attack;            // Angriffskraft
		public float reload_time;       // Nachladezeit der Waffen
		public float speed;             // Basisgeschwindigkeit

		public byte weapon_type;        // Waffenart
		public byte ship_class;         // Klasse (z.B. Zivil, Jäger, Kreuzer, Kriegsschiff etc.)

		public int infra_level;         // Ausbaustufe der Infrastruktur, die benötigt wird, um das Schiff zu bauen.
		public Technology activation;   // Die Technologie, die benötigt wird, um das Raumschiff freizuschalten.

		public float weapon_range;		// Hängt nur von Waffentyp ab.

		public static void init()
		{
			data = new List<Prototype>();
			count = 0;
			string directory = GameWindow.BASE_FOLDER + "units/";


			while (File.Exists(directory + count.ToString().PadLeft(3, '0') + ".unit"))
			{
				new Prototype(directory, count.ToString().PadLeft(3, '0') + ".unit");
			}
		}
		public Prototype(string file_directory, string file_name)
		{
			//Console.WriteLine("Load Building: " + file_directory + file_name);

			using (StreamReader file = new StreamReader(file_directory + file_name, System.Text.Encoding.Default))
			{
				id = count; count++; data.Add(this);



				file.ReadLine();        //.Rohmaterial für Technologiedaten
				file.ReadLine();        //.
				file.ReadLine();        //.Name
				name = file.ReadLine(); // Bezeichnung
				file.ReadLine();
				string txt = file.ReadLine();
				while (!txt.StartsWith("."))
				{
					description = description + txt + "\n";
					txt = file.ReadLine();
				}
				//file.ReadLine();        //.Grafikdatei
				string gfx_file = file.ReadLine();
				if (File.Exists(file_directory + "gfx/" + gfx_file)) shape = new Mesh(file_directory + "gfx/" + gfx_file, true, true);
				file.ReadLine();        //.Skalierung der Grafik (Standard: 1.0)
				shape_scaling = float.Parse(file.ReadLine());

				file.ReadLine();        //Besondere Eigenschaft
				specials = getSpecialsFromTxt(file.ReadLine());
				file.ReadLine();        // Wie stark ist die Eigenschaft ausgeprägt
				special_strength = float.Parse(file.ReadLine());
				file.ReadLine();
				cost = int.Parse(file.ReadLine());
				file.ReadLine();
				population = int.Parse(file.ReadLine());

				file.ReadLine();
				health_max = float.Parse(file.ReadLine());
				file.ReadLine();
				shields = float.Parse(file.ReadLine());
				file.ReadLine();
				attack = float.Parse(file.ReadLine());
				file.ReadLine();
				reload_time = float.Parse(file.ReadLine());
				file.ReadLine();
				speed = float.Parse(file.ReadLine());

				file.ReadLine();
				weapon_type = getWeaponTypeFromTxt(file.ReadLine());
				file.ReadLine();
				ship_class = getClassFromTxt(file.ReadLine());
				file.ReadLine();
				infra_level = int.Parse(file.ReadLine());
				file.ReadLine();        //.Technologie, die das Gebäude freischaltet
				string techname = file.ReadLine();
				activation = Technology.findWithName(techname);
				if (activation == null) activation = Technology.data[0];

				weapon_range = 950.0f;
				if (weapon_type == Prototype.WEAPON_LASER || weapon_type == Prototype.WEAPON_PERSECUTE_AND_LASER || weapon_type == Prototype.WEAPON_EXPLOSIVE) weapon_range = 630.0f;



				Console.WriteLine("Prototype loaded: " + name);

			}
		}
		static byte getSpecialsFromTxt(string text)
		{
			switch (text)
			{
				case "SPECIAL_NONE":
					return SPECIAL_NONE;
				case "SPECIAL_COLONIZE":
					return SPECIAL_COLONIZE;
				case "SPECIAL_ASTEROID_MINING":
					return SPECIAL_ASTEROID_MINING;
				case "SPECIAL_REPAIR_UNITS":
					return SPECIAL_REPAIR_UNITS;
				case "SPECIAL_SPAWN_UNITS":
					return SPECIAL_SPAWN_UNITS;
			}
			return SPECIAL_NONE;
		}
		static byte getWeaponTypeFromTxt(string text)
		{
			switch (text)
			{
				case "WEAPON_NONE":
					return WEAPON_NONE;
				case "WEAPON_STD":
					return WEAPON_STD;
				case "WEAPON_FIND_AIM":
					return WEAPON_FIND_AIM;
				case "WEAPON_PERSECUTE":
					return WEAPON_PERSECUTE;
				case "WEAPON_LASER":
					return WEAPON_LASER;
				case "WEAPON_EXPLOSIVE":
					return WEAPON_EXPLOSIVE;
				case "WEAPON_PERSECUTE_AND_LASER":
					return WEAPON_PERSECUTE_AND_LASER;
			}
			return SPECIAL_NONE;
		}
		static byte getClassFromTxt(string text)
		{
			switch (text)
			{
				case "CLASS_CIVIL":
					return CLASS_CIVIL;
				case "CLASS_HUNTER":
					return CLASS_HUNTER;
				case "CLASS_CRUISER":
					return CLASS_CRUISER;
				case "CLASS_WARSHIP":
					return CLASS_WARSHIP;
				case "CLASS_SPECIAL":
					return CLASS_SPECIAL;
			}
			return CLASS_NONE;
		}






		public int getCost()
		{
			return cost;
		}
		public int getPopulation()
		{
			return population;
		}
		public float getSpeed(Civilisation civ = null)
		{
			return speed; // Wird noch abgewandelt durch Technologien der Zivilisation.
		}
		public float getHealth(Civilisation civ = null)
		{
			return health_max; // Modifikator durch Technologien WIP
		}
		public float getShields(Civilisation civ = null)
		{
			return shields; // Modifikator durch Technologien WIP
		}
		public float getAttack(Civilisation civ = null)
		{
			return attack; // Modifikator durch Technologien WIP
		}

		public byte goodAgainst()
		{
			switch (ship_class)
			{
				case CLASS_HUNTER:
					return CLASS_WARSHIP;
				case CLASS_CRUISER:
					return CLASS_HUNTER;
				case CLASS_WARSHIP:
					return CLASS_CRUISER;
				default:
					return CLASS_NONE;
			}
		}


	}
}
