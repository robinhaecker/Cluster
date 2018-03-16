using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Mathematics;
using Cluster.Rendering.Draw2D;
using OpenTK.Input;
/*
namespace Cluster.GameMechanics.Interface
{
//        static void drawPlanetInfo()
//        {
//            if (_selPlanet != null && (_selIndex > -1))
//            {
//                draw_land_info();
//                return;
//            }
//
//            if (_pickedPlanet == null) return;
//
//            float drawX = (float) Space.spaceToScreenX(_pickedPlanet.x + (float) _pickedPlanet.size * 25.0f) + 20.0f;
//            float drawY = (float) Space.spaceToScreenY(_pickedPlanet.y);
//
//            string text = _pickedPlanet.getTerrainType(_pickedIndex); // +" (" + picked_planet.name + ")";
//            if (_pickedIndex > -1 && _pickedPlanet.infra[_pickedIndex] != null)
//            {
//                text = _pickedPlanet.infra[_pickedIndex].blueprint.name + " (" + text + ")\n" +
//                       _pickedPlanet.infra[_pickedIndex].blueprint.description[0] + "\n" +
//                       _pickedPlanet.infra[_pickedIndex].blueprint.description[1] + "\n" +
//                       _pickedPlanet.infra[_pickedIndex].blueprint.description[2];
//            }
//            else if (_pickedIndex == -1)
//            {
//                text = "Planet:\t" + _pickedPlanet.name + "\nKlima:\t" + _pickedPlanet.getClimate() + "\nGröße:\t" +
//                       _pickedPlanet.size.ToString() + " Felder\n" + _pickedPlanet.getDominantCivName();
//            }
//
//
//            //manager.drawText(text, manager.MouseX() + 20, manager.MouseY());//, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
//            if (Space.zoom < 0.3 || _selPlanet != null)
//            {
//                Text.drawText(text, drawX, drawY - 10.0f * 4.0f); //, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
//            }
//            else
//            {
//                Text.drawText(text, GameWindow.mousePos.x + 20,
//                    GameWindow.mousePos.y); //, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
//            }
//        }

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
        }

    }
}
*/