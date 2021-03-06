﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using Cluster.Rendering.Appearance;
using Cluster.Mathematics;
using Cluster.Properties;


namespace Cluster.Rendering.Draw2D
{
    class Text
    {
        // Textdarstellung
        static Shader textShader;
        static int font;

        const int TEXT_BUFFER_SIZE = 1000;
        static float[] textRgba; //r, g, b, a
        static float[] textPos; //x, y, scale
        static float[] textChar; //chars
        static int bufRgba, bufPos, bufChar;
        static int vertexBufferArray;
        static int numChars;

        static float defaultRed = 1.0f,
            defaultGreen = 1.0f,
            defaultBlue = 1.0f,
            defaultAlpha = 1.0f,
            defaultFontsize = 20.0f;


        public static void setColor(float r, float g, float b, float a)
        {
            defaultAlpha = a;
            defaultRed = r;
            defaultGreen = g;
            defaultBlue = b;
        }

        public static void setTextSize(float sz = 20.0f)
        {
            defaultFontsize = sz;
        }

        public static Vec4 getColor()
        {
            return new Vec4(defaultRed, defaultGreen, defaultBlue, defaultAlpha);
        }

        public static float getTextSize()
        {
            return defaultFontsize;
        }

        public static void drawText(string text, float x, float y)
        {
            drawText(text, x, y, defaultFontsize, defaultRed, defaultGreen, defaultBlue, defaultAlpha);
        }

        public static void drawText(string text, float x, float y, float size, float r, float g, float b, float a)
        {
            float r0 = r, g0 = g, b0 = b;
            float x0 = x;
            int offset = 0;
            bool jump = false;
            //Debug.WriteLine("drawText() -> "+text);
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
                    x += size * 0.5f * (float) (6 - offset);
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
                            r = 1.0f;
                            g = 0.0f;
                            b = 0.0f;
                            break;
                        case 'h':
                            r = 1.5f;
                            g = 1.5f;
                            b = 1.5f;
                            break;
                        case 'g':
                            r = 0.0f;
                            g = 1.0f;
                            b = 0.0f;
                            break;
                        case 'b':
                            r = 0.0f;
                            g = 0.0f;
                            b = 1.0f;
                            break;
                        case 'y':
                            r = 1.0f;
                            g = 1.0f;
                            b = 0.0f;
                            break;
                        case 'm':
                            r = 1.0f;
                            g = 0.0f;
                            b = 1.0f;
                            break;
                        case 'o':
                            r = 1.0f;
                            g = 0.5f;
                            b = 0.0f;
                            break;
                        case 'p':
                            r = 0.5f;
                            g = 0.0f;
                            b = 1.0f;
                            break;
                        case 'k':
                            r = 0.0f;
                            g = 0.0f;
                            b = 0.0f;
                            break;
                        case 'd': //Dunkle Farbe -> Muss noch entschieden werden, welche Farbe dunkel sein muss
                            i++;
                            switch (chars[i])
                            {
                                case 'r':
                                    r = 0.5f;
                                    g = 0.0f;
                                    b = 0.0f;
                                    break;
                                case 'g':
                                    r = 0.0f;
                                    g = 0.5f;
                                    b = 0.0f;
                                    break;
                                case 'b':
                                    r = 0.0f;
                                    g = 0.0f;
                                    b = 0.5f;
                                    break;
                                case 'p':
                                    r = 0.25f;
                                    g = 0.0f;
                                    b = 0.5f;
                                    break;
                                case 'k':
                                    r = 0.3f;
                                    g = 0.3f;
                                    b = 0.3f;
                                    break;
                                case 't':
                                    r = 0.0f;
                                    g = 0.5f;
                                    b = 0.5f;
                                    break;
                                case 'm':
                                    r = 0.5f;
                                    g = 0.0f;
                                    b = 0.5f;
                                    break;
                                case 'w':
                                    r = 0.7f;
                                    g = 0.7f;
                                    b = 0.7f;
                                    break;
                                case 'n':
                                    r = 0.5f * r0;
                                    g = 0.5f * g0;
                                    b = 0.5f * b0;
                                    break;
                            }

                            break;
                        case 't':
                            r = 0.0f;
                            g = 1.0f;
                            b = 1.0f;
                            break;
                        case 'w':
                            r = 1.0f;
                            g = 1.0f;
                            b = 1.0f;
                            break;
                        case 'n':
                            r = r0;
                            g = g0;
                            b = b0;
                            break;
                        default:
                            jump = true;
                            i--;
                            break;
                    }
                }
                else
                {
                    jump = false;
                    textRgba[numChars * 4 + 0] = r;
                    textRgba[numChars * 4 + 1] = g;
                    textRgba[numChars * 4 + 2] = b;
                    textRgba[numChars * 4 + 3] = a;
                    textPos[numChars * 3 + 0] = x;
                    textPos[numChars * 3 + 1] = y;
                    textPos[numChars * 3 + 2] = size;
                    try
                    {
                        textChar[numChars] = (float) Convert.ToByte(chars[i]);
                    }
                    catch
                    {
                        textChar[numChars] = 0.0f;
                    }

                    x += size * 0.5f;
                    numChars++;
                    if (numChars >= TEXT_BUFFER_SIZE) renderText();
                }
            }
        }

        public static float textHeight(string text, float size = -1.0f)
        {
            if (size < 0.0f) size = defaultFontsize;
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

        public static float estimatedTextWidth(string text, float size = -1.0f)
        {
            if (size < 0.0f) size = defaultFontsize;
            float width = 0.0f;
            foreach (string zeile in text.Split('\n'))
            {
                width = Math.Max(width, size * zeile.Length);
            }

            return width;
        }

        public static void init(string fontUrl = "Courier.png")
        {
            textShader = new Shader(Resources.text_vert, Resources.text_frag, Resources.text_geom);

            Bitmap data = new Bitmap(GameWindow.BASE_FOLDER + "textures/" + fontUrl);
            BitmapData bdat = data.LockBits(new Rectangle(0, 0, data.Width, data.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            font = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, font);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bdat.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.LinearMipmapLinear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            data.UnlockBits(bdat);
            //font = new texture(GameWindow.BASE_FOLDER + "textures/" + font_url);


            //font = new texture(FILE_DIRECTORY + "textures/Standard.png");
            textRgba = new float[TEXT_BUFFER_SIZE * 4];
            textPos = new float[TEXT_BUFFER_SIZE * 3];
            textChar = new float[TEXT_BUFFER_SIZE];
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

                bufRgba = GL.GenBuffer();
                bufChar = GL.GenBuffer();
                bufPos = GL.GenBuffer();
            }


            GL.BindVertexArray(vertexBufferArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufRgba);
            GL.BufferData(BufferTarget.ArrayBuffer, textRgba.Length * sizeof(float), textRgba,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufChar);
            GL.BufferData(BufferTarget.ArrayBuffer, textChar.Length * sizeof(float), textChar,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufPos);
            GL.BufferData(BufferTarget.ArrayBuffer, textPos.Length * sizeof(float), textPos,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
        }

        internal static void renderText()
        {
            if (numChars == 0) return;
            //Debug.WriteLine("renderText() -> num_chars = " + num_chars.ToString());
            set_buffers_text();


            textShader.bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, font);


            GL.Uniform3(textShader.getUniformLocation("viewport"), 1.0f / (float) GameWindow.active.width,
                1.0f / (float) GameWindow.active.height, 0.0f);
            GL.DrawArrays(PrimitiveType.Points, 0, numChars);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Shader.unbind();
            numChars = 0;
        }
    }
}