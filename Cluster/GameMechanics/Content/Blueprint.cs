using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cluster.Rendering.Draw2D;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;

namespace Cluster.GameMechanics.Content
{
	class Blueprint
	{
		public enum SpecialAbility
		{
			NONE,
			SETTLEMENT,
			SHIPS,
			ENERGY,
			TERRAFORM,
			DEFEND,
			REPAIR_BUILDINGS,
			REPAIR_SHIPS,
			RESEARCH,
			MINING
		}
		
		public static List<Blueprint> data;
		public static int count;

		public int id;         // ID des Blueprints
		public readonly float healthMax; // Maximale Gesundheit
		public readonly int cost;       // Ressourcenkosten zum Bauen bzw. Upgraden
		public readonly string name;    // Bezeichnung

		public readonly Mesh shape; // Grafische Darstellung des Gebäudes

		public readonly SpecialAbility specials;          // Besondere Eigenschaft
		public readonly float specialStrength; // Wie stark ist die Eigenschaft ausgeprägt
		public readonly string[] description;   // Textbeschreibung (3 Zeilen)
		public readonly float energy;           // Energiebedarf des Gebäudes
		public bool[] buildOn;        // Gibt an, auf welchen Terraintypen das Gebäude gebaut werden kann.

		public bool upgradeOnly;              // Kann das Gebäude nur als Upgrade aus einem bereits bestehenden hervorgehen?
		public readonly List<Blueprint> developInto;   // Liste an weiteren Gebäuden, in die geupgradet werden kann
		public readonly Technology activation;          // Die Technologie, die benötigt wird, um das Gebäude freizuschalten.


		public string getName()
		{
			return name;
		}
		public float getHealth(Civilisation civ)
		{
			return healthMax;
		}
		public int getCost()
		{
			return cost;
		}
		public float getEnergyNeeds()
		{
			return energy;
		}
		public bool isBuildable(byte terrainType)
		{
			return buildOn[terrainType];
		}
		public bool isBuildable(Planet p, int pos)
		{
			return buildOn[(int)p.getTerrain(pos)];
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

		public Blueprint(string fileDirectory, string fileName)
		{
			//Console.WriteLine("Load Building: " + file_directory + file_name);

			using (StreamReader file = new StreamReader(fileDirectory + fileName, System.Text.Encoding.Default))
			{
				id = count; count++; data.Add(this);
				developInto = new List<Blueprint>();

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
				var gfxFile = file.ReadLine();
				if (File.Exists(fileDirectory + "gfx/" + gfxFile)) shape = new Mesh(fileDirectory + "gfx/" + gfxFile, false, true);
				file.ReadLine();        //.Spezialeigenschaft
				specials = getSpecialsFromTxt(file.ReadLine());
				file.ReadLine();        // Stärke der Spezialeigenschaft
				specialStrength = float.Parse(file.ReadLine());
				file.ReadLine();        //.Ressourcenkosten
				cost = int.Parse(file.ReadLine());
				file.ReadLine();        //.Energieverbrauch
				energy = float.Parse(file.ReadLine());
				file.ReadLine();        //.Maximale Gesundheit
				healthMax = float.Parse(file.ReadLine());
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
			foreach (var blueprint in Blueprint.data)
			{
				if (bpname.Contains(blueprint.name) && blueprint != this)
				{
					//Console.WriteLine("Building '"+b.name+"' can be upgraded into '" + this.name + "'.");
					upgradeOnly = true;
					blueprint.developInto.Add(this);
					return blueprint;
				}
			}

			//Console.WriteLine("Warning: Technology with name '" + techname + "' has not been found.");
			return null;
		}
		void getBuildingTerrain(string info)
		{
			buildOn = new bool[Planet.NUMBER_OF_TERRAIN_TYPES];

			if (info == null) return;
			if (info.Contains("TERRA_WATER")) buildOn[(int)Planet.Terrain.WATER] = true;
			if (info.Contains("TERRA_FERTILE")) buildOn[(int)Planet.Terrain.FERTILE] = true;
			if (info.Contains("TERRA_DESERT")) buildOn[(int)Planet.Terrain.DESERT] = true;
			if (info.Contains("TERRA_MOUNTAIN")) buildOn[(int)Planet.Terrain.MOUNTAIN] = true;
			if (info.Contains("TERRA_VOLCANO")) buildOn[(int)Planet.Terrain.VOLCANO] = true;
			if (info.Contains("TERRA_ICE")) buildOn[(int)Planet.Terrain.ICE] = true;
			if (info.Contains("TERRA_RESSOURCES")) buildOn[(int)Planet.Terrain.RESSOURCES] = true;
			if (info.Contains("TERRA_JUNGLE")) buildOn[(int)Planet.Terrain.JUNGLE] = true;
		}
		static SpecialAbility getSpecialsFromTxt(string text)
		{
			switch (text)
			{
				case "SPECIAL_SETTLEMENT":
					return SpecialAbility.SETTLEMENT;
				case "SPECIAL_SHIPS":
					return SpecialAbility.SHIPS;
				case "SPECIAL_ENERGY":
					return SpecialAbility.ENERGY;
				case "SPECIAL_TERRAFORM":
					return SpecialAbility.TERRAFORM;
				case "SPECIAL_DEFEND":
					return SpecialAbility.DEFEND;
				case "SPECIAL_REPAIR_BUILDINGS":
					return SpecialAbility.REPAIR_BUILDINGS;
				case "SPECIAL_REPAIR_SHIPS":
					return SpecialAbility.REPAIR_SHIPS;
				case "SPECIAL_RESEARCH":
					return SpecialAbility.RESEARCH;
				case "SPECIAL_MINING":
					return SpecialAbility.MINING;

			}
			return SpecialAbility.NONE;
		}

		public static IEnumerable<Blueprint> getAllBuildableOn(Planet planet, int pos)
		{
			foreach (var blueprint in data)
			{
				if (blueprint.isBuildable(planet, pos))
				{
					yield return blueprint;
				}
			}
		}

		public string getInfoText()
		{
			return name + "\n" + string.Join("\n", description);
		}
	}
}
