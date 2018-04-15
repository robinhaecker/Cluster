using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cluster.Rendering.Draw2D;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.LivingThings;

namespace Cluster.GameMechanics.Content
{
    class Prototype
    {
        public enum ShipAbility
        {
            NONE,
            COLONIZE,
            ASTEROID_MINING,
            REPAIR_UNITS,
            SPAWN_UNITS
        }

        public enum WeaponType
        {
            NONE,
            STD,
            FIND_AIM,
            PERSECUTE,
            LASER,
            EXPLOSIVE,
            PERSECUTE_AND_LASER
        }

        public enum Class
        {
            NONE,
            HUNTER,
            CRUISER,
            WARSHIP,
            CIVIL,
            SPECIAL
        }

        public static List<Prototype> data;
        public static int count;


        public readonly int id; // ID des Prototypen
        public readonly string name; // Bezeichnung
        public readonly string description; // Textbeschreibung (4 Zeilen)

        public readonly Mesh shape; // Grafische Darstellung des Gebäudes
        public readonly float shapeScaling; // Skalierung der Grafik (Standard: 1.0)

        public readonly ShipAbility specials; // Besondere Eigenschaft
        public readonly float specialStrength; // Wie stark ist die Eigenschaft ausgeprägt

        public readonly int cost; // Ressourcenkosten zum Bauen bzw. Upgraden
        public readonly int population; // Bevölkerungskosten

        public readonly float healthMax; // Maximale Gesundheit
        public readonly float shields; // Schildstärke
        public readonly float attack; // Angriffskraft
        public readonly float reloadTime; // Nachladezeit der Waffen
        public readonly float speed; // Basisgeschwindigkeit

        public readonly WeaponType weaponType; // Waffenart
        public readonly Class shipClass; // Klasse (z.B. Zivil, Jäger, Kreuzer, Kriegsschiff etc.)

        public readonly int infraLevel; // Ausbaustufe der Infrastruktur, die benötigt wird, um das Schiff zu bauen.
        public readonly Technology activation; // Die Technologie, die benötigt wird, um das Raumschiff freizuschalten.

        public readonly float weaponRange; // Hängt nur von Waffentyp ab.

        public static void init()
        {
            data = new List<Prototype>();
            count = 0;
            string directory = GameWindow.BASE_FOLDER + "units/";


            while (File.Exists(directory + count.ToString().PadLeft(3, '0') + ".unit"))
            {
                // ReSharper disable once ObjectCreationAsStatement --> Wird im Konstruktor in Liste aufgenommen.
                new Prototype(directory, count.ToString().PadLeft(3, '0') + ".unit");
            }
        }

        private Prototype(string fileDirectory, string fileName)
        {
            using (var file = new StreamReader(fileDirectory + fileName, System.Text.Encoding.Default))
            {
                id = count;
                count++;
                data.Add(this);

                ignoreLines(file, 3);
                name = read(file); // Bezeichnung
                ignoreLines(file);
                string txt = read(file);
                while (!txt.StartsWith("."))
                {
                    description = description + txt + "\n";
                    txt = read(file);
                }

                string gfxFile = read(file);
                if (File.Exists(fileDirectory + "gfx/" + gfxFile))
                    shape = new Mesh(fileDirectory + "gfx/" + gfxFile, true, true);
                ignoreLines(file);
                shapeScaling = readFloat(file);

                ignoreLines(file);
                specials = getSpecialsFromTxt(read(file));
                ignoreLines(file);
                specialStrength = readFloat(file);
                ignoreLines(file);
                cost = readInt(file);
                ignoreLines(file);
                population = readInt(file);

                ignoreLines(file);
                healthMax = readFloat(file);
                ignoreLines(file);
                shields = readFloat(file);
                ignoreLines(file);
                attack = readFloat(file);
                ignoreLines(file);
                reloadTime = readFloat(file);
                ignoreLines(file);
                speed = readFloat(file);

                ignoreLines(file);
                weaponType = getWeaponTypeFromTxt(read(file));
                ignoreLines(file);
                shipClass = getClassFromTxt(read(file));
                ignoreLines(file);
                infraLevel = readInt(file);
                ignoreLines(file);
                string techname = read(file);
                activation = Technology.findWithName(techname) ?? Technology.data[0];

                weaponRange = hasShortWeaponRange() ? 630.0f : 950.0f;

                Console.WriteLine("Prototype loaded: " + name);
            }
        }

        private static int readInt(StreamReader file)
        {
            return int.Parse(read(file));
        }

        private static float readFloat(StreamReader file)
        {
            return float.Parse(read(file));
        }

        private static string read(StreamReader file)
        {
            return file.ReadLine();
        }

        private static void ignoreLines(StreamReader file, int numberOfLines = 1)
        {
            for (var i = 0; i < numberOfLines; i++)
            {
                file.ReadLine();
            }
        }


        private bool hasShortWeaponRange()
        {
            return weaponType == WeaponType.LASER || weaponType == WeaponType.PERSECUTE_AND_LASER ||
                   weaponType == WeaponType.EXPLOSIVE;
        }

        static ShipAbility getSpecialsFromTxt(string text)
        {
            switch (text)
            {
                case "SPECIAL_NONE":
                    return ShipAbility.NONE;
                case "SPECIAL_COLONIZE":
                    return ShipAbility.COLONIZE;
                case "SPECIAL_ASTEROID_MINING":
                    return ShipAbility.ASTEROID_MINING;
                case "SPECIAL_REPAIR_UNITS":
                    return ShipAbility.REPAIR_UNITS;
                case "SPECIAL_SPAWN_UNITS":
                    return ShipAbility.SPAWN_UNITS;
            }

            return ShipAbility.NONE;
        }

        static WeaponType getWeaponTypeFromTxt(string text)
        {
            switch (text)
            {
                case "WEAPON_NONE":
                    return WeaponType.NONE;
                case "WEAPON_STD":
                    return WeaponType.STD;
                case "WEAPON_FIND_AIM":
                    return WeaponType.FIND_AIM;
                case "WEAPON_PERSECUTE":
                    return WeaponType.PERSECUTE;
                case "WEAPON_LASER":
                    return WeaponType.LASER;
                case "WEAPON_EXPLOSIVE":
                    return WeaponType.EXPLOSIVE;
                case "WEAPON_PERSECUTE_AND_LASER":
                    return WeaponType.PERSECUTE_AND_LASER;
            }

            return WeaponType.NONE;
        }

        static Class getClassFromTxt(string text)
        {
            switch (text)
            {
                case "CLASS_CIVIL":
                    return Class.CIVIL;
                case "CLASS_HUNTER":
                    return Class.HUNTER;
                case "CLASS_CRUISER":
                    return Class.CRUISER;
                case "CLASS_WARSHIP":
                    return Class.WARSHIP;
                case "CLASS_SPECIAL":
                    return Class.SPECIAL;
            }

            return Class.NONE;
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
            return healthMax; // Modifikator durch Technologien WIP
        }

        public float getShields(Civilisation civ = null)
        {
            return shields; // Modifikator durch Technologien WIP
        }

        public float getAttack(Civilisation civ = null)
        {
            return attack; // Modifikator durch Technologien WIP
        }

        public Class goodAgainst()
        {
            switch (shipClass)
            {
                case Class.HUNTER:
                    return Class.WARSHIP;
                case Class.CRUISER:
                    return Class.HUNTER;
                case Class.WARSHIP:
                    return Class.CRUISER;
                default:
                    return Class.NONE;
            }
        }

        public string getInfoText()
        {
            return name + "\n" +
                   string.Join("\n", description);
        }
    }
}