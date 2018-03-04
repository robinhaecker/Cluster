using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;


namespace Cluster.Rendering.Draw3D.ProceduralGeneration
{
	class ColorGenerator
	{

		public static vec3[] genTwinColors(Random r)
		{
			float hue = (float)r.NextDouble();
			float hue2 = (hue + 0.25f*(float)r.NextDouble()) % 1.0f;
			float sat = (float)r.NextDouble();
			float lum = (float)r.NextDouble();

			vec3[] c = new vec3[2];
			c[0] = convertHSLtoRGB(hue, sat, lum);
			c[1] = convertHSLtoRGB(hue2, 1.0f-sat, 1.0f-lum);
			return c;
		}


		public static vec3 convertHSLtoRGB(float h, float s, float l)
		{
			float r = l, g = l, b = l;
			if (s > 0.0f)
			{
				float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
				float p = 2 * l - q;
				r = hue2rgb(p, q, h + 1.0f / 3.0f);
				g = hue2rgb(p, q, h);
				b = hue2rgb(p, q, h - 1.0f / 3.0f);
			}

			return new vec3(r, g, b);
		}

		public static vec3 convertRGBtoHSL(float r, float g, float b)
		{
			float h, s, l;
			float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
			h = s = l = (max + min) / 2.0f;

			if(max == min)
			{
				h = s = 0; // achromatic
			}
			else
			{
				float d = max - min;
				s = l > 0.5f ? d / (2.0f - max - min) : d / (max + min);
				if (max == r)
				{ h = (g - b) / d + (g < b ? 6.0f : 0.0f);}
				else if (max == g){ h = (b - r) / d + 2.0f; }
				else
				{
					h = (r - g) / d + 4.0f;
				}
				h /= 6.0f;
			}

			return new vec3(h, s, l);
		}








		static float hue2rgb(float p, float q, float t)
		{
			if (t < 0) t += 1.0f;
			if (t > 1) t -= 1.0f;
			if (t < 1.0f / 6.0f) return p + (q - p) * 6.0f * t;
			if (t < 1.0f / 2.0f) return q;
			if (t < 2.0f / 3.0f) return p + (q - p) * (2.0f / 3.0f - t) * 6.0f;
			return p;
		}


	}
}
