using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

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
		Dictionary<string, string> modChangelogs = new Dictionary<string, string>();
		int index = 0;
		bool changesLoaded = false;
		float widthMultiplier = Screen.width / 1920f;
		float heightMultiplier = Screen.height / 1080f;

		private void Start()
		{
			changelogRect = new Rect(100, 100, 300f * widthMultiplier, 400f * heightMultiplier);
			LoadChangelogs();
		}
		private void OnGUI()
		{
			if (showChangelog && changesLoaded && modChangelogs.Count > 0)
			{
				changelogRect = GUILayout.Window(89156, changelogRect, DrawChangelogWindow, modChangelogs.Keys.ElementAt(index), GUILayout.Width(300f * widthMultiplier), GUILayout.Height(400f * heightMultiplier));
			}
		}
		private void DrawChangelogWindow(int id)
		{
			GUI.DragWindow(new Rect(0, 0, changelogRect.width, 20));
			changelogScrollPos = GUILayout.BeginScrollView(changelogScrollPos);
			GUILayout.Label(modChangelogs.Values.ElementAt(index));
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
				foreach(ConfigNode vn in cln.GetNodes("VERSION"))
				{
					modChangelog += (vn.GetValue("version") + ":\n");
					foreach(string change in vn.GetValues("change"))
					{
						modChangelog += ("* " + change + "\n");
					}
				}
				Debug.Log($"[KCL] Adding changelog\n{modChangelog} for mod {mod}");
				modChangelogs.Add(mod, modChangelog);
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
			}
			changesLoaded = true;
		}
	}
}