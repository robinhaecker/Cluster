using System;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Cluster.GameMechanics.Behaviour
{
    public class DominanceMap
    {
        private const int TEXTURE_SIZE = 64;
        private const float MAX_POSITION_ON_MAP = 12500.0f;
        private const int COMPONENTS_PER_PIXEL = 4;
        private const int MAX_PIXEL_SPREAD = TEXTURE_SIZE / 16;
        private const float MAX_PIXEL_SPREAD_DISTANCE = (MAX_PIXEL_SPREAD - 0.5f) * (MAX_PIXEL_SPREAD - 0.5f);

        private float[] data;
        private int tex;

        private void init()
        {
            data = new float[TEXTURE_SIZE * TEXTURE_SIZE * COMPONENTS_PER_PIXEL];
            tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
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
            if (tex == 0)
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
                    float reduction = 0.9999f;
                    if (data[(i * TEXTURE_SIZE + j) * COMPONENTS_PER_PIXEL + 3] >= 5.0f)
                    {
                        reduction = 0.99f;
                    }

                    for (int k = 0; k < COMPONENTS_PER_PIXEL; k++)
                    {
                        data[(i * TEXTURE_SIZE + j) * COMPONENTS_PER_PIXEL + k] *= reduction;
                    }
                }
            }

            foreach (Planet planet in Planet.planets)
            {
                Civilisation civ = planet.getDominantCivilisation();
                if (civ != null)
                {
                    spreadColor(civ.red, civ.green, civ.blue, planet.x, planet.y);
                }
            }

            foreach (Unit unit in Unit.units)
            {
                spreadColor(unit.getOwner().red, unit.getOwner().green, unit.getOwner().blue, unit.x, unit.y,
                    0.1f * unit.getPrototype().population);
            }
        }

        private void updateTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TEXTURE_SIZE, TEXTURE_SIZE, 0,
                PixelFormat.Rgba, PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void bindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, tex);
        }

        private void spreadColor(float red, float green, float blue, float x, float y, float spreadIntensity = 1.0f)
        {
            int i = (int) toTexturePosition(y);
            int j = (int) toTexturePosition(x);
            for (int px = Math.Max(0, i - MAX_PIXEL_SPREAD); px < Math.Min(TEXTURE_SIZE, i + MAX_PIXEL_SPREAD); px++)
            {
                for (int py = Math.Max(0, j - MAX_PIXEL_SPREAD);
                    py < Math.Min(TEXTURE_SIZE, j + MAX_PIXEL_SPREAD);
                    py++)
                {
                    float distance = 1.0f * (i - px) * (i - px) + 1.0f * (j - py) * (j - py);
                    distance /= MAX_PIXEL_SPREAD_DISTANCE;
                    if (distance < 1.0)
                    {
                        float intensity = (1.0f - distance) * 0.007f * spreadIntensity;
                        data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 0] += 0.3f * red * intensity;
                        data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 1] += 0.3f * green * intensity;
                        data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 2] += 0.3f * blue * intensity;
                        data[(px * TEXTURE_SIZE + py) * COMPONENTS_PER_PIXEL + 3] += intensity;
                    }
                }
            }
        }

        private float toTexturePosition(float x)
        {
            return (0.5f + 0.5f * x / MAX_POSITION_ON_MAP) * TEXTURE_SIZE;
        }
    }
}