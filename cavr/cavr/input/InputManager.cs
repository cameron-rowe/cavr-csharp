using System;
using System.Collections.Generic;
using System.Diagnostics;

using NLog;
using ProtoBuf;

using cavr.com;

using Mat4 = cavr.math.Matrix4d;

namespace cavr.input
{
    public static class InputManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private static Data data = new Data();

        public static ManagedInput<Button> GetButton { get; set; } = new ManagedInput<Button>();
        public static ManagedInput<Analog> GetAnalog { get; set; } = new ManagedInput<Analog>();
        public static ManagedInput<SixDOF> GetSixDOF { get; set; } = new ManagedInput<SixDOF>();

        public static bool Reset()
        {
            var result = true;
            result &= GetButton.Reset();
            result &= GetAnalog.Reset();
            result &= GetSixDOF.Reset();
            return result;
        }

        public static bool MapInputs(InputMap inputMap)
        {
            var result = true;
            foreach (var pair in inputMap.buttonMap) {
                result &= GetButton.Map(pair.Key, pair.Value);
            }
            foreach (var pair in inputMap.analogMap) {
                result &= GetAnalog.Map(pair.Key, pair.Value);
            }
            foreach (var pair in inputMap.sixdofMap) {
                result &= GetSixDOF.Map(pair.Key, pair.Value);
            }

            if (!result) {
                log.Error("An error occurred mapping inputs");
            }

            return result;
        }

        private delegate void DIFunc(DeviceInputs di);

        public static bool Initialize(Socket sync, Socket pub, bool master, int machineCount)
        {
            data.syncSocket = sync;
            data.pubSocket = pub;
            data.master = master;
            data.numMachines = machineCount;

            DIFunc addDeviceInputs = (DeviceInputs d) => {
                for (int j = 0; j < d.buttons.Count; j++) {
                    GetButton.ByDeviceNameOrCreate(d.buttons[j]);
                }

                for (int j = 0; j < d.analogs.Count; j++) {
                    GetAnalog.ByDeviceNameOrCreate(d.analogs[j]);
                }

                for (int j = 0; j < d.sixdofs.Count; j++) {
                    GetSixDOF.ByDeviceNameOrCreate(d.sixdofs[j]);
                }
            };

            DIFunc buildDeviceInputs = (DeviceInputs d) => {
                var buttons = GetButton.GetDeviceNames();
                foreach (var n in buttons) {
                    d.buttons.Add(n);
                }

                var analogs = GetAnalog.GetDeviceNames();
                foreach (var n in analogs) {
                    d.analogs.Add(n);
                }

                var sixdofs = GetSixDOF.GetDeviceNames();
                foreach (var n in sixdofs) {
                    d.sixdofs.Add(n);
                }
            };

            DeviceInputs di;
            string packet;

            if (master) {
                for (int i = 0; i < machineCount; i++) {
                    if (!sync.Recv(out packet)) {
                        log.Error("Failed to receive device inputs");
                        return false;
                    }

                    di = DeviceProtoUtils.ParseFromString<DeviceInputs>(packet);

                    if (!sync.Send(" ")) {
                        log.Error("Failed to send sync signal");
                        return false;
                    }
                }

                di = DeviceInputs.CreateEmpty();
                buildDeviceInputs(di);

                packet = di.SerializeToString();

                if (!pub.Send(packet)) {
                    log.Error("Failed to push synchronized device inputs");
                    return false;
                }
            } else { // slave
                di = DeviceInputs.CreateEmpty();
                buildDeviceInputs(di);
                packet = di.SerializeToString();

                if (!sync.Send(packet)) {
                    log.Error("Worker failed to send its device inputs");
                    return false;
                }

                if (!pub.Recv(out packet)) {
                    log.Error("Failed to recv complete device inputs");
                    return false;
                }

                di = DeviceProtoUtils.ParseFromString<DeviceInputs>(packet);
                addDeviceInputs(di);
            }

            var buttonNames = GetButton.GetDeviceNames();
            foreach (var n in buttonNames) {
                data.buttons.Add(GetButton.ByDeviceName(n));
            }

            var analogNames = GetAnalog.GetDeviceNames();
            foreach (var n in analogNames) {
                data.analogs.Add(GetAnalog.ByDeviceName(n));
            }

            var sixdofNames = GetSixDOF.GetDeviceNames();
            foreach (var n in sixdofNames) {
                data.sixdofs.Add(GetSixDOF.ByDeviceName(n));
            }

            data.lastTime = Stopwatch.StartNew();
            return true;
        }

        public static bool Sync()
        {
            string syncPacket;

            if (data.master) {
                for (int i = 0; i < data.numMachines; i++) {
                    if (!data.syncSocket.Recv(out syncPacket)) {
                        log.Error("Failed to recv sync");
                        return false;
                    } else {
                        if (!data.syncSocket.Send(syncPacket)) {
                            log.Error("Unable to send sync packet");
                            return false;
                        }
                    }
                }
            } else { // slave
                syncPacket = " ";
                if (!data.syncSocket.Send(syncPacket)) {
                    log.Error("Failed to send sync");
                    return false;
                }

                if (!data.syncSocket.Recv(out syncPacket)) {
                    log.Error("Failed to receive sync packet on slave");
                }
            }

            var ds = DeviceSync.CreateEmpty();
            if (data.master) {
                data.dt = data.lastTime.ElapsedTicks;
                data.lastTime = Stopwatch.StartNew();
                ds.dt = data.dt;
                ds.userData = data.syncData;

                foreach (var button in data.buttons) {
                    button.Sync();
                    ds.buttons.Add(button.Pressed());
                }

                foreach (var analog in data.analogs) {
                    analog.Sync();
                    ds.analogs.Add(analog.Value);
                }

                foreach (var sixdof in data.sixdofs) {
                    sixdof.Sync();
                    var m = sixdof.Matrix;
                    var dim = m.Dimension * m.Dimension;
                    for (int i = 0; i < dim; i++) {
                        ds.sixdofs.Add(m[i]);
                    }
                }

                syncPacket = ds.SerializeToString();
                if (!data.pubSocket.Send(syncPacket)) {
                    log.Error("Failed to publish device sync");
                    return false;
                }
            } else { // slave
                string packet;
                if (!data.pubSocket.Recv(out packet)) {
                    log.Error("Failed to receive device sync");
                    return false;
                }

                ds = DeviceProtoUtils.ParseFromString<DeviceSync>(packet);
                data.dt = ds.dt;
                data.syncData = ds.userData;
                for (int i = 0; i < ds.buttons.Count; i++) {
                    data.buttons[i].SyncState(ds.buttons[i]);
                }

                for (int i = 0; i < ds.analogs.Count; i++) {
                    data.analogs[i].SyncState(ds.analogs[i]);
                }

                var dim = (int)Mat4.DIM * (int)Mat4.DIM;

                for (int i = 0; i < ds.sixdofs.Count / dim; i++) {
                    var m = new Mat4();
                    var k = i * dim;
                    for (int j = 0; i < dim; j++) {
                        m[j] = ds.sixdofs[k + j];
                    }
                    data.sixdofs[i].SyncState(m);
                }
            }

            return true;
        }

        public static double Dt()
        {
            return data.dt;
        }

        public static void SetSyncData(string syncData)
        {
            data.syncData = syncData;
        }

        public static string GetSyncData()
        {
            return data.syncData;
        }

        private struct Data
        {
            public Socket syncSocket;
            public Socket pubSocket;
            public bool master;
            public int numMachines;
            public List<Button> buttons;
            public List<Analog> analogs;
            public List<SixDOF> sixdofs;
            public double dt;
            public Stopwatch lastTime;
            public string syncData;
        }
    }
}

