using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.CelestialBodies;
using OpenTK.Graphics.OpenGL;

namespace Cluster.GameMechanics.Universe.LivingThings
{
    class Shot
    {
        const int FIGHT_UNITS = 0,
            FIGHT_BUILDINGS = 1;


        const int MAX_SHOT_RENDERINGS = 1000;
        const int RENDER_ARRSIZE_POS = 4;
        const int RENDER_ARRSIZE_COL = 3;
        const int RENDER_ARRSIZE_ALPHAS = 2;
        public static int render_shot_count = 0;
        public static int countshots = 0;
        static int shot_gl_data, buf_sh0, buf_sh1, buf_sh2;

        static float[] render_buf_pos = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_POS];
        static float[] render_buf_col = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_COL];
        static float[] render_buf_alphas = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_ALPHAS];


        public static List<Shot>[] list = {new List<Shot>(), new List<Shot>()};
        public static List<Shot> removed = new List<Shot>();

        byte against;

        byte weapon_type;
        Civilisation owner;
        Unit from;
        Unit aim_unit;

        Planet aim_planet;
        //Asteroid aim_asteroid;

        public float x, y, v, rot; // Position, Geschwindigkeit, Richtung
        float damage; // Stärke des Schadens
        float lifespan; // Wie lange hält der Schuss noch durch
        float r, g, b, a; // Farbe
        float length; // Länge der Linie


        public Shot(Unit from, Unit aim = null, byte inrange = 1)
        {
            r = g = b = a = 1.0f;
            length = 13.5f;
            against = FIGHT_UNITS;

            this.from = from;
            owner = from.getOwner();
            aim_unit = aim;
            weapon_type = from.getPrototype().weapon_type;
            x = (float) from.x;
            y = (float) from.y;
            v = 100.0f;
            rot = (float) from.rot;
            damage = from.getPrototype().getAttack(owner);
            lifespan = 10.0f;

            if (weapon_type == Prototype.WEAPON_PERSECUTE_AND_LASER)
            {
                r = 0.25f;
                g = 1.0f;
                b = 0.4f;
                length = 5.0f;
                if (inrange == 2) return; // Zu nahe dran --> zurück ohne hinzufügen des Schusses.
                else if (inrange == 1) weapon_type = Prototype.WEAPON_LASER; // Laserwaffe für Nahkampf
                else weapon_type = Prototype.WEAPON_PERSECUTE; // Lenkraketen für Fernkampf (oder anders herum?)
            }

            if (weapon_type == Prototype.WEAPON_LASER)
            {
                g = b = 0.0f;
                a = 0.35f;
                lifespan = 2.0f;
                damage /= lifespan;
                x = y = 0.0f; // Wird bei Lasern für relativen Offset zum Zeichnen benutzt.
            }


            list[against].Add(this);
        }

        public Shot(Shot from)
        {
            r = a = 1.0f;
            g = (float) GameWindow.random.NextDouble();
            b = 0.0f;
            length = 4.5f;
            against = FIGHT_UNITS;

            this.from = from.from;
            owner = from.owner;
            aim_unit = null;
            weapon_type = Prototype.WEAPON_STD;
            x = (float) from.x;
            y = (float) from.y;
            v = g * 50.0f + 30.0f;
            rot = (float) (GameWindow.random.NextDouble() * 2.0f * Math.PI);
            damage = from.damage + g * 10.0f;
            lifespan = 10.0f;

            list[against].Add(this);
        }

        public Shot(Unit from, Planet pl, int index)
        {
            this.from = from;
            owner = from.getOwner();
            damage = from.getPrototype().getAttack(owner);
            lifespan = 10.0f;

            r = owner.red * 0.25f + 0.75f;
            g = owner.green * 0.25f + 0.75f;
            b = owner.blue * 0.25f + 0.75f;
            a = 0.5f;
            length = 25.0f;
            against = FIGHT_BUILDINGS;

            aim_planet = pl;


            x = (float) from.x;
            y = (float) from.y;
            v = 75.0f;
            rot = (float) Math.Atan2(
                pl.y + (double) pl.size * 20.0 * Math.Sin(((double) index + 0.5) / (double) pl.size * 2.0 * Math.PI) -
                y,
                pl.x + (double) pl.size * 20.0 * Math.Cos(((double) index + 0.5) / (double) pl.size * 2.0 * Math.PI) -
                x);

            list[against].Add(this);
        }


        static public void update(float t = 1.0f)
        {
            countshots = 0;
            foreach (Shot s in list[FIGHT_UNITS])
            {
                s.sim_units(t);
                countshots++;
            }

            foreach (Shot s in list[FIGHT_BUILDINGS])
            {
                s.sim_buildings(t);
                countshots++;
            }

            // Entferne Schüsse, die nicht mehr gebraucht werden.
            foreach (Shot s in removed)
            {
                list[s.against].Remove(s);
            }

            removed.Clear();
        }


        void sim_always(float t)
        {
            if (weapon_type != Prototype.WEAPON_LASER)
            {
                x += v * (float) Math.Cos(rot) * t * 0.002f;
                y += v * (float) Math.Sin(rot) * t * 0.002f;
            }

            lifespan -= t * 0.002f;

            if (lifespan <= 0.0)
            {
                removed.Add(this);
                return;
            }
        }

        void sim_units(float t)
        {
            // Kollisionscheck, Lenkbewegungen etc.
            if (weapon_type != Prototype.WEAPON_LASER)
            {
                float neardist = 1000.0f;
                Sector s = Sector.get(x, y);
                for (int i = 0; i < Civilisation.count + 1; i++)
                {
                    if (i == owner.getId()) continue;
                    foreach (Unit u in s.ships[i])
                    {
                        float dst = (float) Math.Sqrt((u.x - x) * (u.x - x) + (u.y - y) * (u.y - y));
                        if (dst < 5.0f * u.getPrototype().shape_scaling * u.getPrototype().shape.radius)
                        {
                            u.damage(damage);
                            removed.Add(this);
                            return;
                        }
                        else if (weapon_type == Prototype.WEAPON_FIND_AIM && dst < neardist)
                        {
                            neardist = dst;
                            aim_unit = u;
                        }
                        else if (weapon_type == Prototype.WEAPON_EXPLOSIVE && dst <
                                 30.0f * u.getPrototype().shape_scaling * u.getPrototype().shape.radius + 150.0f)
                        {
                            for (int j = 0; j < 15 + owner.getMultiplicator(Civilisation.BONUS_WEAPONS_EXPLOSIVE); j++)
                            {
                                new Shot(this);
                            }

                            removed.Add(this);
                            return;
                        }
                    }
                }

                if (weapon_type == Prototype.WEAPON_FIND_AIM || weapon_type == Prototype.WEAPON_PERSECUTE)
                {
                    TurnTowards(aim_unit.x - x, aim_unit.y - y, t * 0.002);
                }
            }
            else // Laserwaffen anders handeln.
            {
                float mult = 1.0f;
                if (from != null && aim_unit.getPrototype().goodAgainst() == from.getPrototype().ship_class)
                    mult = 0.15f;
                aim_unit.damage(damage * mult);
                damage *= 0.95f;
                if (aim_unit.isDead()) lifespan -= t * 0.005f;
            }

            // Allgemeine Bewegungen
            sim_always(t);
        }

        void sim_buildings(float t)
        {
            // Kollisionscheck etc.
            float dx = x - aim_planet.x, dy = y - aim_planet.y;
            float dst = (float) Math.Sqrt(dx * dx + dy * dy);
            if (dst < aim_planet.size * 20.0f)
            {
                removed.Add(this);
                int feld = (int) (((2.0 * Math.PI + Math.Atan2(dy, dx)) % (2.0 * Math.PI)) *
                                  ((float) aim_planet.size / (2.0 * Math.PI)));
                if (aim_planet.infra[feld] != null && aim_planet.infra[feld].owner != owner)
                {
                    if (aim_planet.terra[feld] == Planet.Terrain.MOUNTAIN)
                    {
                        damage *= 0.5f;
                    }
                    else if (aim_planet.terra[feld] == Planet.Terrain.WATER)
                    {
                        damage *= 0.33f;
                    }

                    if (aim_planet.effect[feld] == Planet.Effect.RAIN)
                    {
                        damage *= 0.5f;
                    } // Niederschlag/Wolken/Schneesturm reduziert Zielgenauigkeit und Schaden um 50%

                    aim_planet.infra[feld].damage(damage);
                    return;
                }
            }

            // Allgemeine Bewegungen
            sim_always(t);
        }


        public static void Render()
        {
            render_shot_count = 0;
            /*float x0 = Space.screenToSpaceX(0), y0 = Space.screenToSpaceY(0);
            float x1 = Space.screenToSpaceX(GameWindow.active.width), y1 = Space.screenToSpaceY(GameWindow.active.height);
            
            float cx = (x0+x1)*0.5f, cy = (y0+y1)*0.5f;
            float dx = (x1-x0)*0.5f, dy = (y1-y0)*0.5f;*/

            foreach (List<Shot> slist in Shot.list)
            {
                foreach (Shot s in slist)
                {
                    if (true) //Space.isVisible(s.x, s.y))//Math.Abs(s.x - cx) <= dx && Math.Abs(s.y - cy) <= dy)
                    {
                        render_buf_col[render_shot_count * RENDER_ARRSIZE_COL + 0] = s.r;
                        render_buf_col[render_shot_count * RENDER_ARRSIZE_COL + 1] = s.g;
                        render_buf_col[render_shot_count * RENDER_ARRSIZE_COL + 2] = s.b;

                        if (s.weapon_type == Prototype.WEAPON_LASER)
                        {
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 0] =
                                s.from.x + (float) Math.Cos(s.from.rot) * s.x - (float) Math.Sin(s.from.rot) * s.y;
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 1] =
                                s.from.y + (float) Math.Sin(s.from.rot) * s.x + (float) Math.Cos(s.from.rot) * s.y;
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 2] = s.aim_unit.x;
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 3] = s.aim_unit.y;
                            render_buf_alphas[render_shot_count * RENDER_ARRSIZE_ALPHAS + 0] =
                                s.a * Math.Min(1.0f, s.lifespan);
                            render_buf_alphas[render_shot_count * RENDER_ARRSIZE_ALPHAS + 1] =
                                s.a * Math.Min(1.0f, s.lifespan);
                        }
                        else
                        {
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 0] = s.x;
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 1] = s.y;
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 2] =
                                s.x - (float) Math.Cos(s.rot) * s.length;
                            render_buf_pos[render_shot_count * RENDER_ARRSIZE_POS + 3] =
                                s.y - (float) Math.Sin(s.rot) * s.length;
                            render_buf_alphas[render_shot_count * RENDER_ARRSIZE_ALPHAS + 0] = 0.0f;
                            render_buf_alphas[render_shot_count * RENDER_ARRSIZE_ALPHAS + 1] =
                                s.a * Math.Min(1.0f, s.lifespan);
                        }

                        render_shot_count++;
                        if (render_shot_count >= MAX_SHOT_RENDERINGS)
                        {
                            render_part2();
                            return;
                        }
                    }
                }
            }

            if (render_shot_count > 0)
            {
                render_part2();
            }
        }

        static void render_part2()
        {
            GL.LineWidth(2.0f);
            GL.Disable(EnableCap.DepthTest);

            if (shot_gl_data == 0)
            {
                shot_gl_data = GL.GenVertexArray();
                buf_sh0 = GL.GenBuffer();
                buf_sh1 = GL.GenBuffer();
                buf_sh2 = GL.GenBuffer();
            }

            GL.BindVertexArray(shot_gl_data);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh0);
            GL.BufferData(BufferTarget.ArrayBuffer, (render_shot_count + 1) * RENDER_ARRSIZE_POS * sizeof(float),
                render_buf_pos, BufferUsageHint.DynamicDraw);
            GL.EnableVertexArrayAttrib(shot_gl_data, 0);
            GL.VertexAttribPointer(0, RENDER_ARRSIZE_POS, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh1);
            GL.BufferData(BufferTarget.ArrayBuffer, (render_shot_count + 1) * RENDER_ARRSIZE_COL * sizeof(float),
                render_buf_col, BufferUsageHint.DynamicDraw);
            GL.EnableVertexArrayAttrib(shot_gl_data, 1);
            GL.VertexAttribPointer(1, RENDER_ARRSIZE_COL, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh2);
            GL.BufferData(BufferTarget.ArrayBuffer, (render_shot_count + 1) * RENDER_ARRSIZE_ALPHAS * sizeof(float),
                render_buf_alphas, BufferUsageHint.DynamicDraw);
            GL.EnableVertexArrayAttrib(shot_gl_data, 2);
            GL.VertexAttribPointer(2, RENDER_ARRSIZE_ALPHAS, VertexAttribPointerType.Float, false, 0, 0);


            Space.shotShader.bind();
            GL.Uniform3(Space.shotShader.getUniformLocation("viewport"), GameWindow.active.mult_x,
                GameWindow.active.mult_y, Space.animation);
            GL.Uniform3(Space.shotShader.getUniformLocation("scroll"), Space.scrollX, Space.scrollY, Space.zoom);
            GL.DrawArrays(PrimitiveType.Points, 0, render_shot_count);


            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }


        public byte GetWeaponType()
        {
            return weapon_type;
        }


        double TurnTowards(float deltaX, float deltaY, double maxAngle = Math.PI)
        {
            rot = (rot + (float) Math.PI * 2.0f) % ((float) Math.PI * 2.0f);
            double phi = (Math.Atan2(deltaY, deltaX) + Math.PI * 2.0 - rot) % (Math.PI * 2.0f);
            if (phi > Math.PI) phi -= Math.PI * 2.0;
            phi = Math.Max(-maxAngle, Math.Min(maxAngle, phi));

            rot += (float) phi;
            return phi;
        }
    }
}