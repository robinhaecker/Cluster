using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using OpenTK.Graphics.OpenGL;

using Cluster;




namespace Cluster.Rendering.Appearance
{

    class Shader
    {
            
        int program;
        int vert, frag, geom;
        


        public Shader(string vertexShaderSrc, string fragmentShaderSrc, string geometryShaderSrc = "")
        {
            
            string vertexShaderSource = File.ReadAllText(GameWindow.BASE_FOLDER + "shaders/" + vertexShaderSrc);
            string fragmentShaderSource = File.ReadAllText(GameWindow.BASE_FOLDER + "shaders/" + fragmentShaderSrc);
			string geometryShaderSource = "";
			if (geometryShaderSrc != "") geometryShaderSource = File.ReadAllText(GameWindow.BASE_FOLDER + "shaders/" + geometryShaderSrc);

            program = GL.CreateProgram();

            vert = GL.CreateShader(ShaderType.VertexShader);
            frag = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(vert, vertexShaderSource);
            GL.ShaderSource(frag, fragmentShaderSource);

            GL.CompileShader(vert);
            GL.CompileShader(frag);



            
            string log = GL.GetShaderInfoLog(vert);
            if (log != "") Console.Write("Compilation Error in '" + vertexShaderSrc + "':\n" + log + "\n");
            log = GL.GetShaderInfoLog(frag);
            if (log != "") Console.Write("Compilation Error in '" + fragmentShaderSrc + "':\n" + log + "\n");


			if (geometryShaderSrc != "")
			{
				geom = GL.CreateShader(ShaderType.GeometryShader);
				GL.ShaderSource(geom, geometryShaderSource);
				GL.CompileShader(geom);
				log = GL.GetShaderInfoLog(geom);
				if (log != "") Console.Write("Compilation Error in '" + geometryShaderSrc + "':\n" + log + "\n");

				GL.AttachShader(program, geom);
			}

            GL.AttachShader(program, vert);
            GL.AttachShader(program, frag);

            GL.LinkProgram(program);
            
            GL.DetachShader(program, vert);
            GL.DetachShader(program, frag);

			if (geom != 0)
			{
				GL.DetachShader(program, geom);
			}

        }


        public void bind()
        {
            GL.UseProgram(program);
        }

        public static void unbind()
        {
            GL.UseProgram(0);
        }

        public int getHandle()
        {
            return program;
        }

        public int getUniformLocation(string uniform)
        {
            return GL.GetUniformLocation(program, uniform);
        }

		public void cleanUp()
		{
			GL.DeleteProgram(program);
			GL.DeleteShader(vert);
			GL.DeleteShader(frag);
			if (geom != 0) GL.DeleteShader(geom);
		}



    }

}
