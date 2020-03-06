using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class ChangelogController : MonoBehaviour
    {
        /*
         * Notes on usage of the changelog:
         * 
         * Changelog MUST be titled Changelog.cfg
         * Changelog MUST be in the following format:
         * 
         * KERBALCHANGELOG
         * {
         *      showChangelog = [true|false]
         *      modName = CoolMod
         *      VERSION
         *      {
         *          version = 1.1
         *          change = Added new parts.
         *          change = Removed most bugs
         *      }
         *      VERSION
         *      {
         *          version = 1.0
         *          change = Release!!!
         *      }
         *  }
         *  
         *  Treat it with the same level of respect as you do regular config files. 
         *  When there is a new release, change showChangelog to true. 
         *  After the changelog is viewed in-game, the value will be automatically set to false.
        */

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

				if(!kclNode.SetValue("showChangelog", false)) //sets changelogs to unviewable
				{
					Debug.Log("[KCL] Unable to set value 'showChangelog' in " + cfgDir.ToString());
				}
				cfgDir.parent.SaveConfigs();
			}
            return retList;
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
                displayWindow = GUILayout.Window(89156, displayWindow, DrawChangelogWindow, dispcl.modName + " " + dispcl.highestVersion, GUILayout.Width(WindowConstants.windowWidth), GUILayout.Height(WindowConstants.windowHeight));
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
			GUIStyle style = new GUIStyle();
			style.richText = true;
            GUILayout.Label(dispcl.ToString(), style); //add the \n for seperation of changelogs
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