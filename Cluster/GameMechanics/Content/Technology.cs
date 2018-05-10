using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Content
{
	public class Technology
	{

		public const byte BONUS_NONE = 0;
		public const byte BONUS_RESEARCH = 1;





		static public List<Technology> data;
		static public int count;



		int id;                 // ID, anhand derer die Technologie eindeutig zu identifizieren ist
		string name;            // Bezeichnung
		string[] description;   // Beschreibung
		Mesh shape;             // Darstellung als Bild

		byte bonus;             // Welche Art Bonus?
		float bonusStrength;   // Wie stark ist der Bonus?
		int cost;               // Wieviele Forschungspunkte werden gebraucht?
		bool triggeredOnly;    // Wird nicht im Tech-Tree angezeigt, da nur Spontan-Entdeckung möglich?

		int era;                        // Wie weit hinten im Techtree wird die Technologie angezeigt?
		List<Technology> requirements;  // Welche Technologien sind die Voraussetzung?
		List<Technology> leadsTo;      // Für welche Technologien bildet diese die Grundlage?
		//List<int> int_req;


		public static void init()
		{
			data = new List<Technology>();
			count = 0;

			string directory = GameWindow.BASE_FOLDER + "technology/";

			while (File.Exists(directory + count.ToString().PadLeft(3, '0') + ".tech"))
			{
				new Technology(directory, count.ToString().PadLeft(3, '0') + ".tech");
			}
			//Debug.WriteLine("All Technologies loaded.");


		}
		/*
		public Technology(string file_directory, string file_name)
		{
			FileStream file = File.OpenRead(file_directory + file_name);
			BinaryReader reader = new BinaryReader(file);

			id = count; count++;

			name = reader.ReadString();
			description = new string[3];
			description[0] = reader.ReadString();
			description[1] = reader.ReadString();
			description[2] = reader.ReadString();

			string meshfile = reader.ReadString();
			shape = new Mesh(file_directory + meshfile);

			bonus = reader.ReadByte();
			bonus_strength = reader.ReadSingle();
			cost = reader.ReadInt32();
			triggered_only = reader.ReadByte()>0;
			era = reader.ReadInt32();

			int_req = new List<int>();
			int num_requirements = reader.ReadInt32();
			for (int i = 0; i < num_requirements; i++)
			{
				int_req.Add(reader.ReadInt32());
			}



			reader.Close();
			file.Close();
		}*/

		public Technology(string fileDirectory, string fileName)
		{
			//Debug.WriteLine("Load Technology: " + file_directory + file_name);

			using (StreamReader file = new StreamReader(fileDirectory + fileName, System.Text.Encoding.Default))
			{
				id = count; count++; data.Add(this);
				requirements = new List<Technology>();
				leadsTo = new List<Technology>();

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
				string gfxFile = file.ReadLine();
				if (File.Exists(fileDirectory + "gfx/" + gfxFile)) shape = new Mesh(fileDirectory + "gfx/" + gfxFile, false, true);
				file.ReadLine();        //.Bonus
				string bonusTxt = file.ReadLine();
				bonus = getBonusFromTxt(bonusTxt);
				file.ReadLine();        //.Stärke des Bonus
				bonusStrength = float.Parse(file.ReadLine());
				file.ReadLine();        //.Era
				era = int.Parse(file.ReadLine());
				file.ReadLine();        //.Forschungspunkte
				cost = int.Parse(file.ReadLine());
				file.ReadLine();        //.Triggered Only
				triggeredOnly = (file.ReadLine() == "ja");
				file.ReadLine();        //.Voraussetzungen

				bool exit = false;
				do
				{
					string require = file.ReadLine();
					if (require == null)
					{
						exit = true;
					}
					else if (require.StartsWith("."))
					{
						exit = true;
					}
					else if (require.Length > 0)
					{
						Technology req = Technology.findWithName(require);
						if (req != null)
						{
							requirements.Add(req);
							req.leadsTo.Add(this);
						}
					}
					else
					{
						exit = true;
					}
				}
				while (!exit);


				//Debug.WriteLine("Technology loaded: " + name);

			}
		}
		public bool researchedBy(Civilisation civ)
		{
			return true;
			return civ.tech[id];
		}


		static byte getBonusFromTxt(string text)
		{
			switch (text)
			{
				case "forschung":
					return BONUS_RESEARCH;

			}
			return BONUS_NONE;
		}
		public static Technology findWithName(string techname)
		{
			foreach (Technology t in Technology.data)
			{
				//Debug.Write("" + t.name + ", ");
				if (techname.Contains(t.name)) return t;
			}

			Debug.WriteLine("Warning: Technology with name '" + techname + "' has not been found.");
			return null;
		}




	}
}
