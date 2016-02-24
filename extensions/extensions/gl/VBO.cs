using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

using cavr.math;

namespace extensions.gl
{
	public class VBO : IDisposable
	{
		public int Id { get; }

		public VBO() {
			Id = GL.GenBuffer();
		}

		public void Dispose() {
			GL.DeleteBuffer(Id);
			GC.SuppressFinalize(this);
		}

		public VBO(List<VectorBasef> v, BufferUsageHint usage = BufferUsageHint.StaticDraw) : this() {
			var size = v.Count == 0 ? 0 : v[0].SizeInBytes * v.Count;
			BufferData(v, size, usage);
		}

		public void BufferData(List<VectorBasef> data, int size, BufferUsageHint usage) {
			GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
			GL.BufferData(BufferTarget.ArrayBuffer, size, FlattenVectorList(data), usage);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		private static float[] FlattenVectorList(List<VectorBasef> list) {
			var ret = new float[list.Count * list[0].Dimension];

			int index = 0;
			foreach(var vec in list) {
				for(int i = 0; i < vec.Dimension; i++) {
					ret[index] = vec[i];
					index++;
				}
			}

			return ret;
		}
	}
}

