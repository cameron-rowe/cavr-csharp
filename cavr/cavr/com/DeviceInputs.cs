using System;
using System.Collections.Generic;
using System.IO;

using ProtoBuf;

namespace cavr.com
{
	[ProtoContract]
	public struct DeviceInputs
	{
		[ProtoMember(1)]
		public List<string> buttons;

		[ProtoMember(2)]
		public List<string> analogs;

		[ProtoMember(3)]
		public List<string> sixdofs;

		public string SerializeToString() {
			return DeviceProtoUtils.SerializeToString(this);
		}

		public static DeviceInputs CreateEmpty() {
			return new DeviceInputs {
				buttons = new List<string>(),
				analogs = new List<string>(),
				sixdofs = new List<string>()
			};
		}
	}

	[ProtoContract]
	public struct DeviceSync
	{
		[ProtoMember(1, IsPacked=true)]
		public List<bool> buttons;

		[ProtoMember(2, IsPacked=true)]
		public List<double> analogs;

		[ProtoMember(3, IsPacked=true)]
		public List<double> sixdofs;

		[ProtoMember(4, IsRequired=false)]
		public double dt;

		[ProtoMember(5, IsRequired=false)]
		public string userData;

		public string SerializeToString() {
			return DeviceProtoUtils.SerializeToString(this);
		}

		public static DeviceSync CreateEmpty() {
			return new DeviceSync {
				buttons = new List<bool>(),
				analogs = new List<double>(),
				sixdofs = new List<double>(),
				dt = 0.0,
				userData = string.Empty
			};
		}
	}

	public static class DeviceProtoUtils
	{
		public static T ParseFromString<T>(string data) {
			T obj;
			using(var memStream = new MemoryStream(Convert.FromBase64String(data))) {
				obj = Serializer.Deserialize<T>(memStream);
			}
			return obj;
		}

		public static string SerializeToString<T>(T obj) {
			string data;
			using(var memStream = new MemoryStream()) {
				Serializer.Serialize(memStream, obj);
				var sr = new StreamReader(memStream);
				data = sr.ReadToEnd();
			}
			return data;
		}
	}
}

