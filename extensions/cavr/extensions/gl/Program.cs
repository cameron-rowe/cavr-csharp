using System;

using NLog;
using OpenTK.Graphics.OpenGL;

namespace cavr.extensions.gl
{
	public class Program : IDisposable
	{
		private const int GL_TRUE = 1;
		private const int GL_FALSE = 0;
		private static Logger log = LogManager.GetCurrentClassLogger();

		public int Id { get; }

		public Program()
		{
			Id = GL.CreateProgram();
		}

		public void Dispose() {
			GL.DeleteProgram(Id);
			GC.SuppressFinalize(this);
		}

		public void AddShader(Shader shader) {
			GL.AttachShader(Id, shader.Id);
		}

		public void Begin() {
			GL.UseProgram(Id);
		}

		public void End() {
			GL.UseProgram(0);
		}

		public int GetUniform(string name) {
			var location = GL.GetUniformLocation(Id, name);
			if(location == -1) {
				log.Warn("Uniform {0} does not exist", name);
			}

			return location;
		}

		public int GetAttribute(string name) {
			var location = GL.GetAttribLocation(Id, name);
			if(location == -1) {
				log.Warn("Attribute {0} does not exist", name);
			}

			return location;
		}

		public void BindFragDataLocation(int location, string name) {
			GL.BindFragDataLocation(Id, location, name);
		}

		public bool Link() {
			GL.LinkProgram(Id);

			var info = GL.GetProgramInfoLog(Id);
			log.Info(info);

			int status;
			GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out status);

			if(status == GL_FALSE) {
				log.Error("Failed to link program");
			}

			return status != GL_FALSE;
		}

		public static Program CreateSimple() {
			using(var vs = Shader.FromSource(simpleVS, ShaderType.VertexShader))
			using(var fs = Shader.FromSource(simpleFS, ShaderType.FragmentShader)) {	
				if(vs == null) {
					log.Error("Failed to load simple vertex shader");
					return null;
				}

				if(fs == null) {
					log.Error("Failed to load simple fragment shader");
					return null;
				}

				var p = new Program();

				p.AddShader(vs);
				p.AddShader(fs);
				p.BindFragDataLocation(0, "frag_color");

				if(!p.Link()) {
					log.Error("Failed to link simple shader");
					p.Dispose();
					return null;
				}

				return p;
			}
		}

		private const string simpleFS = @"
		#version 130
		out vec4 frag_color;
		uniform vec3 color;

		void main() {
			frag_color.xyz = color;
			frag_color.w = 1.0;
		}
		";

		private const string simpleVS = @"
		#version 130
		uniform mat4 model;
		uniform mat4 view;
		uniform mat4 projection
		in vec4 in_position;

		void main() {
			gl_Position = projection * view * mdoel * in_position;
		}
		";
	}
}

