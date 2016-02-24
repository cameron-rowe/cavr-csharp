using System;

using cavr.math;
using cavr.util;

namespace extensions.gfx
{
	public struct Sphere
	{
		public Vector3f Center { get; }
		public float Radius { get; }

		public Sphere(Vector3f center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		public bool Intersect(Ray ray, ref float t) {
			var o = ray.Origin - Center;
			var d = ray.Direction;

			var a = d.Dot(d);
			var b = 2.0f * d.Dot(o);
			var c = o.Dot(o) - (Radius * Radius);

			var discriminant = b * b - 4.0f * a * c;
			if(discriminant < 0.0f) {
				return false;
			}

			var q = 0.5f * (-b + (b < 0.0f ? 1.0f : -1.0f) * (float) Math.Sqrt(discriminant));
			var t0 = q / a;
			var t1 = c / q;

			if(t0 > t1) {
				Util.Swap(ref t0, ref t1);
			}

			if(t1 < 0.0f) {
				return false;
			}

			t = t0 < 0.0f ? t1 : t0;

			return true;
		}
	}
}

