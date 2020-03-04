using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
    public class ChangeSet
    {
        public ChangelogVersion version { get; private set; }
        List<Change> changes = new List<Change>();

        public ChangeSet(ChangelogVersion v, List<Change> ch)
        {
            version = v;
            changes = ch;
        }
        public ChangeSet(ConfigNode vn, string cfgDirName)
        {
            string _version = "";
            if(!vn.TryGetValue("version", ref _version))
            {
                Debug.Log("[KCL] Badly formatted version in directory " + cfgDirName);
                _version = "null";
            }

            version = new ChangelogVersion(_version);

            foreach(string change in vn.GetValues("change"))
            {
                changes.Add(new Change(change, new List<string>()));
            }

            /*
            foreach(ConfigNode chn in vn.GetNodes("CHANGE"))
            {
                changes.Add(new Change(chn, cfgDirName));
            }
            */
        }
        public override string ToString()
        {
            string ret = version + "\n";
            foreach(Change c in changes)
            {
                ret += c.ToString();
            }
            return ret;
        }
    }
}
