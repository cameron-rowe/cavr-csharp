using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Text;

using cavr.util;

namespace cavr.math
{
	public class VectorBasef : IEquatable<VectorBasef>, ICloneable
	{
		protected float[] values;

		public float Length {
			get {
				return (float) Math.Sqrt(LengthSquared);
			}
		}

		public float LengthSquared {
			get {
				float result = 0.0f;

				foreach(var val in values) {
					result += (val * val);
				}

				return result;
			}
		}

		protected VectorBasef(params float[] vals) {
			values = vals;
		}

		public bool Equals(VectorBasef rhs) {
			if(values.Length == rhs.values.Length) {
				for(int i = 0; i < values.Length; i++) {
					if(values[i] != rhs.values[i]) {
						return false;
					}
				}

				return true;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			if(obj is VectorBasef) {
				return Equals((VectorBasef) obj);
			}

			return false;
		}

		// To remove compiler warning
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public float Dot(VectorBasef other) {
			if(values.Length != other.values.Length)
				throw new ArgumentException("Vectors must be same length");
			
			float result = 0.0f;

			for(int i = 0; i < values.Length; i++) {
				result += (values[i] * other.values[i]);
			}

			return result;
		}

		public object Clone() {
			return new VectorBasef((float[])values.Clone());
		}

		public void Normalize() {
			var length = Length;
			for(int i = 0; i < values.Length; i++) {
				values[i] /= length;
			}
		}

		public float this[int index] {
			get { return values[index]; }
		}

		public static bool operator == (VectorBasef lhs, VectorBasef rhs) {
			return lhs.Equals(rhs);
		}

		public static bool operator != (VectorBasef lhs, VectorBasef rhs) {
			return !lhs.Equals(rhs);
		}
	}

	public class Vector2f : VectorBasef, ICopyable<Vector2f>
	{
		public Vector2f(float x = 0.0f, float y = 0.0f) : base(x, y)
		{
		}

		public float x {
			get { return values[0]; }
		}

		public float y {
			get { return values[1]; }
		}

		public Vector2f xy {
			get { return new Vector2f(values[0], values[1]); }
		}

		public Vector2f yx {
			get { return new Vector2f(values[1], values[0]); }
		}

		public Vector2f Normalized() {
			return this / Length;
		}

		public Vector2f Copy() {
			return (Vector2f) Clone();
		}

		public static Vector2f operator + (Vector2f lhs, Vector2f rhs) {
			return new Vector2f(lhs[0] + rhs[0], lhs[1] + rhs[1]);
		}

		public static Vector2f operator - (Vector2f lhs, Vector2f rhs) {
			return new Vector2f(lhs[0] - rhs[0], lhs[1] - rhs[1]);
		}

		public static Vector2f operator * (Vector2f lhs, Vector2f rhs) {
			return new Vector2f(lhs[0] * rhs[0], lhs[1] * rhs[1]);
		}

		public static Vector2f operator / (Vector2f lhs, Vector2f rhs) {
			return new Vector2f(lhs[0] / rhs[0], lhs[1] / rhs[1]);
		}

		public static Vector2f operator * (Vector2f vec, float scalar) {
			return new Vector2f(vec[0] * scalar, vec[1] * scalar);
		}

		public static Vector2f operator / (Vector2f vec, float scalar) {
			return new Vector2f(vec[0] / scalar, vec[1] / scalar);
		}
	}

	public class Vector3f : VectorBasef, ICopyable<Vector3f>
	{
		public Vector3f(float x = 0.0f, float y = 0.0f, float z = 0.0f) : base(x, y, z)
		{
		}

		public float x {
			get { return values[0]; }
		}

		public float y {
			get { return values[1]; }
		}

		public float z {
			get { return values[2]; }
		}

		public Vector2f xy {
			get { return new Vector2f(values[0], values[1]); }
		}

		public Vector2f xz {
			get { return new Vector2f(values[0], values[2]); }
		}

		public Vector2f yx {
			get { return new Vector2f(values[1], values[0]); }
		}

		public Vector2f yz {
			get { return new Vector2f(values[1], values[2]); }
		}

		public Vector2f zx {
			get { return new Vector2f(values[2], values[0]); }
		}

		public Vector2f zy {
			get { return new Vector2f(values[2], values[1]); }
		}

		public Vector3f xyz {
			get { return new Vector3f(values[0], values[1], values[2]); }
		}

		public Vector3f xzy {
			get { return new Vector3f(values[0], values[2], values[1]); }
		}

		public Vector3f yxz {
			get { return new Vector3f(values[1], values[0], values[2]); }
		}

		public Vector3f yzx {
			get { return new Vector3f(values[1], values[2], values[0]); }
		}

		public Vector3f zxy {
			get { return new Vector3f(values[2], values[0], values[1]); }
		}

		public Vector3f zyx {
			get { return new Vector3f(values[2], values[1], values[0]); }
		}

		public Vector3f Cross(Vector3f other) {
			return new Vector3f(this[1] * other[2] - other[1] * this[2],
			                    this[2] * other[0] - other[2] * this[0],
			                    this[0] * other[1] - other[0] * this[1]);
		}

		public Vector3f Normalized() {
			return this / Length;
		}

		public Vector3f Copy() {
			return (Vector3f) Clone();
		}

		public static Vector3f operator + (Vector3f lhs, Vector3f rhs) {
			return new Vector3f(lhs[0] + rhs[0], lhs[1] + rhs[1], lhs[2] + rhs[2]);
		}

		public static Vector3f operator - (Vector3f lhs, Vector3f rhs) {
			return new Vector3f(lhs[0] - rhs[0], lhs[1] - rhs[1], lhs[2] - rhs[2]);
		}

		public static Vector3f operator * (Vector3f lhs, Vector3f rhs) {
			return new Vector3f(lhs[0] * rhs[0], lhs[1] * rhs[1], lhs[2] * rhs[2]);
		}

		public static Vector3f operator / (Vector3f lhs, Vector3f rhs) {
			return new Vector3f(lhs[0] / rhs[0], lhs[1] / rhs[1], lhs[2] / rhs[2]);
		}

		public static Vector3f operator * (Vector3f vec, float scalar) {
			return new Vector3f(vec[0] * scalar, vec[1] * scalar, vec[2] * scalar);
		}

		public static Vector3f operator / (Vector3f vec, float scalar) {
			return new Vector3f(vec[0] / scalar, vec[1] / scalar, vec[2] / scalar);
		}
	}
}

