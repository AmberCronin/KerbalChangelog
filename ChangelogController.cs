using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KerbalChangelog
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class ChangelogController : MonoBehaviour
	{
		Rect displayWindow;
		Vector2 changelogScrollPos = new Vector2();
		Vector2 quickSelectionScrollPos = new Vector2();

		List<Changelog> changelogs = new List<Changelog>();
		int dispIndex = 0;
		Changelog dispcl;

		bool showChangelog = true;
		bool changesLoaded = false;
		bool changelogSelection = false;

		private void Start()
		{
			Debug.Log("[KCL] Starting up");
			//set up the window
			displayWindow = new Rect(100, 100, WindowConstants.windowWidth, WindowConstants.windowHeight);
			changelogs = LoadChangelogs();
			Debug.Log("[KCL] Displaying " + changelogs.Count + " changelogs");
			changesLoaded = true;
		}

		private List<Changelog> LoadChangelogs()
		{
			Debug.Log("[KCL] Loading changelogs...");
			List<Changelog> retList = new List<Changelog>();
			UrlDir.UrlConfig[] cfgDirs = GameDatabase.Instance.GetConfigs("KERBALCHANGELOG");
			foreach (var cfgDir in cfgDirs)
			{
				ConfigNode kclNode = cfgDir.config; //loads the config node from the directory
				retList.Add(new Changelog(kclNode, cfgDir));

				if (!kclNode.SetValue("showChangelog", false)) //sets changelogs to unviewable
				{
					Debug.Log("[KCL] Unable to set value 'showChangelog' in " + cfgDir.ToString());
				}
				cfgDir.parent.SaveConfigs();
			}
			IEnumerable<Changelog> trueList = from Changelog cl in retList
											  where cl.showCL == true
											  select cl;
			Debug.Log("[KCL] Loaded " + retList.Count + " valid changelogs");
			return trueList.ToList();
		}

		private void OnGUI()
		{
			if (changelogs.Count == 0)
			{
				return;
			}
			dispcl = changelogs[dispIndex];
			if (showChangelog && changesLoaded && !changelogSelection)
			{
				displayWindow = GUILayout.Window(89156, displayWindow, DrawChangelogWindow, dispcl.modName + " " + dispcl.highestVersion.ToStringVersionName(), GUILayout.Width(WindowConstants.windowWidth), GUILayout.Height(WindowConstants.windowHeight));
			}
			else if (showChangelog && changesLoaded && changelogSelection)
			{
				displayWindow = GUILayout.Window(89157, displayWindow, DrawChangelogSelection, "Kerbal Changelog", GUILayout.Width(WindowConstants.windowWidth), GUILayout.Height(WindowConstants.windowHeight));
			}
		}

		private void DrawChangelogWindow(int id)
		{
			GUI.DragWindow(new Rect(0, 0, displayWindow.width, 20));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Select changelogs"))
			{
				changelogSelection = true;
			}
			GUILayout.EndHorizontal();
			changelogScrollPos = GUILayout.BeginScrollView(changelogScrollPos);
			//GUIStyle style = new GUIStyle();
			//style.richText = true;
			var style = new GUIStyle(GUI.skin.label);
			style.richText = true;
			if (dispcl.webpageValid)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Visit this mod's website"))
				{
					Application.OpenURL("https://" + dispcl.webpage);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			GUILayout.Label(dispcl.ToString(), style);

			//GUILayout.Label(dispcl.ToString());
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Previous"))
			{
				if (dispIndex == 0)
				{
					dispIndex = changelogs.Count - 1;
				}
				else
				{
					dispIndex--;
				}
			}
			if (GUILayout.Button("Close"))
			{
				showChangelog = false;
			}
			if (GUILayout.Button("Next"))
			{
				if (dispIndex == (changelogs.Count - 1))
				{
					dispIndex = 0;
				}
				else
				{
					dispIndex++;
				}
			}
			GUILayout.EndHorizontal();
		}
		private void DrawChangelogSelection(int id)
		{
			GUI.DragWindow(new Rect(0, 0, displayWindow.width, 20));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Read changelogs"))
			{
				changelogSelection = false;
			}
			GUILayout.EndHorizontal();
			quickSelectionScrollPos = GUILayout.BeginScrollView(quickSelectionScrollPos);

			foreach(Changelog cl in changelogs)
			{
				if(GUILayout.Button(cl.modName))
				{
					dispIndex = changelogs.IndexOf(cl);
					changelogSelection = false;
				}
			}
			GUILayout.EndScrollView();
		}
	}
}