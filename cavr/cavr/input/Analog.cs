using System;
using System.Threading;

namespace cavr.input
{
    public class Analog : Input
    {
        public new const string TypeName = "Analog";

        private double liveValue;
        private double val;
        private ReaderWriterLockSlim liveLock;
        private ReaderWriterLockSlim syncLock;

        public double Value { 
            get {
                syncLock.EnterReadLock();
                double result = val;
                syncLock.ExitReadLock();
                return result;
            } 
            set {
                liveLock.EnterWriteLock();
                liveValue = Math.Max(-1.0, Math.Min(val, 1.0));
                liveLock.ExitWriteLock();
            } 
        }

        public Analog()
        {
            val = 0;
            liveValue = 0;

            liveLock = new ReaderWriterLockSlim();
            syncLock = new ReaderWriterLockSlim();
        }

        public void Sync() {
            liveLock.EnterReadLock();
            double v = liveValue;
            liveLock.ExitReadLock();
            SyncState(v);
        }

        public void SyncState(double v) {
            syncLock.EnterWriteLock();
            val = v;
            syncLock.ExitWriteLock();
        }
    }
}

