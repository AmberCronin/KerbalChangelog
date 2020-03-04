using System;
using System.Collections.Generic;

namespace KerbalChangelog
{
    public class Change
    {
        string change;
        List<string> subchanges;

        public Change(string c, List<string> sc)
        {
            change = c;
            subchanges = sc;
        }
        public Change(ConfigNode chn, string cfgDirName)
        {

        }
        public override string ToString()
        {
            string ret = "";
            ret += "* " + change + "\n";
            foreach(string sc in subchanges)
            {
                ret += " - " + sc + "\n";
            }
            return ret;
        }
    }
}
