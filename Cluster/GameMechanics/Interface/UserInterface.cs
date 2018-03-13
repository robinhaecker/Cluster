﻿using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Mathematics;
using Cluster.Rendering.Draw2D;
using OpenTK.Input;

namespace Cluster.GameMechanics.Interface
{
    enum Selection
    {
        PLANET,
        UNITS,
        ASTEROID
    }

    class UserInterface
    {
        protected const float COL_STD_I = 0.125f,
            COL_STD_ALPHA = 0.85f,
            COL_STD_ALPHA2 = 0.6f,
            COL_YES_R = 0.5f,
            COL_YES_G = 1.0f,
            COL_YES_B = 0.5f,
            COL_CRITICAL_R = 1.0f,
            COL_CRITICAL_G = 0.5f,
            COL_CRITICAL_B = 0.15f,
            COL_NO_R = 1.0f,
            COL_NO_G = 0.25f,
            COL_NO_B = 0.25f;

        private static int fps;
        private static int fps2;
        private static int msec;
        private static int msec2;
        static public bool mhl;
        static public bool mhr;
        static public bool mdl;
        static public bool mdr;
        static Vec3 focus;

        public static Selection selection;

        static Planet _pickedPlanet;
        static int _pickedIndex;
        private static Planet _selPlanet;
        static int _selIndex;

        public static Unit pickedUnit;
        public static List<Unit> sel_u;


        public static void init()
        {
            sel_u = new List<Unit>();
            focus = null;
            _selectionBox = null;
        }


        private static Vec4 _selectionBox;
        static float _selMx, _selMy;

        static public void update()
        {
            if (GameWindow.keyboard.IsKeyDown(Key.Tab))
                Civilisation.player = (Civilisation.player + 1) % Civilisation.count;


            mhl = GameWindow.mouse.IsButtonDown(MouseButton.Left) && !mdl;
            mhr = GameWindow.mouse.IsButtonDown(MouseButton.Right) && !mdr;

            mdl = GameWindow.mouse.IsButtonDown(MouseButton.Left);
            mdr = GameWindow.mouse.IsButtonDown(MouseButton.Right);

            if (mhl && !mouseOverInterface())
            {
                if (_selectionBox == null) // Selektionsbox anfangen.
                {
                    _selMx = GameWindow.mousePos.x;
                    _selMy = GameWindow.mousePos.y;
                    _selectionBox = new Vec4(Space.screenToSpaceX(GameWindow.mousePos.x),
                        Space.screenToSpaceY(GameWindow.mousePos.y), 0.0f, 0.0f);
                }

                mhl = false;
                pick();
                @select();
            }
            else if (mdl && !mhl && _selectionBox != null) // Selektionsbox-Ende ziehen bei gedrückter linker Maustaste.
            {
                _selectionBox.z = Space.screenToSpaceX(GameWindow.mousePos.x);
                _selectionBox.w = Space.screenToSpaceY(GameWindow.mousePos.y);
            }
            else if (_selectionBox != null) // Maustaste nicht mehr gedrückt? Selektionsbox beenden.
            {
                if (Math.Abs(GameWindow.mousePos.x - _selMx) + Math.Abs(GameWindow.mousePos.x - _selMx) > 20.0f
                ) // Wenn nicht nur geklickt, sondern auch vergrößert, dann auswerten.
                {
                    float cx = (_selectionBox.x + _selectionBox.z) * 0.5f,
                        cy = (_selectionBox.y + _selectionBox.w) * 0.5f;
                    float dx = Math.Abs(_selectionBox.x - _selectionBox.z) * 0.5f,
                        dy = Math.Abs(_selectionBox.y - _selectionBox.w) * 0.5f;
                    foreach (Unit u in Unit.units)
                    {
                        if ((Math.Abs(cx - u.x) < dx && Math.Abs(cy - u.y) < dy) &&
                            (u.isAlive() && u.getOwner().getId() == Civilisation.player && !sel_u.Contains(u)))
                        {
                            sel_u.Add(u);
                            u.isSelected = true;
                        }
                        else
                        {
                            u.isSelected = false;
                        }
                    }
                }

                _selectionBox = null; // Wird wieder entfernt.
            }
        }


        public static void render()
        {
            drawAlways();
            drawPlanetInfo();
            drawGuiUnits();
            /*
            switch (selection)
            {
                case SELECTION_NONE:
                    return;
                case SELECTION_PLANET:
                    gui_planet();
                    return;
                case SELECTION_UNITS:
                    gui_units();
                    return;
            }*/
        }


        private static void drawAlways()
        {
            Civilisation civ = Civilisation.getPlayer();

            Text.setTextSize();
            Text.drawText("Ressourcen: \t" + ((int) civ.ressources).ToString(), 10.0f, 10.0f);
            Text.drawText("Forschung:\t" + ((int) civ.science).ToString(), 10.0f,
                35.0f); //, 20.0f, 0.0f, 1.0f, 0.5f, 0.25f);
            Text.drawText("Bevölkerung:\t" + civ.population.ToString() + " / " + civ.maxPopulation.ToString(), 10.0f,
                60.0f); //, 20.0f, 1.0f, 0.0f, 1.0f, 0.5f);

            Text.drawText(
                "FPS: " + fps.ToString() + "\n1/FPS: " + (100.0f / (float) fps) +
                " ms von 16 ms\nParticles rendered: " + Particle.rendered_count.ToString(),
                GameWindow.active.width - 300, 100.0f);

            //manager.drawBox(500.0f, 100.0f, 100.0f, 100.0f, 0.5f, 1.0f, 1.0f, 0.25f);

            //Frames per second
            fps2++;
            msec2 = Environment.TickCount;
            if (msec2 - msec >= 1000)
            {
                msec = msec2;
                fps = fps2;
                fps2 = 0;
            }

            if (_selectionBox != null && Math.Abs(_selectionBox.z) + Math.Abs(_selectionBox.w) > 0.000001f)
            {
                Primitives.setColor(1.0f, 1.0f, 1.0f, 0.125f);
                Primitives.setLineWidth(2.0f);
                Vec4 box = new Vec4(Space.spaceToScreenX(_selectionBox.x), Space.spaceToScreenY(_selectionBox.y),
                    Space.spaceToScreenX(_selectionBox.z), Space.spaceToScreenY(_selectionBox.w));
                Primitives.drawLine(box.x, box.y, box.x, box.w);
                Primitives.drawLine(box.z, box.y, box.z, box.w);
                Primitives.drawLine(box.x, box.y, box.z, box.y);
                Primitives.drawLine(box.x, box.w, box.z, box.w);
                Primitives.setLineWidth(1.0f);
            }
        }


/*
		public static void gui_planet()
		{
			float x0 = 10, y0 = GameWindow.active.height - 205;
			Primitives.drawRect(x0-5, y0-5, 200, 200);
			Text.drawText(sel_p.name, x0, y0);
			Text.drawText(sel_p.)

		}*/


        private static void select()
        {
            _selPlanet = _pickedPlanet;
            _selIndex = _pickedIndex;
            if (!GameWindow.keyboard.IsKeyDown(Key.ShiftLeft))
            {
                foreach (Unit u in sel_u)
                {
                    u.isSelected = false;
                }

                sel_u.Clear();
            }

            if (pickedUnit != null && !sel_u.Contains(pickedUnit))
            {
                sel_u.Add(pickedUnit);
                pickedUnit.isSelected = true;
            }
        }


        private static void pick()
        {
            float mSpaceX = Space.screenToSpaceX(GameWindow.mousePos.x);
            float mSpaceY = Space.screenToSpaceY(GameWindow.mousePos.y);


            _pickedPlanet = null;
            _pickedIndex = -1;
            pickedUnit = null;
            //Planeten picken
            foreach (Planet pl in Planet.planets)
            {
                double
                    delx = mSpaceX - pl
                               .x; //(manager.MouseX() - manager.width *0.5) - (pl.x - Space.scroll_x)*Space.zoom;
                double
                    dely = mSpaceY - pl
                               .y; //(manager.MouseY() - manager.height*0.5) - (pl.y - Space.scroll_y)*Space.zoom;
                double dist = delx * delx + dely * dely;
                if (dist < pl.size * pl.size * 625.0)
                {
                    _pickedPlanet = pl;

                    if (Space.zoom > 0.3 && dist > pl.size * pl.size * 225.0)
                    {
                        double alpha = Math.Atan2(dely, delx) / (2.0 * Math.PI);
                        _pickedIndex = (int) Math.Floor(((alpha + 1.0) % 1.0) * pl.size);
                    }

                    return;
                }
            }


            var sector = Sector.get(mSpaceX, mSpaceY);
            //Schiffe auswählen durch direkt drauf klicken
            foreach (var listOfSectorShips in sector.ships)
            {
                foreach (var unit in listOfSectorShips)
                {
                    if (Math.Sqrt((unit.x - mSpaceX) * (unit.x - mSpaceX) + (unit.y - mSpaceY) * (unit.y - mSpaceY)) <
                        unit.getPrototype().shapeScaling * unit.getPrototype().shape.radius + 20.0f)
                    {
                        pickedUnit = unit;
                        return;
                    }
                }
            }
        }

        static bool mouseOverInterface()
        {
            if (_selPlanet != null && _selIndex >= 0)
            {
                if (mouseInRect(10, GameWindow.active.height - 110, 100, 100)) return true;

                Civilisation player = Civilisation.getPlayer();
                List<Blueprint> options = _selPlanet.listOfBuildables(_selIndex, player);
                int index = 0, hover = -1;
                foreach (var blueprint in options)
                {
                    float drx = 115.0f + (float) (index / 2) * 50.0f;
                    float dry = (float) (GameWindow.active.height - 110) + 52.0f * (float) (index % 2);
                    if (mouseInRect(drx, dry, 48.0f, 48.0f)) return true;
                    index++;
                }

                Blueprint.SpecialAbility subInterface = 0;

                if (_selPlanet.infra[_selIndex] != null && _selPlanet.infra[_selIndex].owner == player
                ) // && sel_planet.infra[sel_index].status == Building.Status.NONE)
                {
                    switch (_selPlanet.infra[_selIndex].blueprint.specials)
                    {
                        case Blueprint.SpecialAbility.SHIPS:
                            subInterface = Blueprint.SpecialAbility.SHIPS;
                            break;

                        case Blueprint.SpecialAbility.RESEARCH:
                            subInterface = Blueprint.SpecialAbility.RESEARCH;
                            break;
                    }
                }


                // Eigene Schiffswerft, in der Schiffe gebaut werden können
                if (subInterface == Blueprint.SpecialAbility.SHIPS)
                {
                    List<Prototype> options2 = _selPlanet.infra[_selIndex].listOfPrototypes(player);

                    index = 0;
                    hover = -1;
                    foreach (Prototype prot in options2)
                    {
                        float drx = 115.0f + (float) (options.Count / 2) * 50.0f + (float) (index / 2) * 50.0f;
                        float dry = (float) (GameWindow.active.height - 110) + 52.0f * (float) (index % 2);
                        float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
                        if (mouseInRect(drx, dry, 48.0f, 48.0f)) return true;
                        index++;
                    }
                }
            }
            else if (sel_u.Count > 0)
            {
                int[] counts = new int[Prototype.count];
                foreach (Unit u in sel_u)
                {
                    if (u.isAlive()) counts[u.getPrototype().id] += 1;
                }

                int index = 0;
                Civilisation player = Civilisation.getPlayer();
                for (int i = 0; i < Prototype.count; i++)
                {
                    if (counts[i] == 0) continue;
                    float drx = 15.0f + (float) (index) * 50.0f;
                    float dry = (float) (GameWindow.active.height - 60);
                    if (mouseInRect(drx, dry, 48.0f, 48.0f))
                    {
                        return true;
                    }

                    index++;
                }
            }

            return false;
        }


        static void drawPlanetInfo()
        {
            if (_selPlanet != null && (_selIndex > -1))
            {
                draw_land_info();
                return;
            }

            if (_pickedPlanet == null) return;

            float drawX = (float) Space.spaceToScreenX(_pickedPlanet.x + (float) _pickedPlanet.size * 25.0f) + 20.0f;
            float drawY = (float) Space.spaceToScreenY(_pickedPlanet.y);

            string text = _pickedPlanet.getTerrainType(_pickedIndex); // +" (" + picked_planet.name + ")";
            if (_pickedIndex > -1 && _pickedPlanet.infra[_pickedIndex] != null)
            {
                text = _pickedPlanet.infra[_pickedIndex].blueprint.name + " (" + text + ")\n" +
                       _pickedPlanet.infra[_pickedIndex].blueprint.description[0] + "\n" +
                       _pickedPlanet.infra[_pickedIndex].blueprint.description[1] + "\n" +
                       _pickedPlanet.infra[_pickedIndex].blueprint.description[2];
            }
            else if (_pickedIndex == -1)
            {
                text = "Planet:\t" + _pickedPlanet.name + "\nKlima:\t" + _pickedPlanet.getClimate() + "\nGröße:\t" +
                       _pickedPlanet.size.ToString() + " Felder\n" + _pickedPlanet.getDominantCivName();
            }


            //manager.drawText(text, manager.MouseX() + 20, manager.MouseY());//, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
            if (Space.zoom < 0.3 || _selPlanet != null)
            {
                Text.drawText(text, drawX, drawY - 10.0f * 4.0f); //, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
            }
            else
            {
                Text.drawText(text, GameWindow.mousePos.x + 20,
                    GameWindow.mousePos.y); //, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
            }
        }

        static void draw_land_info()
        {
            Blueprint.SpecialAbility subInterface = 0;

            Primitives.setColor(COL_STD_I, COL_STD_I, COL_STD_I, COL_STD_ALPHA);
            Primitives.drawRect(10, GameWindow.active.height - 110, 100, 100);
            //manager.drawBox(110, manager.height - 105, 200, 100, 0.125f, 0.125f, 0.125f, 0.85f);
            string text = "";

            if (_selPlanet.infra[_selIndex] != null)
            {
                text = _selPlanet.infra[_selIndex].blueprint.name + " (" + _selPlanet.infra[_selIndex].owner.name +
                       ")\nGesundheit: " + ((int) _selPlanet.infra[_selIndex].health) + "/" +
                       ((int) _selPlanet.infra[_selIndex].healthMax) + "";
                //manager.drawText(text, 5, manager.height - 100 - manager.textHeight(text));

                _selPlanet.infra[_selIndex].blueprint.shape.deferred_draw(10, GameWindow.active.height - 80, 100, 100);

                float gr_r = 0.5f, gr_g = 0.5f, gr_b = 0.5f;
                if (_selPlanet.terra[_selIndex] == Planet.Terrain.WATER)
                {
                    gr_r = 0.5f;
                    gr_g = 0.5f;
                    gr_b = 1.05f;
                }

                Primitives.setColor(gr_r, gr_g, gr_b, 0.25f);
                Primitives.drawRect(10,
                    GameWindow.active.height - 30 + _selPlanet.infra[_selIndex].blueprint.shape.del_y * 50.0f, 100,
                    20.0f - _selPlanet.infra[_selIndex].blueprint.shape.del_y * 50.0f);
                //manager.drawBox(0, manager.height - 10 + sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 100, -10.0f, gr_r, gr_g, gr_b, 0.25f);
                //manager.drawBox(0, manager.height - 20 + sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 100, 20.0f - sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 0.5f, 0.5f, 0.5f, 0.25f);
                //sel_planet.infra[sel_index].bp.shape.draw(0.0f, 0.0f, 100, 100);

                if (_selPlanet.infra[_selIndex].owner == Civilisation.getPlayer()
                ) // && sel_planet.infra[sel_index].status == Building.Status.NONE)
                {
                    switch (_selPlanet.infra[_selIndex].blueprint.specials)
                    {
                        case Blueprint.SpecialAbility.SHIPS:
                            subInterface = Blueprint.SpecialAbility.SHIPS;
                            break;

                        case Blueprint.SpecialAbility.RESEARCH:
                            subInterface = Blueprint.SpecialAbility.RESEARCH;
                            break;
                    }
                }
            }
            else
            {
                text = _selPlanet.getTerrainType(_selIndex) + " (" + _selPlanet.name + ")";
                //manager.drawText(text, 5, manager.height - 100 - manager.textHeight(text));
                Planet.terraImage[(int) _selPlanet.terra[_selIndex]]
                    .deferred_draw(10, GameWindow.active.height - 80, 100, 100);
                //Planet.terra_image[sel_planet.terra[sel_index]].draw(manager.width - 50, 0, 9, 9);
            }

            Civilisation player = Civilisation.getPlayer();
            List<Blueprint> options = _selPlanet.listOfBuildables(_selIndex, player);
            int index = 0, hover = -1;
            foreach (Blueprint bp in options)
            {
                float drx = 115.0f + (float) (index / 2) * 50.0f;
                float dry = (float) (GameWindow.active.height - 110) + 52.0f * (float) (index % 2);
                float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
                if (mouseInRect(drx, dry, 48.0f, 48.0f))
                {
                    hover = index;
                    if (bp.cost <= player.ressources)
                    {
                        boxr = COL_YES_R;
                        boxg = COL_YES_G;
                        boxb = COL_YES_B;
                    }
                    else
                    {
                        boxr = COL_NO_R;
                        boxg = COL_NO_G;
                        boxb = COL_NO_B;
                    }
                }

                Primitives.setColor(boxr, boxg, boxb, boxa);
                Primitives.drawRect(drx, dry, 47.5f, 47.5f);
                bp.shape.deferred_draw(drx, dry + 10.0f, 47.5f, 47.5f);

                index++;
            }

            if (hover > -1)
            {
                string cres = "&r", cene = "&y";
                if (options[hover].getCost() <= player.ressources) cres = "&g";
                text = "&h" + options[hover].name +
                       "\n" + cres + "Ressourcen:" + options[hover].getCost().ToString().PadLeft(4, ' ') +
                       "\n" + cene + "Energie:\t " + options[hover].getEnergyNeeds().ToString().PadLeft(4, ' ') +
                       "\n&n" + options[hover].description[0] + "\n" + options[hover].description[1] + "\n" +
                       options[hover].description[2];

                if (mhl)
                {
                    if (_selPlanet.infra[_selIndex] == null && player.ressources >= options[hover].getCost())
                    {
                        _selPlanet.build(_selIndex, options[hover], player);
                    }
                    else if (_selPlanet.infra[_selIndex] != null && player.ressources >=
                             (options[hover].getCost() - _selPlanet.infra[_selIndex].blueprint.getCost()))
                    {
                        _selPlanet.upgrade(_selIndex, options[hover]);
                    }
                }
            }


            // Eigene Schiffswerft, in der Schiffe gebaut werden können
            if (subInterface == Blueprint.SpecialAbility.SHIPS)
            {
                List<Prototype> options2 = _selPlanet.infra[_selIndex].listOfPrototypes(player);

                index = 0;
                hover = -1;
                foreach (Prototype prot in options2)
                {
                    float drx = 115.0f + (float) (options.Count / 2) * 50.0f + (float) (index / 2) * 50.0f;
                    float dry = (float) (GameWindow.active.height - 110) + 52.0f * (float) (index % 2);
                    float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
                    if (mouseInRect(drx, dry, 48.0f, 48.0f))
                    {
                        hover = index;
                        if (prot.cost <= player.ressources)
                        {
                            boxr = COL_YES_R;
                            boxg = COL_YES_G;
                            boxb = COL_YES_B;
                            if (prot.population > player.populationLeft())
                            {
                                boxr = COL_CRITICAL_R;
                                boxg = COL_CRITICAL_G;
                                boxb = COL_CRITICAL_B;
                            }
                        }
                        else
                        {
                            boxr = COL_NO_R;
                            boxg = COL_NO_G;
                            boxb = COL_NO_B;
                        }
                    }

                    Primitives.setColor(boxr, boxg, boxb, boxa);
                    Primitives.drawRect(drx, dry, 47.5f, 47.5f);

                    if ((_selPlanet.infra[_selIndex].production.Count > 0) &&
                        (_selPlanet.infra[_selIndex].production[0] == prot))
                    {
                        Primitives.setColor(COL_STD_I, COL_STD_I, COL_STD_I, COL_STD_ALPHA);
                        Primitives.setDepth(Primitives.getDepth() - 0.1f);
                        Primitives.drawRect(drx, dry, 47.5f * _selPlanet.infra[_selIndex].productionTimer, 47.5f);
                        Primitives.setDepth(Primitives.getDepth() + 0.1f);
                        Text.setTextSize(15.0f);
                        Text.drawText(((int) (_selPlanet.infra[_selIndex].productionTimer * 100.0)).ToString() + "%",
                            drx, dry);
                    }

                    prot.shape.deferred_drawFit(drx, dry, 47.5f, 47.5f);

                    int ct = _selPlanet.infra[_selIndex].getProductionCount(prot);
                    if (ct > 0)
                    {
                        Text.setTextSize(15.0f);
                        Text.drawText(ct.ToString(), drx + 35.0f, dry);
                    }

                    index++;
                }

                if (hover > -1)
                {
                    text = "&h" + options2[hover].name +
                           "\n&nRessourcen: " + options2[hover].getCost().ToString().PadLeft(4, ' ') +
                           "\nBevölkerung:" + options2[hover].getPopulation().ToString().PadLeft(4, ' ') +
                           "\n" + options2[hover].description;

                    if (mhl)
                    {
                        if ((_selPlanet.infra[_selIndex].status == Building.Status.NONE ||
                             _selPlanet.infra[_selIndex].status == Building.Status.UNDER_CONSTRUCTION) &&
                            player.ressources >= options2[hover].getCost())
                        {
                            _selPlanet.infra[_selIndex].produceUnit(options2[hover]);
                            if (GameWindow.keyboard.IsKeyDown(Key.ShiftLeft) ||
                                GameWindow.keyboard.IsKeyDown(Key.ShiftRight))
                            {
                                if (player.ressources >= options2[hover].getCost())
                                    _selPlanet.infra[_selIndex].produceUnit(options2[hover]);
                                if (player.ressources >= options2[hover].getCost())
                                    _selPlanet.infra[_selIndex].produceUnit(options2[hover]);
                                if (player.ressources >= options2[hover].getCost())
                                    _selPlanet.infra[_selIndex].produceUnit(options2[hover]);
                                if (player.ressources >= options2[hover].getCost())
                                    _selPlanet.infra[_selIndex].produceUnit(options2[hover]);
                            }
                        }
                    }
                    else if (mhr)
                    {
                        if ((_selPlanet.infra[_selIndex].status == Building.Status.NONE ||
                             _selPlanet.infra[_selIndex].status == Building.Status.UNDER_CONSTRUCTION))
                        {
                            _selPlanet.infra[_selIndex].abortUnit(options2[hover]);
                            if (GameWindow.keyboard.IsKeyDown(Key.ShiftLeft) ||
                                GameWindow.keyboard.IsKeyDown(Key.ShiftRight))
                            {
                                _selPlanet.infra[_selIndex].abortUnit(options2[hover]);
                                _selPlanet.infra[_selIndex].abortUnit(options2[hover]);
                                _selPlanet.infra[_selIndex].abortUnit(options2[hover]);
                                _selPlanet.infra[_selIndex].abortUnit(options2[hover]);
                            }
                        }
                    }
                }
            }

            Text.setTextSize(20.0f);
            Text.drawText(text, 10, GameWindow.active.height - 120 - Text.textHeight(text));
        }


        public static void drawGuiUnits()
        {
            if (sel_u.Count == 0) return;

            int[] counts = new int[Prototype.count];
            foreach (Unit u in sel_u)
            {
                if (u.isAlive()) counts[u.getPrototype().id] += 1;
            }

            int index = 0, hover = -1;
            Civilisation player = Civilisation.getPlayer();
            for (int i = 0; i < Prototype.count; i++)
            {
                if (counts[i] == 0) continue;

                float drx = 15.0f + (float) (index) * 50.0f;
                float dry = (float) (GameWindow.active.height - 60); // +52.0f * (float)(index % 2);
                float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
                if (mouseInRect(drx, dry, 48.0f, 48.0f))
                {
                    hover = i;
                    boxr = COL_YES_R;
                    boxg = COL_YES_G;
                    boxb = COL_YES_B; //boxa = COL_STD_ALPHA;
                }

                Primitives.setColor(boxr, boxg, boxb, boxa);
                Primitives.drawRect(drx, dry, 47.5f, 47.5f);
                Prototype.data[i].shape.deferred_drawFit(drx, dry, 47.5f, 47.5f);

                Text.setTextSize(15.0f);
                Text.drawText(counts[i].ToString(), drx + 35.0f, dry);
                index++;
            }

            /*
            if (hover > -1)
            {
                
            }*/
        }


        private static bool mouseInRect(float x0, float y0, float width, float height)
        {
            return !(GameWindow.mousePos.x < x0) && !(GameWindow.mousePos.y < y0) && !(GameWindow.mousePos.x > x0 + width) && !(GameWindow.mousePos.y > y0 + height);
        }
    }
}