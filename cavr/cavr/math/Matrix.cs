using System;

using cavr.util;

namespace cavr.math
{
	public class Matrix4f
	{
		public Matrix4f(params float[] vals)
		{
		}
	}

	public class MatrixBased : IEquatable<MatrixBased>, ICloneable
	{
		public uint Dimension { get; private set; }
		protected double[,] values;

		public double this[int row, int column] {
			get { return values[row, column]; }
			set { values[row, column] = value; }
		}

		protected MatrixBased(uint dimension) {
			Dimension = dimension;
			values = new double[dimension, dimension];
		}

		protected MatrixBased(double diagonal, uint dimension) : this(dimension) {
			for(uint i = 0; i < dimension; i++) {
				values[i, i] = diagonal;
			}
		}

		protected MatrixBased(double[] vals, uint dimension) : this(dimension) {
			uint count = 0;
			for(uint i = 0; i < Dimension; i++) {
				for(uint j = 0; j < Dimension; j++, count++) {
					values[i, j] = vals[count];
				}
			}
		}

		public object Clone() {
			return this;
		}

		// to avoid compiler warning
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			if(obj is MatrixBased) {
				return Equals((MatrixBased) obj);
			}

			return false;
		}

		public bool Equals(MatrixBased other) {
			for(int i = 0; i < Dimension; i++) {
				for(int j = 0; j < Dimension; j++) {
					if(values[i, j] != other.values[i, j]) {
						return false;
					}
				}
			}
			return true;
		}
	}

	public class Matrix3d : MatrixBased, ICopyable<Matrix3d>
	{
		public const uint DIM = 3;

		public Matrix3d() : base(DIM) {
		}

		public Matrix3d(double diagonal) : base(diagonal, DIM) {
		}

		public Matrix3d(params double[] vals) : base(vals, DIM) {
		}

		public Matrix3d Copy() {
			return this;
		}

		public static Matrix3d Scale(double s) {
			var result = new Matrix3d(s);
			return result;
		}
	}

	public class Matrix4d
	{
		public const uint DIM = 4;
	}
}

