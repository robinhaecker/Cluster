using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace Cluster.Rendering.Appearance
{
    public class Shader
    {
        int program;
        int vert, frag, geom;

        public Shader(string vertexShaderSource, string fragmentShaderSource, string geometryShaderSource = "")
        {
            program = GL.CreateProgram();

            vert = GL.CreateShader(ShaderType.VertexShader);
            frag = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vert, vertexShaderSource);
            GL.ShaderSource(frag, fragmentShaderSource);

            GL.CompileShader(vert);
            GL.CompileShader(frag);

            string log = GL.GetShaderInfoLog(vert);
            if (log != "") Debug.Write("Compilation Error in vertex shader:\n" + log + "\n");
            log = GL.GetShaderInfoLog(frag);
            if (log != "") Debug.Write("Compilation Error in fragment shader:\n" + log + "\n");

            if (geometryShaderSource != "")
            {
                geom = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(geom, geometryShaderSource);
                GL.CompileShader(geom);
                log = GL.GetShaderInfoLog(geom);
                if (log != "") Debug.Write("Compilation Error in geometry shader:\n" + log + "\n");

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