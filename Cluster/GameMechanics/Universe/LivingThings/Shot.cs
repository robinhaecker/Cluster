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
        
        private static int _renderShotCount;
        private static int _countshots;
        static int _shotGlData;
        static int _bufSh0;
        static int _bufSh1;
        static int _bufSh2;

        private static readonly float[] renderBufPos = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_POS];
        private static readonly float[] renderBufCol = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_COL];
        private static readonly float[] renderBufAlphas = new float[MAX_SHOT_RENDERINGS * RENDER_ARRSIZE_ALPHAS];

        public static readonly List<Shot>[] list = {new List<Shot>(), new List<Shot>()};
        public static readonly List<Shot> removed = new List<Shot>();

        private readonly FiredAgainst _against;

        private readonly Prototype.WeaponType _weaponType;
        readonly Civilisation _owner;
        private readonly Unit _from;
        Unit _aimUnit;

        readonly Planet _aimPlanet;
        //Asteroid aim_asteroid;

        public float x, y, v, rot; // Position, Geschwindigkeit, Richtung
        float _damage; // Stärke des Schadens
        float _lifespan; // Wie lange hält der Schuss noch durch
        private readonly float _red;
        private readonly float _green;
        private readonly float _blue;
        private readonly float _alpha;
        private readonly float _length;


        public Shot(Unit from, Unit aim = null, byte inrange = 1)
        {
            _red = _green = _blue = _alpha = 1.0f;
            _length = 13.5f;
            _against = FiredAgainst.UNITS;

            this._from = from;
            _owner = from.getOwner();
            _aimUnit = aim;
            _weaponType = from.getPrototype().weaponType;
            x = from.x;
            y = from.y;
            v = 100.0f;
            rot = from.rot;
            _damage = from.getPrototype().getAttack(_owner);
            _lifespan = 10.0f;

            if (_weaponType == Prototype.WeaponType.PERSECUTE_AND_LASER)
            {
                _red = 0.25f;
                _green = 1.0f;
                _blue = 0.4f;
                _length = 5.0f;
                if (inrange == 2) return; // Zu nahe dran --> zurück ohne hinzufügen des Schusses.
                else if (inrange == 1) _weaponType = Prototype.WeaponType.LASER; // Laserwaffe für Nahkampf
                else _weaponType = Prototype.WeaponType.PERSECUTE; // Lenkraketen für Fernkampf (oder anders herum?)
            }

            if (_weaponType == Prototype.WeaponType.LASER)
            {
                _green = _blue = 0.0f;
                _alpha = 0.35f;
                _lifespan = 2.0f;
                _damage /= _lifespan;
                x = y = 0.0f; // Wird bei Lasern für relativen Offset zum Zeichnen benutzt.
            }


            list[(int)_against].Add(this);
        }

        public Shot(Shot from)
        {
            _red = _alpha = 1.0f;
            _green = (float) GameWindow.random.NextDouble();
            _blue = 0.0f;
            _length = 4.5f;
            _against = FiredAgainst.UNITS;

            this._from = from._from;
            _owner = from._owner;
            _aimUnit = null;
            _weaponType = Prototype.WeaponType.STD;
            x = (float) from.x;
            y = (float) from.y;
            v = _green * 50.0f + 30.0f;
            rot = (float) (GameWindow.random.NextDouble() * 2.0f * Math.PI);
            _damage = from._damage + _green * 10.0f;
            _lifespan = 10.0f;

            list[(int)_against].Add(this);
        }

        public Shot(Unit from, Planet pl, int index)
        {
            this._from = from;
            _owner = from.getOwner();
            _damage = from.getPrototype().getAttack(_owner);
            _lifespan = 10.0f;

            _red = _owner.red * 0.25f + 0.75f;
            _green = _owner.green * 0.25f + 0.75f;
            _blue = _owner.blue * 0.25f + 0.75f;
            _alpha = 0.5f;
            _length = 25.0f;
            _against = FiredAgainst.BUILDINGS;

            _aimPlanet = pl;


            x = (float) from.x;
            y = (float) from.y;
            v = 75.0f;
            rot = (float) Math.Atan2(
                pl.y + (double) pl.size * 20.0 * Math.Sin(((double) index + 0.5) / (double) pl.size * 2.0 * Math.PI) -
                y,
                pl.x + (double) pl.size * 20.0 * Math.Cos(((double) index + 0.5) / (double) pl.size * 2.0 * Math.PI) -
                x);

            list[(int)_against].Add(this);
        }


        public static void update(float t = 1.0f)
        {
            _countshots = 0;
            foreach (Shot s in list[(int)FiredAgainst.UNITS])
            {
                s.simAgainstUnits(t);
                _countshots++;
            }

            foreach (Shot s in list[(int)FiredAgainst.BUILDINGS])
            {
                s.simAgainstBuildings(t);
                _countshots++;
            }

            // Entferne Schüsse, die nicht mehr gebraucht werden.
            foreach (Shot s in removed)
            {
                list[(int)s._against].Remove(s);
            }

            removed.Clear();
        }


        private void simAlways(float t)
        {
            if (_weaponType != Prototype.WeaponType.LASER)
            {
                x += v * (float) Math.Cos(rot) * t * 0.002f;
                y += v * (float) Math.Sin(rot) * t * 0.002f;
            }

            _lifespan -= t * 0.002f;

            if (_lifespan <= 0.0)
            {
                removed.Add(this);
            }
        }

        private void simAgainstUnits(float t)
        {
            // Kollisionscheck, Lenkbewegungen etc.
            if (_weaponType != Prototype.WeaponType.LASER)
            {
                float neardist = 1000.0f;
                Sector s = Sector.get(x, y);
                for (int i = 0; i < Civilisation.count + 1; i++)
                {
                    if (i == _owner.getId()) continue;
                    foreach (Unit u in s.ships[i])
                    {
                        float dst = (float) Math.Sqrt((u.x - x) * (u.x - x) + (u.y - y) * (u.y - y));
                        if (dst < 5.0f * u.getPrototype().shapeScaling * u.getPrototype().shape.radius)
                        {
                            u.damage(_damage);
                            removed.Add(this);
                            return;
                        }
                        else if (_weaponType == Prototype.WeaponType.FIND_AIM && dst < neardist)
                        {
                            neardist = dst;
                            _aimUnit = u;
                        }
                        else if (_weaponType == Prototype.WeaponType.EXPLOSIVE && dst <
                                 30.0f * u.getPrototype().shapeScaling * u.getPrototype().shape.radius + 150.0f)
                        {
                            for (int j = 0; j < 15 + _owner.getMultiplicator(Civilisation.BONUS_WEAPONS_EXPLOSIVE); j++)
                            {
                                // ReSharper disable once ObjectCreationAsStatement -> Wird direkt in Liste gespeichert (Konstruktor)
                                new Shot(this);
                            }

                            removed.Add(this);
                            return;
                        }
                    }
                }

                if (_weaponType == Prototype.WeaponType.FIND_AIM || _weaponType == Prototype.WeaponType.PERSECUTE)
                {
                    turnTowards(_aimUnit.x - x, _aimUnit.y - y, t * 0.002);
                }
            }
            else // Laserwaffen anders handeln.
            {
                float mult = 1.0f;
                if (_from != null && _aimUnit.getPrototype().goodAgainst() == _from.getPrototype().shipClass)
                    mult = 0.15f;
                _aimUnit.damage(_damage * mult);
                _damage *= 0.95f;
                if (_aimUnit.isDead()) _lifespan -= t * 0.005f;
            }

            // Allgemeine Bewegungen
            simAlways(t);
        }

        private void simAgainstBuildings(float t)
        {
            // Kollisionscheck etc.
            float dx = x - _aimPlanet.x, dy = y - _aimPlanet.y;
            float dst = (float) Math.Sqrt(dx * dx + dy * dy);
            if (dst < _aimPlanet.size * 20.0f)
            {
                removed.Add(this);
                int feld = (int) (((2.0 * Math.PI + Math.Atan2(dy, dx)) % (2.0 * Math.PI)) *
                                  (_aimPlanet.size / (2.0 * Math.PI)));
                if (_aimPlanet.infra[feld] != null && _aimPlanet.infra[feld].owner != _owner)
                {
                    if (_aimPlanet.terra[feld] == Planet.Terrain.MOUNTAIN)
                    {
                        _damage *= 0.5f;
                    }
                    else if (_aimPlanet.terra[feld] == Planet.Terrain.WATER)
                    {
                        _damage *= 0.33f;
                    }

                    if (_aimPlanet.effect[feld] == Planet.Effect.RAIN)
                    {
                        _damage *= 0.5f;
                    } // Niederschlag/Wolken/Schneesturm reduziert Zielgenauigkeit und Schaden um 50%

                    _aimPlanet.infra[feld].damage(_damage);
                    return;
                }
            }

            // Allgemeine Bewegungen
            simAlways(t);
        }


        public static void render()
        {
            _renderShotCount = 0;
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
                        renderBufCol[_renderShotCount * RENDER_ARRSIZE_COL + 0] = s._red;
                        renderBufCol[_renderShotCount * RENDER_ARRSIZE_COL + 1] = s._green;
                        renderBufCol[_renderShotCount * RENDER_ARRSIZE_COL + 2] = s._blue;

                        if (s._weaponType == Prototype.WeaponType.LASER)
                        {
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 0] =
                                s._from.x + (float) Math.Cos(s._from.rot) * s.x - (float) Math.Sin(s._from.rot) * s.y;
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 1] =
                                s._from.y + (float) Math.Sin(s._from.rot) * s.x + (float) Math.Cos(s._from.rot) * s.y;
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 2] = s._aimUnit.x;
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 3] = s._aimUnit.y;
                            renderBufAlphas[_renderShotCount * RENDER_ARRSIZE_ALPHAS + 0] =
                                s._alpha * Math.Min(1.0f, s._lifespan);
                            renderBufAlphas[_renderShotCount * RENDER_ARRSIZE_ALPHAS + 1] =
                                s._alpha * Math.Min(1.0f, s._lifespan);
                        }
                        else
                        {
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 0] = s.x;
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 1] = s.y;
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 2] =
                                s.x - (float) Math.Cos(s.rot) * s._length;
                            renderBufPos[_renderShotCount * RENDER_ARRSIZE_POS + 3] =
                                s.y - (float) Math.Sin(s.rot) * s._length;
                            renderBufAlphas[_renderShotCount * RENDER_ARRSIZE_ALPHAS + 0] = 0.0f;
                            renderBufAlphas[_renderShotCount * RENDER_ARRSIZE_ALPHAS + 1] =
                                s._alpha * Math.Min(1.0f, s._lifespan);
                        }

                        _renderShotCount++;
                        if (_renderShotCount >= MAX_SHOT_RENDERINGS)
                        {
                            render_part2();
                            return;
                        }
                    }
                }
            }

            if (_renderShotCount > 0)
            {
                render_part2();
            }
        }

        static void render_part2()
        {
            GL.LineWidth(2.0f);
            GL.Disable(EnableCap.DepthTest);

            if (_shotGlData == 0)
            {
                _shotGlData = GL.GenVertexArray();
                _bufSh0 = GL.GenBuffer();
                _bufSh1 = GL.GenBuffer();
                _bufSh2 = GL.GenBuffer();
            }

            GL.BindVertexArray(_shotGlData);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufSh0);
            GL.BufferData(BufferTarget.ArrayBuffer, (_renderShotCount + 1) * RENDER_ARRSIZE_POS * sizeof(float),
                renderBufPos, BufferUsageHint.DynamicDraw);
            GL.EnableVertexArrayAttrib(_shotGlData, 0);
            GL.VertexAttribPointer(0, RENDER_ARRSIZE_POS, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufSh1);
            GL.BufferData(BufferTarget.ArrayBuffer, (_renderShotCount + 1) * RENDER_ARRSIZE_COL * sizeof(float),
                renderBufCol, BufferUsageHint.DynamicDraw);
            GL.EnableVertexArrayAttrib(_shotGlData, 1);
            GL.VertexAttribPointer(1, RENDER_ARRSIZE_COL, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufSh2);
            GL.BufferData(BufferTarget.ArrayBuffer, (_renderShotCount + 1) * RENDER_ARRSIZE_ALPHAS * sizeof(float),
                renderBufAlphas, BufferUsageHint.DynamicDraw);
            GL.EnableVertexArrayAttrib(_shotGlData, 2);
            GL.VertexAttribPointer(2, RENDER_ARRSIZE_ALPHAS, VertexAttribPointerType.Float, false, 0, 0);


            Space.shotShader.bind();
            GL.Uniform3(Space.shotShader.getUniformLocation("viewport"), GameWindow.active.mult_x,
                GameWindow.active.mult_y, Space.animation);
            GL.Uniform3(Space.shotShader.getUniformLocation("scroll"), Space.scrollX, Space.scrollY, Space.zoom);
            GL.DrawArrays(PrimitiveType.Points, 0, _renderShotCount);


            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }


        public Prototype.WeaponType getWeaponType()
        {
            return _weaponType;
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