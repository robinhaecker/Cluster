using System;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
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
        public static Shader buildingShader;
        public static Shader unitShader, unitShieldShader;
        public static Shader shotShader;
        public static Shader particleShader;
        public static Shader spaceShader;


        static Image _spaceTex0, _spaceTex1, _spaceTex2, _spaceTex3;
        static int _glData, _bufPos;

        private static int _millisecs0, _millisecs1;


        public static void init()
        {
            planetShader = new Shader("planet.vert", "planet.frag");
            buildingShader = new Shader("building.vert", "building.frag");
            spaceShader = new Shader("space.vert", "space.frag");
            unitShader = new Shader("unit.vert", "unit.frag");
            unitShieldShader = new Shader("shield.vert", "shield.frag", "shield.geom");
            shotShader = new Shader("shot.vert", "shot.frag", "shot.geom");
            particleShader = new Shader("particle2D.vert", "particle2D.frag");

            _spaceTex0 = new Image("space0.png");
            _spaceTex1 = new Image("space1.png");
            _spaceTex2 = new Image("space2.png");
            _spaceTex3 = new Image("space3.png");

            create_vba();
            Planet.init();
            fillUniverse();

            Sector.init(); // Muss nach dem Erstellen der Zivilisationen gemacht werden, damit die Listen für Raumschiffe die richtige Anzahl haben.
            _millisecs0 = _millisecs1 = 0;
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
            float[] vertices = new float[] {-1.0f, -1.0f, -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f};

            if (_glData == 0)
            {
                _glData = GL.GenVertexArray();
                _bufPos = GL.GenBuffer();
            }


            GL.BindVertexArray(_glData);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexArrayAttrib(_glData, 0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindVertexArray(0);
        }

        public static void render()
        {
            spaceShader.bind();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _spaceTex0.tex);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _spaceTex1.tex);

            GL.Uniform3(spaceShader.getUniformLocation("scroll"), (float) scrollX, (float) scrollY,
                (float) zoom);
            GL.Uniform3(spaceShader.getUniformLocation("viewport"), GameWindow.active.width, GameWindow.active.height,
                animation);

            GL.BindVertexArray(_glData);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 4);
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
            _millisecs0 = Environment.TickCount;
            float time = Math.Min(5.0f, (float) (_millisecs0 - _millisecs1) / 10.0f);
            _millisecs1 = _millisecs0;

            Planet.simulate(time);
            Unit.update(time * 5.0f);
            Shot.update(time * 5.0f);
            Particle.update(time * 5.0f);
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
            if ((Math.Abs(spaceX - scrollX) * zoom <= 1.0f) &&
                (Math.Abs(spaceY - scrollY) * zoom <= 1.0f)) return true;
            return false;
        }
    }
}