using System;

using cavr.util;

using Mat4 = cavr.math.Matrix4x4d;
using Vec3 = cavr.math.Vector3d;

namespace cavr.config
{
	public class Transform : ICopyable<Transform>
	{
		public Mat4 Matrix { get; private set; }

		public Transform() : this(new Mat4()) {
		}

		public Transform(Transform other) : this(other.Matrix.Copy()) {
		}

		public Transform(Mat4 mat) {
			Matrix = mat;
		}

		public Transform Copy() {
			return new Transform(Matrix);
		}

		public static Transform Identity() {
			return new Transform(new Mat4(1.0));
		}

		public static Transform Translate(double x, double y, double z) {
			return new Transform(Mat4.Translate(x, y, z));
		}

		public static Transform Rotate(double radians, double x, double y, double z) {
			return new Transform(Mat4.Rotate(radians, new Vec3(x, y, z)));
		}

		public static Transform operator * (Transform lhs, Transform rhs) {
			var result = lhs.Matrix * rhs.Matrix;
			return new Transform((Mat4) result);
		}

		public static Transform operator * (Transform lhs, double val) {
			var result = lhs.Matrix * val;
			return new Transform((Mat4) result);
		}

		public static Transform operator / (Transform lhs, double val) {
			var result = lhs.Matrix / val;
			return new Transform((Mat4) result);
		}

		public static SixdofMarker operator * (Transform lhs, SixdofMarker rhs) {
			return null;
		}
	}

	public class SixdofMarker
	{
		public string Name { get; private set; }
		public Transform PreTransform { get; private set; }
		public Transform PostTransform { get; private set; }

		public SixdofMarker(string name) : this(Transform.Identity(), name, Transform.Identity()) {
			
		}

		public SixdofMarker(Transform pre, string name, Transform post) {
			PreTransform = new Transform(pre);
			PostTransform = new Transform(post);
			Name = name;
		}

		public static SixdofMarker operator * (SixdofMarker lhs, Transform rhs) {
			return new SixdofMarker(lhs.PreTransform, lhs.Name, lhs.PostTransform * rhs);
		}
	}

	public class Vec
	{
		public Vec3 Vector { get; private set; }

		public Vec() {
			Vector = new Vec3();
		}

		public Vec(Vec3 v) {
			Vector = v.Copy();
		}

		public Vec(double x) {
			Vector = new Vec3(x);
		}

		public Vec(double x, double y, double z) {
			Vector = new Vec3(x, y, z);
		}

		public double this[int index] {
			get {
				return Vector[index];
			}

			set {
				Vector[index] = value;
			}
		}

		public static Vec operator + (Vec lhs, Vec rhs) {
			return new Vec(lhs.Vector + rhs.Vector);
		}

		public static Vec operator - (Vec lhs, Vec rhs) {
			return new Vec(lhs.Vector - rhs.Vector);
		}

		public static Vec operator * (Vec lhs, double val) {
			return new Vec(lhs.Vector * val);
		}

		public static Vec operator / (Vec lhs, double val) {
			return new Vec(lhs.Vector / val);
		}

	}
}

