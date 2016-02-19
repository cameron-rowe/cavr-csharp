using System;

using NetMQ;
using NetMQ.Sockets;

namespace cavr.com
{
	public static class Communications
	{
		private static NetMQContext handle = null;

		public static bool Initialize() {
			if(handle == null) {
				handle = NetMQContext.Create();
			}

			return handle != null;
		}

		public static Socket CreateRequester(string address, int port) {
			try {
				var socket = handle.CreateRequestSocket();
				var endpoint = string.Format("tcp://{0}:{1}", address, port);

				socket.Connect(endpoint);

				return new Socket(socket);
			}
			catch(NetMQException e) {
				Console.Error.WriteLine("Unable to create NetMQ Request Socket: " + e.Message);
				return null;
			}
		}

		public static Socket CreateReplier(int port) {
			try {
				var socket = handle.CreateResponseSocket();
				var bindpoint = string.Format("tcp://*:{0}", port);

				socket.Bind(bindpoint);

				return new Socket(socket);
			}
			catch(NetMQException e) {
				Console.Error.WriteLine("Unable to create NetMQ Response Socket: " + e.Message);
				return null;
			}
		}

		public static Socket CreateSubscriber(string address, int port) {
			try {
				var socket = handle.CreateSubscriberSocket();
				var endpoint = string.Format("tcp://{0}:{1}", address, port);

				socket.Connect(endpoint);

				return new Socket(socket);
			}
			catch(NetMQException e) {
				Console.Error.WriteLine("Unable to create NetMQ Subscriber Socket: " + e.Message);
				return null;
			}

		}

		public static Socket CreatePublisher(int port) {
			try {
				var socket = handle.CreatePublisherSocket();
				var bindpoint = string.Format("tcp://*:{0}", port);

				socket.Bind(bindpoint);

				return new Socket(socket);
			}
			catch(NetMQException e) {
				Console.Error.WriteLine("Unable to create NetMQ Publisher Socket: " + e.Message);
				return null;
			}
		}
	}
}

