using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
    public class Changelog
    {
		public string modName { get; private set; }
		public string license { get; private set; } = null;
		public string author { get; private set; } = null;

		bool showCL = true;

        List<ChangeSet> changeSets = new List<ChangeSet>();
        public ChangelogVersion highestVersion
        {
            get
            {
                changeSets.Sort();
				try
				{
					return changeSets[0].version;
				}
				catch (Exception e)
				{
					Debug.Log("[KCL] No changesets exist.");
					return null;
				}
			}
        }

        public Changelog(string mn, bool show, List<ChangeSet> cs)
        {
            modName = mn;
            showCL = show;
            changeSets = cs;
        }

        public Changelog(ConfigNode cn, UrlDir.UrlConfig cfgDir)
        {
			string cfgDirName = cfgDir.ToString();
            string _modname = "";
            if (!cn.TryGetValue("modName", ref _modname))
            {
                Debug.Log("[KCL] Missing mod name for changelog file in directory: " + cfgDirName);
                Debug.Log("[KCL] Continuing using directory name as mod name...");
                modName = cfgDirName;
            }
            else
            {
                modName = _modname;
            }

            if (!cn.TryGetValue("showChangelog", ref showCL))
            {
                Debug.Log("[KCL] \"showChangelog\" field does not exist in mod ");
                Debug.Log("[KCL] Assuming [true] to show changelog, adding field to changelog...");
                if (!cn.SetValue("showChangelog", false, true)) //creates a new field for the viewing status, setting it to false
                {
                    Debug.Log("[KCL] Unable to create 'showChangelog' in directory " + cfgDirName + " (field was missing in file)");
                }
				cfgDir.parent.SaveConfigs();
            }

			string _author = "";
			if(cn.TryGetValue("author", ref _author))
			{
				author = _author;
			}
			string _license = "";
			if(cn.TryGetValue("license", ref _license))
			{
				license = _license;
			}


			foreach (ConfigNode vn in cn.GetNodes("VERSION"))
            {
                changeSets.Add(new ChangeSet(vn, cfgDirName));
            }
        }

        public override string ToString()
        {
            string ret = modName + "\n";
			ret += ((author == null) ? "" : "Created by: " + author + "\n");
			ret += ((license == null) ? "\n" : "Licensed under the " + license + " license\n\n"); //give a double line break here
            foreach(ChangeSet cs in changeSets)
            {
                ret += cs.ToString();
            }
            return ret;
        }
    }
}