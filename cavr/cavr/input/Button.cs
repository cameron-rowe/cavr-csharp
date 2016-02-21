using System;
using System.Threading;

namespace cavr.input
{
	public class Button : Input
	{
		public new const string TypeName = "Button";

		private Delta delta;
		private bool liveValue;
		private bool val;
		private ReaderWriterLockSlim liveLock;
		private ReaderWriterLockSlim syncLock;

		public Delta DeltaValue {
			get {
				syncLock.EnterReadLock();
				var result = delta;
				syncLock.ExitReadLock();
				return result;
			}
		}

		public Button()
		{
			val = liveValue = false;
			liveLock = new ReaderWriterLockSlim();
			syncLock = new ReaderWriterLockSlim();
		}

		public bool Pressed() {
			syncLock.EnterReadLock();
			var result = val;
			syncLock.ExitReadLock();
			return result;
		}

		public void SetState(bool isPressed) {
			syncLock.EnterWriteLock();
			liveValue = isPressed;
			syncLock.ExitWriteLock();
		}

		public void Sync() {
			liveLock.EnterReadLock();
			var isPressed = liveValue;
			liveLock.ExitReadLock();
			SyncState(isPressed);
		}

		public void SyncState(bool isPressed) {
			syncLock.EnterWriteLock();
			if(isPressed && val) {
				delta = Delta.Held;
			}
			else if(isPressed && !val) {
				delta = Delta.Pressed;
			}
			else if(!isPressed && val) {
				delta = Delta.Released;
			}
			else {
				delta = Delta.Open;
			}

			val = isPressed;

			syncLock.ExitWriteLock();
		}

		public enum Delta {
			Open = 0,
			Pressed = 1,
			Held = 2,
			Released = 3
		}
	}
}

