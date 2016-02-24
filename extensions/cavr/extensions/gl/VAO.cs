using System;

using OpenTK.Graphics.OpenGL;

namespace cavr.extensions.gl
{
	public class VAO : IDisposable
	{
		public int Id { get; }

		public VAO() {
			Id = GL.GenVertexArray();
		}

		public void Dispose() {
			GL.DeleteVertexArray(Id);
			GC.SuppressFinalize(this);
		}

		public void SetAttribute(int index, VBO vbo, int numComponents, VertexAttribPointerType dataType, bool normalized, int stride, int offset) {
			GL.BindVertexArray(Id);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.Id);
			GL.EnableVertexAttribArray(index);
			GL.VertexAttribPointer(index, numComponents, dataType, normalized, stride, new IntPtr(offset));
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		public void Bind() {
			GL.BindVertexArray(Id);
		}
	}
}

