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

		public int Dimension {
			get { return values.Length; }
		}

		public int SizeInBytes {
			get { return sizeof(float) * Dimension; }
		}

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
			set { values[index] = value; }
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

		public static Vector2f operator - (Vector2f vec) {
			return new Vector2f(-vec[0], -vec[1]);
		}

		public static implicit operator Vector3f(Vector2f vec) {
			return new Vector3f(vec[0], vec[1]);
		}

		public static implicit operator Vector2d(Vector2f vec) {
			return new Vector2d((double)vec[0], (double)vec[1]);
		}

		public static implicit operator Vector3d(Vector2f vec) {
			return new Vector3d((double)vec[0], (double)vec[1]);
		}

		public static implicit operator Vector4f(Vector2f vec) {
			return new Vector4f(vec[0], vec[1]);
		}

		public static implicit operator Vector4d(Vector2f vec) {
			return new Vector4d((double)vec[0], (double)vec[1]);
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

		public static Vector3f operator - (Vector3f vec) {
			return new Vector3f(-vec[0], -vec[1]);
		}

		public static implicit operator Vector2f(Vector3f vec) {
			return new Vector2f(vec[0], vec[1]);
		}

		public static implicit operator Vector2d(Vector3f vec) {
			return new Vector2d((double)vec[0], (double)vec[1]);
		}

		public static implicit operator Vector3d(Vector3f vec) {
			return new Vector3d((double)vec[0], (double)vec[1], (double)vec[2]);
		}

		public static implicit operator Vector4f(Vector3f vec) {
			return new Vector4f(vec[0], vec[1], vec[2]);
		}

		public static implicit operator Vector4d(Vector3f vec) {
			return new Vector4d((double)vec[0], (double)vec[1], (double)vec[2]);
		}
	}

	public class Vector4f : VectorBasef, ICopyable<Vector4f>
	{
		public Vector4f(float x = 0.0f, float y = 0.0f, float z = 0.0f, float w = 0.0f) : base(x, y, z, w)
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

		public float w {
			get { return values[3]; }
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

		public Vector4f Normalized() {
			return this / Length;
		}

		public Vector4f Copy() {
			return (Vector4f) Clone();
		}

		public static Vector4f operator + (Vector4f lhs, Vector4f rhs) {
			return new Vector4f(lhs[0] + rhs[0], lhs[1] + rhs[1], lhs[2] + rhs[2], lhs[3] + rhs[3]);
		}

		public static Vector4f operator - (Vector4f lhs, Vector4f rhs) {
			return new Vector4f(lhs[0] - rhs[0], lhs[1] - rhs[1], lhs[2] - rhs[2], lhs[3] - rhs[3]);
		}

		public static Vector4f operator * (Vector4f lhs, Vector4f rhs) {
			return new Vector4f(lhs[0] * rhs[0], lhs[1] * rhs[1], lhs[2] * rhs[2], lhs[3] * rhs[3]);
		}

		public static Vector4f operator / (Vector4f lhs, Vector4f rhs) {
			return new Vector4f(lhs[0] / rhs[0], lhs[1] / rhs[1], lhs[2] / rhs[2], lhs[3] / rhs[3]);
		}

		public static Vector4f operator * (Vector4f vec, float scalar) {
			return new Vector4f(vec[0] * scalar, vec[1] * scalar, vec[2] * scalar, vec[3] * scalar);
		}

		public static Vector4f operator / (Vector4f vec, float scalar) {
			return new Vector4f(vec[0] / scalar, vec[1] / scalar, vec[2] / scalar, vec[3] / scalar);
		}

		public static Vector4f operator - (Vector4f vec) {
			return new Vector4f(-vec[0], -vec[1], -vec[2], -vec[3]);
		}

		public static implicit operator Vector2f(Vector4f vec) {
			return new Vector2f(vec[0], vec[1]);
		}

		public static implicit operator Vector2d(Vector4f vec) {
			return new Vector2d((double)vec[0], (double)vec[1]);
		}

		public static implicit operator Vector3f(Vector4f vec) {
			return new Vector3f(vec[0], vec[1], vec[2]);
		}

		public static implicit operator Vector3d(Vector4f vec) {
			return new Vector3d((double)vec[0], (double)vec[1], (double)vec[2]);
		}

		public static implicit operator Vector4d(Vector4f vec) {
			return new Vector4d((double)vec[0], (double)vec[1], (double)vec[2], (double)vec[3]);
		}
	}

	public class VectorBased : IEquatable<VectorBased>, ICloneable
	{
		protected double[] values;

		public int Dimension {
			get { return values.Length; }
		}

		public int SizeInBytes {
			get { return sizeof(double) * Dimension; }
		}

		public double Length {
			get {
				return (double) Math.Sqrt(LengthSquared);
			}
		}

		public double LengthSquared {
			get {
				double result = 0.0f;

				foreach(var val in values) {
					result += (val * val);
				}

				return result;
			}
		}

		protected VectorBased(params double[] vals) {
			values = vals;
		}

		public bool Equals(VectorBased rhs) {
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
			if(obj is VectorBased) {
				return Equals((VectorBased) obj);
			}

			return false;
		}

		// To remove compiler warning
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public double Dot(VectorBased other) {
			if(values.Length != other.values.Length)
				throw new ArgumentException("Vectors must be same length");

			double result = 0.0f;

			for(int i = 0; i < values.Length; i++) {
				result += (values[i] * other.values[i]);
			}

			return result;
		}

		public object Clone() {
			return new VectorBased((double[])values.Clone());
		}

		public void Normalize() {
			var length = Length;
			for(int i = 0; i < values.Length; i++) {
				values[i] /= length;
			}
		}

		public double this[int index] {
			get { return values[index]; }
			set { values[index] = value; }
		}

		public static bool operator == (VectorBased lhs, VectorBased rhs) {
			return lhs.Equals(rhs);
		}

		public static bool operator != (VectorBased lhs, VectorBased rhs) {
			return !lhs.Equals(rhs);
		}
	}

	public class Vector2d : VectorBased, ICopyable<Vector2d>
	{
		public Vector2d(double x = 0.0, double y = 0.0) : base(x, y)
		{
		}

		public double x {
			get { return values[0]; }
		}

		public double y {
			get { return values[1]; }
		}

		public Vector2d xy {
			get { return new Vector2d(values[0], values[1]); }
		}

		public Vector2d yx {
			get { return new Vector2d(values[1], values[0]); }
		}

		public Vector2d Normalized() {
			return this / Length;
		}

		public Vector2d Copy() {
			return (Vector2d) Clone();
		}

		public static Vector2d operator + (Vector2d lhs, Vector2d rhs) {
			return new Vector2d(lhs[0] + rhs[0], lhs[1] + rhs[1]);
		}

		public static Vector2d operator - (Vector2d lhs, Vector2d rhs) {
			return new Vector2d(lhs[0] - rhs[0], lhs[1] - rhs[1]);
		}

		public static Vector2d operator * (Vector2d lhs, Vector2d rhs) {
			return new Vector2d(lhs[0] * rhs[0], lhs[1] * rhs[1]);
		}

		public static Vector2d operator / (Vector2d lhs, Vector2d rhs) {
			return new Vector2d(lhs[0] / rhs[0], lhs[1] / rhs[1]);
		}

		public static Vector2d operator * (Vector2d vec, double scalar) {
			return new Vector2d(vec[0] * scalar, vec[1] * scalar);
		}

		public static Vector2d operator / (Vector2d vec, double scalar) {
			return new Vector2d(vec[0] / scalar, vec[1] / scalar);
		}

		public static Vector2d operator - (Vector2d vec) {
			return new Vector2d(-vec[0], -vec[1]);
		}

		public static implicit operator Vector3d(Vector2d vec) {
			return new Vector3d(vec[0], vec[1]);
		}

		public static implicit operator Vector2f(Vector2d vec) {
			return new Vector2f((float)vec[0], (float)vec[1]);
		}

		public static implicit operator Vector3f(Vector2d vec) {
			return new Vector3f((float)vec[0], (float)vec[1]);
		}

		public static implicit operator Vector4d(Vector2d vec) {
			return new Vector4d(vec[0], vec[1]);
		}

		public static implicit operator Vector4f(Vector2d vec) {
			return new Vector4f((float)vec[0], (float)vec[1]);
		}
	}

	public class Vector3d : VectorBased, ICopyable<Vector3d>
	{
		public Vector3d(double x = 0.0, double y = 0.0, double z = 0.0) : base(x, y, z)
		{
		}

		public double x {
			get { return values[0]; }
		}

		public double y {
			get { return values[1]; }
		}

		public double z {
			get { return values[2]; }
		}

		public Vector2d xy {
			get { return new Vector2d(values[0], values[1]); }
		}

		public Vector2d xz {
			get { return new Vector2d(values[0], values[2]); }
		}

		public Vector2d yx {
			get { return new Vector2d(values[1], values[0]); }
		}

		public Vector2d yz {
			get { return new Vector2d(values[1], values[2]); }
		}

		public Vector2d zx {
			get { return new Vector2d(values[2], values[0]); }
		}

		public Vector2d zy {
			get { return new Vector2d(values[2], values[1]); }
		}

		public Vector3d xyz {
			get { return new Vector3d(values[0], values[1], values[2]); }
		}

		public Vector3d xzy {
			get { return new Vector3d(values[0], values[2], values[1]); }
		}

		public Vector3d yxz {
			get { return new Vector3d(values[1], values[0], values[2]); }
		}

		public Vector3d yzx {
			get { return new Vector3d(values[1], values[2], values[0]); }
		}

		public Vector3d zxy {
			get { return new Vector3d(values[2], values[0], values[1]); }
		}

		public Vector3d zyx {
			get { return new Vector3d(values[2], values[1], values[0]); }
		}

		public Vector3d Cross(Vector3d other) {
			return new Vector3d(this[1] * other[2] - other[1] * this[2],
				this[2] * other[0] - other[2] * this[0],
				this[0] * other[1] - other[0] * this[1]);
		}

		public Vector3d Normalized() {
			return this / Length;
		}

		public Vector3d Copy() {
			return (Vector3d) Clone();
		}

		public static Vector3d operator + (Vector3d lhs, Vector3d rhs) {
			return new Vector3d(lhs[0] + rhs[0], lhs[1] + rhs[1], lhs[2] + rhs[2]);
		}

		public static Vector3d operator - (Vector3d lhs, Vector3d rhs) {
			return new Vector3d(lhs[0] - rhs[0], lhs[1] - rhs[1], lhs[2] - rhs[2]);
		}

		public static Vector3d operator * (Vector3d lhs, Vector3d rhs) {
			return new Vector3d(lhs[0] * rhs[0], lhs[1] * rhs[1], lhs[2] * rhs[2]);
		}

		public static Vector3d operator / (Vector3d lhs, Vector3d rhs) {
			return new Vector3d(lhs[0] / rhs[0], lhs[1] / rhs[1], lhs[2] / rhs[2]);
		}

		public static Vector3d operator * (Vector3d vec, double scalar) {
			return new Vector3d(vec[0] * scalar, vec[1] * scalar, vec[2] * scalar);
		}

		public static Vector3d operator / (Vector3d vec, double scalar) {
			return new Vector3d(vec[0] / scalar, vec[1] / scalar, vec[2] / scalar);
		}

		public static Vector3d operator - (Vector3d vec) {
			return new Vector3d(-vec[0], -vec[1], -vec[2]);
		}

		public static implicit operator Vector2d(Vector3d vec) {
			return new Vector2d(vec[0], vec[1]);
		}

		public static implicit operator Vector2f(Vector3d vec) {
			return new Vector2f((float)vec[0], (float)vec[1]);
		}

		public static implicit operator Vector3f(Vector3d vec) {
			return new Vector3f((float)vec[0], (float)vec[1], (float)vec[2]);
		}

		public static implicit operator Vector4d(Vector3d vec) {
			return new Vector4d(vec[0], vec[1], vec[2]);
		}

		public static implicit operator Vector4f(Vector3d vec) {
			return new Vector4f((float)vec[0], (float)vec[1], (float)vec[2]);
		}
	}

	public class Vector4d : VectorBased, ICopyable<Vector4d>
	{
		public Vector4d(double x = 0.0, double y = 0.0, double z = 0.0, double w = 0.0) : base(x, y, z, w)
		{
		}

		public double x {
			get { return values[0]; }
		}

		public double y {
			get { return values[1]; }
		}

		public double z {
			get { return values[2]; }
		}

        public double w {
            get { return values[3]; }
        }

		public Vector2d xy {
			get { return new Vector2d(values[0], values[1]); }
		}

		public Vector2d xz {
			get { return new Vector2d(values[0], values[2]); }
		}

		public Vector2d yx {
			get { return new Vector2d(values[1], values[0]); }
		}

		public Vector2d yz {
			get { return new Vector2d(values[1], values[2]); }
		}

		public Vector2d zx {
			get { return new Vector2d(values[2], values[0]); }
		}

		public Vector2d zy {
			get { return new Vector2d(values[2], values[1]); }
		}

		public Vector3d xyz {
			get { return new Vector3d(values[0], values[1], values[2]); }
		}

		public Vector3d xzy {
			get { return new Vector3d(values[0], values[2], values[1]); }
		}

		public Vector3d yxz {
			get { return new Vector3d(values[1], values[0], values[2]); }
		}

		public Vector3d yzx {
			get { return new Vector3d(values[1], values[2], values[0]); }
		}

		public Vector3d zxy {
			get { return new Vector3d(values[2], values[0], values[1]); }
		}

		public Vector3d zyx {
			get { return new Vector3d(values[2], values[1], values[0]); }
		}

		public Vector4d Normalized() {
			return this / Length;
		}

		public Vector4d Copy() {
			return (Vector4d) Clone();
		}

		public static Vector4d operator + (Vector4d lhs, Vector4d rhs) {
			return new Vector4d(lhs[0] + rhs[0], lhs[1] + rhs[1], lhs[2] + rhs[2], lhs[3] + rhs[3]);
		}

		public static Vector4d operator - (Vector4d lhs, Vector4d rhs) {
			return new Vector4d(lhs[0] - rhs[0], lhs[1] - rhs[1], lhs[2] - rhs[2], lhs[3] - rhs[3]);
		}

		public static Vector4d operator * (Vector4d lhs, Vector4d rhs) {
			return new Vector4d(lhs[0] * rhs[0], lhs[1] * rhs[1], lhs[2] * rhs[2], lhs[3] * rhs[3]);
		}

		public static Vector4d operator / (Vector4d lhs, Vector4d rhs) {
			return new Vector4d(lhs[0] / rhs[0], lhs[1] / rhs[1], lhs[2] / rhs[2], lhs[3] / rhs[3]);
		}

		public static Vector4d operator * (Vector4d vec, double scalar) {
			return new Vector4d(vec[0] * scalar, vec[1] * scalar, vec[2] * scalar, vec[3] * scalar);
		}

		public static Vector4d operator / (Vector4d vec, double scalar) {
			return new Vector4d(vec[0] / scalar, vec[1] / scalar, vec[2] / scalar, vec[3] / scalar);
		}

		public static Vector4d operator - (Vector4d vec) {
			return new Vector4d(-vec[0], -vec[1], -vec[2], -vec[3]);
		}

		public static implicit operator Vector2f(Vector4d vec) {
			return new Vector2f((float)vec[0], (float)vec[1]);
		}

		public static implicit operator Vector2d(Vector4d vec) {
			return new Vector2d(vec[0], vec[1]);
		}

		public static implicit operator Vector3f(Vector4d vec) {
			return new Vector3f((float) vec[0], (float) vec[1], (float) vec[2]);
		}

		public static implicit operator Vector3d(Vector4d vec) {
			return new Vector3d(vec[0], vec[1], vec[2]);
		}

		public static implicit operator Vector4f(Vector4d vec) {
			return new Vector4f((float)vec[0], (float)vec[1], (float)vec[2], (float)vec[3]);
		}
	}
}

