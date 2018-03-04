using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using Cluster.Rendering.Appearance;
using Cluster.math;


namespace Cluster.Rendering.Draw2D
{
	class Text
	{

		// Textdarstellung
		static Shader text_shader;
		static int font;

		const int TEXT_BUFFER_SIZE = 1000;
		static float[] text_rgba;   //r, g, b, a
		static float[] text_pos;    //x, y, scale
		static float[] text_char;   //chars
		static int buf_rgba, buf_pos, buf_char;
		static int vertexBufferArray;
		static int num_chars;

		static float default_red = 1.0f, default_green = 1.0f, default_blue = 1.0f, default_alpha = 1.0f, default_fontsize = 20.0f;










		public static void setColor(float r, float g, float b, float a)
		{
			default_alpha = a;
			default_red = r;
			default_green = g;
			default_blue = b;
		}
		public static void setTextSize(float sz = 20.0f)
		{
			default_fontsize = sz;
		}
		public static vec4 getColor()
		{
			return new vec4(default_red, default_green, default_blue, default_alpha);
		}
		public static float getTextSize()
		{
			return default_fontsize;
		}

		public static void drawText(string text, float x, float y)
		{
			drawText(text, x, y, default_fontsize, default_red, default_green, default_blue, default_alpha);
		}
		public static void drawText(string text, float x, float y, float size, float r, float g, float b, float a)
		{
			float r0 = r, g0 = g, b0 = b;
			float x0 = x;
			int offset = 0;
			bool jump = false;
			//Console.WriteLine("drawText() -> "+text);
			char[] chars = text.ToCharArray();
			for (int i = 0; i < text.Length; i++)
			{
				offset = ((offset + 1) % 5);
				if (chars[i] == '\n')
				{
					jump = false;
					x = x0;
					y += size;
					offset = 0;
				}
				else if (chars[i] == '\t')
				{
					jump = false;
					x += size * 0.5f * (float)(6 - offset);
					offset = 5;
				}
				else if (char.IsWhiteSpace(chars[i]))
				{
					jump = false;
					x += size * 0.5f;
				}
				else if (chars[i] == '&' && !jump)
				{
					offset = ((offset + 4) % 5);
					i++;
					switch (chars[i])
					{
						case 'r':
							r = 1.0f; g = 0.0f; b = 0.0f; break;
						case 'h':
							r = 1.5f; g = 1.5f; b = 1.5f; break;
						case 'g':
							r = 0.0f; g = 1.0f; b = 0.0f; break;
						case 'b':
							r = 0.0f; g = 0.0f; b = 1.0f; break;
						case 'y':
							r = 1.0f; g = 1.0f; b = 0.0f; break;
						case 'm':
							r = 1.0f; g = 0.0f; b = 1.0f; break;
						case 'o':
							r = 1.0f; g = 0.5f; b = 0.0f; break;
						case 'p':
							r = 0.5f; g = 0.0f; b = 1.0f; break;
						case 'k':
							r = 0.0f; g = 0.0f; b = 0.0f; break;
						case 'd': //Dunkle Farbe -> Muss noch entschieden werden, welche Farbe dunkel sein muss
							i++;
							switch (chars[i])
							{
								case 'r':
									r = 0.5f; g = 0.0f; b = 0.0f; break;
								case 'g':
									r = 0.0f; g = 0.5f; b = 0.0f; break;
								case 'b':
									r = 0.0f; g = 0.0f; b = 0.5f; break;
								case 'p':
									r = 0.25f; g = 0.0f; b = 0.5f; break;
								case 'k':
									r = 0.3f; g = 0.3f; b = 0.3f; break;
								case 't':
									r = 0.0f; g = 0.5f; b = 0.5f; break;
								case 'm':
									r = 0.5f; g = 0.0f; b = 0.5f; break;
								case 'w':
									r = 0.7f; g = 0.7f; b = 0.7f; break;
								case 'n':
									r = 0.5f * r0; g = 0.5f * g0; b = 0.5f * b0; break;
							}
							break;
						case 't':
							r = 0.0f; g = 1.0f; b = 1.0f; break;
						case 'w':
							r = 1.0f; g = 1.0f; b = 1.0f; break;
						case 'n':
							r = r0; g = g0; b = b0; break;
						default:
							jump = true;
							i--;
							break;
					}
				}
				else
				{
					jump = false;
					text_rgba[num_chars * 4 + 0] = r; text_rgba[num_chars * 4 + 1] = g; text_rgba[num_chars * 4 + 2] = b; text_rgba[num_chars * 4 + 3] = a;
					text_pos[num_chars * 3 + 0] = x; text_pos[num_chars * 3 + 1] = y; text_pos[num_chars * 3 + 2] = size;
					try
					{
						text_char[num_chars] = (float)Convert.ToByte(chars[i]);
					}
					catch
					{
						text_char[num_chars] = 0.0f;
					}

					x += size * 0.5f;
					num_chars++;
					if (num_chars >= TEXT_BUFFER_SIZE) renderText();
				}
			}
		}
		public static float textHeight(string text, float size = -1.0f)
		{
			if (size < 0.0f) size = default_fontsize;
			float h = size;
			char[] chars = text.ToCharArray();
			for (int i = 0; i < text.Length - 1; i++)
			{
				if (chars[i] == '\n')
				{
					h += size;
				}
			}
			return h;
		}











		public static void init(string font_url = "Courier.png")
		{
			text_shader = new Shader("text.vert", "text.frag", "text.geom");

			Bitmap data = new Bitmap(GameWindow.BASE_FOLDER + "textures/" + font_url);
			BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			font = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, font);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			data.UnlockBits(bdat);
			//font = new texture(GameWindow.BASE_FOLDER + "textures/" + font_url);


			//font = new texture(FILE_DIRECTORY + "textures/Standard.png");
			text_rgba = new float[TEXT_BUFFER_SIZE * 4];
			text_pos = new float[TEXT_BUFFER_SIZE * 3];
			text_char = new float[TEXT_BUFFER_SIZE];
		}

		static void set_buffers_text()
		{
			/*
			if (text_gl_data == null)
			{
				text_gl_data = GL.GenVertexArray(); // new VertexBufferArray();
				//text_gl_data.Create(manager.gl);
			}
			GL.BindVertexArray(text_gl_data);//			text_gl_data.Bind(manager.gl);


			//Vertex-Daten uebergeben
			if (buf_rgba == null)
			{
				buf_rgba = GL.GenBuffer();// new VertexBuffer();
				//buf_rgba.Create(manager.gl);
				buf_char = GL.GenBuffer();//new VertexBuffer();
				//buf_char.Create(manager.gl);
				buf_pos = GL.GenBuffer();//new VertexBuffer();
				//buf_pos.Create(manager.gl);
			}
			*/



			if (vertexBufferArray == 0)
			{
				vertexBufferArray = GL.GenVertexArray();

				buf_rgba = GL.GenBuffer();
				buf_char = GL.GenBuffer();
				buf_pos = GL.GenBuffer();
			}


			GL.BindVertexArray(vertexBufferArray);
			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_rgba);
			GL.BufferData(BufferTarget.ArrayBuffer, text_rgba.Length * sizeof(float), text_rgba, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_char);
			GL.BufferData(BufferTarget.ArrayBuffer, text_char.Length * sizeof(float), text_char, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 1);
			GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, buf_pos);
			GL.BufferData(BufferTarget.ArrayBuffer, text_pos.Length * sizeof(float), text_pos, BufferUsageHint.StaticDraw);
			GL.EnableVertexArrayAttrib(vertexBufferArray, 2);
			GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
			
		}
		internal static void renderText()
		{
			if (num_chars == 0) return;
			//Console.WriteLine("renderText() -> num_chars = " + num_chars.ToString());
			set_buffers_text();


			text_shader.bind();
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, font);


			GL.Uniform3(text_shader.getUniformLocation("viewport"), 1.0f/(float)GameWindow.active.width, 1.0f/(float)GameWindow.active.height, 0.0f);
			GL.DrawArrays(PrimitiveType.Points, 0, num_chars);

			GL.BindVertexArray(0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			text_shader.unbind();
			num_chars = 0;
		}
	}
}
