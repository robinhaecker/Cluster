using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.math;
using OpenTK.Graphics.OpenGL;

namespace Cluster.GameMechanics.Universe.LivingThings
{
    class Unit
    {
        public static readonly List<Unit> units = new List<Unit>();
        public static readonly List<Unit> removed = new List<Unit>();
        static int _idCounter;

        public enum Effect
        {
            NONE,
            SPAWNING,
            EXPLODE,
            CAMOUFLAGE,
            STUNNED
        }

        private static int _displayShieldCount;
        private const int DISPLAY_SHIELD_VARIABLES_XYSCALE = 3;
        private const int DISPLAY_SHIELD_VARIABLES_PERCENTAGES = 2;
        private const int DISPLAY_SHIELD_VARIABLES_COLOR = 4;

        private static readonly float[] shieldGlArray0 =
            new float[DISPLAY_SHIELD_VARIABLES_XYSCALE * Civilisation.TOTAL_MAX_POPULATION];

        private static readonly float[] shieldGlArray1 =
            new float[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * Civilisation.TOTAL_MAX_POPULATION];

        private static readonly float[] shieldGlArray2 =
            new float[DISPLAY_SHIELD_VARIABLES_COLOR * Civilisation.TOTAL_MAX_POPULATION];

        private static int _shieldGlData;
        private static int _bufSh0;
        private static int _bufSh1;
        private static int _bufSh2;


        // Instance-specific variables
        int _id;
        readonly Prototype _prototype;
        readonly Civilisation _owner;

        public float x, y, rot, v;
        float _health;
        float _shield;

        public bool isSelected;
        float _reloadTimer;
        float _gotHitTimer;
        float _currentEffectTimer;
        Effect _currentEffect;

        Sector _sector;
        readonly Target _target;
        public Unit enemy;
        public byte inrange;
        public float enemyDistance;


        // Constructors
        public Unit(Prototype p, Civilisation civ, float x, float y, float alpha = 0.0f)
        {
            _id = _idCounter;
            _idCounter++;

            _prototype = p;
            _owner = civ;
            this.x = x;
            this.y = y;
            rot = alpha;
            v = 0.0f;

            _health = _prototype.getHealth(civ);
            _shield = _prototype.getShields(civ);

            _reloadTimer = 0.0f;
            _gotHitTimer = 0.0f;
            _currentEffectTimer = 1.0f;
            _currentEffect = Effect.SPAWNING;
            _target = new Target();
            _target.update(x, y);

            units.Add(this);
        }

        public Unit(Prototype p, Building b)
        {
            _id = _idCounter;
            _idCounter++;

            Planet pl = b.getPlanet();

            _prototype = p;
            _owner = b.owner;
            rot = b.getSpotRotation();
            x = pl.x + (float) Math.Cos(rot) * (pl.size * 20.0f + 75.0f);
            y = pl.y + (float) Math.Sin(rot) * (pl.size * 20.0f + 75.0f);
            v = 0.0f;

            _reloadTimer = 0.0f;
            _gotHitTimer = 0.0f;
            _currentEffectTimer = 1.0f;
            _currentEffect = Effect.SPAWNING;
            _target = new Target();
            _target.update(pl, -1, b.getSpotId());

            _health = _prototype.getHealth(b.owner);
            _shield = _prototype.getShields(b.owner);

            units.Add(this);
        }


        public static void render()
        {
            _displayShieldCount = 0;

            GL.LineWidth(1.5f);
            GL.Disable(EnableCap.DepthTest);
            //Shader shader = Mesh.getShader();
            Space.unitShader.bind();
            foreach (Unit u in Unit.units)
            {
                var alpha = 1.0f;
                var scale = u._prototype.shapeScaling * 5.0f;
                switch (u._currentEffect)
                {
                    case Effect.SPAWNING:
                        scale *= Math.Min(1.0f, Math.Max(0.0f, 1.0f - u._currentEffectTimer));
                        break;
                    case Effect.EXPLODE:
                        alpha = u._currentEffectTimer * 0.5f;
                        break;
                }

                GL.Uniform3(Space.unitShader.getUniformLocation("pos"), (float) (u.x - Space.scrollX),
                    (float) (u.y - Space.scrollY), (float) u.rot);
                GL.Uniform4(Space.unitShader.getUniformLocation("col"), (float) u._owner.red, (float) u._owner.green,
                    (float) u._owner.blue, alpha);
                GL.Uniform3(Space.unitShader.getUniformLocation("scale"), scale,
                    (float) (Space.zoom * GameWindow.active.mult_x), (float) (Space.zoom * GameWindow.active.mult_y));

                GL.BindVertexArray(u._prototype.shape.gl_data);
                GL.DrawArrays(PrimitiveType.Lines, 0, u._prototype.shape.num_lines * 2);


                // Daten für Schuztschilddarstellung setzen.
                if (u.isAlive() && (u._gotHitTimer > 0.0f || u.isSelected))
                {
                    shieldGlArray0[DISPLAY_SHIELD_VARIABLES_XYSCALE * _displayShieldCount + 0] = u.x;
                    shieldGlArray0[DISPLAY_SHIELD_VARIABLES_XYSCALE * _displayShieldCount + 1] = u.y;
                    shieldGlArray0[DISPLAY_SHIELD_VARIABLES_XYSCALE * _displayShieldCount + 2] = scale;

                    shieldGlArray1[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * _displayShieldCount + 0] =
                        u.getHealthFraction();
                    shieldGlArray1[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * _displayShieldCount + 1] =
                        u.getShieldFraction();

                    if (u.isSelected)
                    {
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 0] =
                            u._owner.red * 0.75f + 0.25f;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 1] =
                            u._owner.green * 0.75f + 0.25f;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 2] =
                            u._owner.blue * 0.75f + 0.25f;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 3] = 1.5f;
                    }
                    else
                    {
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 0] = u._owner.red;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 1] = u._owner.green;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 2] = u._owner.blue;
                        shieldGlArray2[DISPLAY_SHIELD_VARIABLES_COLOR * _displayShieldCount + 3] =
                            Math.Min(1.0f, u._gotHitTimer);
                    }

                    _displayShieldCount++;
                }
            }

            // Schutzschilde zeichnen.
            if (_displayShieldCount > 0)
            {
                if (_shieldGlData == 0)
                {
                    _shieldGlData = GL.GenVertexArray();
                    _bufSh0 = GL.GenBuffer();
                    _bufSh1 = GL.GenBuffer();
                    _bufSh2 = GL.GenBuffer();
                }

                GL.BindVertexArray(_shieldGlData);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufSh0);
                GL.BufferData(BufferTarget.ArrayBuffer, shieldGlArray0.Length * sizeof(float), shieldGlArray0,
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexArrayAttrib(_shieldGlData, 0);
                GL.VertexAttribPointer(0, DISPLAY_SHIELD_VARIABLES_XYSCALE, VertexAttribPointerType.Float, false, 0, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufSh1);
                GL.BufferData(BufferTarget.ArrayBuffer, shieldGlArray1.Length * sizeof(float), shieldGlArray1,
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexArrayAttrib(_shieldGlData, 1);
                GL.VertexAttribPointer(1, DISPLAY_SHIELD_VARIABLES_PERCENTAGES, VertexAttribPointerType.Float, false, 0,
                    0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _bufSh2);
                GL.BufferData(BufferTarget.ArrayBuffer, shieldGlArray2.Length * sizeof(float), shieldGlArray2,
                    BufferUsageHint.StaticDraw);
                GL.EnableVertexArrayAttrib(_shieldGlData, 2);
                GL.VertexAttribPointer(2, DISPLAY_SHIELD_VARIABLES_COLOR, VertexAttribPointerType.Float, false, 0, 0);


                Space.unitShieldShader.bind();
                GL.Uniform3(Space.unitShieldShader.getUniformLocation("viewport"), GameWindow.active.mult_x,
                    GameWindow.active.mult_y, Space.animation);
                GL.Uniform3(Space.unitShieldShader.getUniformLocation("scroll"), Space.scrollX, Space.scrollY,
                    Space.zoom);
                GL.DrawArrays(PrimitiveType.Points, 0, _displayShieldCount);
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
                u._owner.population += u._prototype.getPopulation();
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
                u._sector.removeUnit(u);
                units.Remove(u);
            }

            removed.Clear();
        }

        private void simulate(float t)
        {
            updateSector();
            if (_health > 0)
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
            _gotHitTimer = Math.Max(0.0f, _gotHitTimer - t * 0.001f);
        }

        private void simMovement(float t)
        {
            x += v * (float) Math.Cos(rot) * t * 0.002f;
            y += v * (float) Math.Sin(rot) * t * 0.002f;
        }

        private void simEffects(float t)
        {
            if (_currentEffect != Effect.NONE)
            {
                _currentEffectTimer -= t * 0.001f;
                if (_currentEffectTimer <= 0.0f)
                {
                    _currentEffectTimer = 0.0f;
                    if (_currentEffect == Effect.EXPLODE) // Wenn fertig explodiert, dann Einheit aus Liste löschen.
                    {
                        remove();
                        return;
                    }

                    _currentEffect = Effect.NONE;
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

            var deltaMission = _target.getPosition() - new vec2(x, y);
            var delta = getDirectionToNextWaypoint();

            simMoveAndShoot(t, delta, deltaMission);

            if (GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.D)) damage(t * 0.1f); // Nur zu Testzwecken
        }

        private vec2 getDirectionToNextWaypoint()
        {
            var delta = _target.getWaypoint() - new vec2(x, y);
            if (delta.length() < 50.0)
            {
                delta = _target.nextWaypoint() - new vec2(x, y);
            }

            return delta;
        }

        private void simMoveAndShoot(float t, vec2 delta, vec2 deltaMission)
        {
            if (_currentEffect == Effect.SPAWNING || _currentEffect == Effect.STUNNED) {return;}
            var rotationSpeed = simAttackEnemy(t, delta);
            simAccellerate(t, rotationSpeed, deltaMission.length());
        }

        private void rechargeShield(float t)
        {
            var maxshield = _prototype.getShields(_owner);
            _shield = Math.Min(_shield + t * 0.002f * (float) Math.Sqrt(maxshield), maxshield);
            _reloadTimer = Math.Max(_reloadTimer - t * 0.005f, 0.0f);
        }

        private double simAttackEnemy(float t, vec2 delta)
        {
            double rotationSpeed;
            if (enemy != null)
            {
                bool aimtowards = false;
                if ((_prototype.weaponType == Prototype.WeaponType.LASER &&
                     (enemyDistance < _prototype.weaponRange)) ||
                    (_prototype.weaponType != Prototype.WeaponType.STD &&
                     _prototype.weaponType != Prototype.WeaponType.EXPLOSIVE &&
                     (enemyDistance < _prototype.weaponRange) && _prototype.shipClass != Prototype.Class.HUNTER))
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

                if ((_reloadTimer <= 0.0f) && (enemyDistance < _prototype.weaponRange) &&
                    (aimtowards || (_prototype.weaponType == Prototype.WeaponType.PERSECUTE ||
                                    _prototype.weaponType == Prototype.WeaponType.LASER ||
                                    _prototype.weaponType == Prototype.WeaponType.FIND_AIM)))
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
            float maxSpeed = _prototype.getSpeed();
            if (missionDistance < 1000.0 && _prototype.shipClass == Prototype.Class.HUNTER &&
                !(_prototype.shipClass != Prototype.Class.WARSHIP && enemy != null) &&
                _prototype.specials == Prototype.ShipAbility.ASTEROID_MINING)
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
            if (_health <= 0.0f) return false;
            _gotHitTimer = 3.0f;

            if (_shield > dmg)
            {
                _shield -= dmg;
                return false;
            }

            dmg -= _shield;
            _shield = 0.0f;

            _health -= dmg;
            if (_health <= 0.0f)
            {
                _health = 0.0f;
                _currentEffect = Effect.EXPLODE;
                _currentEffectTimer = 1.0f;

                for (int i = 0; i < 50 + (int) Math.Sqrt(_prototype.healthMax / 50.0f); i++)
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
            _reloadTimer = _prototype.reloadTime;
            if (inrange > 0 || (_prototype.weaponType != Prototype.WeaponType.LASER))
            {
                var shot = new Shot(this, enemy, inrange);
                if (_prototype.shipClass == Prototype.Class.WARSHIP && inrange > 0)
                {
                    shot = new Shot(this, enemy, (byte) (2 - inrange));
                    shot.rot += (float) Math.PI * 0.25f;
                    if (shot.getWeaponType() == Prototype.WeaponType.LASER) shot.x -= 20.0f * _prototype.shapeScaling;

                    shot = new Shot(this, enemy, (byte) (2 - inrange));
                    shot.rot -= (float) Math.PI * 0.25f;
                    if (shot.getWeaponType() == Prototype.WeaponType.LASER) shot.x += 20.0f * _prototype.shapeScaling;
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

            if (_reloadTimer <= 0.0f && _prototype.attack > 0.0f)
            {
                int feld;
                Planet bombard = planetInRange(out feld);
                if (bombard != null)
                {
                    new Shot(this, bombard, feld);
                    if (_prototype.weaponType == Prototype.WeaponType.EXPLOSIVE)
                    {
                        new Shot(this, bombard, feld);
                        new Shot(this, bombard, feld);
                        new Shot(this, bombard, feld);
                    }

                    _reloadTimer = _prototype.reloadTime;
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
                        if (planet.infra[tmp] != null && planet.infra[tmp].owner != _owner)
                        {
                            bombardIndex = tmp;
                            return planet;
                        }

                        if (f == 0)
                        {
                            continue;
                        }

                        tmp = (planet.size + feld - f) % planet.size;
                        if (planet.infra[tmp] != null && planet.infra[tmp].owner != _owner)
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
            return _prototype;
        }

        public bool isDead()
        {
            return (_health <= 0.0) || (_currentEffect == Effect.EXPLODE);
        }

        public bool isAlive()
        {
            return !isDead();
        }

        public float getMaxHealth()
        {
            return _prototype.healthMax;
        }

        public float getMaxShields()
        {
            return _prototype.shields;
        }

        public Civilisation getOwner()
        {
            return _owner;
        }

        private void updateSector()
        {
            if (_sector != null)
            {
                if (_sector.containsPoint(x, y)) return;
                _sector.removeUnit(this);
                _sector = null;
            }

            _sector = Sector.get(x, y);
            _sector.addUnit(this);
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
            return _health / _prototype.getHealth(_owner);
        }

        public float getShieldFraction()
        {
            return _shield / _prototype.getShields(_owner);
        }
    }
}