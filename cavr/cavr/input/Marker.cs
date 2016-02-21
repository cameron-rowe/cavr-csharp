using System;

using cavr.math;

namespace cavr.input
{
	public interface Marker
	{
		Vector3d Position { get; }
	}

	public struct StaticMarker : Marker
	{
		public Vector3d Position { get; private set; }

		public StaticMarker(Vector3d pos) {
			Position = pos;
		}
	}

	public struct SixDOFMarker : Marker
	{
		private SixDOF sixdof;
		private Matrix4x4d pretransform;
		private Matrix4x4d posttransform;

		public Vector3d Position {
			get {
				var m = pretransform * sixdof.Matrix * posttransform as Matrix4x4d;
				return m.Row(3);
			}
		}

		public SixDOFMarker(Matrix4x4d pre, Matrix4x4d post, SixDOF sixd) {
			sixdof = sixd;
			pretransform  = pre;
			posttransform = post;
		}
	}
}

