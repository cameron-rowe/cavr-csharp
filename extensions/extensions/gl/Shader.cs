using System;

using NLog;
using OpenTK.Graphics.OpenGL;

using cavr.util;

namespace extensions.gl
{
	public class Shader : IDisposable
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public int Id { get; }

		private Shader(int id)
		{
			Id = id;
		}

		public void Dispose() {
			GL.DeleteShader(Id);
			GC.SuppressFinalize(this);
		}

		public static Shader FromFile(string path, ShaderType type) {
			string source;
			if(!File.LoadIntoString(path, out source)) {
				log.Error("Could not load shader {0}", path);
				return null;
			}

			return FromSource(source, type);
		}

		public static Shader FromSource(string source, ShaderType type) {
			var id = GL.CreateShader(type);
			GL.ShaderSource(id, source);
			GL.CompileShader(id);

			var info = GL.GetShaderInfoLog(id);
			log.Info(info);

			int status;
			GL.GetShader(id, ShaderParameter.CompileStatus, out status);

			if(status != 1) {
				log.Error("Failed to compile {0} shader", type);
				GL.DeleteShader(id);
				return null;
			}

			return new Shader(id);
		}
	}
}

