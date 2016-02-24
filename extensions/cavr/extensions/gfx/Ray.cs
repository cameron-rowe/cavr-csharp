
using cavr.math;

namespace cavr.extensions.gfx
{
	public struct Ray
	{
		public Vector3f Origin { get; }
		public Vector3f Direction { get; }

		public Ray(Vector3f origin, Vector3f direction) {
			Origin = origin.Copy();
			Direction = direction.Copy();
		}
	}
}

