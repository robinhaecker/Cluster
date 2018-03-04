using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using OpenTK.Graphics.OpenGL;

using Cluster.Rendering.Draw2D;
using Cluster.Rendering.Appearance;
using Cluster.math;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Behaviour;

namespace Cluster.GameMechanics.Universe
{
	class Unit
	{

		public static List<Unit> units = new List<Unit>();
		public static List<Unit> removed = new List<Unit>();
		static int id_counter;

		public static byte EFFECT_NONE = 0,
							EFFECT_SPAWNING = 1,
							EFFECT_EXPLODE = 2,
							EFFECT_CAMOUFLAGE = 3,
							EFFECT_STUNNED = 4;


		public static int display_shield_count = 0;
		public const int DISPLAY_SHIELD_VARIABLES_XYSCALE = 3; //x, y, scalation
		public const int DISPLAY_SHIELD_VARIABLES_PERCENTAGES = 2; //percentage_shield, percentage_health
		public const int DISPLAY_SHIELD_VARIABLES_COLOR = 4; //r,g,b
		public static float[] shield_gl_array0 = new float[DISPLAY_SHIELD_VARIABLES_XYSCALE * Civilisation.TOTAL_MAX_POPULATION];
		public static float[] shield_gl_array1 = new float[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * Civilisation.TOTAL_MAX_POPULATION];
		public static float[] shield_gl_array2 = new float[DISPLAY_SHIELD_VARIABLES_COLOR * Civilisation.TOTAL_MAX_POPULATION];
		public static int shield_gl_data = 0, buf_sh0, buf_sh1, buf_sh2;






		// Instance-specific variables
		int id;
		Prototype proto;
		Civilisation owner;

		public float x, y, rot, v;
		float health, shield;

		public bool is_selected;
		float reload_timer;
		float got_hit_timer;
		float current_effect_timer;
		byte current_effect;

		Sector sector;
		Target target;
		public Unit enemy;
		public byte inrange;
		public float enemy_distance;


		// Constructors
		public Unit(Prototype p, Civilisation civ, float x, float y, float alpha = 0.0f)
		{
			this.id = id_counter;
			id_counter++;

			this.proto = p;
			this.owner = civ;
			this.x = x;
			this.y = y;
			this.rot = alpha;
			this.v = 0.0f;

			this.health = proto.getHealth(civ);
			this.shield = proto.getShields(civ);

			reload_timer = 0.0f;
			got_hit_timer = 0.0f;
			current_effect_timer = 1.0f;
			current_effect = EFFECT_SPAWNING;
			target = new Target();
			target.update(x, y);

			units.Add(this);
		}

		public Unit(Prototype p, Building b)
		{
			this.id = id_counter;
			id_counter++;

			Planet pl = b.getPlanet();

			this.proto = p;
			this.owner = b.owner;
			this.rot = b.getSpotRotation();
			this.x = pl.x + (float)Math.Cos(rot) * (pl.size * 20.0f + 75.0f);
			this.y = pl.y + (float)Math.Sin(rot) * (pl.size * 20.0f + 75.0f);
			this.v = 0.0f;

			reload_timer = 0.0f;
			got_hit_timer = 0.0f;
			current_effect_timer = 1.0f;
			current_effect = EFFECT_SPAWNING;
			target = new Target();
			target.update(pl, -1, b.getSpotID());

			this.health = proto.getHealth(b.owner);
			this.shield = proto.getShields(b.owner);

			units.Add(this);
		}

		
		public static void render()
		{
			display_shield_count = 0;

			GL.LineWidth(1.5f);
			GL.Disable(EnableCap.DepthTest);
			//Shader shader = Mesh.getShader();
			Space.unit_shader.bind();
			foreach (Unit u in Unit.units)
			{
				float alpha = 1.0f;
				float scale = u.proto.shape_scaling * 5.0f;
				if (u.current_effect == EFFECT_SPAWNING) scale *= Math.Min(1.0f, Math.Max(0.0f, 1.0f - u.current_effect_timer));
				if (u.current_effect == EFFECT_EXPLODE) alpha = u.current_effect_timer * 0.5f;
				float selected = 0.0f;

				
				GL.Uniform3(Space.unit_shader.getUniformLocation("pos"), (float)(u.x - Space.scroll_x), (float)(u.y - Space.scroll_y), (float)u.rot);
				GL.Uniform4(Space.unit_shader.getUniformLocation("col"), u.owner.r, u.owner.g, u.owner.b, alpha);
				GL.Uniform3(Space.unit_shader.getUniformLocation("scale"), scale, (float)(Space.zoom * GameWindow.active.mult_x), (float)(Space.zoom * GameWindow.active.mult_y));
				//GL.Uniform3(Space.unit_shader.getUniformLocation("shield"), u.health / u.max_health(), u.shield / u.max_shields(), selected);


				//manager.mesh_shader.id.SetUniform1(manager.gl, "pos_x", (x + width * 0.5f) * manager.mult_x * 2.0f - 1.0f);
				//manager.mesh_shader.id.SetUniform1(manager.gl, "pos_y", (y + height * 0.5f * (1.0f + del_y)) * manager.mult_y * 2.0f - 0.0f);
				//manager.mesh_shader.id.SetUniform3(manager.gl, "scale", width * manager.mult_x, height * manager.mult_y, clamp);

				GL.BindVertexArray(u.proto.shape.gl_data);
				GL.DrawArrays(PrimitiveType.Lines, 0, u.proto.shape.num_lines * 2);


				// Daten für Schuztschilddarstellung setzen.
				if (u.isAlive() && (u.got_hit_timer > 0.0f || u.is_selected))
				{
					shield_gl_array0[DISPLAY_SHIELD_VARIABLES_XYSCALE * display_shield_count + 0] = u.x;
					shield_gl_array0[DISPLAY_SHIELD_VARIABLES_XYSCALE * display_shield_count + 1] = u.y;
					shield_gl_array0[DISPLAY_SHIELD_VARIABLES_XYSCALE * display_shield_count + 2] = scale;
					
					shield_gl_array1[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * display_shield_count + 0] = u.getHealthFraction();
					shield_gl_array1[DISPLAY_SHIELD_VARIABLES_PERCENTAGES * display_shield_count + 1] = u.getShieldFraction();

					if (u.is_selected)
					{
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 0] = u.owner.r * 0.75f + 0.25f;
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 1] = u.owner.g * 0.75f + 0.25f;
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 2] = u.owner.b * 0.75f + 0.25f;
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 3] = 1.5f;
					}
					else
					{
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 0] = u.owner.r;
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 1] = u.owner.g;
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 2] = u.owner.b;
						shield_gl_array2[DISPLAY_SHIELD_VARIABLES_COLOR * display_shield_count + 3] = Math.Min(1.0f, u.got_hit_timer);
					}
					display_shield_count++;
				}
			}



			// Schutzschilde zeichnen.
			if (display_shield_count > 0)
			{
				if (shield_gl_data == 0)
				{
					shield_gl_data = GL.GenVertexArray();
					buf_sh0 = GL.GenBuffer();
					buf_sh1 = GL.GenBuffer();
					buf_sh2 = GL.GenBuffer();
				}
				GL.BindVertexArray(shield_gl_data);

				GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh0);
				GL.BufferData(BufferTarget.ArrayBuffer, shield_gl_array0.Length * sizeof(float), shield_gl_array0, BufferUsageHint.StaticDraw);
				GL.EnableVertexArrayAttrib(shield_gl_data, 0);
				GL.VertexAttribPointer(0, DISPLAY_SHIELD_VARIABLES_XYSCALE, VertexAttribPointerType.Float, false, 0, 0);

				GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh1);
				GL.BufferData(BufferTarget.ArrayBuffer, shield_gl_array1.Length * sizeof(float), shield_gl_array1, BufferUsageHint.StaticDraw);
				GL.EnableVertexArrayAttrib(shield_gl_data, 1);
				GL.VertexAttribPointer(1, DISPLAY_SHIELD_VARIABLES_PERCENTAGES, VertexAttribPointerType.Float, false, 0, 0);

				GL.BindBuffer(BufferTarget.ArrayBuffer, buf_sh2);
				GL.BufferData(BufferTarget.ArrayBuffer, shield_gl_array2.Length * sizeof(float), shield_gl_array2, BufferUsageHint.StaticDraw);
				GL.EnableVertexArrayAttrib(shield_gl_data, 2);
				GL.VertexAttribPointer(2, DISPLAY_SHIELD_VARIABLES_COLOR, VertexAttribPointerType.Float, false, 0, 0);


				Space.unit_shield_shader.bind();
				GL.Uniform3(Space.unit_shield_shader.getUniformLocation("viewport"), GameWindow.active.mult_x, GameWindow.active.mult_y, Space.animation);
				GL.Uniform3(Space.unit_shield_shader.getUniformLocation("scroll"), Space.scroll_x, Space.scroll_y, Space.zoom);
				GL.DrawArrays(PrimitiveType.Points, 0, display_shield_count);
			}


			GL.BindVertexArray(0);
			GL.Enable(EnableCap.DepthTest);
			//manager.null_vao.Bind(manager.gl);
			
		}


		public static void update(float t = 1.0f)
		{
			Sector.getShipEnemies(); // Nächste Gegner suchen.

			foreach (Civilisation civ in Civilisation.data)
			{
				civ.population = 0;
			}
			foreach (Unit u in Unit.units)
			{
				u.owner.population += u.proto.getPopulation();
				u.simulate(t);
			}
			/*
			foreach (Sector sec in Sector.data)
			{
				if (sec == null) continue;
				sec.thread = new Thread(new ThreadStart(sec.sim_units));
				sec.thread.Start();
			}
			/*
			foreach (Sector sec in Sector.data)
			{
				if (sec == null) continue;
				sec.thread.Join();
			}*/

			// Gekillte Einheiten aus Liste entfernen. Geht nämlich nur, wenn nicht gerade in der Schleife. Also merken und jetzt löschen.
			removeKilledUnits();

		}

		static void removeKilledUnits()
		{
			foreach (Unit u in Unit.removed)
			{
				u.sector.removeUnit(u);
				Unit.units.Remove(u);
			}
			Unit.removed.Clear();
		}


		public void simulate(float t)
		{
			updateSector();
			if (health > 0) sim_alive(t);
			else sim_death(t);

			got_hit_timer = Math.Max(0.0f, got_hit_timer - t * 0.001f);
			x += v * (float)Math.Cos(rot) * t * 0.002f;
			y += v * (float)Math.Sin(rot) * t * 0.002f;

			if (current_effect != EFFECT_NONE)
			{
				current_effect_timer -= t * 0.001f;
				if (current_effect_timer <= 0.0f)
				{
					current_effect_timer = 0.0f;
					if (current_effect == EFFECT_EXPLODE) // Wenn fertig explodiert, dann Einheit aus Liste löschen.
					{
						remove();
						return;
					}
					current_effect = EFFECT_NONE;
				}
			}
		}

		void remove()
		{
			removed.Add(this);
		}

		void sim_alive(float t)
		{
			float maxshield = proto.getShields(owner);
			shield = Math.Min(shield + t * 0.002f * (float)Math.Sqrt(maxshield), maxshield);
			reload_timer = Math.Max(reload_timer - t * 0.005f, 0.0f);

			vec2 deltaMission = target.getPosition() - new vec2((float)x, (float)y);
			vec2 delta = target.getWaypoint() - new vec2((float)x, (float)y);

			if(delta.length() < 50.0) // Nah am Ziel
			{
				delta = target.nextWaypoint() - new vec2((float)x, (float)y);
			}


			// Schiff richtet sich auf sein Ziel aus (und schießt evtl)
			if (current_effect != EFFECT_SPAWNING && current_effect != EFFECT_STUNNED)
			{
				double rotationSpeed = 0.0;
				if (enemy != null)
				{
					bool aimtowards = false;
					if ((proto.weapon_type == Prototype.WEAPON_LASER && (enemy_distance < proto.weapon_range)) || (proto.weapon_type != Prototype.WEAPON_STD && proto.weapon_type != Prototype.WEAPON_EXPLOSIVE && (enemy_distance < proto.weapon_range) && proto.ship_class != Prototype.CLASS_HUNTER))
					{
						rotationSpeed = turnTowards(delta.x, delta.y, t * 0.001);
					}
					else
					{
						rotationSpeed = turnTowards(enemy.x - x, enemy.y - y, t * 0.001);
						if (Math.Abs(rotationSpeed) <= 0.000002 * t) { aimtowards = true; }
					}
					if ((reload_timer <= 0.0f) && (enemy_distance < proto.weapon_range) && (aimtowards || (proto.weapon_type == Prototype.WEAPON_PERSECUTE || proto.weapon_type == Prototype.WEAPON_LASER || proto.weapon_type == Prototype.WEAPON_FIND_AIM)))
					{
						fireWeapons();
					}

				}
				else
				{
					rotationSpeed = turnTowards(delta.x, delta.y, t * 0.001);
					bombardBuilding(); // Falls eins in der Nähe ist natürlich ;-)
				}

				// Beschleunigen des Raumschiffs
				sim_accellerate(t, rotationSpeed, deltaMission.length());
			}

			if (GameWindow.keyboard.IsKeyDown(OpenTK.Input.Key.D)) damage(t * 0.1f); // Nur zu Testzwecken

		}

		void sim_accellerate(float t, double rotationSpeed, float missionDistance)
		{
			float max_speed = proto.getSpeed();
			if (missionDistance < 1000.0 && proto.ship_class == Prototype.CLASS_HUNTER && !(proto.ship_class != Prototype.CLASS_WARSHIP && enemy != null) && proto.specials == Prototype.SPECIAL_ASTEROID_MINING)
			{
				//if (!orbit_enemy)v=Max(v-0.01*max_speed*t, max_speed*0.2); else
				v = Math.Max(v - 0.01f * max_speed * t, max_speed * 0.5f);
			}
			else if (Math.Abs(rotationSpeed) < 0.03) // Richtung stimmt gut? Beschleunigen.
			{
				v = Math.Min(v + 0.01f * max_speed * t, max_speed);
			}
			else if (Math.Abs(rotationSpeed) > 0.15) // Viel zu schlechte Richtung, Abbremsen
			{
				v = Math.Max(v - 0.01f * max_speed * t, max_speed * 0.1f);
			}
		}

		void sim_death(float t)
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
			if (health<=0.0f) return false;
			got_hit_timer = 3.0f;

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
				current_effect = EFFECT_EXPLODE;
				current_effect_timer = 1.0f;

				for (int i = 0; i < 50 + (int)Math.Sqrt(proto.health_max/50.0f); i++)
				{
					float speed = (float)GameWindow.random.NextDouble();
					float angle = (float)GameWindow.random.NextDouble() * 2.0f * (float)Math.PI;
					Particle p = new Particle(x, y, v * (float)Math.Cos(rot) + 100.0f * speed * (float)Math.Cos(angle), v * (float)Math.Sin(rot) + 100.0f * speed * (float)Math.Sin(angle));
					p.setColor(1.0f, (float)GameWindow.random.NextDouble() * 0.7f + speed * 0.3f, speed * 0.25f, 0.2f + (float)GameWindow.random.NextDouble()*0.7f);
				}

				return true;
			}
			return false;
		}

		void fireWeapons()
		{
			reload_timer = proto.reload_time;
			if ( inrange > 0 || (proto.weapon_type != Prototype.WEAPON_LASER) )
			{
				Shot s = new Shot(this, enemy, inrange);
				if (proto.ship_class == Prototype.CLASS_WARSHIP && inrange > 0)
				{
					s = new Shot(this, enemy, (byte)(2 - inrange));
					s.rot += (float)Math.PI * 0.25f;
					if (s.getWeaponType() == Prototype.WEAPON_LASER) s.x -= 20.0f * proto.shape_scaling;

					s = new Shot(this, enemy, (byte)(2 - inrange));
					s.rot -= (float)Math.PI * 0.25f;
					if (s.getWeaponType() == Prototype.WEAPON_LASER) s.x += 20.0f * proto.shape_scaling;
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
					ElseIf cl.weapontype<>WEAPON_BEAM Or inrange Then --> done
						Shot.Create(Self, fe, False)
						If cl.behave=CLASS_KRIEGSSCHIFF And inrange Then
							Local ws:Shot=Shot.Create(Self, fe, 2-inrange)
							If ws<>Null Then
								ws.phi:+45
								If ws.art=WEAPON_BEAM Then ws.x:+20.0*cl.scale
							EndIf
							ws=Shot.Create(Self, fe, 2-inrange)
							If ws<>Null Then
								ws.phi:-45
								If ws.art=WEAPON_BEAM Then ws.x:-20.0*cl.scale
							EndIf
						EndIf
					EndIf
			 */
		}
		void bombardBuilding()
		{
			if ( enemy != null ) {return;}

			//current_effect != EFFECT_SPAWNING && current_effect != EFFECT_STUNNED && // --> sind schon erfüllt, wenn die Methode aufgerufen wird.
			if (reload_timer <= 0.0f && proto.attack > 0.0f)
			{
				int feld;
				Planet bombard = planetInRange(out feld);
				if (bombard != null)
				{
					new Shot(this, bombard, feld);
					if (proto.weapon_type == Prototype.WEAPON_EXPLOSIVE)
					{
						new Shot(this, bombard, feld);
						new Shot(this, bombard, feld);
						new Shot(this, bombard, feld);
					}
					reload_timer = proto.reload_time;
				}
			}
		}
		Planet planetInRange(out int bombard_index)
		{
			foreach (Planet p in Planet.planets)
			{
				float dx = x - p.x, dy = y - p.y;
				float dist = (float)Math.Sqrt(dx * dx + dy * dy);
				if ((dist<1000.0f) && (dist > 20.0f*p.size + 150.0f))
				{
					int feld = (int)(((2.0 * Math.PI + Math.Atan2(dy, dx)) % (2.0 * Math.PI)) * ((float)p.size / (2.0 * Math.PI)));
					int dF = Math.Min((int)Math.Floor((float)p.size * 0.2f), 3);
					for (int f = 0; f <= dF; f++)
					{
						int tmp = (p.size + feld + f) % p.size;
						if (p.infra[tmp] != null && p.infra[tmp].owner != owner)
						{
							bombard_index = tmp;
							return p;
						}
						if (f == 0) { continue; }
						tmp = (p.size + feld - f) % p.size;
						if (p.infra[tmp] != null && p.infra[tmp].owner != owner)
						{
							bombard_index = tmp;
							return p;
						}
					}
				}

			}
			bombard_index = -1;
			return null;
			/*	Method PlanetInRange:Planet(bombardement_only:Byte Var)
		
				For Local p:Planet=EachIn Planet.list
					Local dx:Float=x-p.x, dy:Float=y-p.y
					Local dist:Float=Sqr(dx*dx+dy*dy)
					If dist<1000 And dist>30.0*p.size Then
				
						If bombardement_only Then
							Local feld:Int=Int(Float(Int(360.0+ATan2(dy, dx)) Mod 360)*Float(p.size)/360.0)
							Local dF:Int=Min(Floor(p.size/5.0), 4)
							For Local f:Int=0 To dF
								Local build:Building=p.infra[(p.size+feld+f) Mod p.size]
								If build<>Null And build.owner<>owner And owner.diplomacy[build.owner.id]=GESINNUNG_FEINDSELIG Then
									bombardement_only=Byte((p.size+feld+f) Mod p.size)
									Return p:Planet
								ElseIf f>0 Then
									build:Building=p.infra[(p.size+feld-f) Mod p.size]
									If build<>Null And build.owner<>owner Then
										bombardement_only=Byte((p.size+feld-f) Mod p.size)
										Return p:Planet
									EndIf
								EndIf
							Next
					
						Else
							Return p:Planet
						EndIf
					EndIf
				Next
				Return Null
			EndMethod
		*/
		}




		// Methods
		public Prototype getPrototype()
		{
			return proto;
		}
		public bool isDead()
		{
			if ((health <= 0.0) || (current_effect == EFFECT_EXPLODE)) return true;
			return false;
		}
		public bool isAlive()
		{
			return !isDead();
		}
		public float max_health()
		{
			return proto.health_max;
		}
		public float max_shields()
		{
			return proto.shields;
		}
		public Civilisation getOwner() { return owner; }

		public void updateSector()
		{
			if (sector != null)
			{
				if (sector.containsPoint(x, y)) return;
				sector.removeUnit(this);
				sector = null;
			}

			sector = Sector.get((float)x, (float)y);
			sector.addUnit(this);
		}
		public double turnTowards(float delta_x, float delta_y, double maxAngle = Math.PI)
		{
			rot = (rot + (float)Math.PI * 2.0f) % ((float)Math.PI * 2.0f);
			double phi = (Math.Atan2(delta_y, delta_x)  + Math.PI * 2.0 - rot) % (Math.PI * 2.0f);
			if (phi > Math.PI) phi -= Math.PI * 2.0;
			phi = Math.Max(-maxAngle, Math.Min(maxAngle, phi));

			rot += (float)phi;
			return phi;
		}
		
		public float getHealthFraction()
		{
			return health / proto.getHealth(owner);
		}
		public float getShieldFraction()
		{
			return shield / proto.getShields(owner);
		}
		
	}
}
