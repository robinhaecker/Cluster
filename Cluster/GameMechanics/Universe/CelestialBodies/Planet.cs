using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Rendering.Appearance;
using Cluster.Rendering.Draw2D;
using OpenTK.Graphics.OpenGL;

namespace Cluster.GameMechanics.Universe.CelestialBodies
{
    public class Planet
    {
        // constants
        public const int PLANET_DETAIL = 15;
        public const int NUMBER_OF_TERRAIN_TYPES = 8;

        public enum Effect
        {
            NONE,
            RAIN,
            VOLCANO_ERUPTION
        }

        public enum Terrain
        {
            WATER,
            FERTILE,
            DESERT,
            MOUNTAIN,
            VOLCANO,
            ICE,
            RESSOURCES,
            JUNGLE
        }

        public enum Climate
        {
            NORMAL,
            COLD,
            HOT,
            RAINY,
            TOXIC
        }

        private const int NUMBER_OF_CLIMATES = 5;
        private const byte JUNGLE_VARIETY = 2;


        // statics
        public static readonly List<Planet> planets = new List<Planet>();
        static int countId;

        private static Mesh[,] jungle;
        public static Mesh[] terraImage;


        // fields
        public float x, y;
        public readonly int size;
        public readonly string name;

        public readonly Climate climate;
        public Terrain[] terra;
        public Effect[] effect;
        public float[] timer;
        public Building[] infra;

        private readonly int random;
        private readonly int id;

        private bool canseeTemp;
        private bool seenbefore;
        internal float red;
        internal float green;
        internal float blue;

        int glData, bufPos, bufTer, bufCol;
        private bool glDataUpdate;


        // constructors
        public Planet(float x, float y, int size = 15)
        {
            planets.Add(this);
            id = countId;
            countId++;
            glDataUpdate = true;

            this.x = x;
            this.y = y;
            random = GameWindow.random.Next(1000);

            this.size = size;
            terra = new Terrain[size];
            effect = new Effect[size];
            timer = new float[size];
            infra = new Building[size];

            //r = 0.41f; g = 0.25f; b = 0.0f;
            //b = 1.0f;
            //g = 0.91f; r = 0.15f; b = 0.0f;
            red = 1.0f;
            green = 1.0f;
            blue = 1.0f;

            red = Civilisation.data[random % Civilisation.count].red;
            green = Civilisation.data[random % Civilisation.count].green;
            blue = Civilisation.data[random % Civilisation.count].blue;

            seenbefore = false;
            canseeTemp = false;

            name = "Planet_" + id.ToString();
            climate = (Climate) GameWindow.random.Next(NUMBER_OF_CLIMATES);

            //Planeten mit Landschaft fuellen
            for (int i = 0; i < size; i++)
            {
                double percentage = GameWindow.random.NextDouble();
                if (percentage < 0.15)
                {
                    terra[i] = Terrain.DESERT;
                }
                else if (percentage < 0.3)
                {
                    terra[i] = Terrain.MOUNTAIN;
                }
                else if (percentage < 0.5)
                {
                    terra[i] = Terrain.WATER;
                }
                else if (percentage < 0.65)
                {
                    switch (climate)
                    {
                        case Climate.COLD:
                            terra[i] = Terrain.ICE;
                            break;
                        case Climate.HOT:
                            terra[i] = Terrain.DESERT;
                            break;
                        case Climate.RAINY:
                            terra[i] = Terrain.JUNGLE;
                            break;
                        case Climate.TOXIC:
                            terra[i] = Terrain.VOLCANO;
                            break;
                        default:
                            terra[i] = Terrain.FERTILE;
                            break;
                    }
                }
                else if (percentage < 0.7)
                {
                    terra[i] = Terrain.JUNGLE;

//                    build(i, Blueprint.data[5], Civilisation.data[0]);
                }
                else
                {
                    terra[i] = Terrain.FERTILE;
                    build(i, Blueprint.data[GameWindow.random.Next(Blueprint.count)],
                        Civilisation.data[GameWindow.random.Next(Civilisation.count)], false);
                }

                //Debug.WriteLine(terra[i].ToString());
            }
        }


        // Initiation
        public static void init()
        {
            jungle = new Mesh[JUNGLE_VARIETY, NUMBER_OF_CLIMATES];
            for (byte i = 0; i < JUNGLE_VARIETY; i++)
            {
                for (byte j = 0; j < NUMBER_OF_CLIMATES; j++)
                {
                    //jungle[i,j] = new Mesh("planets/tree" + i.ToString() + ".vg");
                    jungle[i, j] = new Mesh("planets/tree_" + j.ToString() + "_" + i.ToString() + ".vg");
                }
            }

            terraImage = new Mesh[NUMBER_OF_TERRAIN_TYPES];
            terraImage[0] = new Mesh("planets/terra0b.vg");
            terraImage[1] = new Mesh("planets/terra1b.vg");
            terraImage[2] = new Mesh("planets/terra2b.vg");
            terraImage[3] = new Mesh("planets/terra3b.vg");
            terraImage[4] = new Mesh("planets/terra4b.vg");
            terraImage[5] = new Mesh("planets/terra5b.vg");
            terraImage[6] = new Mesh("planets/terra6b.vg");
            terraImage[7] = new Mesh("planets/terra7b.vg");
        }


        // rendering stuff
        public static void render()
        {
            Space.planetAtmosphereShader.bind();
            GL.Uniform3(Space.planetAtmosphereShader.getUniformLocation("scroll"),
                (float) Space.scrollX,
                (float) Space.scrollY,
                (float) Space.zoom);
            GL.Uniform3(Space.planetAtmosphereShader.getUniformLocation("viewport"), GameWindow.active.multX,
                GameWindow.active.multY, Space.animation);

            foreach (Planet pl in planets)
            {
                if (true) //pl.cansee_temp || pl.seenbefore)
                {
                    pl._render_atmosphere();
                }
            }
            
            Space.planetShader.bind();
            GL.Uniform3(Space.planetShader.getUniformLocation("scroll"),
                (float) Space.scrollX,
                (float) Space.scrollY,
                (float) Space.zoom);
            GL.Uniform3(Space.planetShader.getUniformLocation("viewport"), GameWindow.active.multX,
                GameWindow.active.multY, Space.animation);
            Space.animation += 0.001f;

            foreach (Planet pl in planets)
            {
                if (true) //pl.cansee_temp || pl.seenbefore)
                {
                    pl._render();
                }
            }


            if (Space.zoom > 0.2)
            {
                float maxwidth = GL.GetFloat(GetPName.AliasedLineWidthRange);
                float linewidth = Math.Min(maxwidth, 3.0f * (float) Math.Max(Space.zoom, 1.0));
                GL.LineWidth(Math.Min(maxwidth, 1.5f));
                Space.buildingShader.bind();
                GL.Uniform3(Space.buildingShader.getUniformLocation("scroll"), (float) Space.scrollX,
                    (float) Space.scrollY, (float) Space.zoom);
                GL.Uniform3(Space.buildingShader.getUniformLocation("viewport"), GameWindow.active.multX,
                    GameWindow.active.multY, Space.animation);
                foreach (Planet pl in planets)
                {
                    if (true) //pl.cansee_temp)
                    {
                        pl._render_buildings();
                    }
                }
            }

            Shader.unbind();
        }

        private void prepare_vba()
        {
            float[] vertices = new float[(size * PLANET_DETAIL + 2) * 2];
            float[] terrain = new float[(size * PLANET_DETAIL + 2) * 1];
            float[] colour = new float[(size * PLANET_DETAIL + 2) * 3];

            //Mitte
            terrain[0] = -1.0f;
            vertices[0] = vertices[1] = 0.0f;
            colour[0] = colour[1] = colour[2] = 0.0f;
            float baseHeight = 20.0f * (float) size;

            //Vertices außen herum.
            for (int i = 0; i < size; i++)
            {
                //terra[i] = Terrain.MOUNTAIN;
                double randmont = (1.0 + (double) GameWindow.random.NextDouble() * 3.0) * Math.PI;
                if (terra[(i + 1) % size] == Terrain.MOUNTAIN)
                {
                    randmont = (1.0 + (double) GameWindow.random.Next(3)) * Math.PI;
                }

                double randwat = (1.0 + (double) GameWindow.random.NextDouble() * 3.0) * Math.PI;
                if (terra[(i + 1) % size] == Terrain.WATER)
                {
                    randwat = (1.0 + (double) GameWindow.random.Next(3)) * Math.PI;
                }

                for (int j = 0; j < PLANET_DETAIL; j++)
                {
                    float height = baseHeight;
                    double randvulk = (1.0 + (double) GameWindow.random.Next(2)) * Math.PI;

                    //Terrain setzen
                    terrain[(i * PLANET_DETAIL + j + 1) + 0] = (float) terra[i % size];


                    //Farben setzen
                    switch (terra[i])
                    {
                        case Terrain.DESERT:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 1.0f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 1.0f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.0f;
                            if (climate == Climate.TOXIC)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] =
                                    0.13f + Math.Max(0.0f, (float) GameWindow.random.Next(3) - 1.0f) * 0.35f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.13f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.1f;
                            }
                            else if (climate == Climate.COLD)
                            {
                                float hotness = (float) GameWindow.random.NextDouble();
                                if (hotness > 0.5)
                                {
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = hotness * 0.7f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = hotness * 0.7f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = hotness * 0.75f;
                                }
                            }
                            else if (climate == Climate.RAINY)
                            {
                                float hotness = (float) GameWindow.random.NextDouble();
                                if (hotness > 0.75)
                                {
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = hotness * 0.97f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = hotness * 0.64f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = hotness * 0.05f;
                                }
                            }

                            height += (float) GameWindow.random.NextDouble() * 4.0f;
                            break;

                        case Terrain.FERTILE:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.2f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] =
                                0.5f + (float) GameWindow.random.NextDouble() * 0.5f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.2f;
                            if (climate == Climate.TOXIC)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.13f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] =
                                    0.13f + Math.Min(1.0f, (float) GameWindow.random.Next(3)) * 0.35f +
                                    (float) GameWindow.random.NextDouble() * 0.15f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.1f;
                            }
                            else if (climate == Climate.HOT)
                            {
                                double hotness = GameWindow.random.NextDouble();
                                if (hotness > 0.5)
                                {
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = (float) hotness;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 1.0f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.0f;
                                }
                            }
                            else if (climate == Climate.COLD)
                            {
                                double hotness = GameWindow.random.NextDouble();
                                if (hotness > 0.75)
                                {
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 1.0f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 1.0f;
                                    colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
                                }
                            }

                            height -= (float) GameWindow.random.NextDouble() * 4.0f;
                            break;

                        case Terrain.JUNGLE:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.1f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] =
                                0.2f + (float) GameWindow.random.NextDouble() * 0.5f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.1f;
                            height += (float) (GameWindow.random.NextDouble() - 0.2) * 8.0f;
                            break;

                        case Terrain.ICE:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.8f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.8f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
                            break;

                        case Terrain.MOUNTAIN:
                            float tone = (float) GameWindow.random.NextDouble();
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.35f + tone * 0.03f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.35f + tone * 0.03f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.35f + tone * 0.03f;
                            if (climate == Climate.COLD)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] += 0.5f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] += 0.5f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] += 0.6f;
                            }
                            else if (climate == Climate.HOT)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] += 0.3f + tone * 0.2f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] += 0.1f + tone * 0.3f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] -= 0.05f;
                            }
                            else if (climate == Climate.RAINY)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] -= 0.15f + tone * 0.02f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] -= 0.1f + tone * 0.075f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] -= 0.15f + tone * 0.02f;
                            }
                            else if (climate == Climate.TOXIC)
                            {
                                tone = (float) GameWindow.random.NextDouble();
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] -= 0.15f + tone * 0.03f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] -= 0.2f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] -= 0.2f + tone * 0.02f;
                            }

                            float hilly = (float) Math.Sin((double) j / (double) PLANET_DETAIL * randmont);
                            float maxamp = (1.0f - Math.Abs((float) j / (float) PLANET_DETAIL - 0.5f) * 2.0f);
                            if (j > PLANET_DETAIL / 2 && terra[(i + 1) % size] == Terrain.MOUNTAIN)
                            {
                                maxamp = 1.0f;
                            }
                            else if (j < PLANET_DETAIL / 2 && terra[(i - 1 + size) % size] == Terrain.MOUNTAIN)
                            {
                                maxamp = 1.0f;
                            }

                            height += maxamp * 30.0f * (hilly * (hilly + 0.013f) + 0.5f);
                            break;

                        case Terrain.RESSOURCES:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 1.0f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.0f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
                            break;

                        case Terrain.VOLCANO:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.2f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.2f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.2f;
                            if ((j > PLANET_DETAIL * 0.45) &&
                                (j < PLANET_DETAIL * 0.55))
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.5f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.0f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.0f;
                            }

                            float hilly2 = (float) Math.Sin((double) j / (double) PLANET_DETAIL * randvulk * 0.3);
                            float maxamp2 = (1.0f - Math.Abs((float) j / (float) PLANET_DETAIL - 0.5f) * 2.0f);
                            if (j > PLANET_DETAIL / 2 && terra[(i + 1) % size] == Terrain.MOUNTAIN)
                            {
                                maxamp = 1.0f;
                            }
                            else if (j < PLANET_DETAIL / 2 && terra[(i - 1 + size) % size] == Terrain.MOUNTAIN)
                            {
                                maxamp = 1.0f;
                            }

                            height += maxamp2 * 30.0f * (hilly2 * (hilly2 + 0.513f) + 0.5f);
                            break;

                        case Terrain.WATER:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.051f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.051f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.8f;
                            if (climate == Climate.TOXIC)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.003f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.43f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.31f;
                            }
                            else if (climate == Climate.COLD)
                            {
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.8f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.8f;
                                colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 1.0f;
                            }

                            hilly = Math.Abs((float) Math.Sin((double) j / (double) PLANET_DETAIL * randwat));
                            maxamp = (1.0f - Math.Abs((float) j / (float) PLANET_DETAIL - 0.5f));
                            if (j > PLANET_DETAIL / 2 && terra[(i + 1) % size] == Terrain.WATER)
                            {
                                maxamp = 1.0f;
                            }
                            else if (j < PLANET_DETAIL / 2 && terra[(i - 1 + size) % size] == Terrain.WATER)
                            {
                                maxamp = 1.0f;
                            }

                            float multor = 10.0f;
                            if (climate == Climate.HOT || climate == Climate.TOXIC) multor = 6.0f;
                            else if (climate == Climate.RAINY) multor = 13.0f;

                            height -= maxamp * multor *
                                      (1.0f + hilly * 0.12f +
                                       (hilly + 1.05f) * (hilly + 1.75f) * 0.15f);
                            break;

                        default:
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 0] = 0.41f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 1] = 0.25f;
                            colour[(i * PLANET_DETAIL + j + 1) * 3 + 2] = 0.05f;
                            break;
                    }

                    if (j == 0 && terra[i] != Terrain.WATER && terra[(i - 1 + size) % size] == Terrain.WATER)
                    {
                        height = baseHeight;
                    }

                    if (j == PLANET_DETAIL - 1 && terra[i] != Terrain.WATER &&
                        terra[(i + 1 + size) % size] == Terrain.WATER)
                    {
                        height = baseHeight;
                    }

                    //Vertices setzen
                    double alpha = 2.0 * (double) Math.PI * ((double) i + ((double) j / (double) PLANET_DETAIL)) /
                                   (double) size;
                    vertices[(i * PLANET_DETAIL + j + 1) * 2 + 0] = (float) Math.Cos(alpha) * height;
                    vertices[(i * PLANET_DETAIL + j + 1) * 2 + 1] = (float) Math.Sin(alpha) * height;
                }
            }


            vertices[(size * PLANET_DETAIL + 1) * 2 + 0] = vertices[2];
            vertices[(size * PLANET_DETAIL + 1) * 2 + 1] = vertices[3];
            terrain[(size * PLANET_DETAIL + 1) + 0] = terrain[1];
            colour[(size * PLANET_DETAIL + 1) * 3 + 0] = colour[3];
            colour[(size * PLANET_DETAIL + 1) * 3 + 1] = colour[4];
            colour[(size * PLANET_DETAIL + 1) * 3 + 2] = colour[5];


            //Create the vertex array object.
            if (glData == 0)
            {
                glData = GL.GenVertexArray();
                bufPos = GL.GenBuffer();
                bufTer = GL.GenBuffer();
                bufCol = GL.GenBuffer();
            }

            GL.BindVertexArray(glData);


            GL.BindBuffer(BufferTarget.ArrayBuffer, bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufTer);
            GL.BufferData(BufferTarget.ArrayBuffer, terrain.Length * sizeof(float), terrain,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufCol);
            GL.BufferData(BufferTarget.ArrayBuffer, colour.Length * sizeof(float), colour, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindVertexArray(0);
            glDataUpdate = false;
        }

        private void _render_atmosphere()
        {
            if (glDataUpdate) prepare_vba();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Space.planetTex0.tex);

            GL.Uniform1(Space.planetAtmosphereShader.getUniformLocation("pos_x"), (float) x);
            GL.Uniform1(Space.planetAtmosphereShader.getUniformLocation("pos_y"), (float) y);
            GL.Uniform1(Space.planetAtmosphereShader.getUniformLocation("size"), (float) size);
            GL.Uniform3(Space.planetAtmosphereShader.getUniformLocation("rgb"), red, green, blue);

            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(glData);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }

        private void _render()
        {
            if (glDataUpdate) prepare_vba();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Space.planetTex0.tex);

            GL.Uniform1(Space.planetShader.getUniformLocation("pos_x"), x);
            GL.Uniform1(Space.planetShader.getUniformLocation("pos_y"), y);
            GL.Uniform1(Space.planetShader.getUniformLocation("size"), (float) size);
            GL.Uniform3(Space.planetShader.getUniformLocation("rgb"), red, green, blue);

            GL.Disable(EnableCap.Blend);
            GL.BindVertexArray(glData);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, size * PLANET_DETAIL + 2);
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.Blend);
        }

        private void _render_buildings()
        {
            GL.LineWidth(2.0f);
            GL.Enable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.DepthTest);

            GL.Uniform1(Space.buildingShader.getUniformLocation("pos_x"), (float) x);
            GL.Uniform1(Space.buildingShader.getUniformLocation("pos_y"), (float) y);
            GL.Uniform1(Space.buildingShader.getUniformLocation("size"), (float) size);

            for (int ii = 0; ii < size; ii++)
            {
                var transp = 1.0f;
                if (terra[ii] == Terrain.JUNGLE)
                {
                    int cc = (random * size + ii) % JUNGLE_VARIETY;
                    if (infra[ii] != null) transp = 1.0f - infra[ii].getHealthFraction() * 0.75f;
                    GL.Uniform3(Space.buildingShader.getUniformLocation("info"), (float) ii, 1.0f, transp);
                    GL.Uniform3(Space.buildingShader.getUniformLocation("rgb"), 1.0f, 1.0f, 1.0f);
                    GL.BindVertexArray(jungle[cc, (int) climate].glData);
                    GL.DrawArrays(PrimitiveType.Lines, 0, jungle[cc, (int) climate].numLines * 2);
                }

                if (infra[ii] == null)
                {
                    continue;
                }

                var ycut = 1.0f;
                transp = 1.0f;
                if (infra[ii].status == Building.Status.DESTROYED ||
                    infra[ii].status == Building.Status.DESTROYED_DURING_CONSTRUCTION)
                    transp = 1.0f - infra[ii].timer * 0.01f;
                if (infra[ii].status == Building.Status.UNDER_CONSTRUCTION ||
                    infra[ii].status == Building.Status.DESTROYED_DURING_CONSTRUCTION)
                    ycut = infra[ii].getHealthFraction() * infra[ii].blueprint.shape.boxY;


                GL.Uniform3(Space.buildingShader.getUniformLocation("info"), (float) ii, (float) ycut, transp);
                GL.Uniform3(Space.buildingShader.getUniformLocation("rgb"), 0.5f + 0.5f * infra[ii].owner.red,
                    0.5f + 0.5f * infra[ii].owner.green, 0.5f + 0.5f * infra[ii].owner.blue);

                GL.BindVertexArray(infra[ii].blueprint.shape.glData);
                GL.DrawArrays(PrimitiveType.Lines, 0, infra[ii].blueprint.shape.numLines * 2);
            }

            GL.BindVertexArray(0);

            GL.Enable(EnableCap.DepthTest);
        }

        // static functions
        public static int count()
        {
            return planets.Count;
        }


        // methods
        public Terrain getTerrain(int i)
        {
            return terra[i];
        }

        public string getTerrainType(int i)
        {
            if (i < 0 || i >= size) return "";
            switch (terra[i])
            {
                case Terrain.DESERT:
                    return "Wüste";
                case Terrain.FERTILE:
                    return "Ebene";
                case Terrain.ICE:
                    return "Eis";
                case Terrain.JUNGLE:
                    return "Dschungel";
                case Terrain.MOUNTAIN:
                    return "Gebirge";
                case Terrain.RESSOURCES:
                    return "Ressourcenanhäufung";
                case Terrain.VOLCANO:
                    return "Vulkan";
                case Terrain.WATER:
                    return "Ozean";
                default:
                    return "Unbekanntes Gelände";
            }
        }

        public string getClimate()
        {
            switch (climate)
            {
                case Climate.COLD:
                    return "Kalt";
                case Climate.HOT:
                    return "Heiß";
                case Climate.NORMAL:
                    return "Gemäßigt";
                case Climate.TOXIC:
                    return "Toxisch";
                case Climate.RAINY:
                    return "Humid";
                default:
                    return "Unbekannt";
            }
        }

        private Civilisation getDominantCiv()
        {
            byte[] points = new byte[Civilisation.count];
            for (int i = 0; i < size; i++)
            {
                if (infra[i] != null)
                {
                    points[infra[i].owner.getId()]++;
                }
            }

            Civilisation maxCiv = null;
            byte maxPoints = 0;
            for (int i = 0; i < Civilisation.count; i++)
            {
                if (points[i] > maxPoints)
                {
                    maxPoints = points[i];
                    maxCiv = Civilisation.data[i];
                }
            }

            return maxCiv;
        }

        public string getDominantCivName()
        {
            var civ = getDominantCiv();
            if (civ == null)
            {
                return "Status:\tunbesiedelt";
            }

            if (civ == Civilisation.getPlayer())
            {
                return "Status:\teigene Kolonie";
            }

            return "Status:\tbesetzt (" + civ.name + ")";
        }

        public Building build(int i, Blueprint bp, Civilisation civ, bool payForIt = true)
        {
            if (payForIt) civ.ressources -= bp.getCost();
            infra[i] = new Building(this, i, bp, civ);
            return infra[i];
        }

        public Building upgrade(int i, Blueprint bp, bool payForIt = true)
        {
            if (infra[i].blueprint.developInto.Contains(bp))
            {
                if (payForIt) infra[i].owner.ressources -= (bp.getCost() - infra[i].blueprint.getCost());
                infra[i].blueprint = bp;
                infra[i].status = Building.Status.UNDER_CONSTRUCTION;
                infra[i].healthMax = bp.getHealth(infra[i].owner);
            }

            return infra[i];
        }

        public List<Blueprint> listOfBuildables(int spot, Civilisation civ)
        {
            List<Blueprint> list = new List<Blueprint>();
            if (!canBuild(civ)) return list;

            if (infra[spot] == null)
            {
                foreach (Blueprint bp in Blueprint.data)
                {
                    if (bp.activation.researchedBy(civ) && bp.buildOn[(int) terra[spot]] && !bp.upgradeOnly)
                        list.Add(bp);
                }
            }
            else if (infra[spot].owner == civ && infra[spot].status == Building.Status.NONE)
            {
                foreach (Blueprint bp in infra[spot].blueprint.developInto)
                {
                    if (bp.activation.researchedBy(civ) && bp.buildOn[(int) terra[spot]]) list.Add(bp);
                }
            }


            return list;
        }

        public bool canBuild(Civilisation civ)
        {
            return true;
            for (int i = 0; i < size; i++)
            {
                if (infra[i] != null && infra[i].owner == civ &&
                    infra[i].blueprint.specials == Blueprint.SpecialAbility.SETTLEMENT &&
                    infra[i].status == Building.Status.NONE) return true;
            }

            return false;
        }

        public Civilisation getDominantCivilisation()
        {
            //TODO: Hat aus irgendeinem Grund nicht funktioniert, daher vorerst auskommentiert
//            float[] counts = new float[Civilisation.count];
//            foreach (Building building in infra)
//            {
//                counts[building.owner.getId()] += building.health;
//            }
//
//            int dominantId = -1;
//            float maximum = 100.0f;
//            for (int i = 0; i < counts.Length; i++)
//            {
//                if (counts[i] > maximum)
//                {
//                    dominantId = i;
//                    maximum = counts[i];
//                }
//            }
//
//            if (dominantId > -1)
//            {
//                return Civilisation.data[dominantId];
//            }
            return null;
        }

        public float getEnergy(Civilisation civ)
        {
            float energy = 100.0f;
            for (int i = 0; i < size; i++)
            {
                if (infra[i] != null)
                {
                    if (infra[i].owner == civ)
                    {
                        energy -= infra[i].blueprint.energy;
                        if (infra[i].blueprint.specials == Blueprint.SpecialAbility.ENERGY)
                        {
                            energy += infra[i].blueprint.specialStrength;
                        }
                    }
                }
            }

            return energy;
        }

        public static void simulate(float dt = 1.0f)
        {
            foreach (Civilisation civ in Civilisation.data)
            {
                civ.maxPopulationNew = 10;
            }

            foreach (Planet pl in planets)
            {
                pl.sim(dt);
            }

            int maxpop = Civilisation.getMaxPopulationPerCiv();
            foreach (Civilisation civ in Civilisation.data)
            {
                civ.maxPopulation = Math.Min(civ.maxPopulationNew, maxpop);
            }
        }

        private void sim(float dt)
        {
            float[] efficiency = new float[Civilisation.count];


            for (int i = 0; i < size; i++)
            {
                if (infra[i] != null)
                {
                    if (infra[i].owner == null)
                    {
                        infra[i].simulate(dt, 0.0f);
                    }
                    else
                    {
                        infra[i].simulate(dt, efficiency[infra[i].owner.getId()]);
                    }
                }


                // Natureffekte wechseln durch
                timer[i] -= dt * 0.01f;
                if (timer[i] <= 0.0f)
                {
                    timer[i] = (float) GameWindow.random.NextDouble() * 100.0f + 10.0f;
                    effect[i] = Effect.NONE;

                    if ((climate == Climate.RAINY && GameWindow.random.Next(20) == 0) ||
                        (climate == Climate.NORMAL && GameWindow.random.Next(40) == 0) ||
                        (climate == Climate.COLD && GameWindow.random.Next(10) == 0)) // Fängt an zu regnen
                    {
                        effect[i] = Effect.RAIN;
                    }
                    else if (terra[i] == Terrain.VOLCANO && GameWindow.random.Next(100) == 0) // Vulkanausbruch
                    {
                        effect[i] = Effect.VOLCANO_ERUPTION;
                    }
                }

                //Regnet es gerade?
                switch (effect[i])
                {
                    case Effect.RAIN:
                        particleEffectRain(i);

                        break;
                    case Effect.VOLCANO_ERUPTION:
                        particleEffectEruption(i);

                        break;
                }
            }
        }

        private void particleEffectEruption(int i)
        {
            float rot;
            float height;
            Particle p;
            rot = (float) Math.PI * 2.0f * (0.5f + (float) i) / (float) size;
            height = 21.5f * size;
            p = new Particle(x + (float) Math.Cos(rot) * height, y + (float) Math.Sin(rot) * height);

            height = (float) GameWindow.random.NextDouble() * 20.0f +
                     20.0f * (1.5f + (float) Math.Cos(timer[i] * 0.13f));
            if ((float) GameWindow.random.NextDouble() < Math.Min(0.5f, timer[i] * 0.1f)) // Lava
            {
                rot += (((float) GameWindow.random.NextDouble() - 0.5f) * 0.3f +
                        0.3f * (float) Math.Sin(timer[i] * 0.1f + (float) i)) * (float) Math.PI;
                p.setColor(0.8f, (float) GameWindow.random.NextDouble() * 0.7f, 0.0f, 0.2f);
                p.setSpeed((float) Math.Cos(rot) * height, (float) Math.Sin(rot) * height);
                p.setGravity(this);
            }
            else // Nur Qualm
            {
                rot += 0.75f * (((float) GameWindow.random.NextDouble() - 0.5f) * 0.7f +
                                0.4f * (float) Math.Sin(timer[i] * 0.1f + (float) i)) * (float) Math.PI;
                p.setColor(0.72f, 0.72f, 0.72f, 0.06f);
                p.setSpeed((float) Math.Cos(rot) * height, (float) Math.Sin(rot) * height);
            }
        }

        private void particleEffectRain(int i)
        {
            if ((float) GameWindow.random.NextDouble() < Math.Min(0.5f, timer[i] * 0.1f))
            {
                float subfield = (float) GameWindow.random.NextDouble();
                var height = 20.0f * size + 100.0f + 30.0f * ((float) GameWindow.random.NextDouble() - 0.5f) *
                             (1.0f - 4.0f * (subfield - 0.5f) * (subfield - 0.5f));
                if (climate == Climate.COLD)
                {
                    height = 20.0f * size + 10.0f + 50.0f * ((float) GameWindow.random.NextDouble()) *
                             (2.0f - 4.0f * (subfield - 0.5f) * (subfield - 0.5f));
                    subfield *= 4.0f - 1.5f;
                }
                else
                {
                    subfield *= 2.0f - 0.5f;
                }

                var rot = (float) Math.PI * 2.0f * (subfield + i) / size;
                var particle = new Particle(x + (float) Math.Cos(rot) * height, y + (float) Math.Sin(rot) * height);
                particle.setSpeed(-(float) Math.Sin(rot) * (float) Math.Sin(timer[i] * 0.1f + i) * 30.0f,
                    (float) Math.Cos(rot) * (float) Math.Sin(timer[i] * 0.1f + i) * 30.0f);
                if (climate == Climate.COLD)
                {
                    particle.setColor(1.0f, 1.0f, 1.0f, 0.05f);
                }
                else
                {
                    particle.setColor(0.7f, 0.7f, 1.0f, 0.1f);
                    particle.setGravity(this);
                }
            }
        }
    }
}