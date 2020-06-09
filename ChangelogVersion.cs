using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KerbalChangelog
{
	public class ChangelogVersion : IComparable
	{
		bool versionNull = false;
		bool malformedVersionString = false;

		public int major { get; private set; }
		public int minor { get; private set; }
		public int patch { get; private set; }
		public int build { get; private set; }

		bool buildExisted;

		public string versionName { get; private set; } = null;
		public bool versionNameExists
		{
			get
			{
				if (versionName == null)
					return false;
				return true;
			}
		}
		public string versionDate { get; private set; } = null;
		public bool versionDateExists
		{
			get
			{
				if (versionDate == null)
					return false;
				return true;
			}
		}
		public string versionKSP { get; private set; } = null;
		public bool versionKSPExists
		{
			get
			{
				if (versionKSP == null)
					return false;
				return true;
			}
		}

		public ChangelogVersion(int maj, int min, int pat)
		{
			major = maj;
			minor = min;
			patch = pat;
			buildExisted = false;
		}
		public ChangelogVersion(int maj, int min, int pat, int bui)
		{
			major = maj;
			minor = min;
			patch = pat;
			build = bui;
			buildExisted = true;
		}
		public ChangelogVersion(int maj, int min, int pat, string vName) : this(maj, min, pat)
		{
			versionName = vName;
		}
		public ChangelogVersion(int maj, int min, int pat, int bui, string vName) : this(maj, min, pat, bui)
		{
			versionName = vName;
		}

		// May fail, needs a try/catch
		public ChangelogVersion(string version, string cfgDirName)
		{
			if (version == "null")
			{
				versionNull = true;
				return;
			}

			Regex pattern = new Regex("(\\d+\\.\\d+\\.\\d+(\\.\\d+)?)"); //matches version numbers starting at the beginning to the end of the string
			Regex malformedPattern = new Regex("\\d+\\.\\d+(\\.\\d+)?(\\.\\d+)?");
			if (!pattern.IsMatch(version))
			{
				if (!malformedPattern.IsMatch(version))
				{
					Debug.Log("[KCL] broken version string: " + version);
					throw new ArgumentException("version is not a valid version");
				}
				Debug.Log("[KCL] malformed version string: " + version + " in directory " + cfgDirName);
				malformedVersionString = true;
			}
			string[] splitVersions = version.Split('.');

			major = int.Parse(splitVersions[0]);
			minor = int.Parse(splitVersions[1]);
			if (!malformedVersionString)
				patch = int.Parse(splitVersions[2]);
			else
				patch = 0;
			if (splitVersions.Length > 3)
			{
				build = int.Parse(splitVersions[3]);
				buildExisted = true;
			}
			else
			{
				build = 0;
				buildExisted = false;
			}
		}
		public ChangelogVersion(string version, string cfgDirName, string vName) : this(version, cfgDirName)
		{
			if (vName != "")
				versionName = vName;
		}
		public ChangelogVersion(string version, string cfgDirName, string vName, string vDate, string vKSP) : this(version, cfgDirName, vName)
		{
			if (vDate != "")
				versionDate = vDate;
			if (vKSP != "")
				versionKSP = vKSP;
		}

		public override string ToString()
		{
			if (versionNull)
				return "D.N.E";
			if (!buildExisted)
				return $"{major}.{minor}.{patch}" + (versionNameExists ? " \"" + versionName + "\"" : "") + (versionDateExists ? ", released " + versionDate : "") + (versionKSPExists ? ", for KSP version " + versionKSP : "");
			return $"{major}.{minor}.{patch}.{build}" + (versionNameExists ? " \"" + versionName + "\"" : "") + (versionDateExists ? ", released " + versionDate : "") + (versionKSPExists ? ", for KSP version " + versionKSP : "");
		}
		public string ToStringPure()
		{
			if (versionNull)
				return "D.N.E";
			if (!buildExisted)
				return $"{major}.{minor}.{patch}";
			return $"{major}.{minor}.{patch}.{build}";
		}
		public string ToStringVersionName()
		{
			if (versionNull)
				return "D.N.E";
			if (!buildExisted)
				return $"{major}.{minor}.{patch}" + (versionNameExists ? " \"" + versionName + "\"" : "");
			return $"{major}.{minor}.{patch}" + (versionNameExists ? " \"" + versionName + "\"" : "");
		}

		//This comparator will sort objects from highest version to lowest version
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (obj is ChangelogVersion oCLV)
			{
				if (oCLV.major - this.major == 0)
				{
					if (oCLV.minor - this.minor == 0)
					{
						if (oCLV.patch - this.patch == 0)
						{
							return oCLV.build.CompareTo(this.build);
						}
						return oCLV.patch.CompareTo(this.patch);
					}
					return oCLV.minor.CompareTo(this.minor);
				}
				return oCLV.major.CompareTo(this.major);
			}
			else
				throw new ArgumentException("Object is not a ChangelogVersion");
		}
	}
}