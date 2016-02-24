using System;
using System.Linq;

using cavr.util;

namespace cavr.math
{
	public class HomogeneousMatrixBasef : IEquatable<HomogeneousMatrixBasef>, ICloneable
	{
		protected float[,] values;

		public uint Dimension { get; private set; }

		public float this[int index] {
			get {
				var row = index / (int) Dimension;
				var col = index % (int) Dimension;
				return values[row, col];
			}
			set {
				var row = index / Dimension;
				var col = index % Dimension;
				values[row, col] = value;
			}
		}

		public float this[int row, int column] {
			get { return values[row, column]; }
			set { values[row, column] = value; }
		}

		protected HomogeneousMatrixBasef() {
		}

		protected HomogeneousMatrixBasef(uint dimension) {
			Dimension = dimension;
			values = new float[dimension, dimension];
		}

		protected HomogeneousMatrixBasef(float diagonal, uint dimension) : this(dimension) {
			for(uint i = 0; i < dimension; i++) {
				values[i, i] = diagonal;
			}
		}

		protected HomogeneousMatrixBasef(float[] vals, uint dimension) : this(dimension) {
			if(vals.Length != (dimension * dimension)) {
				throw new ArgumentException("Incorrect number of parameters in Matrix constructor");
			}

			uint count = 0;
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++, count++) {
					values[i, j] = vals[count];
				}
			}
		}

		protected HomogeneousMatrixBasef(float[,] vals, uint dimension) {
			if(vals.Length != (dimension * dimension)) {
				throw new ArgumentException("Incorrect number of parameters in Matrix constructor");
			}

			Dimension = dimension;
			values = (float[,]) vals.Clone();
		}

		public object Clone() {
			return (HomogeneousMatrixBased) Activator.CreateInstance(GetType(), values);
		}

		// to avoid compiler warning
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			if(obj is HomogeneousMatrixBasef) {
				return Equals((HomogeneousMatrixBasef) obj);
			}

			return false;
		}

		public bool Equals(HomogeneousMatrixBasef other) {
			if(Dimension != other.Dimension)
				return false;

			return Enumerable.SequenceEqual(values.Cast<float>(), other.values.Cast<float>());
		}

		public void Transpose() {
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++) {
					Util.Swap(ref values[i, j], ref values[j, i]);
				}
			}
		}

		public HomogeneousMatrixBasef Transposed() {
			var result = (HomogeneousMatrixBasef) Activator.CreateInstance(GetType(), Dimension);
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++) {
					result.values[i, j] = result.values[j, i];
				}
			}

			return result;
		}

		protected void Zero() {
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++) {
					values[i, j] = 0.0f;
				}
			}
		}

		public static bool operator == (HomogeneousMatrixBasef lhs, HomogeneousMatrixBasef rhs) {
			return lhs.Equals(rhs);
		}

		public static bool operator != (HomogeneousMatrixBasef lhs, HomogeneousMatrixBasef rhs) {
			return !lhs.Equals(rhs);
		}

		public static HomogeneousMatrixBasef operator + (HomogeneousMatrixBasef lhs, HomogeneousMatrixBasef rhs) {
			if(lhs.Dimension != rhs.Dimension) {
				throw new ArgumentException("Matrix sizes do not match");
			}

			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBasef) Activator.CreateInstance(lhs.GetType(), dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] = lhs.values[i, j] + rhs.values[i, j];
				}
			}

			return result;
		}

		public static HomogeneousMatrixBasef operator - (HomogeneousMatrixBasef lhs, HomogeneousMatrixBasef rhs) {
			if(lhs.Dimension != rhs.Dimension) {
				throw new ArgumentException("Matrix sizes do not match");
			}

			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBasef) Activator.CreateInstance(lhs.GetType(), dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] = lhs.values[i, j] - rhs.values[i, j];
				}
			}

			return result;
		}

		public static HomogeneousMatrixBasef operator * (HomogeneousMatrixBasef lhs, HomogeneousMatrixBasef rhs) {
			if(lhs.Dimension != rhs.Dimension) {
				throw new ArgumentException("Matrix sizes do not match");
			}

			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBasef) Activator.CreateInstance(lhs.GetType(), dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					for(uint k = 0; k < dim; k++) {
						result.values[i, k] += (lhs.values[i, j] * rhs.values[k, j]);
					}
				}
			}

			return result;
		}

		public static HomogeneousMatrixBasef operator * (HomogeneousMatrixBasef lhs, float scalar) {
			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBasef) Activator.CreateInstance(lhs.GetType(), lhs.values, dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] *= scalar;
				}
			}

			return result;
		}

		public static HomogeneousMatrixBasef operator / (HomogeneousMatrixBasef lhs, float scalar) {
			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBasef) Activator.CreateInstance(lhs.GetType(), lhs.values, dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] /= scalar;
				}
			}

			return result;
		}
	}

	public class Matrix4f : HomogeneousMatrixBasef, ICopyable<Matrix4f>
	{
		public const uint DIM = 4;

		public Matrix4f() : base(DIM) {
		}

		public Matrix4f(float diagonal) : base(diagonal, DIM) {
		}

		public Matrix4f(params float[] vals) : base(vals, DIM) {
		}

		public Matrix4f(float[,] vals) : base(vals, DIM) {
		}

		public Matrix4f Copy() {
			return (Matrix4f) Clone();
		}

		public Vector4f Row(int index) {
			return new Vector4f(values[index, 0], values[index, 1], values[index, 2], values[index, 3]);
		}

		public static Matrix4f Translate(float x, float y, float z) {
			var result = new Matrix4f(1.0f);
			result[3, 0] = x;
			result[3, 1] = y;
			result[3, 2] = z;
			return result;
		}

		public static Matrix4f Translate(Vector3f pos) {
			var result = new Matrix4f(1.0f);
			result[3, 0] = pos.x;
			result[3, 1] = pos.y;
			result[3, 2] = pos.z;
			return result;
		}

		public static Matrix4f Scale(float s) {
			var result = new Matrix4f(1.0f);
			result[0, 0] = s;
			result[1, 1] = s;
			result[2, 2] = s;
			return result;
		}

		public static Matrix4f Rotate(float radians, Vector3f axis) {
			var result = new Matrix4f(1.0f);
			float cosR = (float) Math.Cos(radians);
			float sinR = (float) Math.Sin(radians);
			float oneMinusCosR = 1.0f - cosR;
			float xx = axis.x * axis.x;
			float xy = axis.x * axis.y;
			float xz = axis.x * axis.z;
			float yy = axis.y * axis.y;
			float yz = axis.y * axis.z;
			float zz = axis.z * axis.z;
			float xSinR = axis.x * sinR;
			float ySinR = axis.y * sinR;
			float zSinR = axis.z * sinR;
			float xyOneMinusCosR = xy * oneMinusCosR;
			float xzOneMinusCosR = xz * oneMinusCosR;
			float yzOneMinusCosR = yz * oneMinusCosR;

			result[0, 0] = cosR + xx * oneMinusCosR;
			result[0, 1] = xyOneMinusCosR + zSinR;
			result[0, 2] = xzOneMinusCosR - ySinR;
			result[1, 0] = xyOneMinusCosR - zSinR;
			result[1, 1] = cosR + yy * oneMinusCosR;
			result[1, 2] = yzOneMinusCosR + xSinR;
			result[2, 0] = xzOneMinusCosR + ySinR;
			result[2, 1] = yzOneMinusCosR - xSinR;
			result[2, 2] = cosR + zz * oneMinusCosR;

			return result;
		}

		public static Matrix4f LookAt(Vector3f eyePoint, Vector3f lookPoint, Vector3f upDirection) {
			var result = new Matrix4f(1.0f);
			var zAxis = (lookPoint - eyePoint).Normalized();
			var xAxis = zAxis.Cross(upDirection.Normalized());
			xAxis.Normalize();
			var yAxis = xAxis.Cross(zAxis).Normalized();

			result[0, 0] = xAxis.x;
			result[0, 1] = yAxis.x;
			result[0, 2] = -zAxis.x;

			result[1, 0] = xAxis.y;
			result[1, 1] = yAxis.y;
			result[1, 2] = -zAxis.y;

			result[2, 0] = xAxis.z;
			result[2, 1] = yAxis.z;
			result[2, 2] = -zAxis.z;

			result[3, 0] = -xAxis.Dot(eyePoint);
			result[3, 1] = -yAxis.Dot(eyePoint);
			result[3, 2] = -zAxis.Dot(eyePoint);

			return result;
		}

		public static Matrix4f Ortho(float left, float right, float bottom, float top, float near, float far) {
			float dx = right - left;
			float dy = top - bottom;
			float dz = far - near;
			float tx = -(right + left) / dx;
			float ty = -(top + bottom) / dy;
			float tz = -(far + near) / dz;

			return new Matrix4f(2.0f / dx, 0, 0, 0,
			                    0, 2.0f / dy, 0, 0,
			                    0, 0, -2.0f / dz, 0,
			                    tx, ty, tz, 1);
		}

		public static Matrix4f Frustrum(float left, float right, float bottom, float top, float near, float far) {
			var result = new Matrix4f();

			result[0, 0] = 2.0f * near / (right - left);
			result[1, 1] = 2.0f * near / (top - bottom);
			result[2, 0] = (right + left) / (right - left);
			result[2, 1] = (top + bottom) / (top - bottom);
			result[2, 2] = -(far + near) / (far - near);
			result[2, 3] = -1;
			result[3, 2] = -2.0f * far * near / (far - near);

			return result;
		}

		public static Matrix4f Perspective(float fieldOfViewYRadians, float aspectRatio, float near, float far) {
			float f = 1.0f / (float) Math.Tan(fieldOfViewYRadians * 0.5f);
			float dz = near - far;

			return new Matrix4f(f / aspectRatio, 0, 0, 0,
			                    0, f, 0, 0,
			                    0, 0, (far + near) / dz, -1,
			                    0, 0, 2.0f * far * near / dz, 0);
		}

		public static implicit operator Matrix4d(Matrix4f mat) {
			return new Matrix4d(mat.values.Cast<double>().ToArray());
		}
	}

	public class HomogeneousMatrixBased : IEquatable<HomogeneousMatrixBased>, ICloneable
	{
		protected double[,] values;

		public uint Dimension { get; private set; }

		public double this[int index] {
			get {
				var row = index / (int) Dimension;
				var col = index % (int) Dimension;
				return values[row, col];
			}
			set {
				var row = index / Dimension;
				var col = index % Dimension;
				values[row, col] = value;
			}
		}

		public double this[int row, int column] {
			get { return values[row, column]; }
			set { values[row, column] = value; }
		}

		protected HomogeneousMatrixBased() {
		}

		protected HomogeneousMatrixBased(uint dimension) {
			Dimension = dimension;
			values = new double[dimension, dimension];
		}

		protected HomogeneousMatrixBased(double diagonal, uint dimension) : this(dimension) {
			for(uint i = 0; i < dimension; i++) {
				values[i, i] = diagonal;
			}
		}

		protected HomogeneousMatrixBased(double[] vals, uint dimension) : this(dimension) {
			if(vals.Length != (dimension * dimension)) {
				throw new ArgumentException("Incorrect number of parameters in Matrix constructor");
			}

			uint count = 0;
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++, count++) {
					values[i, j] = vals[count];
				}
			}
		}

		protected HomogeneousMatrixBased(double[,] vals, uint dimension) {
			if(vals.Length != (dimension * dimension)) {
				throw new ArgumentException("Incorrect number of parameters in Matrix constructor");
			}

			Dimension = dimension;
			values = (double[,]) vals.Clone();
		}

		public object Clone() {
			return (HomogeneousMatrixBased) Activator.CreateInstance(GetType(), values);
		}

		// to avoid compiler warning
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			if(obj is HomogeneousMatrixBased) {
				return Equals((HomogeneousMatrixBased) obj);
			}

			return false;
		}

		public bool Equals(HomogeneousMatrixBased other) {
			if(Dimension != other.Dimension)
				return false;
			
			return Enumerable.SequenceEqual(values.Cast<double>(), other.values.Cast<double>());
		}

		public void Transpose() {
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++) {
					Util.Swap(ref values[i, j], ref values[j, i]);
				}
			}
		}

		public HomogeneousMatrixBased Transposed() {
			var result = (HomogeneousMatrixBased) Activator.CreateInstance(GetType(), Dimension);
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++) {
					result.values[i, j] = result.values[j, i];
				}
			}

			return result;
		}

		protected void Zero() {
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++) {
					values[i, j] = 0.0;
				}
			}
		}

		public static bool operator == (HomogeneousMatrixBased lhs, HomogeneousMatrixBased rhs) {
			return lhs.Equals(rhs);
		}

		public static bool operator != (HomogeneousMatrixBased lhs, HomogeneousMatrixBased rhs) {
			return !lhs.Equals(rhs);
		}

		public static HomogeneousMatrixBased operator + (HomogeneousMatrixBased lhs, HomogeneousMatrixBased rhs) {
			if(lhs.Dimension != rhs.Dimension) {
				throw new ArgumentException("Matrix sizes do not match");
			}

			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBased) Activator.CreateInstance(lhs.GetType(), dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] = lhs.values[i, j] + rhs.values[i, j];
				}
			}

			return result;
		}

		public static HomogeneousMatrixBased operator - (HomogeneousMatrixBased lhs, HomogeneousMatrixBased rhs) {
			if(lhs.Dimension != rhs.Dimension) {
				throw new ArgumentException("Matrix sizes do not match");
			}

			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBased) Activator.CreateInstance(lhs.GetType(), dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] = lhs.values[i, j] - rhs.values[i, j];
				}
			}

			return result;
		}

		public static HomogeneousMatrixBased operator * (HomogeneousMatrixBased lhs, HomogeneousMatrixBased rhs) {
			if(lhs.Dimension != rhs.Dimension) {
				throw new ArgumentException("Matrix sizes do not match");
			}

			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBased) Activator.CreateInstance(lhs.GetType(), dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					for(uint k = 0; k < dim; k++) {
						result.values[i, k] += (lhs.values[i, j] * rhs.values[k, j]);
					}
				}
			}

			return result;
		}

		public static HomogeneousMatrixBased operator * (HomogeneousMatrixBased lhs, double scalar) {
			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBased) Activator.CreateInstance(lhs.GetType(), lhs.values, dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] *= scalar;
				}
			}

			return result;
		}

		public static HomogeneousMatrixBased operator / (HomogeneousMatrixBased lhs, double scalar) {
			var dim = lhs.Dimension;
			var result = (HomogeneousMatrixBased) Activator.CreateInstance(lhs.GetType(), lhs.values, dim);
			for(uint i = 0; i < dim; i++) {
				for(uint j = 0; j < dim; j++) {
					result.values[i, j] /= scalar;
				}
			}

			return result;
		}
	}

	public class Matrix4d : HomogeneousMatrixBased, ICopyable<Matrix4d>
	{
		public const uint DIM = 4;

		public Matrix4d() : base(DIM) {
		}

		public Matrix4d(double diagonal) : base(diagonal, DIM) {
		}

		public Matrix4d(params double[] vals) : base(vals, DIM) {
		}

		public Matrix4d(double[,] vals) : base(vals, DIM) {
		}

		public Matrix4d Copy() {
			return (Matrix4d) Clone();
		}

		public Vector4d Row(int index) {
			return new Vector4d(values[index, 0], values[index, 1], values[index, 2], values[index, 3]);
		}

		public static Matrix4d Translate(double x, double y, double z) {
			var result = new Matrix4d(1.0);
			result[3, 0] = x;
			result[3, 1] = y;
			result[3, 2] = z;
			return result;
		}

		public static Matrix4d Translate(Vector3d pos) {
			var result = new Matrix4d(1.0);
			result[3, 0] = pos.x;
			result[3, 1] = pos.y;
			result[3, 2] = pos.z;
			return result;
		}

		public static Matrix4d Scale(double s) {
			var result = new Matrix4d(1.0);
			result[0, 0] = s;
			result[1, 1] = s;
			result[2, 2] = s;
			return result;
		}

		public static Matrix4d Rotate(double radians, Vector3d axis) {
			var result = new Matrix4d(1.0);
			double cosR = Math.Cos(radians);
			double sinR = Math.Sin(radians);
			double oneMinusCosR = 1.0 - cosR;
			double xx = axis.x * axis.x;
			double xy = axis.x * axis.y;
			double xz = axis.x * axis.z;
			double yy = axis.y * axis.y;
			double yz = axis.y * axis.z;
			double zz = axis.z * axis.z;
			double xSinR = axis.x * sinR;
			double ySinR = axis.y * sinR;
			double zSinR = axis.z * sinR;
			double xyOneMinusCosR = xy * oneMinusCosR;
			double xzOneMinusCosR = xz * oneMinusCosR;
			double yzOneMinusCosR = yz * oneMinusCosR;

			result[0, 0] = cosR + xx * oneMinusCosR;
			result[0, 1] = xyOneMinusCosR + zSinR;
			result[0, 2] = xzOneMinusCosR - ySinR;
			result[1, 0] = xyOneMinusCosR - zSinR;
			result[1, 1] = cosR + yy * oneMinusCosR;
			result[1, 2] = yzOneMinusCosR + xSinR;
			result[2, 0] = xzOneMinusCosR + ySinR;
			result[2, 1] = yzOneMinusCosR - xSinR;
			result[2, 2] = cosR + zz * oneMinusCosR;

			return result;
		}

		public static Matrix4d LookAt(Vector3d eyePoint, Vector3d lookPoint, Vector3d upDirection) {
			var result = new Matrix4d(1.0);
			var zAxis = (lookPoint - eyePoint).Normalized();
			var xAxis = zAxis.Cross(upDirection.Normalized());
			xAxis.Normalize();
			var yAxis = xAxis.Cross(zAxis).Normalized();

			result[0, 0] = xAxis.x;
			result[0, 1] = yAxis.x;
			result[0, 2] = -zAxis.x;

			result[1, 0] = xAxis.y;
			result[1, 1] = yAxis.y;
			result[1, 2] = -zAxis.y;

			result[2, 0] = xAxis.z;
			result[2, 1] = yAxis.z;
			result[2, 2] = -zAxis.z;

			result[3, 0] = -xAxis.Dot(eyePoint);
			result[3, 1] = -yAxis.Dot(eyePoint);
			result[3, 2] = -zAxis.Dot(eyePoint);

			return result;
		}

		public static Matrix4d Ortho(double left, double right, double bottom, double top, double near, double far) {
			double dx = right - left;
			double dy = top - bottom;
			double dz = far - near;
			double tx = -(right + left) / dx;
			double ty = -(top + bottom) / dy;
			double tz = -(far + near) / dz;

			return new Matrix4d(2.0 / dx, 0, 0, 0,
			                      0, 2.0 / dy, 0, 0,
			                      0, 0, -2.0 / dz, 0,
			                      tx, ty, tz, 1);
		}

		public static Matrix4d Frustrum(double left, double right, double bottom, double top, double near, double far) {
			var result = new Matrix4d();

			result[0, 0] = 2.0 * near / (right - left);
			result[1, 1] = 2.0 * near / (top - bottom);
			result[2, 0] = (right + left) / (right - left);
			result[2, 1] = (top + bottom) / (top - bottom);
			result[2, 2] = -(far + near) / (far - near);
			result[2, 3] = -1;
			result[3, 2] = -2.0 * far * near / (far - near);

			return result;
		}

		public static Matrix4d Perspective(double fieldOfViewYRadians, double aspectRatio, double near, double far) {
			double f = 1.0 / Math.Tan(fieldOfViewYRadians * 0.5);
			double dz = near - far;

			return new Matrix4d(f / aspectRatio, 0, 0, 0,
			                      0, f, 0, 0,
			                      0, 0, (far + near) / dz, -1,
			                      0, 0, 2.0 * far * near / dz, 0);
		}

		public static implicit operator Matrix4f(Matrix4d mat) {
			return new Matrix4f(mat.values.Cast<float>().ToArray());
		}
	}
}

