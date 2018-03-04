using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;
using Cluster.math;
using Cluster.Rendering.Appearance;
using Cluster.Rendering.Draw3D.Collisions;


namespace Cluster.Rendering.Draw3D
{
    class Camera
    {
        vec4 viewport;
        public vec4 clear_color;
        mat4 projection, m;
        float near, far;



        public Camera(float x0, float y0, float width, float height)
        {
            viewport = new vec4(x0, y0, width, height);
            clear_color = new vec4(0.0f, 0.0f, 0.0f, 1.0f);

            near = 1.0f;
            far = 1000.0f;
            float ratio = height / width * near;

            projection = mat4.projectionPerspective(-near, near, -ratio, ratio, near, far);
            m = new mat4();
        }

        public float getNear()
        {
            return near;
        }
        public float getFar()
        {
            return far;
        }
        public float[] getProjection()
        {
            return projection.convertToGL();
        }
        public float[] getInversePosition()
        {
			//return getShadowMatrix().convertToGL();
            mat4 inv = m.inverse();
            if(inv == null)
            {
                Console.WriteLine("getInversePosition(): Matrix irreversible");
            }
            return inv.convertToGL();
        }

		public vec4[] getFrustumPlanes()
		{
			mat4 M = projection *m.inverse();
			/*
			vec4[] planes = new vec4[1];

			planes[0] = new vec4(1, 0, 0, 16.0f);
			//planes[1] = new vec4(-1, 0, 0, -10.0f);


			return planes;
			*/
			vec4[] planes = new vec4[6];
			planes[0] = (M.getLine(3) + M.getLine(0)); planes[0].mult(-1.0f);//left
			planes[1] = (M.getLine(3) - M.getLine(0)); planes[1].mult(-1.0f);//right
			planes[2] = (M.getLine(3) + M.getLine(1)); planes[2].mult(-1.0f);//bottom
			planes[3] = (M.getLine(3) - M.getLine(1)); planes[3].mult(-1.0f);//top
			planes[4] = (M.getLine(3) + M.getLine(2)); planes[4].mult(-1.0f);//near
			planes[5] = (M.getLine(3) - M.getLine(2)); planes[5].mult(-1.0f);//far

			return planes;
		}

		public mat4 getShadowMatrix()
		{
			return new mat4(); // Ist momentan egal.
			/*
			mat4 M = projection * m.inverse();
			vec4 near = (M.getLine(3) + M.getLine(2)); near.mult(-1.0f);
			vec4 far = (M.getLine(3) - M.getLine(2)); far.mult(-1.0f);
			far.*/
			mat4 inv = m.inverse();
			/*
			vec3 z = Terrain.sun.normalize();
			vec3 help = inv.getAxis3(0); help.y = 0.0f;

			vec3 y = help % z;
			vec3 x = z % y;
			*/

			float ratio = (float)GameWindow.active.shadows.getHeight() / (float)GameWindow.active.shadows.getWidth();

			vec3 light = new vec3(0.0f, 0.2f, 0.0f);
			vec3 x = new vec3(0.2f, 0.0f, 0.0f);
			vec3 z = new vec3(0.0f, 0.0f, 0.2f);
			mat4 shadow = new mat4();
			shadow.setAxis(0, x);
			shadow.setAxis(1, z);
			shadow.setAxis(2, light);
			shadow = mat4.projectionOrtho(-28.0f, 28.0f, -28.0f, 28.0f, -20.0f, 40.0f);// *mat4.rotateX(-0.5f * (float)Math.PI);//, -50.0f, 50.0f);// ;

			//shadow = mat4.scale(-1.0f / 20.0f, -1.0f / 20.0f, -1.0f / 50.0f) *mat4.rotateX(-0.5f * (float)Math.PI);//, -50.0f, 50.0f);// ;
			//near = -30.0f;
			//far = 30.0f;
			return shadow;
		}




        public void render(int prerender = 0)
        {



			if(prerender == 0)
			{
				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);
				GL.Enable(EnableCap.ScissorTest);
				GL.Enable(EnableCap.DepthTest);

				if (Terrain.active != null)
				{
					render(1);
					render(2);
					//render(3);
				}
            }






			GL.Disable(EnableCap.ClipDistance0);

            if (prerender > 0)
            {
                GL.Enable(EnableCap.ClipDistance0);
				//GL.Disable(EnableCap.ClipDistance0);

                //GL.Disable(EnableCap.Blend);
                //manager.gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
				
                if (prerender == 1)
                {
					//Terrain.oben.Bind();
					float darken = 0.5f;
					//GL.ClearColor(clear_color.x * darken, clear_color.y * darken, clear_color.z * darken, 1.0f);
					GL.ClearColor(darken, darken, darken, 1.0f);
					GL.Viewport(0, 0, GameWindow.active.width, GameWindow.active.height);
                }
                else if (prerender == 2)
                {
					//Terrain.unten.Bind();

					float light = (float)Math.Min(1.0f, Math.Max(0.0f, Terrain.sun.y / Terrain.sun.length()));
					GL.ClearColor(0.7f * light, 0.5f * light, 0.1f * light, 1.0f);
					GL.Viewport(0, 0, GameWindow.active.width, GameWindow.active.height);
                }
				else if (prerender == 3)
				{
					GL.CullFace(CullFaceMode.Front);
					GL.DrawBuffer(DrawBufferMode.None);
					GL.Disable(EnableCap.ClipDistance0);
					//GL.Viewport(0, 0, GameWindow.active.shadows.getWidth(), GameWindow.active.shadows.getHeight());
					GL.Enable(EnableCap.DepthTest);
				}

				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //manager.gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, terrain.frame_buffers[prerender - 1]);
                //Console.WriteLine(terrain.frame_buffers[prerender - 1].ToString()); 

            }
            else
            {
                GL.Viewport((int)viewport.x, (int)(GameWindow.active.height - viewport.y - viewport.w), (int)viewport.z, (int)viewport.w);
                GL.Scissor((int)viewport.x, (int)(GameWindow.active.height - viewport.y - viewport.w), (int)viewport.z, (int)viewport.w);
                GL.ClearColor(clear_color.x, clear_color.y, clear_color.z, 1.0f);//clear_color.w);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				GL.Enable(EnableCap.DepthTest);
            }




            if(Terrain.active != null)
            {
                Terrain.active.render(this, prerender);
            }

            
            foreach (Model mod in Model.models)
            {
                mod.render(this, prerender);
            }





			foreach (ParticleSystem ps in ParticleSystem.list)
			{
				ps.render(this, prerender);
			}


            GL.Disable(EnableCap.ClipDistance0);





			FrameBuffer.Unbind();
			if (prerender == 1)
			{
				Terrain.oben.BindTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, GameWindow.active.width, GameWindow.active.height, 0);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			else if (prerender == 2)
			{
				Terrain.unten.BindTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, GameWindow.active.width, GameWindow.active.height, 0);
				//GL.BindTexture(TextureTarget.Texture2D, 0);

				Terrain.unten.BindDepthTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, 0, 0, GameWindow.active.width, GameWindow.active.height, 0);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			else if (prerender == 3)
			{
				//GL.ColorMask(true, true, true, true);
				GL.DrawBuffer(DrawBufferMode.Back);
				GL.CullFace(CullFaceMode.Back);
				/*
				GameWindow.active.shadows.BindTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, GameWindow.active.width, GameWindow.active.height, 0);
				//GL.BindTexture(TextureTarget.Texture2D, 0);*/
				GameWindow.active.shadows.BindDepthTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, 0, 0, GameWindow.active.shadows.getWidth(), GameWindow.active.shadows.getHeight(), 0); 
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			else if (prerender == 0)
			{
				PostProcessing.BindTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, GameWindow.active.width, GameWindow.active.height, 0);
				GL.BindTexture(TextureTarget.Texture2D, 0);

				PostProcessing.BindDepthTexture(0);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, 0, 0, GameWindow.active.width, GameWindow.active.height, 0);
				GL.BindTexture(TextureTarget.Texture2D, 0);

				PostProcessing.render(this);
			}





            GL.Viewport(0, 0, GameWindow.active.width, GameWindow.active.height);
            GL.Scissor(0, 0, GameWindow.active.width, GameWindow.active.height);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.CullFace);
        }








        public float[] getGLPosition()
        {
            return m.convertToGL();
        }


        public void translate(float x, float y, float z)
        {
            vec3 old = m.getAxis3(3);
            m.setAxis(3, x + old.x, y + old.y, z + old.z);
        }

        public void move(float x, float y, float z)
        {
            m = m * mat4.translate(x, y, z);
        }

        public void turnX(float alpha)
        {
            m = m * mat4.rotateX(alpha);
        }
        public void turnY(float alpha)
        {
            m = m * mat4.rotateY(alpha);
        }
        public void turnZ(float alpha)
        {
            m = m * mat4.rotateZ(alpha);
        }

        public void rotateX(float alpha)
        {
            vec3 trans = m.getAxis3(3);
            m.setAxis(3, 0.0f, 0.0f, 0.0f);
            m = mat4.rotateX(alpha) * m;
            m.setAxis(3, trans);
        }
        public void rotateY(float alpha)
        {
            vec3 trans = m.getAxis3(3);
            m.setAxis(3, 0.0f, 0.0f, 0.0f);
            m = mat4.rotateY(alpha) * m;
            m.setAxis(3, trans);
        }
        public void rotateZ(float alpha)
        {
            vec3 trans = m.getAxis3(3);
            m.setAxis(3, 0.0f, 0.0f, 0.0f);
            m = mat4.rotateZ(alpha) * m;
            m.setAxis(3, trans);
        }


        public float getX()
        {
            return m.m[3, 0];
        }
        public float getY()
        {
            return m.m[3, 1];
        }
        public float getZ()
        {
            return m.m[3, 2];
        }



		public Ray Pick(float screen_x, float screen_y)
		{
			Console.WriteLine("Somthing isn't qute right with picking yet.");
			float xx = 2.0f * screen_x / (float)GameWindow.active.width - 1.0f;
			float yy = -(2.0f * screen_y / (float)GameWindow.active.height - 1.0f);
			//if(Math.Abs(xx)> 1.0f || Math.Abs(yy)> 1.0f) return null;
			Ray r = new Ray(new vec3(0, 0, 1), new vec3(xx * near, yy * near * (float)GameWindow.active.height / (float)GameWindow.active.width, -near));
			return r.Transform(m);
		}









    }
}
