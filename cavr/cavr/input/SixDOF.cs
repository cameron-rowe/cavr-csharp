using System;
using System.Threading;

using Vec3 = cavr.math.Vector3d;
using Mat4 = cavr.math.Matrix4x4d;

namespace cavr.input
{
	public class SixDOF : Input
	{
		public new const string TypeName = "SixDOF";

		private Mat4 state;
		private Mat4 liveState;
		private ReaderWriterLockSlim syncLock;
		private ReaderWriterLockSlim liveLock;

		public Vec3 Position {
			get {
				syncLock.EnterReadLock();
				var result = state.Row(3);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Vec3 Forward {
			get {
				syncLock.EnterReadLock();
				var result = -state.Row(2);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Vec3 Backward {
			get {
				syncLock.EnterReadLock();
				var result = state.Row(2);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Vec3 Left {
			get {
				syncLock.EnterReadLock();
				var result = -state.Row(0);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Vec3 Right {
			get {
				syncLock.EnterReadLock();
				var result = state.Row(0);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Vec3 Up {
			get {
				syncLock.EnterReadLock();
				var result = state.Row(1);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Vec3 Down {
			get {
				syncLock.EnterReadLock();
				var result = -state.Row(2);
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Mat4 Matrix {
			get {
				syncLock.EnterReadLock();
				var result = state.Copy();
				syncLock.ExitReadLock();
				return result;
			}
		}

		public SixDOF()
		{
			state = new Mat4(1.0);
			liveState = new Mat4(1.0);

			syncLock = new ReaderWriterLockSlim();
			liveLock = new ReaderWriterLockSlim();
		}

		public void Sync() {
			liveLock.EnterReadLock();
			var m = liveState.Copy();
			liveLock.ExitReadLock();
			SyncState(m);
		}

		public void SyncState(Mat4 m) {
			liveLock.EnterWriteLock();
			state = m;
			liveLock.ExitWriteLock();
		}
	}
}

