using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;

namespace Cluster.Rendering.Draw3D.ProceduralGeneration
{
	class FishGenerator
	{
		const int GENE_RUMPF = 0;
		const int GENE_FARBE = 7;
		const int GENE_FINS = 14;
		const int GENE_BEHAVIOUR = 25;

		static public Model Generate(int seed = -1)
		{
			Model fish = new Model();


			float[] genom = genEtics(seed);

			genBody(fish, genom);
			genAnimation(fish, genom);

			fish.finishPreparation();

			return fish;
		}
		static public Model Generate(Model fish, int seed = -1)
		{
			fish.clearData();


			float[] genom = genEtics(seed);

			genBody(fish, genom);
			genAnimation(fish, genom);

			fish.finishPreparation();

			return fish;
		}





		static float[] genEtics(int seed = -1)
		{
			Random rnd = GameWindow.random;
			if (seed >= 0) rnd = new Random(seed);


			float[] genom = new float[30];
			genom[GENE_RUMPF + 0] = (float)(rnd.Next(3) + 13);	// Anzahl Segmente in Länge.
			genom[GENE_RUMPF + 1] = (float)(rnd.Next(4) + 14);	// Anzahl Segmente in Breite.
			genom[GENE_RUMPF + 2] = 0.5f + 0.5f * (float)rnd.NextDouble();	// Breite.
			genom[GENE_RUMPF + 3] = 0.25f + 0.85f * (float)rnd.NextDouble();	// Höhe oben (Rücken)
			genom[GENE_RUMPF + 4] = 0.25f + 0.75f * (float)rnd.NextDouble();	// Höhe unten (Bauch)
			genom[GENE_RUMPF + 5] = 0.35f + 0.5f * (float)rnd.NextDouble();	// An welcher Stelle ist der Fisch am dicksten? 0 = vorne, 1 = hinten.
			genom[GENE_RUMPF + 6] = (float)rnd.NextDouble();// Wie Spitzig ist er an den Enden? Winkelangaben in Grad.
			/*
			genom[GENE_FARBE + 0] = 0.1f;// 0.8f * (float)rnd.NextDouble();// r1
			genom[GENE_FARBE + 1] = 0.1f;//0.8f * (float)rnd.NextDouble();// g1
			genom[GENE_FARBE + 2] = 0.1f;//0.8f * (float)rnd.NextDouble();// b1
			genom[GENE_FARBE + 3] = 0.9f;// 0.15f + 0.25f * (float)rnd.NextDouble() + genom[GENE_FARBE + 1];// r2
			genom[GENE_FARBE + 4] = 0.9f;//0.15f + 0.25f * (float)rnd.NextDouble() + genom[GENE_FARBE + 2];// g2
			genom[GENE_FARBE + 5] = 0.9f;//0.15f + 0.25f * (float)rnd.NextDouble() + genom[GENE_FARBE + 0];// b2
			 * */

			vec3[] colors = ColorGenerator.genTwinColors(rnd);
			genom[GENE_FARBE + 0] = colors[0].x;// 0.8f * (float)rnd.NextDouble();// r1
			genom[GENE_FARBE + 1] = colors[0].y;//0.8f * (float)rnd.NextDouble();// g1
			genom[GENE_FARBE + 2] = colors[0].z;//0.8f * (float)rnd.NextDouble();// b1
			genom[GENE_FARBE + 3] = colors[1].x;// 0.15f + 0.25f * (float)rnd.NextDouble() + genom[GENE_FARBE + 1];// r2
			genom[GENE_FARBE + 4] = colors[1].y;//0.15f + 0.25f * (float)rnd.NextDouble() + genom[GENE_FARBE + 2];// g2
			genom[GENE_FARBE + 5] = colors[1].z;//0.15f + 0.25f * (float)rnd.NextDouble() + genom[GENE_FARBE + 0];// b2
			genom[GENE_FARBE + 6] = (float)rnd.NextDouble();// Musterung. 0-0.5 = oben/unten, 0.5-1 = vorne/hinten. Abstufungen geben Musterform an.

			genom[GENE_FINS + 0] = (float)rnd.Next(2); // Fisch (0) oder Wal (1) ?
			genom[GENE_FINS + 1] = 0.2f + 0.35f * (float)rnd.NextDouble(); // Flossenlänge
			genom[GENE_FINS + 2] = 0.25f + 0.25f * (float)rnd.NextDouble(); // Flossenhöhe
			genom[GENE_FINS + 3] = -0.5f + 1.35f * (float)rnd.NextDouble(); // Form (Eingedellt, ausgebeult)
			genom[GENE_FINS + 4] = 0.85f * (float)rnd.NextDouble(); // Form (Eingedellt, ausgebeult)
			genom[GENE_FINS + 5] = 0.5f - (float)rnd.NextDouble();	// Form (Eingedellt, ausgebeult)
			genom[GENE_FINS + 6] = (float)rnd.NextDouble();			//Position der Rückenflosse falls vorhanden.
			genom[GENE_FINS + 7] = (float)rnd.NextDouble();			//Position der Bauchflosse falls vorhanden.
			genom[GENE_FINS + 8] = (float)rnd.NextDouble();			//Position der Seitenflossen (höhe) falls vorhanden.
			genom[GENE_FINS + 9] = (float)rnd.NextDouble();			//Position der Seitenflossen (vorne/hinten) falls vorhanden.
			genom[GENE_FINS + 10] = (float)rnd.NextDouble();		//Position der Seitenflossen 2 (vorne/hinten) falls vorhanden.

			genom[GENE_BEHAVIOUR] = (float)(100 + rnd.Next(100));	//Anzahl an Frames --> bestimmt auch Bewegungsgeschwindigkeit.

			return genom;
		}


		static float bodyShape(float x, float[] genom) //x zwischen 0 und 1.
		{
			float rx = 0;
			if (x <= genom[GENE_RUMPF + 5])
			{
				rx = (float)Math.Sin(x / genom[GENE_RUMPF + 5] * Math.PI * 0.5f);
			}
			else
			{
				rx = Math.Abs((float)Math.Sin(1.0 - (x - genom[GENE_RUMPF + 5]) / (1.0 - genom[GENE_RUMPF + 5]) * Math.PI * 0.5f)) + 0.05f;
				if (genom[GENE_RUMPF + 6] > 0.75f)
				{
					rx = Math.Abs((float)Math.Sin((1.0f - (genom[GENE_RUMPF + 5] - x) / (1.0f - genom[GENE_RUMPF + 5])) * Math.PI * 0.5f)) + 0.02f;
				}
				else if (genom[GENE_RUMPF + 6] > 0.5f)
				{
					rx = (float)Math.Sqrt(1.0f - ((genom[GENE_RUMPF + 5] - x) / (1.0f - genom[GENE_RUMPF + 5])) * ((genom[GENE_RUMPF + 5] - x) / (1.0f - genom[GENE_RUMPF + 5])));
				}
			}
			return rx;
		}


		static void genBody(Model fish, float[] genom)
		{

			Vertex[,] m = new Vertex[(int)genom[GENE_RUMPF + 0] + 1, (int)genom[GENE_RUMPF + 1]];
			Vertex mouth = new Vertex(1.0f, 0.0f, 0.0f, genom[GENE_FARBE + 0], genom[GENE_FARBE + 1], genom[GENE_FARBE + 2]);
			Vertex tail = new Vertex(-0.9f, 0.0f, 0.0f, genom[GENE_FARBE + 0], genom[GENE_FARBE + 1], genom[GENE_FARBE + 2]);
			Vertex tail2 = new Vertex(-1.0f, 0.0f, 0.0f, genom[GENE_FARBE + 0], genom[GENE_FARBE + 1], genom[GENE_FARBE + 2]);
			Vertex center = new Vertex(0.0f, 0.0f, 0.0f, genom[GENE_FARBE + 0], genom[GENE_FARBE + 1], genom[GENE_FARBE + 2]);
			fish.AddVertex(mouth);
			fish.AddVertex(tail);
			fish.AddVertex(tail2);
			fish.AddVertex(center);

			for (int l = 0; l < (int)genom[GENE_RUMPF + 0]; l++)
			{
				float x = ((float)l+1.0f) / (genom[GENE_RUMPF + 0]+1);
				float rx = bodyShape(x, genom);
				x = x * 2.0f - 1.0f;


				for (int r = 0; r < (int)genom[GENE_RUMPF + 1]; r++)
				{
					float alpha = ((float)r / genom[GENE_RUMPF + 1] + 0.25f) * (float)Math.PI * 2.0f;

					float z = (float)Math.Cos(alpha) * 0.5f * genom[GENE_RUMPF + 2] * rx;
					float y = (float)Math.Sin(alpha) * 0.5f;
					
					if (y > 0)		{y = y * rx * genom[GENE_RUMPF + 3];}
							else	{y = y * rx * genom[GENE_RUMPF + 4];}

					float cr = genom[GENE_FARBE + 0], cg = genom[GENE_FARBE + 1], cb = genom[GENE_FARBE + 2];
					if (genom[GENE_FARBE + 6] > 0.9f)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					else if (genom[GENE_FARBE + 6] > 0.8f)
					{
						if (r % 2 == 0)
						{
							cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
						}
					}
					else if (genom[GENE_FARBE + 6] > 0.7f)
					{
						if (l % 2 == 0)
						{
							cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
						}
					}
					else if (genom[GENE_FARBE + 6] > 0.40f)
					{
						float blend = Math.Max(0.0f, Math.Min(1.0f, (float)Math.Sin(alpha)*5.0f + 0.5f));
						cr = cr * blend + (1.0f - blend) * genom[GENE_FARBE + 3]; cg = cg * blend + (1.0f - blend) * genom[GENE_FARBE + 4]; cb = cb * blend + (1.0f - blend) * genom[GENE_FARBE + 5];
					}
					else// if (genom[GENE_FARBE + 6] > 0.5f)
					{
						if (GameWindow.random.Next(5) == 0)
						{
							cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
						}
						else if (GameWindow.random.Next(2) == 0)
						{
							float blend = (float)GameWindow.random.NextDouble();
							cr = cr*blend + (1.0f-blend)*genom[GENE_FARBE + 3]; cg = cg*blend + (1.0f-blend)*genom[GENE_FARBE + 4]; cb = cb*blend + (1.0f-blend)*genom[GENE_FARBE + 5];
						}
					}

					m[l, r] = new Vertex(x, y, z, cr, cg, cb);
					m[l, r].addFrame(x, y, z);
					fish.AddVertex(m[l, r]);
				}
			}
			
			//Schwanzflosse
			for (int r = 0; r < (int)genom[GENE_RUMPF + 1]; r++)
			{
				float alpha = (float)r / genom[GENE_RUMPF + 1] * (float)Math.PI * 2.0f;
				float height = ((float)r / genom[GENE_RUMPF + 1] * 2.0f - 1.0f);//(float)Math.Sin(alpha) * genom[GENE_FINS + 2];
				float x = -1.0f - genom[GENE_FINS + 1] * (1.0f + (height * height) * genom[GENE_FINS + 3]
						  + Math.Abs(genom[GENE_FINS + 5] + height) * genom[GENE_FINS + 4] * (1.0f - genom[GENE_FINS + 0]));// +(float)GameWindow.random.NextDouble() * 0.25f;

				float cr = genom[GENE_FARBE + 0], cg = genom[GENE_FARBE + 1], cb = genom[GENE_FARBE + 2];
				if (genom[GENE_FARBE + 6] > 0.5f)
				{
					cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
				}
				else if (genom[GENE_FARBE + 6] > 0.15f)
				{
					float blend = (float)GameWindow.random.NextDouble();
					cr = cr * blend + (1.0f - blend) * genom[GENE_FARBE + 3]; cg = cg * blend + (1.0f - blend) * genom[GENE_FARBE + 4]; cb = cb * blend + (1.0f - blend) * genom[GENE_FARBE + 5];
				}
				else if (r % 2 == 0)
				{
					cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
				}

				if (genom[GENE_FINS + 0] > 0.5f)
				{
					m[(int)genom[GENE_RUMPF + 0], r] = new Vertex(x, 0, height * genom[GENE_FINS + 2], cr, cg, cb);
				}
				else
				{
					m[(int)genom[GENE_RUMPF + 0], r] = new Vertex(x, height * genom[GENE_FINS + 2], 0, cr, cg, cb);
				}
				fish.AddVertex(m[(int)genom[GENE_RUMPF + 0], r]);
			}

			//Rückenflosse
			if (genom[GENE_FINS + 6] > 0.5f)
			{
				float length = genom[GENE_FINS + 6];
				float x0 = -length * 0.5f;

				float bfsize = 0.25f;

				Vertex[] back = new Vertex[4];
				for (int bf = 0; bf < 4; bf++)
				{
					float x = x0 - length * (float)bf / 4.0f;
					float y = (float)Math.Sin((x + 1.0f) * Math.PI * 0.5f) * 0.5f * genom[GENE_RUMPF + 3]; // (float)Math.Sin(1.0 - (x - genom[GENE_RUMPF + 5]) / (1.0 - genom[GENE_RUMPF + 5]) * Math.PI * 0.5f);// ;


					float cr = genom[GENE_FARBE + 0], cg = genom[GENE_FARBE + 1], cb = genom[GENE_FARBE + 2];
					if (genom[GENE_FARBE + 6] > 0.5f)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					else if (genom[GENE_FARBE + 6] > 0.15f)
					{
						float blend = (float)GameWindow.random.NextDouble();
						cr = cr * blend + (1.0f - blend) * genom[GENE_FARBE + 3]; cg = cg * blend + (1.0f - blend) * genom[GENE_FARBE + 4]; cb = cb * blend + (1.0f - blend) * genom[GENE_FARBE + 5];
					}
					else if (bf % 2 == 0)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					back[bf] = new Vertex(x, y + bfsize + (float)GameWindow.random.NextDouble() * 0.1f, 0, cr, cg, cb);
					fish.AddVertex(back[bf]);
					if (bf > 0)
					{
						fish.AddTriangle(new Triangle(center, back[bf - 1], back[bf]));
						fish.AddTriangle(new Triangle(back[bf - 1], center, back[bf]));
					}

				}
			}
			
			//Bauchflosse
			if (genom[GENE_FINS + 7] > 0.75f && genom[GENE_FINS + 0] < 0.5f)
			{
				float length = genom[GENE_FINS + 7];
				float x0 = -length * 0.5f;

				float bfsize = 0.25f;

				Vertex[] back = new Vertex[4];
				for (int bf = 0; bf < 4; bf++)
				{
					float x = x0 - length * (float)bf / 4.0f;
					float y = (float)Math.Sin((x + 1.0f) * Math.PI * 0.5f) * 0.5f * genom[GENE_RUMPF + 4]; // (float)Math.Sin(1.0 - (x - genom[GENE_RUMPF + 5]) / (1.0 - genom[GENE_RUMPF + 5]) * Math.PI * 0.5f);// ;


					float cr = genom[GENE_FARBE + 0], cg = genom[GENE_FARBE + 1], cb = genom[GENE_FARBE + 2];
					if (genom[GENE_FARBE + 6] > 0.5f)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					else if (genom[GENE_FARBE + 6] > 0.15f)
					{
						float blend = (float)GameWindow.random.NextDouble();
						cr = cr * blend + (1.0f - blend) * genom[GENE_FARBE + 3]; cg = cg * blend + (1.0f - blend) * genom[GENE_FARBE + 4]; cb = cb * blend + (1.0f - blend) * genom[GENE_FARBE + 5];
					}
					else if (bf % 2 == 0)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					back[bf] = new Vertex(x, -y - bfsize - (float)GameWindow.random.NextDouble() * 0.1f, 0, cr, cg, cb);
					fish.AddVertex(back[bf]);
					if (bf > 0)
					{
						fish.AddTriangle(new Triangle(center, back[bf - 1], back[bf]));
						fish.AddTriangle(new Triangle(back[bf - 1], center, back[bf]));
					}

				}
			}

			//Seitenflossen
			else if (genom[GENE_FINS + 7] > 0.25f)
			{

				float length = (genom[GENE_FINS + 6] + 1.5f);
				float x0 = 0.8f - 1.6f*genom[GENE_FINS + 9];
				float rfish = bodyShape((x0 + 1.0f)*0.5f, genom);
				/*
				float rfish = (float)Math.Sin((x0 + 1.0f) * Math.PI * 0.5f) * 0.5f;
				if ((x0 + 1.0f)*0.5f > genom[GENE_RUMPF + 5])
				{
					rfish = (float)Math.Sin(1.0 - (1.0f + (x0 - genom[GENE_RUMPF + 5]) / (1.0 - genom[GENE_RUMPF + 5])) * Math.PI * 0.5f) * 0.5f;
				}*/
				rfish = Math.Max(rfish, 0.0f);

				float y0 = -rfish * genom[GENE_RUMPF + 4] * (float)Math.Sin(genom[GENE_FINS + 8]*0.2f);
				float z0 = rfish * genom[GENE_RUMPF + 2] * (float)Math.Cos(genom[GENE_FINS + 8]*0.2f);

				float bfsize = 0.55f + genom[GENE_FINS + 1];
				float totalarea = bfsize * length * 5.0f;
				length /= totalarea;
				bfsize /= totalarea;
				length += 0.2f;
				bfsize += 0.1f;

				float cr = genom[GENE_FARBE + 0], cg = genom[GENE_FARBE + 1], cb = genom[GENE_FARBE + 2];

				Vertex cenl = new Vertex(x0 * 0.5f, y0 * 0.5f, z0 * 0.5f, cr, cg, cb), cenr = new Vertex(x0 * 0.5f, y0 * 0.5f, -z0 * 0.5f, cr, cg, cb);
				//cenl.flag = true;
				//cenr.flag = true;
				fish.AddVertex(cenl);
				fish.AddVertex(cenr);
				Vertex[] side = new Vertex[8];
				for (int bf = 0; bf < 4; bf++)
				{
					float x = x0 - length * (float)bf / 4.0f;
					bfsize = Math.Max(0.0f, bfsize + genom[GENE_FINS + 5] * 0.25f + (genom[GENE_FINS + 4]-0.45f) * 0.05f * (float)bf);
					/*
					rfish = (float)Math.Sin((x + 1.0f) * Math.PI * 0.5f) * 0.5f;
					if ((x + 1.0f) * 0.5f > genom[GENE_RUMPF + 5])
					{
						rfish = (float)Math.Sin(1.0 - (1.0f + (x - genom[GENE_RUMPF + 5]) / (1.0 - genom[GENE_RUMPF + 5])) * Math.PI * 0.5f) * 0.5f;
					}*/
					rfish = bodyShape((x + 1.0f)*0.5f, genom);
					rfish = Math.Max(rfish, 0.0f);
					y0 = -rfish * genom[GENE_RUMPF + 4] * (float)Math.Sin(genom[GENE_FINS + 8] * 0.25f);
					z0 = rfish * genom[GENE_RUMPF + 2] * (float)Math.Cos(genom[GENE_FINS + 8] * 0.25f);

					cr = genom[GENE_FARBE + 0]; cg = genom[GENE_FARBE + 1]; cb = genom[GENE_FARBE + 2];
					if (genom[GENE_FARBE + 6] > 0.5f)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					else if (bf % 2 == 0 && genom[GENE_FARBE + 6] > 0.25f)
					{
						cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
					}
					Console.WriteLine("cr" + cr.ToString());
					side[bf] = new Vertex(x, y0 - bfsize * (float)Math.Sin(genom[GENE_FINS + 8]), z0 + bfsize * (float)Math.Cos(genom[GENE_FINS + 8]), cr, cg, cb);
					fish.AddVertex(side[bf]);
					side[bf + 4] = new Vertex(x, y0 - bfsize * (float)Math.Sin(genom[GENE_FINS + 8]), -z0 - bfsize * (float)Math.Cos(genom[GENE_FINS + 8]), cr, cg, cb);
					side[bf].flag = true;
					side[bf+4].flag = true;
					fish.AddVertex(side[bf + 4]);
					if (bf > 0)
					{
						fish.AddTriangle(new Triangle(cenl, side[bf - 1], side[bf]));
						fish.AddTriangle(new Triangle(side[bf - 1], cenl, side[bf]));
						fish.AddTriangle(new Triangle(cenr, side[bf - 1 + 4], side[bf + 4]));
						fish.AddTriangle(new Triangle(side[bf - 1 + 4], cenr, side[bf + 4]));
					}

				}

				if (genom[GENE_FINS + 10] > genom[GENE_FINS + 9] + length*0.5f)
				{
					length = (genom[GENE_FINS + 6] + 1.5f);
					x0 = 0.8f - 1.6f * genom[GENE_FINS + 10];

					rfish = bodyShape((x0 + 1.0f)*0.5f, genom);
					//rfish = (float)Math.Sin((x0 + 1.0f) * Math.PI * 0.5f) * 0.5f;
					y0 = -rfish * genom[GENE_RUMPF + 4] * (float)Math.Sin(genom[GENE_FINS + 8] * 0.25f);
					z0 = rfish * genom[GENE_RUMPF + 2] * (float)Math.Cos(genom[GENE_FINS + 8] * 0.25f);

					bfsize = 0.55f + genom[GENE_FINS + 1];
					totalarea = bfsize * length * 5.0f;
					length /= totalarea;
					bfsize /= totalarea;
					length += 0.2f;
					bfsize += 0.05f;

					cr = genom[GENE_FARBE + 0]; cg = genom[GENE_FARBE + 1]; cb = genom[GENE_FARBE + 2];
					cenl = new Vertex(x0 * 0.5f, y0 * 0.5f, z0 * 0.5f, cr, cg, cb); cenr = new Vertex(x0 * 0.5f, y0 * 0.5f, -z0 * 0.5f, cr, cg, cb);
					//cenr.flag = true;
					//cenl.flag = true;
					fish.AddVertex(cenl);
					fish.AddVertex(cenr);
					side = new Vertex[8];
					for (int bf = 0; bf < 4; bf++)
					{
						float x = x0 - length * (float)bf / 4.0f;
						/*
						rfish = (float)Math.Sin((x + 1.0f) * Math.PI * 0.5f) * 0.5f;
						if ((x + 1.0f) * 0.5f > genom[GENE_RUMPF + 5])
						{
							rfish = (float)Math.Sin(1.0 - (1.0f + (x - genom[GENE_RUMPF + 5]) / (1.0 - genom[GENE_RUMPF + 5])) * Math.PI * 0.5f) * 0.5f;
						}*/
						rfish = bodyShape((x + 1.0f)*0.5f, genom);
						rfish = Math.Max(rfish, 0.0f);
						y0 = -rfish * genom[GENE_RUMPF + 4] * (float)Math.Sin(genom[GENE_FINS + 8] * 0.25f);
						z0 = rfish * genom[GENE_RUMPF + 2] * (float)Math.Cos(genom[GENE_FINS + 8] * 0.25f);

						bfsize = Math.Max(0.0f, bfsize + genom[GENE_FINS + 5] * 0.25f + (genom[GENE_FINS + 4] - 0.45f) * 0.05f * (float)bf);
						//bfsize += genom[GENE_FINS + 5] * 0.25f + (genom[GENE_FINS + 4] - 0.45f) * 0.05f * (float)bf;

						cr = genom[GENE_FARBE + 0]; cg = genom[GENE_FARBE + 1]; cb = genom[GENE_FARBE + 2];
						if (genom[GENE_FARBE + 6] > 0.5f)
						{
							cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
						}
						else if (bf % 2 == 0 && genom[GENE_FARBE + 6] > 0.25f)
						{
							cr = genom[GENE_FARBE + 3]; cg = genom[GENE_FARBE + 4]; cb = genom[GENE_FARBE + 5];
						}
						side[bf] = new Vertex(x, y0 - bfsize * (float)Math.Sin(genom[GENE_FINS + 8]), z0 + bfsize * (float)Math.Cos(genom[GENE_FINS + 8]), cr, cg, cb);
						side[bf].flag = true;
						fish.AddVertex(side[bf]);
						side[bf + 4] = new Vertex(x, y0 - bfsize * (float)Math.Sin(genom[GENE_FINS + 8]), -z0 - bfsize * (float)Math.Cos(genom[GENE_FINS + 8]), cr, cg, cb);
						side[bf + 4].flag = true;
						fish.AddVertex(side[bf + 4]);
						if (bf > 0)
						{
							fish.AddTriangle(new Triangle(cenl, side[bf - 1], side[bf]));
							fish.AddTriangle(new Triangle(side[bf - 1], cenl, side[bf]));
							fish.AddTriangle(new Triangle(cenr, side[bf - 1 + 4], side[bf + 4]));
							fish.AddTriangle(new Triangle(side[bf - 1 + 4], cenr, side[bf + 4]));
						}

					}
				}

			}


			//Dreiecke
			for (int l = 0; l < (int)genom[GENE_RUMPF + 0]-1; l++)
			{
				for (int r = 0; r < (int)genom[GENE_RUMPF + 1]; r++)
				{
					fish.AddTriangle(new Triangle(m[l, r], m[l + 1, r], m[l + 1, (r + 1) % (int)genom[GENE_RUMPF + 1]]));
					fish.AddTriangle(new Triangle(m[l, r], m[l + 1, (r + 1) % (int)genom[GENE_RUMPF + 1]], m[l, (r + 1) % (int)genom[GENE_RUMPF + 1]]));

				}
			}
			
			for (int r = 0; r < (int)genom[GENE_RUMPF + 1]; r++)
			{
				fish.AddTriangle(new Triangle(m[(int)genom[GENE_RUMPF + 0] - 1, r], mouth, m[(int)genom[GENE_RUMPF + 0] - 1, (r + 1) % (int)genom[GENE_RUMPF + 1]]));
				fish.AddTriangle(new Triangle(tail2, m[0, r], m[0, (r + 1) % (int)genom[GENE_RUMPF + 1]]));

				if (r + 1 < (int)genom[GENE_RUMPF + 1])
				{
					fish.AddTriangle(new Triangle(tail, m[(int)genom[GENE_RUMPF + 0], r], m[(int)genom[GENE_RUMPF + 0], r + 1]));
					fish.AddTriangle(new Triangle(m[(int)genom[GENE_RUMPF + 0], r], tail, m[(int)genom[GENE_RUMPF + 0], r + 1]));
				}

				//fish.AddTriangle(new Triangle(m[0, r], mouth, m[0, (r + 1) % (int)genom[GENE_RUMPF + 1]]));

				//fish.AddTriangle(new Triangle(m[(int)genom[GENE_RUMPF + 0], r], m[0, r], m[(int)genom[GENE_RUMPF + 0], (r + 1) % (int)genom[GENE_RUMPF + 1]]));
				//fish.AddTriangle(new Triangle(m[0, r], m[0, (r + 1) % (int)genom[GENE_RUMPF + 1]], m[(int)genom[GENE_RUMPF + 0], (int)genom[GENE_RUMPF + 1] / 2]));
				//if (m[(int)genom[GENE_RUMPF + 0], r].getPos().x > 0 || m[(int)genom[GENE_RUMPF + 0], r].getPos().x > 0)


				//fish.AddTriangle(new Triangle(m[0, r], m[0, (r + 1) % (int)genom[GENE_RUMPF + 1]], m[(int)genom[GENE_RUMPF + 0], (r + 1) % (int)genom[GENE_RUMPF + 1]]));
				//fish.AddTriangle(new Triangle(m[(int)genom[GENE_RUMPF + 0], r], m[0, r], m[(int)genom[GENE_RUMPF + 0], (r + 1) % (int)genom[GENE_RUMPF + 1]]));
				

				//fish.AddTriangle(new Triangle(m[0, r], m[0, (r + 1) % (int)genom[GENE_RUMPF + 1]], m[(int)genom[GENE_RUMPF + 0], r]));
				//fish.AddTriangle(new Triangle(m[0, r], m[(int)genom[GENE_RUMPF + 0], r], m[(int)genom[GENE_RUMPF + 0], (r + 1) % (int)genom[GENE_RUMPF + 1]]));

				//fish.AddTriangle(new Triangle(m[0, r], end, m[0, (r + 1) % (int)genom[GENE_RUMPF + 1]]));
			}
			





		}













		static void genAnimation(Model fish, float[] genom)
		{

			float ratio = 1.0f - fish.getDimensionX() * 0.1f / 100.0f;
			foreach (Vertex v in fish.vertices)
			{
				vec3 p0 = v.getPos(0);
				bool invmov = v.flag;// getCol().x < 0;
				bool secmov = true;
				bool whale_like = false;

				if (genom[GENE_FINS + 0] > 0.5f)
				{
					whale_like = true;
					secmov = false;
					if (genom[GENE_FINS + 0] > 0.75f && invmov) secmov = true;
					invmov = true;
				}
				/*
				if (invmov)
				{
					v.col.Add(new vec3(1.0f, 0.0f, 0.0f));//v.getCol() v.col[0].x = 1.0f;
					v.col.RemoveAt(0);
				}*/


				int num_frames = (int)genom[GENE_BEHAVIOUR + 0];
				float anim_step = 4.0f / genom[GENE_BEHAVIOUR + 0];

				for (int i = 0; i < num_frames; i++)
				{
					float alpha = -(float)Math.Sin((i * anim_step * 2.0f + Math.Min(-p0.x, 10.0f) * 0.2f) * 2.0f * Math.PI) * 10.0f * (float)Math.PI / 180.0f * ((Math.Min(-p0.x, 1.0f) + 4.0f) * 0.25f); // Fish-like
					if (whale_like) alpha = -(float)Math.Sin((i * anim_step + p0.x * 0.1f) * 2.0f * Math.PI) * 10.0f * (float)Math.PI / 180.0f; //whale-like

					vec3 pp;

					if (invmov)
					{
						pp = new vec3((float)Math.Cos(alpha * 2.0f) * p0.x - (float)Math.Sin(alpha * 2.0f) * p0.y,
											(float)Math.Sin(alpha * 2.0f) * p0.x + (float)Math.Cos(alpha * 2.0f) * p0.y,
											p0.z);

						if (secmov)	pp = new vec3((float)Math.Cos(alpha * 0.5f) * pp.x - (float)Math.Sin(alpha * 0.5f) * pp.z,
											pp.y,
											(float)Math.Sin(alpha * 0.5f) * pp.x + (float)Math.Cos(alpha * 0.5f) * pp.z);
					}
					else
					{
						pp = new vec3((float)Math.Cos(alpha) * p0.x - (float)Math.Sin(alpha) * p0.z,
											p0.y,
											(float)Math.Sin(alpha) * p0.x + (float)Math.Cos(alpha) * p0.z);
					}

					if (whale_like) pp.y += (float)Math.Cos((i * anim_step + p0.x * 0.1f) * 2.0f * Math.PI) * 0.1f;
					v.addFrame(pp.x, pp.y, pp.z);

				}

				v.pos.RemoveAt(0);
			}

		}







	}
}
