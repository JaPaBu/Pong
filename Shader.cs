using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace Pong
{
    internal class Shader
    {
        public static Dictionary<string, ShaderType> ShaderTypes = new Dictionary<string, ShaderType>()
        {
            {".fs", ShaderType.FragmentShader},
            {".vs", ShaderType.VertexShader},
            {".gs", ShaderType.GeometryShader}
        };


        private readonly int _programId;

        public Shader(string resourcePath)
        {
            _programId = GL.CreateProgram();

            var exists = false;
            foreach (var entry in ShaderTypes)
            {
                var path = resourcePath + entry.Key;
                if (File.Exists(path))
                {
                    AttachShader(entry.Value, File.ReadAllText(path));
                    exists = true;
                }
            }

            if (!exists)
            {
                Console.WriteLine($"Shader \"{resourcePath}\" was not found!");
                return;
            }

            GL.LinkProgram(_programId);
            var infoLog = GL.GetProgramInfoLog(_programId);
            if (!string.IsNullOrEmpty(infoLog))
                Console.WriteLine($"There was an error linking shader \"{resourcePath}\": {infoLog}");
        }

        public void Bind() => GL.UseProgram(_programId);


        private void AttachShader(ShaderType type, string source)
        {
            var id = GL.CreateShader(type);
            GL.ShaderSource(id, source);
            GL.CompileShader(id);
            GL.AttachShader(_programId, id);
        }
    }
}
