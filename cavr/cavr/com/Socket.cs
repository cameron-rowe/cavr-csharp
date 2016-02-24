using System;
using System.Text;

using NetMQ;
using NetMQ.Sockets;

namespace cavr.com
{
    public class Socket : IDisposable
    {
        private NetMQSocket socket;

        public Socket(NetMQSocket zmqSocket)
        {
            socket = zmqSocket;
        }


        public bool Send(string data) {
            var msg = new Msg();

            var bytes = data.ToBytes();
            msg.InitGC(bytes, bytes.Length);

            bool result = socket.TrySend(ref msg, TimeSpan.Zero, false);

            if(!result) {
                Console.Error.WriteLine("ZeroMQ Socket Send Error");
            }

            msg.Close();

            return result;
        }

        public bool Recv(out string data) {
            var msg = new Msg();
            msg.InitEmpty();

            bool result = socket.TryReceive(ref msg, TimeSpan.Zero);

            if(result) {
                data = GetStringFromBytes(msg.Data);
            }
            else {
                Console.Error.WriteLine("ZeroMQ Socket Recv Error");
                data = string.Empty;
            }

            msg.Close();

            return result;
        }

        public void Dispose() {
            socket.Dispose();
            GC.SuppressFinalize(this);
        }

        public static string GetStringFromBytes(byte[] bytes) {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }


    public static class SocketStringExtensions
    {
        public static byte[] ToBytes(this string str) {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}

