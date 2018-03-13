using System.Collections.Generic;
using System.IO;
using System.Text;
using Cluster.GameMechanics.Content;
using Cluster.Mathematics;

namespace Cluster.GameMechanics.Universe.LivingThings
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

        static List<string> _names;

        readonly int _id;
        internal readonly string name;
        public readonly float red, green, blue;
        readonly bool _alive;

        public readonly bool[] tech;
        readonly float[] _boni;
        public float ressources;
        public float science;
        public int population;
        public int maxPopulation;
        public int maxPopulationNew;


        public static void init(int number = 5)
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


        private Civilisation()
        {
            _id = count;
            count++;
            data.Add(this);
            _alive = true;
            tech = new bool[Technology.count];
            _boni = new float[NUMBER_OF_BONI];
            ressources = 1000.0f;
            science = 0.0f;
            population = 0;
            maxPopulation = 10;

            name = getRandomName();
            var colors = getRandomColors();
            red = colors.x;
            green = colors.y;
            blue = colors.z;
        }

        public Civilisation(string name, float red, float green, float blue) : this()
        {
            this.name = name;
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        private static void loadNames()
        {
            _names = new List<string>();
            using (StreamReader file =
                new StreamReader(GameWindow.BASE_FOLDER + "civs/names.txt", Encoding.Default))
            {
                while (!file.EndOfStream)
                {
                    _names.Add(file.ReadLine());
                }
            }
        }

        private static string getRandomName()
        {
            int i = GameWindow.random.Next(_names.Count);
            var s = _names[i];
            _names.RemoveAt(i);
            return s;
        }

        private Vec3 getRandomColors()
        {
            switch (_id)
            {
                case 0:
                    return new Vec3(0.2f, 0.2f, 1.0f);
                case 1:
                    return new Vec3(1.0f, 0.2f, 0.2f);
                case 2:
                    return new Vec3(0.2f, 1.0f, 0.2f);
                case 3:
                    return new Vec3(1.0f, 1.0f, 1.0f);
                case 4:
                    return new Vec3(1.0f, 1.0f, 0.1f);
                case 5:
                    return new Vec3(1.0f, 0.1f, 1.0f);
                case 6:
                    return new Vec3(0.1f, 1.0f, 1.0f);
                case 7:
                    return new Vec3(0.2f, 0.2f, 0.2f);
                case 8:
                    return new Vec3(1.0f, 0.5f, 0.05f);
                case 9:
                    return new Vec3(0.57f, 0.47f, 0.05f);
                default:
                    return new Vec3(
                        (float) GameWindow.random.NextDouble(),
                        (float) GameWindow.random.NextDouble(),
                        (float) GameWindow.random.NextDouble());
            }
        }

        public static Civilisation getPlayer()
        {
            return data[player];
        }

        public static int countCivilisations()
        {
            int num = 0;
            foreach (Civilisation civ in data)
            {
                if (civ._alive) num++;
            }

            return num;
        }

        public static int getMaxPopulationPerCiv()
        {
            return TOTAL_MAX_POPULATION / countCivilisations();
        }

        public int getId()
        {
            return _id;
        }

        public float getMultiplicator(byte id)
        {
            return (1.0f + _boni[id]);
        }

        public int populationLeft()
        {
            return maxPopulation - population;
        }
    }
}