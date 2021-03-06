﻿using System;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Properties;
using Cluster.Rendering.Appearance;
using Cluster.Rendering.Draw2D;
using OpenTK.Graphics.OpenGL;

namespace Cluster.GameMechanics.Universe
{
    internal static class Space
    {
        public static float scrollX = 0.0f, scrollY = 0.0f, zoom = 1.0f;
        public static float animation = 0.0f;

        public static Shader planetShader;
        public static Shader planetAtmosphereShader;
        public static Shader buildingShader;
        public static Shader unitShader, unitShieldShader;
        public static Shader shotShader;
        public static Shader particleShader;
        private static Shader spaceShader;

        private static DominanceMap dominanceMap;
        static Image spaceTex0;
        static Image spaceTex1;
        static Image spaceTex2;
        static Image spaceTex3;
        public static Image planetTex0;
        static int glData;
        static int bufPos;

        private static int millisecs0, millisecs1;


        public static void init()
        {
            spaceShader = new Shader(Resources.space_vert, Resources.space_frag);
            planetShader = new Shader(Resources.planet_vert, Resources.planet_frag);
            planetAtmosphereShader = new Shader(Resources.atmosphere_vert, Resources.atmosphere_frag, Resources.atmosphere_geom);
            buildingShader = new Shader(Resources.building_vert, Resources.building_frag);
            unitShader = new Shader(Resources.unit_vert, Resources.unit_frag);
            unitShieldShader = new Shader(Resources.shield_vert, Resources.shield_frag, Resources.shield_geom);
            shotShader = new Shader(Resources.shot_vert, Resources.shot_frag, Resources.shot_geom);
            particleShader = new Shader(Resources.particle2D_vert, Resources.particle2D_frag);

            dominanceMap = new DominanceMap();
            
            spaceTex0 = new Image("space0.png").setClamp(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            spaceTex1 = new Image("space1.png").setClamp(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            spaceTex2 = new Image("space2.png").setClamp(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            spaceTex3 = new Image("space3.png").setClamp(TextureWrapMode.MirroredRepeat, TextureWrapMode.MirroredRepeat);
            planetTex0 = new Image("planet1.png").setClamp(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            create_vba();
            Planet.init();
            fillUniverse();

            Sector.init(); // Muss nach dem Erstellen der Zivilisationen gemacht werden, damit die Listen für Raumschiffe die richtige Anzahl haben.
            millisecs0 = millisecs1 = 0;
        }

        private static void fillUniverse()
        {
            Civilisation.init(5);

            new Planet(0.0f, 0.0f, 25);
            for (int i = 0; i < 100; i++)
            {
                float px = ((float) GameWindow.random.NextDouble() - 0.5f) * 20000.0f;
                float py = ((float) GameWindow.random.NextDouble() - 0.5f) * 20000.0f;

                bool tooclose = false;
                foreach (Planet p in Planet.planets)
                {
                    if (Math.Sqrt((px - p.x) * (px - p.x) + (py - p.y) * (py - p.y)) < 2000.0)
                    {
                        tooclose = true;
                        break;
                    }
                }

                if (!tooclose) new Planet(px, py, 7 + GameWindow.random.Next(19));
            }
        }

        static void create_vba()
        {
            float[] vertices = {-1.0f, -1.0f, -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f};

            if (glData == 0)
            {
                glData = GL.GenVertexArray();
                bufPos = GL.GenBuffer();
            }
            GL.BindVertexArray(glData);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindVertexArray(0);
        }

        public static void render()
        {
            spaceShader.bind();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, spaceTex0.tex);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, spaceTex1.tex);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, spaceTex2.tex);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, spaceTex3.tex);
            GL.ActiveTexture(TextureUnit.Texture4);
            dominanceMap.bindTexture();

            GL.Uniform3(spaceShader.getUniformLocation("scroll"), scrollX, scrollY, zoom);
            GL.Uniform3(spaceShader.getUniformLocation("viewport"), GameWindow.active.width, GameWindow.active.height,
                animation);

            GL.BindVertexArray(glData);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            GL.BindVertexArray(0);
            /*
            gl_data.Bind(manager.gl);

            manager.gl.DrawArrays(OpenGL.GL_TRIANGLE_FAN, 0, 4);
            gl_data.Unbind(manager.gl);
            //texture.unbind(0); texture.unbind(1); texture.unbind(2); texture.unbind(3);
            space_shader.unbind();*/
        }


        public static void update()
        {
            millisecs0 = Environment.TickCount;
            float time = Math.Min(5.0f, (millisecs0 - millisecs1) / 10.0f);
            millisecs1 = millisecs0;

            Planet.simulate(time);
            Unit.update(time * 5.0f);
            Shot.update(time * 5.0f);
            Particle.update(time * 5.0f);
            dominanceMap.update();
        }


        public static float spaceToScreenX(float spaceX)
        {
            return ((spaceX - scrollX) * zoom + (float) GameWindow.active.width) * 0.5f;
        }

        public static float spaceToScreenY(float spaceY)
        {
            return (-(spaceY - scrollY) * zoom + (float) GameWindow.active.height) * 0.5f;
        }

        public static float screenToSpaceX(float screenX)
        {
            return (screenX * 2.0f - (float) GameWindow.active.width) / zoom + scrollX;
        }

        public static float screenToSpaceY(float screenY)
        {
            return -((screenY * 2.0f - (float) GameWindow.active.height) / zoom - scrollY);
        }

        public static bool isVisible(float spaceX, float spaceY)
        {
            return (Math.Abs(spaceX - scrollX) * zoom <= 1.0f) &&
                   (Math.Abs(spaceY - scrollY) * zoom <= 1.0f);
        }
    }
}