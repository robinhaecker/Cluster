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
        private enum FiredAgainst
        {
            UNITS,
            BUILDINGS
        }

        private const int MAX_SHOT_RENDERINGS = 1000;
        private const int RENDER_ARRSIZE_POS = 4;
        private const int RENDER_ARRSIZE_COL = 3;
        private const int RENDER_ARRSIZE_ALPHAS = 2;

        private static int renderShotCount;
        private static int countshots;
        static int shotGlData;
        static int bufSh0;
        static int bufSh1;
        static int bufSh2;

        private static readonly float[] renderBufPos = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_POS];
        private static readonly float[] renderBufCol = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_COL];
        private static readonly float[] renderBufAlphas = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_ALPHAS];

        public static readonly List<Shot>[] list = {new List<Shot>(), new List<Shot>()};
        public static readonly List<Shot> removed = new List<Shot>();

        private readonly FiredAgainst against;

        private readonly Prototype.WeaponType weaponType;
        readonly Civilisation owner;
        private readonly Unit @from;
        Unit aimUnit;

        readonly Planet aimPlanet;
        //Asteroid aim_asteroid;

        public float x, y, v, rot; // Position, Geschwindigkeit, Richtung
        float damage; // Stärke des Schadens
        float lifespan; // Wie lange hält der Schuss noch durch
        private readonly float red;
        private readonly float green;
        private readonly float blue;
        private readonly float alpha;
        private readonly float length;

        public Shot(Building from)
        {
            red = from.owner.red;
            green = from.owner.green;
            blue = from.owner.blue;
            length = 108.0f;
            against = FiredAgainst.UNITS;
            weaponType = Prototype.WeaponType.FIND_AIM;
            this.@from = null;
            owner = from.owner;
            v = 100.0f;
            rot = @from.getSpotRotation();
            x = from.getPlanet().x + (float)Math.Cos(rot) * from.getPlanet().size * 20.0f;
            y = from.getPlanet().y + (float)Math.Sin(rot) * from.getPlanet().size * 20.0f;
        }
        
        public Shot(Unit from, Unit aim = null, byte inrange = 1)
        {
            red = green = blue = alpha = 1.0f;
            length = 13.5f;
            against = FiredAgainst.UNITS;

            this.@from = from;
            owner = from.getOwner();
            aimUnit = aim;
            weaponType = from.getPrototype().weaponType;
            x = from.x;
            y = from.y;
            v = 100.0f;
            rot = from.rot;
            damage = from.getPrototype().getAttack(owner);
            lifespan = 10.0f;

            if (weaponType == Prototype.WeaponType.PERSECUTE_AND_LASER)
            {
                red = 0.25f;
                green = 1.0f;
                blue = 0.4f;
                length = 5.0f;
                if (inrange == 2) return; // Zu nahe dran --> zurück ohne hinzufügen des Schusses.
                else if (inrange == 1) weaponType = Prototype.WeaponType.LASER; // Laserwaffe für Nahkampf
                else weaponType = Prototype.WeaponType.PERSECUTE; // Lenkraketen für Fernkampf (oder anders herum?)
            }

            if (weaponType == Prototype.WeaponType.LASER)
            {
                green = blue = 0.0f;
                alpha = 0.35f;
                lifespan = 2.0f;
                damage /= lifespan;
                x = y = 0.0f; // Wird bei Lasern für relativen Offset zum Zeichnen benutzt.
            }


            list[(int) against].Add(this);
        }

        public Shot(Shot from)
        {
            red = alpha = 1.0f;
            green = (float) GameWindow.random.NextDouble();
            blue = 0.0f;
            length = 4.5f;
            against = FiredAgainst.UNITS;

            this.@from = from.@from;
            owner = from.owner;
            aimUnit = null;
            weaponType = Prototype.WeaponType.STD;
            x = (float) from.x;
            y = (float) from.y;
            v = green * 50.0f + 30.0f;
            rot = (float) (GameWindow.random.NextDouble() * 2.0f * Math.PI);
            damage = from.damage + green * 10.0f;
            lifespan = 10.0f;

            list[(int) against].Add(this);
        }

        public Shot(Unit from, Planet pl, int index)
        {
            this.@from = from;
            owner = from.getOwner();
            damage = from.getPrototype().getAttack(owner);
            lifespan = 10.0f;

            red = owner.red * 0.25f + 0.75f;
            green = owner.green * 0.25f + 0.75f;
            blue = owner.blue * 0.25f + 0.75f;
            alpha = 0.75f;
            length = 25.0f;
            against = FiredAgainst.BUILDINGS;

            aimPlanet = pl;


            x = (float) from.x;
            y = (float) from.y;
            v = 75.0f;
            rot = (float) Math.Atan2(
                pl.y + (double) pl.size * 20.0 * Math.Sin(((double) index + 0.5) / (double) pl.size * 2.0 * Math.PI) -
                y,
                pl.x + (double) pl.size * 20.0 * Math.Cos(((double) index + 0.5) / (double) pl.size * 2.0 * Math.PI) -
                x);

            list[(int) against].Add(this);
        }


        public static void update(float t = 1.0f)
        {
            countshots = 0;
            foreach (Shot s in list[(int) FiredAgainst.UNITS])
            {
                s.simAgainstUnits(t);
                countshots++;
            }

            foreach (Shot s in list[(int) FiredAgainst.BUILDINGS])
            {
                s.simAgainstBuildings(t);
                countshots++;
            }

            // Entferne Schüsse, die nicht mehr gebraucht werden.
            foreach (Shot s in removed)
            {
                list[(int) s.against].Remove(s);
            }

            removed.Clear();
        }


        private void simAlways(float t)
        {
            if (weaponType != Prototype.WeaponType.LASER)
            {
                x += v * (float) Math.Cos(rot) * t * 0.002f;
                y += v * (float) Math.Sin(rot) * t * 0.002f;
            }

            lifespan -= t * 0.002f;

            if (lifespan <= 0.0)
            {
                removed.Add(this);
            }
        }

        private void simAgainstUnits(float t)
        {
            // Kollisionscheck, Lenkbewegungen etc.
            if (weaponType != Prototype.WeaponType.LASER)
            {
                float neardist = 1000.0f;
                Sector s = Sector.get(x, y);
                for (int i = 0; i < Civilisation.count + 1; i++)
                {
                    if (i == owner.getId()) continue;
                    foreach (Unit u in s.ships[i])
                    {
                        float dst = (float) Math.Sqrt((u.x - x) * (u.x - x) + (u.y - y) * (u.y - y));
                        if (dst < 5.0f * u.getPrototype().shapeScaling * u.getPrototype().shape.radius)
                        {
                            u.damage(damage);
                            removed.Add(this);
                            return;
                        }
                        else if (weaponType == Prototype.WeaponType.FIND_AIM && dst < neardist)
                        {
                            neardist = dst;
                            aimUnit = u;
                        }
                        else if (weaponType == Prototype.WeaponType.EXPLOSIVE && dst <
                                 30.0f * u.getPrototype().shapeScaling * u.getPrototype().shape.radius + 150.0f)
                        {
                            for (int j = 0; j < 15 + owner.getMultiplicator(Civilisation.BONUS_WEAPONS_EXPLOSIVE); j++)
                            {
                                // ReSharper disable once ObjectCreationAsStatement -> Wird direkt in Liste gespeichert (Konstruktor)
                                new Shot(this);
                            }

                            removed.Add(this);
                            return;
                        }
                    }
                }

                if (weaponType == Prototype.WeaponType.FIND_AIM || weaponType == Prototype.WeaponType.PERSECUTE)
                {
                    turnTowards(aimUnit.x - x, aimUnit.y - y, t * 0.002);
                }
            }
            else // Laserwaffen anders handeln.
            {
                float mult = 1.0f;
                if (@from != null && aimUnit.getPrototype().goodAgainst() == @from.getPrototype().shipClass)
                    mult = 0.15f;
                aimUnit.damage(damage * mult);
                damage *= 0.95f;
                if (aimUnit.isDead()) lifespan -= t * 0.005f;
            }

            // Allgemeine Bewegungen
            simAlways(t);
        }

        private void simAgainstBuildings(float t)
        {
            // Kollisionscheck etc.
            float dx = x - aimPlanet.x, dy = y - aimPlanet.y;
            float dst = (float) Math.Sqrt(dx * dx + dy * dy);
            if (dst < aimPlanet.size * 20.0f)
            {
                removed.Add(this);
                int feld = (int) (((2.0 * Math.PI + Math.Atan2(dy, dx)) % (2.0 * Math.PI)) *
                                  (aimPlanet.size / (2.0 * Math.PI)));
                if (aimPlanet.infra[feld] != null && aimPlanet.infra[feld].owner != owner)
                {
                    if (aimPlanet.terra[feld] == Planet.Terrain.MOUNTAIN)
                    {
                        damage *= 0.5f;
                    }
                    else if (aimPlanet.terra[feld] == Planet.Terrain.WATER)
                    {
                        damage *= 0.33f;
                    }

                    if (aimPlanet.effect[feld] == Planet.Effect.RAIN)
                    {
                        damage *= 0.5f;
                    } // Niederschlag/Wolken/Schneesturm reduziert Zielgenauigkeit und Schaden um 50%

                    aimPlanet.infra[feld].damage(damage);
                    return;
                }
            }

            // Allgemeine Bewegungen
            simAlways(t);
        }


        public static void render()
        {
            renderShotCount = 0;
            /*float x0 = Space.screenToSpaceX(0), y0 = Space.screenToSpaceY(0);
            float x1 = Space.screenToSpaceX(GameWindow.active.width), y1 = Space.screenToSpaceY(GameWindow.active.height);
            
            float cx = (x0+x1)*0.5f, cy = (y0+y1)*0.5f;
            float dx = (x1-x0)*0.5f, dy = (y1-y0)*0.5f;*/

            foreach (List<Shot> slist in list)
            {
                foreach (Shot s in slist)
                {
                    if (true) //Space.isVisible(s.x, s.y))//Math.Abs(s.x - cx) <= dx && Math.Abs(s.y - cy) <= dy)
                    {
                        renderBufCol[renderShotCount * RENDER_ARRSIZE_COL + 0] = s.red;
                        renderBufCol[renderShotCount * RENDER_ARRSIZE_COL + 1] = s.green;
                        renderBufCol[renderShotCount * RENDER_ARRSIZE_COL + 2] = s.blue;

                        if (s.weaponType == Prototype.WeaponType.LASER)
                        {
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 0] =
                                s.@from.x + (float) Math.Cos(s.@from.rot) * s.x - (float) Math.Sin(s.@from.rot) * s.y;
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 1] =
                                s.@from.y + (float) Math.Sin(s.@from.rot) * s.x + (float) Math.Cos(s.@from.rot) * s.y;
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 2] = s.aimUnit.x;
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 3] = s.aimUnit.y;
                            renderBufAlphas[renderShotCount * RENDER_ARRSIZE_ALPHAS + 0] =
                                s.alpha * Math.Min(1.0f, s.lifespan);
                            renderBufAlphas[renderShotCount * RENDER_ARRSIZE_ALPHAS + 1] =
                                s.alpha * Math.Min(1.0f, s.lifespan);
                        }
                        else
                        {
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 0] = s.x;
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 1] = s.y;
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 2] =
                                s.x - (float) Math.Cos(s.rot) * s.length;
                            renderBufPos[renderShotCount * RENDER_ARRSIZE_POS + 3] =
                                s.y - (float) Math.Sin(s.rot) * s.length;
                            renderBufAlphas[renderShotCount * RENDER_ARRSIZE_ALPHAS + 0] =
                                s.alpha * Math.Min(1.0f, s.lifespan) * 0.25f;
                            renderBufAlphas[renderShotCount * RENDER_ARRSIZE_ALPHAS + 1] =
                                s.alpha * Math.Min(1.0f, s.lifespan);
                        }

                        renderShotCount++;
                        if (renderShotCount >= MAX_SHOT_RENDERINGS)
                        {
                            render_part2();
                            return;
                        }
                    }
                }
            }

            if (renderShotCount > 0)
            {
                render_part2();
            }
        }

        static void render_part2()
        {
            GL.LineWidth(3.0f);
            GL.Disable(EnableCap.DepthTest);

            if (shotGlData == 0)
            {
                shotGlData = GL.GenVertexArray();
                bufSh0 = GL.GenBuffer();
                bufSh1 = GL.GenBuffer();
                bufSh2 = GL.GenBuffer();
            }

            GL.BindVertexArray(shotGlData);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufSh0);
            GL.BufferData(BufferTarget.ArrayBuffer, (renderShotCount + 1) * RENDER_ARRSIZE_POS * sizeof(float),
                renderBufPos, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, RENDER_ARRSIZE_POS, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufSh1);
            GL.BufferData(BufferTarget.ArrayBuffer, (renderShotCount + 1) * RENDER_ARRSIZE_COL * sizeof(float),
                renderBufCol, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, RENDER_ARRSIZE_COL, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufSh2);
            GL.BufferData(BufferTarget.ArrayBuffer, (renderShotCount + 1) * RENDER_ARRSIZE_ALPHAS * sizeof(float),
                renderBufAlphas, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, RENDER_ARRSIZE_ALPHAS, VertexAttribPointerType.Float, false, 0, 0);


            Space.shotShader.bind();
            GL.Uniform3(Space.shotShader.getUniformLocation("viewport"), GameWindow.active.multX,
                GameWindow.active.multY, Space.animation);
            GL.Uniform3(Space.shotShader.getUniformLocation("scroll"), Space.scrollX, Space.scrollY, Space.zoom);
            GL.DrawArrays(PrimitiveType.Points, 0, renderShotCount);


            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }


        public Prototype.WeaponType getWeaponType()
        {
            return weaponType;
        }


        double turnTowards(float deltaX, float deltaY, double maxAngle = Math.PI)
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