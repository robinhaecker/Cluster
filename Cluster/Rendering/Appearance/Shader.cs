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
        


        public Shader(string vertex_shader_src, string fragment_shader_src, string geometry_shader_src = "")
        {
            
            string vertexShaderSource = File.ReadAllText(GameWindow.BASE_FOLDER + "shaders/" + vertex_shader_src);
            string fragmentShaderSource = File.ReadAllText(GameWindow.BASE_FOLDER + "shaders/" + fragment_shader_src);
			string geometryShaderSource = "";
			if (geometry_shader_src != "") geometryShaderSource = File.ReadAllText(GameWindow.BASE_FOLDER + "shaders/" + geometry_shader_src);

            program = GL.CreateProgram();

            vert = GL.CreateShader(ShaderType.VertexShader);
            frag = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(vert, vertexShaderSource);
            GL.ShaderSource(frag, fragmentShaderSource);

            GL.CompileShader(vert);
            GL.CompileShader(frag);



            
            string log = GL.GetShaderInfoLog(vert);
            if (log != "") Console.Write("Compilation Error in '" + vertex_shader_src + "':\n" + log + "\n");
            log = GL.GetShaderInfoLog(frag);
            if (log != "") Console.Write("Compilation Error in '" + fragment_shader_src + "':\n" + log + "\n");


			if (geometry_shader_src != "")
			{
				geom = GL.CreateShader(ShaderType.GeometryShader);
				GL.ShaderSource(geom, geometryShaderSource);
				GL.CompileShader(geom);
				log = GL.GetShaderInfoLog(geom);
				if (log != "") Console.Write("Compilation Error in '" + geometry_shader_src + "':\n" + log + "\n");

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
