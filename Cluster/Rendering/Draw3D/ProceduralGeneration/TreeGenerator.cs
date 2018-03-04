using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;



namespace Cluster.Rendering.Draw3D.ProceduralGeneration
{


	class TreeGenerator
	{





		static public Model Generate(int seed = -1)
		{
			Model tree = new Model();//"grass.krak");
			//tree.load_krak(GameWindow.BASE_FOLDER + "/models/" + "grass.krak");

			float[] genom = genEtics(seed);

			int offset_branches = 15;

			int detail = (int)genom[offset_branches + 0];				// Anzahl an Vertices pro "Baumscheibe"
			int num_branches = (int)genom[offset_branches + 1];			// Anzahl an Unterteilungen
			float probability_split = genom[offset_branches + 2];	// Wahrscheinlichkeit mehrerer Äste pro Unterteilung
			float probability_unbranch = genom[offset_branches + 3];// Wahrscheinlichkeit eines vorzeitigen Astendes.
			int split_count = (int)genom[offset_branches + 4];		// Anzahl Äste bei Aufspaltung
			float split_angle = genom[offset_branches + 5];		// Winkelöffnung bei Unterteilung
			float bending_angle = genom[offset_branches + 6];	// Biegung ohne Unterteilung
			float branch_length = genom[offset_branches + 7];	// Länge bis zur nächsten Unterteilung
			float thickness = genom[offset_branches + 8];		// Dicke des Baumstamms zu Beginn.
			float updrive = genom[offset_branches + 9];			// Wie stark strebt der Baum nach oben?
			float decline_const = genom[offset_branches + 10];
			float decline_mult = genom[offset_branches + 11];
			vec3 col1 = new vec3(genom[offset_branches + 12], genom[offset_branches + 13], genom[offset_branches + 14]);
			vec3 col2 = new vec3(genom[offset_branches + 15], genom[offset_branches + 16], genom[offset_branches + 17]);
			float color_pattern = genom[offset_branches + 18];
			float leaves_on_trunk = genom[offset_branches + 19];

			/*
			int detail = 5;					// Anzahl an Vertices pro "Baumscheibe"
			int num_branches = 10;			// Anzahl an Unterteilungen
			float probability_split = 0.3f;	// Wahrscheinlichkeit mehrerer Äste pro Unterteilung
			float probability_unbranch = 0.25f;	// Wahrscheinlichkeit eines vorzeitigen Astendes.
			int split_count = 2;			// Anzahl Äste bei Aufspaltung
			float split_angle = 50.0f;		// Winkelöffnung bei Unterteilung
			float bending_angle = 20.0f;	// Biegung ohne Unterteilung
			float branch_length = 0.25f;	// Länge bis zur nächsten Unterteilung
			float thickness = 0.15f;		// Dicke des Baumstamms zu Beginn.
			float updrive = 0.15f;			// Wie stark strebt der Baum nach oben?
			float decline_const = 0.01f;
			float decline_mult = 0.9f;
			*/
			vec3 pos = new vec3();
			vec3 dir = new vec3(((float)GameWindow.random.NextDouble()-0.5f)*0.25f, 2.0f, ((float)GameWindow.random.NextDouble()-0.5f)*0.25f).normalize();
			int branch_id = 1;

			List<float[]> branches = new List<float[]>();
			List<float[]> leaves = new List<float[]>();

			branches.Add(genBranch(-1, 0, pos, dir, thickness));


			//Generierung des "Grundgerüsts"
			for(int i = 0; i < num_branches; i++)
			{
				bool dieout_possible = false;
				foreach(float[] b in branches)
				{
					if ((dieout_possible || b[8]<= 0.03f) && (probability_unbranch > (float)GameWindow.random.NextDouble())) continue;
					if(b[1] == i && branches.Count < num_branches)
					{
						pos.x = b[2]; pos.y = b[3]; pos.z = b[4];
						dir.x = b[5]; dir.y = b[6]; dir.z = b[7];
						//dir = dir.normalize();

						int num_spawns = 1;
						if (probability_split > (float)GameWindow.random.NextDouble() && (num_branches - branches.Count > 3)) num_spawns = Math.Min(GameWindow.random.Next(split_count - 2) + 2, num_branches - branches.Count);

						vec3 b1 = new vec3(1.0f, 0.0f, 0.0f);
						vec3 b2 = (dir % b1).normalize();
						b1 = (dir % b2).normalize();
						
						float angle1 = (float)GameWindow.random.NextDouble() * 2.0f * (float)Math.PI;
						float angle2 = 0.0f;
						if (num_spawns > 1)
						{
							angle2 = (((float)GameWindow.random.NextDouble() - 0.0f) * 0.25f + 1.0f) / 180.0f * (float)Math.PI * split_angle;
						}
						else
						{
							angle2 = (((float)GameWindow.random.NextDouble() - 0.5f) * 0.25f + 1.0f) / 180.0f * (float)Math.PI * bending_angle;
						}

						for(int s = 0; s < num_spawns; s++)
						{
							b[9] = 1.0f;
							vec3 perp = (float)Math.Cos(angle1) * b1 + (float)Math.Sin(angle1) * b2;
							angle1 += 2.0f * (float)Math.PI * (float) s / (float) num_spawns;

							vec3 newdir = ((float)Math.Cos(angle2) * dir + (float)Math.Sin(angle2) * perp + new vec3(0.0f, updrive, 0.0f)).normalize();

							leaves.Add(genBranch((int)b[1], (int)branch_id, pos+newdir*branch_length, newdir, Math.Max(0.03f, b[8]*decline_mult - decline_const)*(0.5f + 0.5f/(float)num_spawns) )); branch_id++;
						}
						dieout_possible = true;

					}
				}
				branches.AddRange(leaves);
				leaves.Clear();

			}

			num_branches = branches.Count;

			vec3 col = new vec3(1.0f, 1.0f, 1.0f);
			Vertex[,] v = new Vertex[num_branches+1, detail];

			// Erstellung der Vertices aus dem Grundgerüst
			foreach(float[] b in branches)
			{
				int j = (int)b[1];

				
				pos.x = b[2]; pos.y = b[3]; pos.z = b[4];
				dir.x = b[5]; dir.y = b[6]; dir.z = b[7];
				dir = dir.normalize();
				vec3 b1 = new vec3(1.0f, 0.0f, 0.0f);
				vec3 b2 =  (dir % b1).normalize();
				b1 = (dir % b2).normalize();

				for (int i = 0; i < detail; i++)
				{
					float alpha = (float)i / (float)detail *2.0f * (float)Math.PI;
					vec3 vert = pos + ((float)Math.Cos(alpha)*b1 + (float)Math.Sin(alpha)*b2) * (b[8] * 0.5f + 0.02f);
					//Console.WriteLine("pos = "+vert.x.ToString() + " " + vert.y.ToString() + " " +vert.z.ToString());
					//col = new vec3((float)GameWindow.random.NextDouble() * 0.5f + 0.5f, (float)GameWindow.random.NextDouble() * 0.3f + 0.3f, (float)GameWindow.random.NextDouble() * 0.2f + 0.1f);
					float rand_blend = (float)GameWindow.random.NextDouble();
					vec3 color_pref = col1;
					if((j % 2) == 0) color_pref = col2;
					col = (col1 * rand_blend + col2 * (1.0f - rand_blend)) * (1.0f - color_pattern) + color_pattern * color_pref;
					v[j, i] = new Vertex(vert.x, vert.y, vert.z, col.x, col.y, col.z);
					v[j, i].addFrame(vert.x, vert.y, vert.z);
					tree.AddVertex(v[j, i]);
				}

				//Falls Ende des Baumes, dann Blätter generieren
				if (b[9] < 0 || leaves_on_trunk > (float)GameWindow.random.NextDouble())
				{
					Vertex connect = addLeaves(tree, genom, 0.5f*(v[j, 0].getPos()+v[j, detail/2].getPos()), dir, b[8]+0.025f);
					if (connect != null && b[9] < 0)
					{
						for (int i = 0; i < detail + 1; i++)
						{
							tree.AddTriangle(new Triangle(v[j, i % detail], connect, v[j, (i + 1) % detail]));
						}
					}
				}

			}

			/*
			Vertex middle = new Vertex(0.0f, 1.0f, 0.0f);
			middle.addFrame(0.0f, 1.0f, 0.0f);
			tree.AddVertex(middle);

			Vertex middle2 = new Vertex(2.0f, 2.0f, 0.0f);
			middle2.addFrame(2.0f, 2.0f, 0.0f);
			tree.AddVertex(middle2);
			*/

			foreach (float[] b in branches)
			{

				/*Console.WriteLine("Branch: ");
				Console.WriteLine(b[2].ToString());
				Console.WriteLine(b[3].ToString());
				Console.WriteLine(b[4].ToString());*/
				if ((int)b[0] >= 0)
				{
					int u = (int)b[0], uu = (int)b[1];


					int detwist = 0;
					float twistlength = 100000.0f;
					for (int i = 0; i < detail; i++)
					{
						float dst = (v[u, 0].getPos() - v[uu, (i) % detail].getPos()).length();
						if (dst < twistlength)
						{
							twistlength = dst;
							detwist = i;
						}
					}

					for (int i = 0; i < detail+1; i++)
					{
						tree.AddTriangle(new Triangle(v[u, (i + 1) % detail], v[u, i % detail], v[uu, (i + 1 + detwist) % detail]));
						tree.AddTriangle(new Triangle(v[u, i % detail], v[uu, (i + detwist) % detail], v[uu, (i + 1 + detwist) % detail]));
					}
				}
			}

			tree.finishPreparation();
			//Console.WriteLine("Number of frames (generated Tree): "+tree.getFrameCount().ToString());

			return tree;
		}



		static float[] genBranch(int parent_id, int self_id, vec3 pos, vec3 dir, float size)
		{
			float[] branch = new float[10];
			branch[0] = parent_id;
			branch[1] = self_id;

			branch[2] = pos.x;
			branch[3] = pos.y;
			branch[4] = pos.z;

			branch[5] = dir.x;
			branch[6] = dir.y;
			branch[7] = dir.z;

			branch[8] = size;
			branch[9] = -1.0f;
			return branch;
		}

		static float[] genEtics(int seed = -1)
		{
			Random rnd = GameWindow.random;
			if (seed >= 0) rnd = new Random(seed);


			int offset_leaves = 0;
			int offset_branches = 15;

			float[] genes = new float[offset_branches + 20];
			//Blattform
			genes[offset_leaves + 0] = 4 + rnd.Next(4);
			genes[offset_leaves + 1] = 3 + rnd.Next(4);
			genes[offset_leaves + 2] = 1.2f + (float)rnd.NextDouble() * 0.5f;
			genes[offset_leaves + 3] = (float)rnd.NextDouble() * 1.2f - 0.4f;
			if (genes[offset_leaves + 3] > 0.7) genes[offset_leaves + 3] += (float)rnd.NextDouble();
			genes[offset_leaves + 4] = 0.3f + (float)rnd.NextDouble() * 2.0f;
			genes[offset_leaves + 5] = (float)rnd.NextDouble() * 0.04f;
			genes[offset_leaves + 6] = (float)rnd.NextDouble();
			genes[offset_leaves + 7] = ((float)rnd.NextDouble() - 0.5f) * 0.5f / genes[offset_leaves + 0];
			//Blattfarbe1
			genes[offset_leaves + 8] = (float)rnd.NextDouble() * 0.3f;
			genes[offset_leaves + 9] = (float)rnd.NextDouble();
			genes[offset_leaves + 10] = (float)rnd.NextDouble() * 0.3f;
			//Blattfarbe2
			genes[offset_leaves + 12] = (float)rnd.NextDouble() * 0.1f;
			genes[offset_leaves + 11] = (float)rnd.NextDouble() * 0.5f;
			genes[offset_leaves + 13] = (float)rnd.NextDouble() * 0.15f;

			genes[offset_leaves + 14] = (float)rnd.Next(2); // Einzelne Blätter, palmenartig


			//Stammform
			genes[offset_branches + 0] = 5;
			genes[offset_branches + 1] = 8 + rnd.Next(5);
			genes[offset_branches + 2] = (float)rnd.NextDouble() * (float)rnd.NextDouble() * 0.7f;
			if (genes[offset_branches + 2] < 0.3) genes[offset_branches + 1] -= 4;
			genes[offset_branches + 3] = (float)rnd.NextDouble();
			genes[offset_branches + 4] = 2 + rnd.Next(3);

			genes[offset_branches + 5] = 45.0f + (float)rnd.NextDouble() * 15.0f;
			genes[offset_branches + 6] = (float)rnd.NextDouble() * 35.0f;
			genes[offset_branches + 7] = 0.15f + (float)rnd.NextDouble() * 0.3f + 0.01f * genes[offset_branches + 2] * genes[offset_branches + 4];
			genes[offset_branches + 8] = 0.1f + (float)rnd.NextDouble() * 0.5f;
			genes[offset_branches + 9] = 0.2f + (float)rnd.NextDouble() * 0.4f;

			genes[offset_branches + 10] = 0.005f + ((float)rnd.NextDouble() + 0.2f) * 0.02f * 0.1f * (float)Math.Sqrt(genes[offset_branches + 8] * 10.0f) / genes[offset_branches + 2];
			genes[offset_branches + 11] = 0.95f + (float)rnd.NextDouble() * 0.05f;
			//Stammfarbe1
			genes[offset_branches + 12] = (float)rnd.NextDouble() * 0.5f + 0.5f;
			genes[offset_branches + 13] = (float)rnd.NextDouble() * 0.3f + 0.3f;
			genes[offset_branches + 14] = (float)rnd.NextDouble() * 0.2f + 0.1f;
			//Stammfarbe2
			genes[offset_branches + 15] = (float)rnd.NextDouble() * 0.7f;
			genes[offset_branches + 16] = genes[offset_branches + 15];
			genes[offset_branches + 17] = (float)rnd.NextDouble() * genes[offset_branches + 16];
			//Stammmuster
			genes[offset_branches + 18] = (float)rnd.NextDouble();
			if (genes[offset_leaves + 4] < 0.75f && genes[offset_leaves + 2] < 1.2f && genes[offset_branches + 1] < 8) genes[offset_branches + 19] = Math.Max(0.0f, (float)rnd.NextDouble() * 10.0f - 1.0f);
			
			/*
			int detail = 5;					// Anzahl an Vertices pro "Baumscheibe"
			int num_branches = 10;			// Anzahl an Unterteilungen
			float probability_split = 0.3f;	// Wahrscheinlichkeit mehrerer Äste pro Unterteilung
			float probability_unbranch = 0.25f;	// Wahrscheinlichkeit eines vorzeitigen Astendes.
			int split_count = 2;			// Anzahl Äste bei Aufspaltung
			 * 
			float split_angle = 50.0f;		// Winkelöffnung bei Unterteilung
			float bending_angle = 20.0f;	// Biegung ohne Unterteilung
			float branch_length = 0.25f;	// Länge bis zur nächsten Unterteilung
			float thickness = 0.15f;		// Dicke des Baumstamms zu Beginn.
			float updrive = 0.15f;			// Wie stark strebt der Baum nach oben?
			 * 
			float decline_const = 0.01f;
			float decline_mult = 0.9f;
			*/

			return genes;
		}


		static Vertex addLeaves(Model tree, float[] genetics, vec3 pos, vec3 dir, float scale = 1.0f)
		{
			scale = Math.Min(scale, 0.5f);
			/*
			int detail = 5;
			int height_detail = 3;
			float scalation = 6.5f;		//Skalierung
			float spikeyness = 0.20f;	//Hohe Werte: Spitzige Baumwipfel, niedrige Werte: flache Baumwipfel
			float roundness = 1.7f;		//Verhältnis zwischen höhe und breite/tiefe der Baumkrone
			float distortion = 0.025f;	//Wie stark die Vertizes von ihrer theoretischen Form versetzt werden.
			*/
			int gene_offset = 0;

			int detail = (int)genetics[gene_offset+0];
			int height_detail = (int)genetics[gene_offset + 1];
			float scalation = genetics[gene_offset + 2];		//Skalierung
			float spikeyness = genetics[gene_offset + 3];		//Hohe Werte: Spitzige Baumwipfel, niedrige Werte: flache Baumwipfel
			float roundness = genetics[gene_offset + 4];		//Verhältnis zwischen höhe und breite/tiefe der Baumkrone
			float distortion = genetics[gene_offset + 5];		//Wie stark die Vertizes von ihrer theoretischen Form versetzt werden.
			float mutation = 1.0f + 0.1f * (genetics[gene_offset + 6] - 0.5f);		//Wie stark die Eigenschaften dieser Version veränder werden. Wert zwischen 0 und 1.
			float twistation = genetics[gene_offset + 7];
			int type_of_leaves = (int)genetics[gene_offset + 14];

			distortion *= mutation;
			roundness *= mutation;
			spikeyness *= mutation;
			scalation *= mutation;
			twistation *= mutation;


			vec3 color = new vec3(genetics[gene_offset + 8], genetics[gene_offset + 9], genetics[gene_offset + 10]) * mutation * mutation;
			vec3 color2 = new vec3(genetics[gene_offset + 11], genetics[gene_offset + 12], genetics[gene_offset + 13]) * mutation * mutation;


			scalation *= scale;

			//dir = new vec3(0.0f, 1.0f, 0.0f);// dir.normalize();//
			vec3 b1 = new vec3(1.0f, 0.0f, 0.0f);
			vec3 b2 = (dir % b1).normalize()*scalation;
			b1 = (dir % b2).normalize() * scalation;
			dir = dir * scalation;
			pos = pos + dir * 0.8f;



			if(type_of_leaves == 0)
			{
				Vertex top = new Vertex(pos.x + dir.x, pos.y + dir.y, pos.z + dir.z, color.x, color.y, color.z); tree.AddVertex(top);
				Vertex bottom = new Vertex(pos.x - dir.x, pos.y - dir.y, pos.z - dir.z, color.x, color.y, color.z); tree.AddVertex(bottom);
				Vertex[,] m = new Vertex[detail, height_detail];


				vec3 rnd2;
				for (int i = 0; i < 10; i++)
				{
					top.addFrame(pos.x + dir.x +((float)GameWindow.random.NextDouble() - 0.5f)*0.05f, pos.y + dir.y +((float)GameWindow.random.NextDouble() - 0.5f)*0.05f, pos.z + dir.z+((float)GameWindow.random.NextDouble() - 0.5f)*0.05f);
				}
				top.addFrame(pos.x + dir.x, pos.y + dir.y, pos.z + dir.z);


				float delrot = (float)GameWindow.random.NextDouble();

				for (int y = 0; y < height_detail; y++)
				{
					float py = (float)Math.Cos(((float)y + 1.0f) / (float)(height_detail + 1) * Math.PI);
					float pr = (float)Math.Sin(((float)y + 1.0f) / (float)(height_detail + 1) * Math.PI) * roundness;
					py -= spikeyness * pr;

					delrot += ((float)GameWindow.random.NextDouble()-0.5f)*0.02f + twistation;

					for (int x = 0; x < detail; x++)
					{
						vec3 rnd = new vec3(((float)GameWindow.random.NextDouble()-0.5f),((float)GameWindow.random.NextDouble()-0.5f),((float)GameWindow.random.NextDouble()-0.5f));
						vec3 vp = pos + py * dir + pr * ((float)Math.Cos((delrot + ((float)x + rnd.x * (0.3f + distortion)) / (float)detail) * 2.0f * Math.PI) * b1 + (float)Math.Sin((delrot + ((float)x + rnd.x * (0.3f + distortion)) / (float)detail) * 2.0f * Math.PI) * b2);
						vp = vp + rnd * distortion;

						float blend = (float)GameWindow.random.NextDouble();
						vec3 col = color * blend + color2 * (1.0f - blend);

						m[x, y] = new Vertex(vp.x, vp.y, vp.z, col.x, col.y, col.z);
						tree.AddVertex(m[x, y]);

						for (int i = 0; i < 10; i++)
						{
							rnd2 = new vec3(((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f)) * 0.5f;
							vp = pos + py * dir + pr * ((float)Math.Cos((delrot + ((float)x + (rnd.x + rnd2.x) * (0.3f + distortion)) / (float)detail) * 2.0f * Math.PI) * b1 + (float)Math.Sin((delrot + ((float)x + (rnd.x + rnd2.x) * (0.3f + distortion)) / (float)detail) * 2.0f * Math.PI) * b2);
							vp = vp + (rnd) * distortion + rnd2 * 0.075f;
							m[x, y].addFrame(vp.x, vp.y, vp.z);

						}
						rnd2 = m[x, y].getPos(0);
						m[x, y].addFrame(rnd2.x, rnd2.y, rnd2.z);
						
					}
				}
				
				//tree.setFrameCount(m[0, 0].getFrameCount());

				// Oben und unten zu machen.
				for (int x = 0; x < detail; x++)
				{
					tree.AddTriangle(new Triangle(m[x % detail, 0], top, m[(x + 1) % detail, 0]));
					tree.AddTriangle(new Triangle(m[(x + 1) % detail, height_detail-1], bottom, m[x % detail, height_detail-1]));

					for (int y = 0; y < height_detail-1; y++)
					{
						if ((m[x % detail, y].getPos() - m[(x + 1) % detail, y + 1].getPos()).length() < (m[(x+1) % detail, y].getPos() - m[(x) % detail, y + 1].getPos()).length())
						{
							tree.AddTriangle(new Triangle(m[x % detail, y], m[(x + 1) % detail, y], m[(x + 1) % detail, y + 1]));
							tree.AddTriangle(new Triangle(m[(x) % detail, y + 1], m[x % detail, y], m[(x + 1) % detail, y + 1]));
						}
						else
						{
							tree.AddTriangle(new Triangle(m[x % detail, y], m[(x + 1) % detail, y], m[(x) % detail, y + 1]));
							tree.AddTriangle(new Triangle(m[(x) % detail, y + 1], m[(x + 1) % detail, y], m[(x + 1) % detail, y + 1]));
						}
					}
				}
			}
			else //Einzelne Blätter erzeugen.
			{
				Vertex top = new Vertex(pos.x, pos.y, pos.z, color.x, color.y, color.z); tree.AddVertex(top);
				//Vertex bottom = new Vertex(pos.x + dir.x, pos.y + dir.y, pos.z + dir.z, color.x, color.y, color.z); tree.AddVertex(top);

				vec3 dir2 = dir;
				vec3 pos2 = pos;
				scalation += 0.2f;
				for (int y = 0; y < height_detail-2; y++ )
				{
					vec3 rnd = new vec3(((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f));
					pos = pos + dir2 * ((float)GameWindow.random.NextDouble() - 0.5f) * 0.1f + rnd * 0.2f;
					dir = (dir2.normalize() + rnd * 0.2f).normalize();
					b1 = new vec3(1.0f, 0.0f, 0.0f);
					b2 = (dir % b1).normalize() * scalation;
					b1 = (dir % b2).normalize() * scalation;
					dir = dir * scalation;

					float rotation = (float)GameWindow.random.NextDouble();

					for (int x = 0; x < detail-2; x++)
					{
						float blend = (float)GameWindow.random.NextDouble();
						vec3 col = color * blend + color2 * (1.0f - blend);


						float delrot = rotation + ((float)GameWindow.random.NextDouble() - 0.5f) * 0.02f + twistation * x;
						vec3 grow = (float)Math.Cos((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b1 + (float)Math.Sin((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b2;
						vec3 perp = -(float)Math.Sin((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b1 + (float)Math.Cos((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b2;
						perp = perp * roundness * 1.75f / (float)detail;
						grow = grow * 2.5f;

						vec3 pp = pos + grow * (0.5f * (0.5f+spikeyness) + 0.5f * distortion / 0.04f) - 0.25f * dir;
						Vertex end = new Vertex(pp.x, pp.y, pp.z, col.x, col.y, col.z); tree.AddVertex(end);

						pp = pos + 0.5f * (0.5f + spikeyness) * grow + perp - 0.125f * dir;
						Vertex left = new Vertex(pp.x, pp.y, pp.z, col.x, col.y, col.z); tree.AddVertex(left);

						pp = pos + 0.5f * (0.5f + spikeyness) * grow - perp - 0.125f * dir;
						Vertex right = new Vertex(pp.x, pp.y, pp.z, col.x, col.y, col.z); tree.AddVertex(right);

						pp = pos + 0.5f * (0.5f + spikeyness) * grow + 0.125f * dir;
						Vertex center = new Vertex(pp.x, pp.y, pp.z, col.x * 1.1f, col.y * 1.1f, col.z * 1.1f); tree.AddVertex(center);

						for (int i = 0; i < 10; i++)
						{
							
							vec3 b1x = b1 + new vec3(((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f)) * 0.1f;
							vec3 b2x = b2 + new vec3(((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f)) * 0.1f;
							vec3 dirx = dir + new vec3(((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f)) * 0.1f;
							/*
							vec3 dirx, b1x, b2x;
							vec3 rnd2 = new vec3(((float)GameWindow.random.NextDouble() - 0.5f), ((float)GameWindow.random.NextDouble() - 0.5f)*2.0f, ((float)GameWindow.random.NextDouble() - 0.5f));
							pos = pos + dir * ((float)GameWindow.random.NextDouble() - 0.5f) * 0.2f + rnd2 * 0.2f;
							dirx = (dir.normalize() + rnd * 0.2f).normalize();
							b1x = new vec3(1.0f, 0.0f, 0.0f) + rnd2*0.1f;
							b2x = (dirx % b1x).normalize() * scalation;
							b1x = (dirx % b2x).normalize() * scalation;
							dirx = dirx * scalation;
							*/
							
							float delrot2 = delrot + ((float)GameWindow.random.NextDouble() - 0.5f) * 0.02f;
							grow = (float)Math.Cos((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b1x + (float)Math.Sin((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b2x;
							perp = -(float)Math.Sin((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b1x + (float)Math.Cos((delrot + (float)x / (float)(detail - 2)) * 2.0f * Math.PI) * b2x;
							perp = perp * roundness * 1.75f / (float)detail;
							grow = grow * 2.5f;

							pp = pos + grow * (0.5f * (0.5f + spikeyness) + 0.5f * distortion / 0.04f) - 0.25f * dirx;
							end.addFrame(pp.x, pp.y, pp.z);

							pp = pos + 0.5f * (0.5f + spikeyness) * grow + perp - 0.125f * dirx;
							left.addFrame(pp.x, pp.y, pp.z);

							pp = pos + 0.5f * (0.5f + spikeyness) * grow - perp - 0.125f * dirx;
							right.addFrame(pp.x, pp.y, pp.z);

							pp = pos + 0.5f * (0.5f + spikeyness) * grow + 0.125f * dirx;
							center.addFrame(pp.x, pp.y, pp.z);
						}

						pp = end.getPos(0); end.addFrame(pp.x, pp.y, pp.z);
						pp = left.getPos(0); left.addFrame(pp.x, pp.y, pp.z);
						pp = right.getPos(0); right.addFrame(pp.x, pp.y, pp.z);
						pp = center.getPos(0); center.addFrame(pp.x, pp.y, pp.z);


						tree.AddTriangle(new Triangle(top, left, center));
						tree.AddTriangle(new Triangle(top, center, right));
						tree.AddTriangle(new Triangle(left, end, center));
						tree.AddTriangle(new Triangle(center, end, right));

					}

				}
				return top;


			}




			return null;

		} // End of Function






	}
}
