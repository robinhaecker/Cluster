using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Content;
using Cluster.Rendering.Draw2D;
using Cluster.math;

namespace Cluster.GameMechanics.Interface
{
	class GUI
	{
		const byte  SELECTION_NONE     = 0,
					SELECTION_PLANET   = 1,
					SELECTION_UNITS    = 2,
					SELECTION_ASTEROID = 3;

		const float COL_STD_I = 0.125f,
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

		static int fps, fps2, msec, msec2;
		static public bool mhl, mhr;
		static public bool mdl, mdr;
		static vec3 focus;


		public static byte selection;

		static Planet picked_planet;
		static int picked_index;
		static Planet sel_planet;
		static int sel_index;

		public static Unit picked_unit;
		public static List<Unit> sel_u;



		public static void init()
		{
			sel_u = new List<Unit>();
			focus = null;
			selection_box = null;
		}




		static vec4 selection_box;
		static float selMX, selMY;

		static public void update()
		{
			changeView();
			if (GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.Tab)) Civilisation.player = (Civilisation.player + 1) % Civilisation.count;



			mhl = GameWindow.mouse.IsButtonDown(OpenTK.Input.MouseButton.Left) && !mdl;
			mhr = GameWindow.mouse.IsButtonDown(OpenTK.Input.MouseButton.Right) && !mdr;

			mdl = GameWindow.mouse.IsButtonDown(OpenTK.Input.MouseButton.Left);
			mdr = GameWindow.mouse.IsButtonDown(OpenTK.Input.MouseButton.Right);

			if (mhl && !mouseOverInterface())
			{
				if (selection_box == null) // Selektionsbox anfangen.
				{
					selMX = GameWindow.mouse_pos.x;
					selMY = GameWindow.mouse_pos.y;
					selection_box = new vec4(Space.screenToSpaceX(GameWindow.mouse_pos.x), Space.screenToSpaceY(GameWindow.mouse_pos.y), 0.0f, 0.0f);
				}

				mhl = false;
				pick();
				select();
			}
			else if (mdl && !mhl && selection_box != null) // Selektionsbox-Ende ziehen bei gedrückter linker Maustaste.
			{
				selection_box.z = Space.screenToSpaceX(GameWindow.mouse_pos.x);
				selection_box.w = Space.screenToSpaceY(GameWindow.mouse_pos.y);
			}
			else if (selection_box != null) // Maustaste nicht mehr gedrückt? Selektionsbox beenden.
			{
				if (Math.Abs(GameWindow.mouse_pos.x - selMX) + Math.Abs(GameWindow.mouse_pos.x - selMX) > 20.0f) // Wenn nicht nur geklickt, sondern auch vergrößert, dann auswerten.
				{
					float cx = (selection_box.x + selection_box.z) * 0.5f, cy = (selection_box.y + selection_box.w) * 0.5f;
					float dx = Math.Abs(selection_box.x - selection_box.z) * 0.5f, dy = Math.Abs(selection_box.y - selection_box.w) * 0.5f;
					foreach (Unit u in Unit.units)
					{
						if ((Math.Abs(cx - u.x) < dx && Math.Abs(cy - u.y) < dy) && (u.isAlive() && u.getOwner().getID() == Civilisation.player && !sel_u.Contains(u))) { sel_u.Add(u); u.is_selected = true; }
						else { u.is_selected = false; }
					}
				}
				selection_box = null; // Wird wieder entfernt.
			}


		}


		static float mouseZ, mouseZspeed;
		static void changeView()
		{
			mouseZspeed = GameWindow.mouse.WheelPrecise - mouseZ;
			mouseZ = GameWindow.mouse.WheelPrecise;

			Space.zoom = Math.Min(1.5f, Math.Max(0.03f, Space.zoom + mouseZspeed*0.025f));
			if (GameWindow.mouse_pos.x < 10 && Space.scroll_x > -20000.0f) Space.scroll_x -= 1.5f / Space.zoom;
			if (GameWindow.mouse_pos.x > GameWindow.active.width - 10 && Space.scroll_x < 20000.0f) Space.scroll_x += 1.5f / Space.zoom;
			if (GameWindow.mouse_pos.y < 10 && Space.scroll_y < 20000.0f) Space.scroll_y += 1.5f / Space.zoom;
			if (GameWindow.mouse_pos.y > GameWindow.active.height - 10 && Space.scroll_y > -20000.0f) Space.scroll_y -= 1.5f / Space.zoom;
				/*If Interface.mx<10 And cam_x>-20000.0 Then cam_x:-15.5/zoom*deltaT2
		If Interface.mx>GraphicsWidth()-10 And cam_x<20000.0 Then cam_x:+15.5/zoom*deltaT2
		If Interface.my<10  And cam_y>-20000.0 Then cam_y:-15.5/zoom*deltaT2
		If Interface.my>GraphicsHeight()-10 And cam_y<20000.0 Then cam_y:+15.5/zoom*deltaT2
	EndIf
	
	zoom=Min(1.5, Max(0.03, zoom+MouseZSpeed()*0.025))*/


			if (GameWindow.mouse.IsButtonDown(OpenTK.Input.MouseButton.Middle))
			{
				if (sel_planet != null) focus = new vec3(sel_planet.x, sel_planet.y, 1.0f);
			}
			if (focus != null)
			{
				Space.scroll_x = Space.scroll_x * focus.z + focus.x * (1.0f - focus.z);
				Space.scroll_y = Space.scroll_y * focus.z + focus.y * (1.0f - focus.z);
				focus.z -= 0.002f;
				if (focus.z <= 0.0f) focus = null;
			}


		}



		public static void render()
		{
			Primitives.setDepth(-0.1f);
			draw_always();
			draw_planet_info();
			draw_gui_units();
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


		static public void draw_always()
		{
			Civilisation civ = Civilisation.getPlayer();

			Text.setTextSize();
			Text.drawText("Ressourcen: \t" + ((int)civ.ress).ToString(), 10.0f, 10.0f);
			Text.drawText("Forschung:\t" + ((int)civ.science).ToString(), 10.0f, 35.0f);//, 20.0f, 0.0f, 1.0f, 0.5f, 0.25f);
			Text.drawText("Bevölkerung:\t" + civ.population.ToString() + " / " + civ.max_population.ToString(), 10.0f, 60.0f);//, 20.0f, 1.0f, 0.0f, 1.0f, 0.5f);

			Text.drawText("FPS: " + fps.ToString() + "\n1/FPS: " + (100.0f / (float)fps).ToString() + " ms von 16 ms\nParticles rendered: " + Particle.rendered_count.ToString(), GameWindow.active.width - 300, 100.0f);

			//manager.drawBox(500.0f, 100.0f, 100.0f, 100.0f, 0.5f, 1.0f, 1.0f, 0.25f);

			//Frames per second
			fps2++;
			msec2 = System.Environment.TickCount;
			if (msec2 - msec >= 1000)
			{
				msec = msec2;
				fps = fps2;
				fps2 = 0;
			}

			if (selection_box != null && Math.Abs(selection_box.z)+Math.Abs(selection_box.w) > 0.000001f)
			{
				Primitives.setColor(1.0f, 1.0f, 1.0f, 0.125f);
				Primitives.setLineWidth(2.0f);
				vec4 box = new vec4(Space.spaceToScreenX(selection_box.x), Space.spaceToScreenY(selection_box.y), Space.spaceToScreenX(selection_box.z), Space.spaceToScreenY(selection_box.w));
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



		static void select()
		{


			sel_planet = picked_planet;
			sel_index = picked_index;
			if (!GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.ShiftLeft))
			{
				foreach (Unit u in sel_u) { u.is_selected = false; }
				sel_u.Clear();
			}
			if (picked_unit != null && !sel_u.Contains(picked_unit)) { sel_u.Add(picked_unit); picked_unit.is_selected = true; }


		}



		static public void pick()
		{
			float mSpaceX = Space.screenToSpaceX(GameWindow.mouse_pos.x);
			float mSpaceY = Space.screenToSpaceY(GameWindow.mouse_pos.y);


			picked_planet = null;
			picked_index = -1;
			picked_unit = null;
			//Planeten picken
			foreach (Planet pl in Planet.planets)
			{
				double delx = mSpaceX - pl.x;//(manager.MouseX() - manager.width *0.5) - (pl.x - Space.scroll_x)*Space.zoom;
				double dely = mSpaceY - pl.y;//(manager.MouseY() - manager.height*0.5) - (pl.y - Space.scroll_y)*Space.zoom;
				double dist = delx * delx + dely * dely;
				if (dist < pl.size * pl.size * 625.0)
				{
					picked_planet = pl;

					if (Space.zoom > 0.3 && dist > pl.size * pl.size * 225.0)
					{
						double alpha = Math.Atan2(dely, delx) / (2.0 * Math.PI);
						picked_index = (int)Math.Floor(((alpha + 1.0) % 1.0) * (double)pl.size);
					}
					return;
				}
			}

			
			Sector sec = Sector.get(mSpaceX, mSpaceY);
			//Schiffe auswählen durch direkt drauf klicken
			foreach (List<Unit> list in sec.ships)
			{
				foreach (Unit u in list)
				{
					if (Math.Sqrt((u.x - mSpaceX) * (u.x - mSpaceX) + (u.y - mSpaceY) * (u.y - mSpaceY)) < u.getPrototype().shape_scaling * u.getPrototype().shape.radius + 20.0f)
					{
						picked_unit = u;
						return;
					}
				}
			}


		}

		static bool mouseOverInterface()
		{
			if (sel_planet != null && sel_index >= 0)
			{
				if (MouseInRect(10, GameWindow.active.height - 110, 100, 100)) return true;

				Civilisation player = Civilisation.getPlayer();
				List<Blueprint> options = sel_planet.ListOfBuildables(sel_index, player);
				int index = 0, hover = -1;
				foreach (Blueprint bp in options)
				{
					float drx = 115.0f + (float)(index / 2) * 50.0f;
					float dry = (float)(GameWindow.active.height - 110) + 52.0f * (float)(index % 2);
					if (MouseInRect(drx, dry, 48.0f, 48.0f)) return true;
					index++;
				}

				byte sub_interface = 0;

				if (sel_planet.infra[sel_index] != null && sel_planet.infra[sel_index].owner == player)// && sel_planet.infra[sel_index].status == Building.STATUS_NONE)
				{
					switch (sel_planet.infra[sel_index].bp.specials)
					{
						case Blueprint.SPECIAL_SHIPS:
							sub_interface = Blueprint.SPECIAL_SHIPS;
							break;

						case Blueprint.SPECIAL_RESEARCH:
							sub_interface = Blueprint.SPECIAL_RESEARCH;
							break;
					}
				}


				// Eigene Schiffswerft, in der Schiffe gebaut werden können
				if (sub_interface == Blueprint.SPECIAL_SHIPS)
				{
					List<Prototype> options2 = sel_planet.infra[sel_index].ListOfPrototypes(player);

					index = 0; hover = -1;
					foreach (Prototype prot in options2)
					{
						float drx = 115.0f + (float)(options.Count / 2) * 50.0f + (float)(index / 2) * 50.0f;
						float dry = (float)(GameWindow.active.height - 110) + 52.0f * (float)(index % 2);
						float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
						if (MouseInRect(drx, dry, 48.0f, 48.0f)) return true;
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
					float drx = 15.0f + (float)(index) * 50.0f;
					float dry = (float)(GameWindow.active.height - 60);
					if (MouseInRect(drx, dry, 48.0f, 48.0f))
					{
						return true;
					}
					index++;
				}
			}

			return false;
		}











		static void draw_planet_info()
		{
			if (sel_planet != null && (sel_index > -1))
			{
				draw_land_info();
				return;
			}

			if (picked_planet == null) return;

			float drawX = (float)Space.spaceToScreenX(picked_planet.x + (float)picked_planet.size * 25.0f) + 20.0f;
			float drawY = (float)Space.spaceToScreenY(picked_planet.y);

			string text = picked_planet.getTerrainType(picked_index);// +" (" + picked_planet.name + ")";
			if (picked_index > -1 && picked_planet.infra[picked_index] != null)
			{
				text = picked_planet.infra[picked_index].bp.name + " (" + text + ")\n" +
					picked_planet.infra[picked_index].bp.description[0] + "\n" +
					picked_planet.infra[picked_index].bp.description[1] + "\n" +
					picked_planet.infra[picked_index].bp.description[2];
			}
			else if (picked_index == -1)
			{
				text = "Planet:\t" + picked_planet.name + "\nKlima:\t" + picked_planet.getClimate() + "\nGröße:\t" + picked_planet.size.ToString() + " Felder\n" + picked_planet.getDominantCivName();
			}


			//manager.drawText(text, manager.MouseX() + 20, manager.MouseY());//, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
			if (Space.zoom < 0.3 || sel_planet != null)
			{
				Text.drawText(text, drawX, drawY - 10.0f * 4.0f);//, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
			}
			else
			{
				
				Text.drawText(text, GameWindow.mouse_pos.x + 20, GameWindow.mouse_pos.y);//, 20.0f, 0.0f, 0.0f, 0.0f, 0.5f);
			}

		}
		static void draw_land_info()
		{
			byte sub_interface = 0;

			Primitives.setColor(COL_STD_I, COL_STD_I, COL_STD_I, COL_STD_ALPHA);
			Primitives.drawRect(10, GameWindow.active.height - 110, 100, 100);
			//manager.drawBox(110, manager.height - 105, 200, 100, 0.125f, 0.125f, 0.125f, 0.85f);
			string text = "";

			if (sel_planet.infra[sel_index] != null)
			{
				text = sel_planet.infra[sel_index].bp.name + " (" + sel_planet.infra[sel_index].owner.name + ")\nGesundheit: " + ((int)sel_planet.infra[sel_index].health).ToString() + "/" + ((int)sel_planet.infra[sel_index].health_max).ToString() + "";
				//manager.drawText(text, 5, manager.height - 100 - manager.textHeight(text));

				sel_planet.infra[sel_index].bp.shape.deferred_draw(10, GameWindow.active.height - 80, 100, 100);

				float gr_r = 0.5f, gr_g = 0.5f, gr_b = 0.5f;
				if (sel_planet.terra[sel_index] == Planet.TERRA_WATER)
				{
					gr_r = 0.5f;
					gr_g = 0.5f;
					gr_b = 1.05f;
				}
				Primitives.setColor(gr_r, gr_g, gr_b, 0.25f);
				Primitives.drawRect(10, GameWindow.active.height - 30 + sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 100, 20.0f - sel_planet.infra[sel_index].bp.shape.del_y * 50.0f);
				//manager.drawBox(0, manager.height - 10 + sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 100, -10.0f, gr_r, gr_g, gr_b, 0.25f);
				//manager.drawBox(0, manager.height - 20 + sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 100, 20.0f - sel_planet.infra[sel_index].bp.shape.del_y * 50.0f, 0.5f, 0.5f, 0.5f, 0.25f);
				//sel_planet.infra[sel_index].bp.shape.draw(0.0f, 0.0f, 100, 100);

				if (sel_planet.infra[sel_index].owner == Civilisation.getPlayer())// && sel_planet.infra[sel_index].status == Building.STATUS_NONE)
				{
					switch (sel_planet.infra[sel_index].bp.specials)
					{
						case Blueprint.SPECIAL_SHIPS:
							sub_interface = Blueprint.SPECIAL_SHIPS;
							break;

						case Blueprint.SPECIAL_RESEARCH:
							sub_interface = Blueprint.SPECIAL_RESEARCH;
							break;
					}
				}



			}
			else
			{
				text = sel_planet.getTerrainType(sel_index) + " (" + sel_planet.name + ")";
				//manager.drawText(text, 5, manager.height - 100 - manager.textHeight(text));
				Planet.terra_image[sel_planet.terra[sel_index]].deferred_draw(10, GameWindow.active.height - 80, 100, 100);
				//Planet.terra_image[sel_planet.terra[sel_index]].draw(manager.width - 50, 0, 9, 9);
			}

			Civilisation player = Civilisation.getPlayer();
			List<Blueprint> options = sel_planet.ListOfBuildables(sel_index, player);
			int index = 0, hover = -1;
			foreach (Blueprint bp in options)
			{
				float drx = 115.0f + (float)(index / 2) * 50.0f;
				float dry = (float)(GameWindow.active.height - 110) + 52.0f * (float)(index % 2);
				float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
				if (MouseInRect(drx, dry, 48.0f, 48.0f))
				{
					hover = index;
					if (bp.cost <= player.ress)
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
				if (options[hover].getCost() <= player.ress) cres = "&g";
				text = "&h" + options[hover].name +
						"\n" + cres + "Ressourcen:" + options[hover].getCost().ToString().PadLeft(4, ' ') +
						"\n" + cene + "Energie:\t " + options[hover].getEnergyNeeds().ToString().PadLeft(4, ' ') +
						"\n&n" + options[hover].description[0] + "\n" + options[hover].description[1] + "\n" + options[hover].description[2];

				if (mhl)
				{
					if (sel_planet.infra[sel_index] == null && player.ress >= options[hover].getCost())
					{
						sel_planet.build(sel_index, options[hover], player);
					}
					else if (sel_planet.infra[sel_index] != null && player.ress >= (options[hover].getCost() - sel_planet.infra[sel_index].bp.getCost()))
					{
						sel_planet.upgrade(sel_index, options[hover]);
					}
				}
			}















			// Eigene Schiffswerft, in der Schiffe gebaut werden können
			if (sub_interface == Blueprint.SPECIAL_SHIPS)
			{
				List<Prototype> options2 = sel_planet.infra[sel_index].ListOfPrototypes(player);

				index = 0; hover = -1;
				foreach (Prototype prot in options2)
				{
					float drx = 115.0f + (float)(options.Count / 2) * 50.0f + (float)(index / 2) * 50.0f;
					float dry = (float)(GameWindow.active.height - 110) + 52.0f * (float)(index % 2);
					float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
					if (MouseInRect(drx, dry, 48.0f, 48.0f))
					{
						hover = index;
						if (prot.cost <= player.ress)
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

					if ((sel_planet.infra[sel_index].production.Count > 0) && (sel_planet.infra[sel_index].production[0] == prot))
					{
						Primitives.setColor(COL_STD_I, COL_STD_I, COL_STD_I, COL_STD_ALPHA);
						Primitives.setDepth(Primitives.getDepth() - 0.1f);
						Primitives.drawRect(drx, dry, 47.5f * sel_planet.infra[sel_index].production_timer, 47.5f);
						Primitives.setDepth(Primitives.getDepth() + 0.1f);
						Text.setTextSize(15.0f);
						Text.drawText(((int)(sel_planet.infra[sel_index].production_timer * 100.0)).ToString() + "%", drx, dry);
					}

					prot.shape.deferred_drawFit(drx, dry, 47.5f, 47.5f);

					int ct = sel_planet.infra[sel_index].getProductionCount(prot);
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
						if ((sel_planet.infra[sel_index].status == Building.STATUS_NONE || sel_planet.infra[sel_index].status == Building.STATUS_UNDERCONSTRUCTION) && player.ress >= options2[hover].getCost())
						{
							sel_planet.infra[sel_index].produceUnit(options2[hover]);
							if (GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.ShiftLeft) || GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.ShiftRight))
							{
								if (player.ress >= options2[hover].getCost()) sel_planet.infra[sel_index].produceUnit(options2[hover]);
								if (player.ress >= options2[hover].getCost()) sel_planet.infra[sel_index].produceUnit(options2[hover]);
								if (player.ress >= options2[hover].getCost()) sel_planet.infra[sel_index].produceUnit(options2[hover]);
								if (player.ress >= options2[hover].getCost()) sel_planet.infra[sel_index].produceUnit(options2[hover]);
							}
						}

					}
					else if (mhr)
					{
						if ((sel_planet.infra[sel_index].status == Building.STATUS_NONE || sel_planet.infra[sel_index].status == Building.STATUS_UNDERCONSTRUCTION))
						{
							sel_planet.infra[sel_index].abortUnit(options2[hover]);
							if (GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.ShiftLeft) || GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.ShiftRight))
							{
								sel_planet.infra[sel_index].abortUnit(options2[hover]);
								sel_planet.infra[sel_index].abortUnit(options2[hover]);
								sel_planet.infra[sel_index].abortUnit(options2[hover]);
								sel_planet.infra[sel_index].abortUnit(options2[hover]);
							}
						}
					}
				}
			}

			Text.setTextSize(20.0f);
			Text.drawText(text, 10, GameWindow.active.height - 120 - Text.textHeight(text));

		}


		public static void draw_gui_units()
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

				float drx = 15.0f + (float)(index) * 50.0f;
				float dry = (float)(GameWindow.active.height - 60);// +52.0f * (float)(index % 2);
				float boxr = COL_STD_I, boxg = COL_STD_I, boxb = COL_STD_I, boxa = COL_STD_ALPHA2;
				if (MouseInRect(drx, dry, 48.0f, 48.0f))
				{
					hover = i;
					boxr = COL_YES_R; boxg = COL_YES_G; boxb = COL_YES_B; //boxa = COL_STD_ALPHA;
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






		public static bool MouseInRect(float x0, float y0, float width, float height)
		{
			if (GameWindow.mouse_pos.x < x0 || GameWindow.mouse_pos.y < y0 || GameWindow.mouse_pos.x > x0 + width || GameWindow.mouse_pos.y > y0 + height) return false;
			return true;
		}




	}
}
