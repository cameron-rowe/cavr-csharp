using System;
using System.Collections.Generic;

using NLog;

namespace cavr.input
{
	public class Switch : Input
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private new const string TypeName = "Switch";
		private const string outOfRange = "OUT OF RANGE";

		private List<string> stateNames;
		private List<string> stateDescriptions;
		private uint numStates;

		public uint State { get; set; }

		public Switch(uint states = 1)
		{
			numStates = states;
			stateNames = new List<string>((int) numStates);
			stateDescriptions = new List<string>((int) numStates);
			State = 0;
		}

		public void SetStateName(uint i, string name) {
			if(i >= numStates) {
				log.Error("Inavlid state index [{0}] out of {1}", i, numStates);
			}

			else {
				stateNames[(int) i] = name;
			}
		}

		public void SetStateDescription(uint i, string description) {
			if(i >= numStates) {
				log.Error("Inavlid state index [{0}] out of {1}", i, numStates);
			}

			else {
				stateDescriptions[(int) i] = description;
			}
		}

		public uint GetNumberOfStates() {
			return numStates;
		}

		public string GetStateName(uint i) {
			if(i >= numStates) {
				log.Error("Inavlid state index [{0}] out of {1}", i, numStates);
				return outOfRange;
			}

			return stateNames[(int) i];
		}

		public string GetStateDescription(uint i) {
			if(i >= numStates) {
				log.Error("Inavlid state index [{0}] out of {1}", i, numStates);
				return outOfRange;
			}

			return stateDescriptions[(int) i];
		}
	}
}

