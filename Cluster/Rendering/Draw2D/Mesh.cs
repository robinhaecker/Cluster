﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cluster.Properties;
using OpenTK.Graphics.OpenGL;
using Cluster.Rendering.Appearance;

namespace Cluster.Rendering.Draw2D
{
    public class Mesh
    {
        public static List<MeshDraw> drawCall;

        static Shader meshShader;
        public int glData, bufPos, bufCol;

        public float boxX, boxY, radius, delY, cx, cy;
        public int numVertices, numLines, numTriangles;

        List<Vertex> vex;
        List<Line> lines;

        public static void init()
        {
            meshShader = new Shader(Resources.mesh_vert, Resources.mesh_frag);
            drawCall = new List<MeshDraw>();
        }

        public static Shader getShader()
        {
            return meshShader;
        }

        public static void render()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.LineWidth(2.0f);
            foreach (MeshDraw md in drawCall)
            {
                if (md.fit)
                {
                    md.mesh.drawFit(md.x, md.y, md.width, md.height, md.clamp, md.r, md.g, md.b, md.a);
                }
                else
                {
                    md.mesh.draw(md.x, md.y, md.width, md.height, md.clamp, md.r, md.g, md.b, md.a);
                }
            }

            GL.Enable(EnableCap.DepthTest);
            drawCall.Clear();
        }

        private void fillPlaceholder()
        {
            Vertex a = new Vertex(-0.5f, -0.5f);
            Vertex b = new Vertex(0.5f, -0.5f);
            Vertex c = new Vertex(-0.5f, 0.5f);
            Vertex d = new Vertex(0.5f, 0.5f);
            vex.Add(a);
            vex.Add(b);
            vex.Add(c);
            vex.Add(d);
            lines.Add(new Line(a, b));
            lines.Add(new Line(a, c));
            lines.Add(new Line(a, d));
            lines.Add(new Line(b, c));
            lines.Add(new Line(b, d));
            lines.Add(new Line(c, d));

            boxX = 0.5f;
            boxY = 0.5f;
            radius = 0.75f;
            numVertices = 4;
            numLines = 6;
            numTriangles = 0;
        }


        public Mesh(string filename, bool centering = false, bool absolutePath = false)
        {
            vex = new List<Vertex>();
            lines = new List<Line>();

            filename = absolutePath
                ? filename
                : GameWindow.BASE_FOLDER + filename;

            if (File.Exists(filename))
            {
                fillFromFile(filename, centering);
            }
            else
            {
                fillPlaceholder();
            }

            prepare_vba();
        }

        private void fillFromFile(string filename, bool centering)
        {
            FileStream file = File.OpenRead(filename);
            BinaryReader reader = new BinaryReader(file);

            numVertices = reader.ReadInt32();
            numLines = reader.ReadInt32();
            numTriangles = reader.ReadInt32();

            float xa = 1000.0f, xb = -1000.0f, ya = 1000.0f, yb = -1000.0f;

            for (int i = 0; i < numVertices; i++)
            {
                Vertex v = new Vertex();
                v.r = ((float) reader.ReadByte()) / 255.0f;
                v.g = ((float) reader.ReadByte()) / 255.0f;
                v.b = ((float) reader.ReadByte()) / 255.0f;

                v.x = reader.ReadSingle();
                v.y = reader.ReadSingle();

                boxX = Math.Max(boxX, Math.Abs(v.x));
                boxY = Math.Max(boxY, Math.Abs(v.y));

                xa = Math.Min(xa, v.x);
                xb = Math.Max(xb, v.x);

                ya = Math.Min(ya, v.y);
                yb = Math.Max(yb, v.y);

                delY = Math.Min(delY, v.y);
                radius = Math.Max(radius, (v.x * v.x + v.y + v.y));
                vex.Add(v);
            }

            radius = (float) Math.Sqrt((double) radius);
            cx = (xa + xb) * 0.5f;
            cy = (ya + yb) * 0.5f;

            if (centering)
            {
                foreach (Vertex v in vex)
                {
                    v.x -= cx;
                    v.y -= cy;
                }
            }

            for (int i = 0; i < numLines; i++)
            {
                lines.Add(new Line(vex[reader.ReadInt16()], vex[reader.ReadInt16()]));
            }

            reader.Close();
            file.Close();
        }


        void prepare_vba()
        {
            float[] vertices = new float[numLines * 4];
            float[] colour = new float[numLines * 6];

            int i = 0;
            foreach (Line l in lines)
            {
                vertices[i * 2 + 0] = l.a.x;
                vertices[i * 2 + 1] = l.a.y;
                colour[i * 3 + 0] = l.a.r;
                colour[i * 3 + 1] = l.a.g;
                colour[i * 3 + 2] = l.a.b;
                i++;
                vertices[i * 2 + 0] = l.b.x;
                vertices[i * 2 + 1] = l.b.y;
                colour[i * 3 + 0] = l.b.r;
                colour[i * 3 + 1] = l.b.g;
                colour[i * 3 + 2] = l.b.b;
                i++;
            }

            if (glData == 0)
            {
                glData = GL.GenVertexArray();
                bufPos = GL.GenBuffer();
                bufCol = GL.GenBuffer();
            }

            GL.BindVertexArray(glData);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufCol);
            GL.BufferData(BufferTarget.ArrayBuffer, colour.Length * sizeof(float), colour, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindVertexArray(0);
        }


        public void deferred_draw(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f,
            float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
        {
            MeshDraw tmp = new MeshDraw();
            tmp.mesh = this;
            tmp.x = x;
            tmp.y = y;
            tmp.width = width;
            tmp.height = height;
            tmp.clamp = clamp;
            tmp.r = r;
            tmp.g = g;
            tmp.b = b;
            tmp.a = a;
            tmp.fit = false;
            drawCall.Add(tmp);
        }

        public void deferred_drawFit(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f,
            float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
        {
            MeshDraw tmp = new MeshDraw();
            tmp.mesh = this;
            tmp.x = x;
            tmp.y = y;
            tmp.width = width;
            tmp.height = height;
            tmp.clamp = clamp;
            tmp.r = r;
            tmp.g = g;
            tmp.b = b;
            tmp.a = a;
            tmp.fit = true;
            drawCall.Add(tmp);
        }


        public void draw(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f,
            float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
        {
            meshShader.bind();
            GL.Uniform2(meshShader.getUniformLocation("pos"),
                (x + width * 0.5f) * GameWindow.active.multX * 2.0f - 1.0f,
                (y + height * 0.5f * (1.0f + delY)) * GameWindow.active.multY * 2.0f - 0.0f);
            GL.Uniform3(meshShader.getUniformLocation("scale"), width * GameWindow.active.multX,
                height * GameWindow.active.multY, clamp);
            GL.Uniform4(meshShader.getUniformLocation("col"), r, g, b, a);
            GL.BindVertexArray(glData);
            GL.DrawArrays(PrimitiveType.Lines, 0, numLines * 2);
            GL.BindVertexArray(0);
        }


        public void drawFit(float x, float y, float width = 100.0f, float height = 100.0f, float clamp = 1.0f,
            float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1.0f)
        {
            float sc = Math.Max(Math.Max(boxX, boxY), 0.5f) * 2.5f;
            meshShader.bind();
            GL.Uniform2(meshShader.getUniformLocation("pos"),
                (x + width * 0.5f) * GameWindow.active.multX * 2.0f - 1.0f,
                (y + height * 0.5f) * GameWindow.active.multY * 2.0f - 0.0f);
            GL.Uniform3(meshShader.getUniformLocation("scale"), width * GameWindow.active.multX / sc,
                height * GameWindow.active.multY / sc, clamp);
            GL.Uniform4(meshShader.getUniformLocation("col"), r, g, b, a);

            GL.BindVertexArray(glData);
            GL.DrawArrays(PrimitiveType.Lines, 0, numLines * 2);
            GL.BindVertexArray(0);
        }
    }


    class Vertex
    {
        public float x, y;

        public float r, g, b;

        //public float nx, ny, ct;
        public Vertex() : this(0.0f, 0.0f)
        {
        }

        public Vertex(float x, float y, float r = 1.0f, float g = 1.0f, float b = 1.0f)
        {
            this.x = x;
            this.y = y;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    class Line
    {
        public Vertex a, b;

        public Line(Vertex a, Vertex b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public class MeshDraw
    {
        public Mesh mesh;
        public float x, y, width, height, clamp, r, g, b, a;
        public bool fit;
    }
}