using System;
using System.Collections.Generic;

namespace cavr.config
{
	public enum ParameterType : int
	{
		kUnknown = 0,
		kNumber = 1,
		kString = 2,
		kTransform = 3,
		kStringList = 4,
		kConfigurationList = 5,
		kBoolean = 6,
		kOneOf = 7,
		kMarker = 8
	}

	public struct ParameterTraits<T>
	{
		public static ParameterType Type {
			get {
				Type t = typeof(T);

				if(t == typeof(double)) {
					return ParameterType.kNumber;
				}

				if(t == typeof(string)) {
					return ParameterType.kString;
				}

				if(t == typeof(Transform)) {
					return ParameterType.kTransform;
				}

				if(t == typeof(List<string>)) {
					return ParameterType.kStringList;
				}

				if(t == typeof(bool)) {
					return ParameterType.kBoolean;
				}

				return ParameterType.kUnknown;
			}
		}
	}
}

