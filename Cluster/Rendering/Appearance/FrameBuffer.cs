using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;

namespace Cluster.Rendering.Appearance
{
    class FrameBuffer
    {

        int framebuffer;
        int texture, depthtexture;
        int depthbuffer;

        int width, height;


        public FrameBuffer(int width = 512, int height = 0)
        {
            if (height == 0) height = width;

			this.width = width;
			this.height = height;

            framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texture, 0);
			GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

			/*
            depthbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthbuffer);
			*/

			
			depthtexture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, depthtexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);


			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthtexture, 0);
			//GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32, width, height);
			//GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthbuffer);
			

			Console.WriteLine("Framebuffer status: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Framebuffer error: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());
                Console.ForegroundColor = ConsoleColor.White;
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

		/*
		public FrameBuffer()
		{
			int width = GameWindow.active.width, height = GameWindow.active.height;
			
			//Generate framebuffer
			framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

			// create a RGBA color texture
			texture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, texture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
								width, height,
								0, (PixelFormat)PixelInternalFormat.Rgba, PixelType.UnsignedByte,
								IntPtr.Zero);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texture, 0);
			GL.BindTexture(TextureTarget.Texture2D, 0);


			// create a depth texture
			depthtexture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, depthtexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32,
								width, height,
								0, (PixelFormat)PixelInternalFormat.DepthComponent32, PixelType.Float,
								IntPtr.Zero);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthtexture, 0);
			GL.BindTexture(TextureTarget.Texture2D, 0);

			////Create color attachment texture

			DrawBuffersEnum[] bufs = new DrawBuffersEnum[1] { (DrawBuffersEnum)FramebufferAttachment.ColorAttachment0 };
			GL.DrawBuffers(bufs.Length, bufs);


			Console.WriteLine("Framebuffer status: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


		}*/






        public void cleanUp()
        {
            GL.DeleteFramebuffer(framebuffer);
            GL.DeleteTexture(texture);
            GL.DeleteRenderbuffer(depthbuffer);
        }


        public void bind()
        {
			GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.Viewport(0, 0, width, height);
			//GL.Disable(EnableCap.ScissorTest);
			//GL.Scissor(0, 0, width, height);
        }

        static public void unbind()
        {
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			//GL.Enable(EnableCap.ScissorTest);
        }

		public void bindTexture(int layer = 0)
		{
			GL.ActiveTexture(TextureUnit.Texture0 + layer);
			GL.BindTexture(TextureTarget.Texture2D, texture);
		}

		public void bindDepthTexture(int layer = 0)
		{
			GL.ActiveTexture(TextureUnit.Texture0 + layer);
			GL.BindTexture(TextureTarget.Texture2D, depthtexture);
		}

		public int getTexture()
		{
			return texture;
		}

		public int getWidth()
		{
			return width;
		}
		public int getHeight()
		{
			return height;
		}


    }
}
