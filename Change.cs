using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
	public class Change
	{
		string change = "";
		public ChangeType type { get; private set; }
		List<string> subchanges = new List<string>();

		public Change(string c)
		{
			change = c;
			subchanges = new List<string>();
		}
		public Change(string c, List<string> sc)
		{
			change = c;
			subchanges = sc;
		}
		public Change(ConfigNode chn, string cfgDirName)
		{
			if (!chn.TryGetValue("change", ref change))
			{
				Debug.Log("[KCL] Could not find a needed change field in directory " + cfgDirName);
			}
			string _changeType = "";
			if (chn.TryGetValue("type", ref _changeType))
			{
				switch (_changeType.Substring(0, 1).ToUpper())
				{
					case "A":
						type = ChangeType.Add;
						break;
					case "C":
						type = ChangeType.Change;
						break;
					case "D":
						type = ChangeType.Depreciate;
						break;
					case "R":
						type = ChangeType.Remove;
						break;
					case "F":
						type = ChangeType.Fix;
						break;
					case "S":
						type = ChangeType.Security;
						break;
					case "H":
						type = ChangeType.HighPriority;
						break;
					default:
						type = ChangeType.None;
						break;
				}
			}
			else
			{
				type = ChangeType.None;
			}
			foreach (string sc in chn.GetValues("subchange"))
			{
				subchanges.Add(sc);
			}
		}
		public override string ToString()
		{
			string ret = "";
			ret += " * " + change + "\n";
			foreach(string sc in subchanges)
			{
				ret += "   " + "   " + "- " + sc + "\n"; //6 spaces ought to look good (or it does to me)
			}
			return ret;
		}
	}
}