using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
    public class Change
    {
        string change = "";
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
                ret += "      - " + sc + "\n"; //6 spaces ought to look good (or it does to me)
            }
            return ret;
        }
    }
}