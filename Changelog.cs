using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace KerbalChangelog
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class Changelog : MonoBehaviour
	{
		/*
		 * Notes on usage of the changelog:
		 * 
		 * Changelog MUST be titled Changelog.cfg
		 * Changelog MUST be in the following format:
		 * 
		 * KERBALCHANGELOG
		 * {
		 *		showChangelog = [true|false]
		 *		modName = CoolMod
		 *		VERSION
		 *		{
		 *			version = 1.1
		 *			change = Added new parts.
		 *			change = Removed most bugs.
		 *		}
		 *		VERSION
		 *		{
		 *			version = 1.0
		 *			change = Release!!!
		 *		}
		 *	}
		 *	
		 *	Treat it with the same level of respect as you do regular config files. 
		 *	When there is a new release, change showChangelog to true. 
		 *	After the changelog is viewed in-game, the value will be automatically set to false.
		*/
		// No creating new instances of this
		private Changelog()
		{

		}
		bool showChangelog = true;
		Rect changelogRect;
		Vector2 changelogScrollPos = new Vector2();
		Vector2 quickSelectionScrollPos = new Vector2();
		//Dictionary<string, string> modChangelogs = new Dictionary<string, string>();
		List<Tuple<string, string, string>> modChangelogs= new List<Tuple<string, string, string>>();
		int index = 0;
		bool changesLoaded = false;
		float widthMultiplier = Screen.width / 1920f;
		float heightMultiplier = Screen.height / 1080f;
		float windowWidth;
		float windowHeight;
		bool changelogSelection = false;

		private void Start()
		{
			windowWidth = 600f * widthMultiplier;
			windowHeight = 800f * heightMultiplier;
			changelogRect = new Rect(100, 100, windowWidth, windowHeight);
			LoadChangelogs();
		}
		private void OnGUI()
		{
			if (showChangelog && changesLoaded && modChangelogs.Count > 0 && !changelogSelection)
			{
				changelogRect = GUILayout.Window(89156, changelogRect, DrawChangelogWindow, modChangelogs[index].item1 + " " + modChangelogs[index].item3, GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));
			}
			else if (showChangelog && changesLoaded && modChangelogs.Count > 0 && changelogSelection)
			{
				changelogRect = GUILayout.Window(89157, changelogRect, DrawChangelogSelectionWindow, "Kerbal Changelog", GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));
			}
		}
		private void DrawChangelogSelectionWindow(int id)
		{ 
			GUI.DragWindow(new Rect(0, 0, changelogRect.width, 20));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Read changelogs"))
			{
				changelogSelection = false;
			}
			GUILayout.EndHorizontal();
			quickSelectionScrollPos = GUILayout.BeginScrollView(quickSelectionScrollPos);
			foreach(Tuple<string, string, string> modChange in modChangelogs)
			{
				if(GUILayout.Button(modChange.item1))
				{
					index = modChangelogs.IndexOf(modChange);
					changelogSelection = false;
				}
			}
			GUILayout.EndScrollView();
		}
		private void DrawChangelogWindow(int id)
		{
			GUI.DragWindow(new Rect(0, 0, changelogRect.width, 20));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Select changelogs"))
			{
				changelogSelection = true;
			}
			GUILayout.EndHorizontal();
			changelogScrollPos = GUILayout.BeginScrollView(changelogScrollPos);
			GUILayout.Label(modChangelogs[index].item2);
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Previous"))
			{
				if(index == 0)
				{
					index = modChangelogs.Count - 1;
				}
				else
				{
					index--;
				}
			}
			if (GUILayout.Button("Close"))
			{
				showChangelog = false;
			}
			if(GUILayout.Button("Next"))
			{
				if(index == (modChangelogs.Count - 1))
				{
					index = 0;
				}
				else
				{
					index++;
				}
			}
			GUILayout.EndHorizontal();
		}
		private void LoadChangelogs()
		{
			Debug.Log("[KCL] Loading changelogs...");
			ConfigNode[] changelogNodes = GameDatabase.Instance.GetConfigNodes("KERBALCHANGELOG");
			Debug.Log($"[KCL] {changelogNodes.Length} changelogs found");
			UrlDir.UrlConfig[] cfgDirs = GameDatabase.Instance.GetConfigs("KERBALCHANGELOG");
			UrlDir gameDatabaseUrl = GameDatabase.Instance.root;
			bool firstVersionNumber = true;
			foreach (UrlDir.UrlConfig cfgDir in cfgDirs)
			{
				bool show = true;
				ConfigNode cln = cfgDir.config;
				cln.TryGetValue("showChangelog", ref show);
				if (!show)
				{
					continue;
				}
				string mod = cln.GetValue("modName");
				Debug.Log($"[KCL] {mod}'s changelog is being displayed: {show}");
				string modChangelog = "";
				List<string> version = new List<string>();
				foreach(ConfigNode vn in cln.GetNodes("VERSION"))
				{
					if(!firstVersionNumber)
					{
						modChangelog += "\n";
					}
					firstVersionNumber = false;
					version.Add(vn.GetValue("version"));
					modChangelog += (vn.GetValue("version") + ":\n");
					foreach(string change in vn.GetValues("change"))
					{
						modChangelog += ("* " + change + "\n");
					}
				}
				Debug.Log($"[KCL] Adding changelog\n{modChangelog} for mod {mod}");
				modChangelogs.Add(new Tuple<string, string, string>(mod, modChangelog, HighestVersion(version)));
				if (!cln.SetValue("showChangelog", false))
				{
					Debug.Log("[KCL] Unable to set value 'showChangelog'.");
				}
				else
				{
					Debug.Log("[KCL] Set value of 'showChangelog' to False");
				}
				Debug.Log($"[KCL] {cfgDir.parent.fullPath}");
				cfgDir.config.Save(cfgDir.parent.fullPath);
				firstVersionNumber = true;
			}
			changesLoaded = true;
		}
		private string HighestVersion(List<string> versions)
		{
			Debug.Log($"[KCL] Getting highest version");
			int numOfMinVersions = -1;
			foreach(string s in versions)
			{
				if((NumberOfVersionSeperators(s) + 1) > numOfMinVersions)
				{
					numOfMinVersions = NumberOfVersionSeperators(s) + 1;
				}
			}
			Debug.Log($"[KCL] numOfMinVersions={numOfMinVersions}");
			int[] highestVersions = new int[numOfMinVersions];
			FillArray(ref highestVersions, -1);
			List<string> avalibleVersions = new List<string>(versions);
			for(int i = 0; i < avalibleVersions.Count; i++)
			{
				if (avalibleVersions[i].Split('.').Length < numOfMinVersions)
				{
					for (int j = 0; j < numOfMinVersions - avalibleVersions[i].Split('.').Length; j++)
					{
						avalibleVersions[i] += ".0";
					}
				}
			}
			List<string> avalibleVersionsCopy = new List<string>(avalibleVersions);
			for (int i = 0; i < numOfMinVersions; i++)
			{
				Debug.Log($"[KCL] version segment = {i}");
				foreach (string s in avalibleVersions)
				{
					Debug.Log($"[KCL] s={s}");
					if (int.Parse(s.Split('.')[i]) > highestVersions[i])
					{
						Debug.Log($"[KCL] s was bigger than highestVersions[{i}]");
						highestVersions[i] = int.Parse(s.Split('.')[i]);
						avalibleVersionsCopy.Clear();
						avalibleVersionsCopy.Add(s);
					}
					else if (int.Parse(s.Split('.')[i]) == highestVersions[i])
					{
						Debug.Log($"[KCL] s was the same as highestVersions[{i}]");
						avalibleVersionsCopy.Add(s);
					}
				}
				avalibleVersions = new List<string>(avalibleVersionsCopy);
			}
			string returnVersion = "v";
			foreach (string s in avalibleVersions)
			{
				Debug.Log($"[KCL] length of avalibleVersions={avalibleVersions.Count}");
				string[] versionArray = s.Split('.');
				int numOfActualVersions = versionArray.Length;
				for(int i = versionArray.Length - 1; i >= 0; i--)
				{
					if(int.Parse(versionArray[i]) == 0)
					{
						numOfActualVersions--;
					}
					else if(int.Parse(versionArray[i]) != 0)
					{
						break;
					}
				}
				Debug.Log($"[KCL] length of numOfActualVerions={numOfActualVersions}");
				for (int i = 0; i < numOfActualVersions; i++)
				{
					returnVersion += ("." + s.Split('.')[i]);
				}
				returnVersion += ".0";
				return returnVersion;
			}
			Debug.Log($"[KCL] Highest version is {returnVersion}");
			return returnVersion;
		}
		private void FillArray(ref int[] array, int num)
		{
			for(int i = 0; i < array.Length; i++)
			{
				array[i] = num;
			}
		}
		private void FillArray(ref int[,] array, int num)
		{
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for(int j = 0; j < array.GetLength(1); j++)
				{
					array[i, j] = num;
				}
			}
		}
		private int NumberOfVersionSeperators(string version)
		{
			int periods = 0;
			char[] charVersion = version.ToCharArray();
			foreach(char c in charVersion)
			{
				if(c == '.')
				{
					periods++;
				}
			}
			return periods;
		}
	}
}