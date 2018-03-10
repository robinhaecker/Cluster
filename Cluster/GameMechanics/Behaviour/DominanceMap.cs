using System;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Cluster.GameMechanics.Behaviour
{
    public class DominanceMap
    {
        private const int TEXTURE_SIZE = 128;
        private const int COMPONENTS_PER_PIXEL = 4;

        private float[] _data;
        private int _tex;

        private void init()
        {
            _data = new float[TEXTURE_SIZE * TEXTURE_SIZE * COMPONENTS_PER_PIXEL];
            _tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int) TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int) TextureWrapMode.ClampToBorder);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void update()
        {
            if (_tex == 0)
            {
                init();
            }

            updateDominanceMap();
            updateTexture();
        }

        void updateDominanceMap()
        {
            for (int i = 0; i < TEXTURE_SIZE; i++)
            {
                for (int j = 0; j < TEXTURE_SIZE; j++)
                {
                    for (int k = 0; k < COMPONENTS_PER_PIXEL; k++)
                    {
                        _data[(i * TEXTURE_SIZE + j) * COMPONENTS_PER_PIXEL + k] *= 0.999f;
                    }
                }
            }

            spreadColor(1.0f, 0.0f, 0.0f, 5.0f, 0.0f);
            foreach (Planet planet in Planet.planets)
            {
                spreadColor(1.0f, 0.0f, 0.0f, planet.x, planet.y);
            }

//            foreach (Unit unit in Unit.units)
//            {
//            }
        }

        private void updateTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TEXTURE_SIZE, TEXTURE_SIZE, 0,
                PixelFormat.Rgba, PixelType.Float, _data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void bindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, _tex);
        }

        private void spreadColor(float red, float green, float blue, float x, float y)
        {
            int i = (int) (x/1000.0f);
            int j = (int) (y/1000.0f);
            for (int px = Math.Max(0, i - 5); px < Math.Min(TEXTURE_SIZE, i + 5); px++)
            {
                for (int py = Math.Max(0, j - 5); py < Math.Min(TEXTURE_SIZE, j + 5); py++)
                {
//                    float distance = 1.0f * (i - px) * (i - px) + 1.0f * (j - py) * (j - py);
//                    if (distance < 20.0)
                    {
                        float intensity = 1.0f;// / distance * 10.1f;
                        _data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 0] += red * intensity;
                        _data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 1] += red * intensity;
                        _data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 2] += red * intensity;
                        _data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 3] += intensity;
                    }
                }
            }
        }
    }
}