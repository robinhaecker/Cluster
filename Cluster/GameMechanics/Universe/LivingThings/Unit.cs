using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace Cluster.GameMechanics.Universe.LivingThings
{
    class Unit
    {
        public static readonly List<Unit> units = new List<Unit>();
        public static readonly List<Unit> removed = new List<Unit>();
        static int idCounter;

        public enum Effect
        {
            NONE,
            SPAWNING,
            EXPLODE,
            CAMOUFLAGE,
            STUNNED
        }

        private static int displayShieldCount;
        private const int DISPLAY_SHIELD_VARIABLES_XYSCALE = 3;
        private const int DISPLAY_SHIELD_VARIABLES_PERCENTAGES = 2;
        private const int DISPLAY_SHIELD_VARIABLES_COLOR = 4;

        private static readonly float[] shieldGlArray0 =
            new float[DISPLAY_SHIELD_VARIABLES_XYSCALE * Civilisation.TOTAL_MAX_POPULATION];

        private static readonly float[] shieldGlArray1 =
            new float[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * Civilisation.TOTAL_MAX_POPULATION];

        private static readonly float[] shieldGlArray2 =
            new float[DISPLAY_SHIELD_VARIABLES_COLOR * Civilisation.TOTAL_MAX_POPULATION];

        private static int shieldGlData;
        private static int bufSh0;
        private static int bufSh1;
        private static int bufSh2;


        // Instance-specific variables
        int id;
        readonly Prototype prototype;
        readonly Civilisation owner;

        public float x, y, rot, v;
        float health;
        float shield;

        public bool isSelected;
        float reloadTimer;
        float gotHitTimer;
        float currentEffectTimer;
        Effect currentEffect;

        Sector sector;
        readonly Target target;
        public Unit enemy;
        public byte inrange;
        public float enemyDistance;


        // Constructors
        public Unit(Prototype p, Civilisation civ, float x, float y, float alpha = 0.0f)
        {
            id = idCounter;
            idCounter++;

            prototype = p;
            owner = civ;
            this.x = x;
            this.y = y;
            rot = alpha;
            v = 0.0f;

            health = prototype.getHealth(civ);
            shield = prototype.getShields(civ);

            reloadTimer = 0.0f;
            gotHitTimer = 0.0f;
            currentEffectTimer = 1.0f;
            currentEffect = Effect.SPAWNING;
            target = new Target();
            target.update(x, y);

            units.Add(this);
        }

        public Unit(Prototype p, Building b)
        {
            id = idCounter;
            idCounter++;

            Planet pl = b.getPlanet();

            prototype = p;
            owner = b.owner;
            rot = b.getSpotRotation();
            x = pl.x + (float) Math.Cos(rot) * (pl.size * 20.0f + 75.0f);
            y = pl.y + (float) Math.Sin(rot) * (pl.size * 20.0f + 75.0f);
            v = 0.0f;

            reloadTimer = 0.0f;
            gotHitTimer = 0.0f;
            currentEffectTimer = 1.0f;
            currentEffect = Effect.SPAWNING;
            target = new Target();
            target.update(pl, -1, b.getSpotId());

            health = prototype.getHealth(b.owner);
            shield = prototype.getShields(b.owner);

            units.Add(this);
        }


        public static void render()
        {
            displayShieldCount = 0;

            GL.LineWidth(1.5f);
            GL.Disable(EnableCap.DepthTest);
            //Shader shader = Mesh.getShader();
            Space.unitShader.bind();
            foreach (Unit u in Unit.units)
            {
                var alpha = 1.0f;
                var scale = u.prototype.shapeScaling * 5.0f;
                switch (u.currentEffect)
                {
                    case Effect.SPAWNING:
                        scale *= Math.Min(1.0f, Math.Max(0.0f, 1.0f - u.currentEffectTimer));
                        break;
                    case Effect.EXPLODE:
                        alpha = u.currentEffectTimer * 0.5f;
                        break;
                }

                GL.Uniform3(Space.unitShader.getUniformLocation("pos"), (float) (u.x - Space.scrollX),
                    (float) (u.y - Space.scrollY), (float) u.rot);
                GL.Uniform4(Space.unitShader.getUniformLocation("col"), (float) u.owner.red, (float) u.owner.green,
                    (float) u.owner.blue, alpha);
                GL.Uniform3(Space.unitShader.getUniformLocation("scale"), scale,
                    (float) (Space.zoom * GameWindow.active.multX), (float) (Space.zoom * GameWindow.active.multY));

                GL.BindVertexArray(u.prototype.shape.glData);
                GL.DrawArrays(PrimitiveType.Lines, 0, u.prototype.shape.numLines * 2);


                // Daten für Schuztschilddarstellung setzen.
                if (u.isAlive() && (u.gotHitTimer > 0.0f || u.isSelected))
                {
                    shieldGlArray0[DISPLAY_SHIELD_VARIABLES_XYSCALE * displayShieldCount + 0] = u.x;
                    shieldGlArray0[DISPLAY_SHIELD_VARIABLES_XYSCALE * displayShieldCount + 1] = u.y;
                    shieldGlArray0[DISPLAY_SHIELD_VARIABLES_XYSCALE * displayShieldCount + 2] = scale;

                    shieldGlArray1[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * displayShieldCount + 0] =
                        u.getHealthFraction();
                    shieldGlArray1[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * displayShieldCount + 1] =
                        u.getShieldFraction();

                    if (u.isSelected)
                    {
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 0] =
                            u.owner.red * 0.75f + 0.25f;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 1] =
                            u.owner.green * 0.75f + 0.25f;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 2] =
                            u.owner.blue * 0.75f + 0.25f;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 3] = 1.5f;
                    }
                    else
                    {
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 0] = u.owner.red;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 1] = u.owner.green;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 2] = u.owner.blue;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * displayShieldCount + 3] =
                            Math.Min(1.0f, u.gotHitTimer);
                    }

                    displayShieldCount++;
                }
            }

            // Schutzschilde zeichnen.
            if (displayShieldCount > 0)
            {
                if (shieldGlData == 0)
                {
                    shieldGlData = GL.GenVertexArray();
                    bufSh0 = GL.GenBuffer();
                    bufSh1 = GL.GenBuffer();
                    bufSh2 = GL.GenBuffer();
                }

                GL.BindVertexArray(shieldGlData);

                GL.BindBuffer(BufferTarget.ArrayBuffer, bufSh0);
                GL.BufferData(BufferTarget.ArrayBuffer, shieldGlArray0.Length * sizeof(float), shieldGlArray0,
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, DISPLAY_SHIELD_VARIABLES_XYSCALE, VertexAttribPointerType.Float, false, 0, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, bufSh1);
                GL.BufferData(BufferTarget.ArrayBuffer, shieldGlArray1.Length * sizeof(float), shieldGlArray1,
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, DISPLAY_SHIELD_VARIABLES_PERCENTAGES, VertexAttribPointerType.Float, false, 0,
                    0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, bufSh2);
                GL.BufferData(BufferTarget.ArrayBuffer, shieldGlArray2.Length * sizeof(float), shieldGlArray2,
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, DISPLAY_SHIELD_VARIABLES_COLOR, VertexAttribPointerType.Float, false, 0, 0);


                Space.unitShieldShader.bind();
                GL.Uniform3(Space.unitShieldShader.getUniformLocation("viewport"), GameWindow.active.multX,
                    GameWindow.active.multY, Space.animation);
                GL.Uniform3(Space.unitShieldShader.getUniformLocation("scroll"), Space.scrollX, Space.scrollY,
                    Space.zoom);
                GL.DrawArrays(PrimitiveType.Points, 0, displayShieldCount);
            }

            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }


        public static void update(float t = 1.0f)
        {
            Sector.findNearestEnemies();
            resetPopulationCounterForAllCivs();
            updateAllUnitsAndAddPopulationCounter(t);
            removeKilledUnits();
        }

        private static void updateAllUnitsAndAddPopulationCounter(float t)
        {
            foreach (Unit u in Unit.units)
            {
                u.owner.population += u.prototype.getPopulation();
                u.simulate(t);
            }
        }

        private static void resetPopulationCounterForAllCivs()
        {
            foreach (Civilisation civ in Civilisation.data)
            {
                civ.population = 0;
            }
        }

        private static void removeKilledUnits()
        {
            foreach (Unit u in Unit.removed)
            {
                u.sector.removeUnit(u);
                units.Remove(u);
            }

            removed.Clear();
        }

        private void simulate(float t)
        {
            updateSector();
            if (health > 0)
            {
                simAlive(t);
            }
            else
            {
                simDeath(t);
            }

            simHitTimer(t);
            simMovement(t);
            simEffects(t);
        }

        private void simHitTimer(float t)
        {
            gotHitTimer = Math.Max(0.0f, gotHitTimer - t * 0.001f);
        }

        private void simMovement(float t)
        {
            x += v * (float) Math.Cos(rot) * t * 0.002f;
            y += v * (float) Math.Sin(rot) * t * 0.002f;
        }

        private void simEffects(float t)
        {
            if (currentEffect != Effect.NONE)
            {
                currentEffectTimer -= t * 0.001f;
                if (currentEffectTimer <= 0.0f)
                {
                    currentEffectTimer = 0.0f;
                    if (currentEffect == Effect.EXPLODE) // Wenn fertig explodiert, dann Einheit aus Liste löschen.
                    {
                        remove();
                        return;
                    }

                    currentEffect = Effect.NONE;
                }
            }
        }

        void remove()
        {
            removed.Add(this);
        }

        void simAlive(float t)
        {
            rechargeShield(t);

            var deltaMission = target.getPosition() - new Vec2(x, y);
            var delta = getDirectionToNextWaypoint();

            simMoveAndShoot(t, delta, deltaMission);

            if (GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.D)) damage(t * 0.1f); // Nur zu Testzwecken
        }

        private Vec2 getDirectionToNextWaypoint()
        {
            var delta = target.getWaypoint() - new Vec2(x, y);
            if (delta.length() < 50.0)
            {
                delta = target.nextWaypoint() - new Vec2(x, y);
            }

            return delta;
        }

        private void simMoveAndShoot(float t, Vec2 delta, Vec2 deltaMission)
        {
            if (currentEffect == Effect.SPAWNING || currentEffect == Effect.STUNNED)
            {
                return;
            }

            var rotationSpeed = simAttackEnemy(t, delta);
            simAccellerate(t, rotationSpeed, deltaMission.length());
        }

        private void rechargeShield(float t)
        {
            var maxshield = prototype.getShields(owner);
            shield = Math.Min(shield + t * 0.002f * (float) Math.Sqrt(maxshield), maxshield);
            reloadTimer = Math.Max(reloadTimer - t * 0.005f, 0.0f);
        }

        private double simAttackEnemy(float t, Vec2 delta)
        {
            double rotationSpeed;
            if (enemy != null)
            {
                bool aimtowards = false;
                if ((prototype.weaponType == Prototype.WeaponType.LASER &&
                     (enemyDistance < prototype.weaponRange)) ||
                    (prototype.weaponType != Prototype.WeaponType.STD &&
                     prototype.weaponType != Prototype.WeaponType.EXPLOSIVE &&
                     (enemyDistance < prototype.weaponRange) && prototype.shipClass != Prototype.Class.HUNTER))
                {
                    rotationSpeed = turnTowards(delta.x, delta.y, t * 0.001);
                }
                else
                {
                    rotationSpeed = turnTowards(enemy.x - x, enemy.y - y, t * 0.001);
                    if (Math.Abs(rotationSpeed) <= 0.000002 * t)
                    {
                        aimtowards = true;
                    }
                }

                if ((reloadTimer <= 0.0f) && (enemyDistance < prototype.weaponRange) &&
                    (aimtowards || (prototype.weaponType == Prototype.WeaponType.PERSECUTE ||
                                    prototype.weaponType == Prototype.WeaponType.LASER ||
                                    prototype.weaponType == Prototype.WeaponType.FIND_AIM)))
                {
                    fireWeapons();
                }
            }
            else
            {
                rotationSpeed = turnTowards(delta.x, delta.y, t * 0.001);
                bombardBuilding(); // Falls eins in der Nähe ist natürlich ;-)
            }

            return rotationSpeed;
        }

        void simAccellerate(float t, double rotationSpeed, float missionDistance)
        {
            float maxSpeed = prototype.getSpeed();
            if (missionDistance < 1000.0 && prototype.shipClass == Prototype.Class.HUNTER &&
                !(prototype.shipClass != Prototype.Class.WARSHIP && enemy != null) &&
                prototype.specials == Prototype.ShipAbility.ASTEROID_MINING)
            {
                //if (!orbit_enemy)v=Max(v-0.01*max_speed*t, max_speed*0.2); else
                v = Math.Max(v - 0.01f * maxSpeed * t, maxSpeed * 0.5f);
            }
            else if (Math.Abs(rotationSpeed) < 0.03) // Richtung stimmt gut? Beschleunigen.
            {
                v = Math.Min(v + 0.01f * maxSpeed * t, maxSpeed);
            }
            else if (Math.Abs(rotationSpeed) > 0.15) // Viel zu schlechte Richtung, Abbremsen
            {
                v = Math.Max(v - 0.01f * maxSpeed * t, maxSpeed * 0.1f);
            }
        }

        void simDeath(float t)
        {
            /*
            if (current_effect_timer > 0.97f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float speed = (float)GameWindow.random.NextDouble();
                    float angle = (float)GameWindow.random.NextDouble() * 2.0f * (float)Math.PI;
                    Particle p = new Particle(x, y, v * (float)Math.Cos(rot) + 100.0f * speed * (float)Math.Cos(angle), v * (float)Math.Sin(rot) + 100.0f * speed * (float)Math.Sin(angle));
                    p.setColor(1.0f, (float)GameWindow.random.NextDouble() * 0.7f + speed * 0.3f, speed * 0.25f, 0.1f);
                }
            }*/
        }


        public bool damage(float dmg)
        {
            if (health <= 0.0f) return false;
            gotHitTimer = 3.0f;

            if (shield > dmg)
            {
                shield -= dmg;
                return false;
            }

            dmg -= shield;
            shield = 0.0f;

            health -= dmg;
            if (health <= 0.0f)
            {
                health = 0.0f;
                currentEffect = Effect.EXPLODE;
                currentEffectTimer = 1.0f;

                for (int i = 0; i < 50 + (int) Math.Sqrt(prototype.healthMax / 50.0f); i++)
                {
                    float speed = (float) GameWindow.random.NextDouble();
                    float angle = (float) GameWindow.random.NextDouble() * 2.0f * (float) Math.PI;
                    Particle p = new Particle(x, y,
                        v * (float) Math.Cos(rot) + 100.0f * speed * (float) Math.Cos(angle),
                        v * (float) Math.Sin(rot) + 100.0f * speed * (float) Math.Sin(angle));
                    p.setColor(1.0f, (float) GameWindow.random.NextDouble() * 0.7f + speed * 0.3f, speed * 0.25f,
                        0.2f + (float) GameWindow.random.NextDouble() * 0.7f);
                }

                return true;
            }

            return false;
        }

        void fireWeapons()
        {
            reloadTimer = prototype.reloadTime;
            if (inrange > 0 || (prototype.weaponType != Prototype.WeaponType.LASER))
            {
                var shot = new Shot(this, enemy, inrange);
                if (prototype.shipClass == Prototype.Class.WARSHIP && inrange > 0)
                {
                    shot = new Shot(this, enemy, (byte) (2 - inrange));
                    shot.rot += (float) Math.PI * 0.25f;
                    if (shot.getWeaponType() == Prototype.WeaponType.LASER) shot.x -= 20.0f * prototype.shapeScaling;

                    shot = new Shot(this, enemy, (byte) (2 - inrange));
                    shot.rot -= (float) Math.PI * 0.25f;
                    if (shot.getWeaponType() == Prototype.WeaponType.LASER) shot.x += 20.0f * prototype.shapeScaling;
                }
            }

            /*
             * MISSING:
             *		If cl.specials=4 Then
                        Local spawn:Ship=Create(Class.data[10], owner, x, y)
                        spawn.rot=rot+45
                        spawn.v=spawn.max_speed/2
                        spawn.reload=10.0
                        spawn.dest_x = dest_x
                        spawn.dest_y = dest_y
                        spawn:Ship=Create(Class.data[10], owner, x, y)
                        spawn.rot=rot-45
                        spawn.v=spawn.max_speed/2
                        spawn.reload=10.0
                        spawn.dest_x = dest_x
                        spawn.dest_y = dest_y
                    ElseIf cl.specials=8 And owner.population<owner.max_population Then
                        Local spawn:Ship=Create(Class.data[11], owner, x, y)
                        spawn.rot=rot+45*(Rand(0,1)*2-1)
                        spawn.v=spawn.max_speed/2
                        spawn.reload=10.0
                        spawn.dest_x = dest_x+Rnd(-100,100)
                        spawn.dest_y = dest_y+Rnd(-100,100)
                        spawn.aim_p=aim_p
                        spawn.aimtype=aimtype
                        reload=cl.reloadtime*10.0
             * 
             * DONE:
                    ElseIf cl.weapontype<>WeaponType.BEAM Or inrange Then --> done
                        Shot.Create(Self, fe, False)
                        If cl.behave=Class.KRIEGSSCHIFF And inrange Then
                            Local ws:Shot=Shot.Create(Self, fe, 2-inrange)
                            If ws<>Null Then
                                ws.phi:+45
                                If ws.art=WeaponType.BEAM Then ws.x:+20.0*cl.scale
                            EndIf
                            ws=Shot.Create(Self, fe, 2-inrange)
                            If ws<>Null Then
                                ws.phi:-45
                                If ws.art=WeaponType.BEAM Then ws.x:-20.0*cl.scale
                            EndIf
                        EndIf
                    EndIf
             */
        }

        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")] // new Shot --> wird direkt zu Liste hinzugefügt.
        void bombardBuilding()
        {
            if (enemy != null)
            {
                return;
            }

            if (reloadTimer <= 0.0f && prototype.attack > 0.0f)
            {
                int feld;
                Planet bombard = planetInRange(out feld);
                if (bombard != null)
                {
                    new Shot(this, bombard, feld);
                    if (prototype.weaponType == Prototype.WeaponType.EXPLOSIVE)
                    {
                        new Shot(this, bombard, feld);
                        new Shot(this, bombard, feld);
                        new Shot(this, bombard, feld);
                    }

                    reloadTimer = prototype.reloadTime;
                }
            }
        }

        private Planet planetInRange(out int bombardIndex)
        {
            foreach (var planet in Planet.planets)
            {
                float dx = x - planet.x, dy = y - planet.y;
                float dist = (float) Math.Sqrt(dx * dx + dy * dy);
                if ((dist < 1000.0f) && (dist > 20.0f * planet.size + 150.0f))
                {
                    int feld = (int) (((2.0 * Math.PI + Math.Atan2(dy, dx)) % (2.0 * Math.PI)) *
                                      ((float) planet.size / (2.0 * Math.PI)));
                    int dF = Math.Min((int) Math.Floor((float) planet.size * 0.2f), 3);
                    for (int f = 0; f <= dF; f++)
                    {
                        int tmp = (planet.size + feld + f) % planet.size;
                        if (planet.infra[tmp] != null && planet.infra[tmp].owner != owner)
                        {
                            bombardIndex = tmp;
                            return planet;
                        }

                        if (f == 0)
                        {
                            continue;
                        }

                        tmp = (planet.size + feld - f) % planet.size;
                        if (planet.infra[tmp] != null && planet.infra[tmp].owner != owner)
                        {
                            bombardIndex = tmp;
                            return planet;
                        }
                    }
                }
            }

            bombardIndex = -1;
            return null;
        }


        public Prototype getPrototype()
        {
            return prototype;
        }

        public bool isDead()
        {
            return (health <= 0.0) || (currentEffect == Effect.EXPLODE);
        }

        public bool isAlive()
        {
            return !isDead();
        }

        public float getMaxHealth()
        {
            return prototype.healthMax;
        }

        public float getMaxShields()
        {
            return prototype.shields;
        }

        public Civilisation getOwner()
        {
            return owner;
        }

        private void updateSector()
        {
            if (sector != null)
            {
                if (sector.containsPoint(x, y)) return;
                sector.removeUnit(this);
                sector = null;
            }

            sector = Sector.get(x, y);
            sector.addUnit(this);
        }

        private double turnTowards(float deltaX, float deltaY, double maxAngle = Math.PI)
        {
            rot = (rot + (float) Math.PI * 2.0f) % ((float) Math.PI * 2.0f);
            var phi = (Math.Atan2(deltaY, deltaX) + Math.PI * 2.0 - rot) % (Math.PI * 2.0f);
            if (phi > Math.PI) phi -= Math.PI * 2.0;
            phi = Math.Max(-maxAngle, Math.Min(maxAngle, phi));

            rot += (float) phi;
            return phi;
        }

        public float getHealthFraction()
        {
            return health / prototype.getHealth(owner);
        }

        public float getShieldFraction()
        {
            return shield / prototype.getShields(owner);
        }
    }
}